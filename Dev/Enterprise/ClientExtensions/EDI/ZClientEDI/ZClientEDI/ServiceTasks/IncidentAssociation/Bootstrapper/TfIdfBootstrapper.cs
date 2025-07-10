using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using WTG.MachineLearning.NLP;
using WTG.MachineLearning.NLP.Preprocessing;
using WTG.Numerics.LinearAlgebra;

namespace Enterprise.Client.EDI
{
	public interface ITfIdfBootstrapper
	{
		void BootstrapBetween(ZDateTime fromDate, ZDateTime toDate);
	}

	public class TfIdfBootstrapper : ITfIdfBootstrapper
	{
		readonly ILogger logger;
		readonly int newVersion;

		readonly ISimilarIncidentRepository similarIncidentRepository;
		readonly ILinearAlgebra linearAlgebra;
		readonly IIncidentTokenizer tokenizer;
		readonly ITfIdfBootstrapperQueryExecutor tfIdfBootstrapperQueryExecutor;
		readonly IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfIdfProvider;
		readonly IMetaSupportIncidentProvider metaSupportIncidentProvider;

		public TfIdfBootstrapper(int newVersion, IDictionary<string, int> tokens, ILogger logger) : this(
			newVersion,
			tokens.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture)),
			logger) { }

		public TfIdfBootstrapper(int newVersion, IDictionary<string, string> tokens, ILogger logger) : this(
			newVersion,
			new IncidentTokenizer(tokens, new PorterStemmer()),
			logger) { }

		public TfIdfBootstrapper(int newVersion,IIncidentTokenizer tokenizer,ILogger logger) : this(
			newVersion,
			new SimilarIncidentRepository(newVersion),
			new BasicLinearAlgebra(),
			tokenizer,
			new TfIdfBootstrapperQueryExecutor(),
			new MetaIncidentSimilarityTfIdfProvider(),
			new MetaSupportIncidentProvider(),
			logger) { }

		public TfIdfBootstrapper(
			int newVersion,
			ISimilarIncidentRepository similarIncidentRepository,
			ILinearAlgebra linearAlgebra,
			IIncidentTokenizer tokenizer,
			ITfIdfBootstrapperQueryExecutor tfIdfBootstrapperQueryExecutor,
			IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfIdfProvider,
			IMetaSupportIncidentProvider metaSupportIncidentProvider,
			ILogger logger)
		{
			this.logger = logger;
			this.tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
			this.similarIncidentRepository = similarIncidentRepository ?? throw new ArgumentNullException(nameof(similarIncidentRepository));
			this.linearAlgebra = linearAlgebra ?? throw new ArgumentNullException(nameof(linearAlgebra));
			this.tfIdfBootstrapperQueryExecutor = tfIdfBootstrapperQueryExecutor ?? throw new ArgumentNullException(nameof(tfIdfBootstrapperQueryExecutor));
			this.metaIncidentSimilarityTfIdfProvider = metaIncidentSimilarityTfIdfProvider ?? throw new ArgumentNullException(nameof(metaIncidentSimilarityTfIdfProvider));
			this.metaSupportIncidentProvider = metaSupportIncidentProvider ?? throw new ArgumentNullException(nameof(metaSupportIncidentProvider));

			var latestVersion = similarIncidentRepository.GetLatestVersion();
			if (latestVersion > newVersion)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The new version ({0}) is older than latest version {1}.", newVersion, latestVersion));
			}
			this.newVersion = newVersion;
		}

		public void BootstrapBetween(ZDateTime fromDate, ZDateTime toDate)
		{
			var tokens = similarIncidentRepository.GetTokenPairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture));
			var terms = tokens.Values.Distinct().OrderBy(s => s).ToList();

			var dummyIDF = terms.ToDictionary(term => term, term => 1.0);
			var termFrequencyProvider = new TfIdfDocumentSimilarity(dummyIDF, linearAlgebra);

			var state = metaIncidentSimilarityTfIdfProvider.Load(newVersion).ToList();
			logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Version {0} contains {1} records.", newVersion, state.Count));

			if (state.Any(metaISV => metaISV.Status == IncidentSimilarityTfIdf.Status.MatrixComputed))
			{
				logger?.Information(
					string.Format(CultureInfo.InvariantCulture, "There are some incidents already at the matrix stage, which means TF-IDF stage has previously completed for version {0}.", newVersion));
				return;
			}

			var batchSize = EDIDataRegistry.Instance.RelatedIncidentsTfIdfBatchSize.Value;

			if (!state.Any(metaISV => metaISV.Status == IncidentSimilarityTfIdf.Status.TfIdfComputed))
			{
				// No incidents have yet progressed to the TF-IDF stage, therefore TF may not yet be completed:

				var allIncidentGuids = tfIdfBootstrapperQueryExecutor.GetIncidentGuidsBetween(fromDate.ToDateTime(), toDate.ToDateTime());

				if (!TokenizeAndComputeTermFrequencies(state, allIncidentGuids, termFrequencyProvider, batchSize))
				{
					return;
				}
			}

			if (state.Count == 0)
			{
				logger?.Information("State is empty so will not proceed to IDF calculation, TF augmentation, or TF-IDF computation.");
				return;
			}

			logger?.Debug("Computing IDF vector");
			var idf = ComputeIdf(state).ToList();
			similarIncidentRepository.SetIdfVector(idf);

			logger?.Debug("Updating TFs and computing TF-IDFs");
			ComputeTfIdfs(state, idf, batchSize);
		}

		bool TokenizeAndComputeTermFrequencies(
			List<IMetaIncidentSimilarityTfIdf> metaTfIdf,
			IEnumerable<Guid> allIncidentGuids,
			TfIdfDocumentSimilarity termFrequencyProvider,
			int batchSize)
		{
			var donePKs = metaTfIdf
				.Where(isv => isv.Status == IncidentSimilarityTfIdf.Status.TermFrequencyComputed)
				.Select(isv => isv.IncidentGuid)
				.OrderBy(guid => guid)
				.ToList();

			// Only process those incidents that haven't been processed yet:
			var remainingIncidentGuids = allIncidentGuids.Where(guid => !donePKs.Contains(guid)).ToList();

			var stopwatch = new Stopwatch();

			while (remainingIncidentGuids.Any())
			{
				stopwatch.Restart();

				var pkBatch = remainingIncidentGuids.Take(batchSize).ToList();
				remainingIncidentGuids = remainingIncidentGuids.Skip(batchSize).ToList();

				var metaSupportIncidents = metaSupportIncidentProvider.FromGuids(pkBatch).ToList();

				var newMetaTfIdfList = metaSupportIncidents
					.AsParallel()
					.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
					.Where(msi => msi.Description.IsLatin())
					.Select(msi => (IMetaIncidentSimilarityTfIdf)new MetaIncidentSimilarityTfIdf()
					{
						IncidentGuid = msi.PK,
						Version = newVersion,
						IncidentLastModified = msi.SystemLastEditTimeUtc,
						TermFrequency = termFrequencyProvider.DocumentToBagOfWords(tokenizer.TokenizeIncident(msi.ToString())).Select(d => (double)d).ToList(),
						TFIDF = null,
						Status = IncidentSimilarityTfIdf.Status.TermFrequencyComputed
					})
					.ToList();

				metaIncidentSimilarityTfIdfProvider.Save(newMetaTfIdfList);
				metaTfIdf.AddRange(newMetaTfIdfList);

				donePKs.AddRange(pkBatch);

				stopwatch.Stop();
				logger?.Debug(string.Format(CultureInfo.InvariantCulture, "{0}ms/Incident", (stopwatch.Elapsed.TotalMilliseconds / batchSize).ToString("F1", CultureInfo.InvariantCulture)));

				var secondsRemaining = remainingIncidentGuids.Count * (stopwatch.Elapsed.TotalSeconds / batchSize);

				logger?.Debug(string.Format(
					CultureInfo.InvariantCulture,
					"{0}s remain - ETA: {1}",
					secondsRemaining.ToString("F1", CultureInfo.InvariantCulture),
					ZDateTime.UtcNow.AddSeconds((int)Math.Ceiling(secondsRemaining)).ToString()));
			}

			return true;
		}

		IEnumerable<double> ComputeIdf(List<IMetaIncidentSimilarityTfIdf> state)
		{
			var rawTermFrequencies = state
				.Select(tfIdfOb => tfIdfOb.TermFrequency)
				.ToList();

			var idfProvider = new InverseDocumentFrequency();
			return idfProvider.ComputeIdf(rawTermFrequencies);
		}

		void ComputeTfIdfs(
			IList<IMetaIncidentSimilarityTfIdf> state,
			IList<double> idf,
			int batchSize = 1000)
		{
			var updatePairs = new List<TfidfPairDataTransferObject>();
			var readyForFinalisation = state.Where(isv => isv.Status == IncidentSimilarityTfIdf.Status.TermFrequencyComputed).ToList();

			var start = 0;
			while (start < readyForFinalisation.Count)
			{
				logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Now processing vectors {0} to {1} of {2}", start + 1, start + batchSize, readyForFinalisation.Count));

				updatePairs.AddRange(readyForFinalisation
					.Skip(start)
					.Take(batchSize)
					.AsParallel()
					.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
					.Select(metaISV => (metaISV.PK, TFIDF: metaISV.AugmentedTermFrequency.Select((x, i) => x * idf[i])))
					.Select(metaISV => new TfidfPairDataTransferObject(metaISV.PK, metaISV.TFIDF)));

				start += batchSize;
			}

			logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Now committing {0} vectors", readyForFinalisation.Count));
			tfIdfBootstrapperQueryExecutor.UpdateTfidfPairs(updatePairs, batchSize);
		}
	}
}
