using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.MachineLearning.NLP;
using WTG.MachineLearning.NLP.Preprocessing;
using WTG.Numerics;
using WTG.Numerics.LinearAlgebra;

namespace Enterprise.Client.EDI
{
	public class IncidentAssociationNewIncidentsRunner
	{
		readonly ILogger logger;
		readonly IIncidentTokenizer incidentTokenizer;
		readonly ISimilarIncidentRepository similarIncidentRepository;
		readonly ILinearAlgebra linearAlgebra;
		readonly IMetaSupportIncidentProvider metaSupportIncidentProvider;
		readonly ISimilarityMatrixBootstrapperQueryExecutor similarityMatrixBootstrapperQueryExecutor;

		public IncidentAssociationNewIncidentsRunner(ILogger logger)
		{
			this.logger = logger;

			similarIncidentRepository = new SimilarIncidentRepository();
			incidentTokenizer = new IncidentTokenizer(similarIncidentRepository.GetTokenPairs().ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString(CultureInfo.InvariantCulture)), new PorterStemmer());
			linearAlgebra = new BasicLinearAlgebra();
			metaSupportIncidentProvider = new MetaSupportIncidentProvider();
			similarityMatrixBootstrapperQueryExecutor = new SimilarityMatrixBootstrapperQueryExecutor();
		}

		public IncidentAssociationNewIncidentsRunner(
			IIncidentTokenizer incidentTokenizer,
			ISimilarIncidentRepository similarIncidentRepository,
			ILinearAlgebra linearAlgebra,
			IMetaSupportIncidentProvider metaSupportIncidentProvider,
			ISimilarityMatrixBootstrapperQueryExecutor similarityMatrixBootstrapperQueryExecutor,
			ILogger logger)
		{
			this.logger = logger;

			this.incidentTokenizer = incidentTokenizer ?? throw new ArgumentNullException(nameof(incidentTokenizer));
			this.similarIncidentRepository = similarIncidentRepository ?? throw new ArgumentNullException(nameof(similarIncidentRepository));
			this.linearAlgebra = linearAlgebra ?? throw new ArgumentNullException(nameof(linearAlgebra));
			this.metaSupportIncidentProvider = metaSupportIncidentProvider ?? throw new ArgumentNullException(nameof(metaSupportIncidentProvider));
			this.similarityMatrixBootstrapperQueryExecutor = similarityMatrixBootstrapperQueryExecutor ?? throw new ArgumentNullException(nameof(similarityMatrixBootstrapperQueryExecutor));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Consistency with surrounding code, which will not be touched for this defect fix")]
		public void CleanUpOldIncidentAssociations()
		{
			var fromDate = DateTime.UtcNow.AddMonths(-EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value);

			logger?.Information("Deleteing old rows from dbo.IncidentSimilarityTfIdf");
			var guids = similarityMatrixBootstrapperQueryExecutor.DeleteOldIncidentsFromIncidentSimilarityTfIdf(fromDate);
			logger?.Information("Deleteing old rows from IncidentSimilarityMatix");
			similarityMatrixBootstrapperQueryExecutor.DeleteIncidentsFromIncidentSimilarityMatix(guids);
			logger?.Information("Deleteing old rows from dbo.IncidentSimilarityExclusion");
			similarityMatrixBootstrapperQueryExecutor.DeleteOldIncidentsFromIncidentSimilarityExclusion(fromDate);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1061:DoNotUseDateTimeUtcNow", Justification = "Baseline")]
		public void ProcessNewIncidentVectors()
		{
			logger?.Information("Processing new incident vectors.");

			var toDate = DateTime.UtcNow;
			var fromDate = toDate.AddMonths(-EDIDataRegistry.Instance.RelatedIncidentsMonthsToStore.Value);

			var factory = new BusinessObjectFactory();
			var newAndUpdatedGuids = similarityMatrixBootstrapperQueryExecutor
				.GetNewAndUpdatedIncidentGuids(similarIncidentRepository.RepositoryVersion, fromDate, toDate)
				.ToList();

			if (newAndUpdatedGuids.Count == 0)
			{
				return;
			}

			var newAndUpdatedIncidents = metaSupportIncidentProvider.FromGuids(newAndUpdatedGuids).ToList();

			var idfVector = similarIncidentRepository.GetIdfVector().ToList();
			var synonymGroups = similarIncidentRepository.GetTokenPairs();

			var idfDict = synonymGroups
				.Values
				.Select(s => s.ToString(CultureInfo.InvariantCulture))
				.Distinct()
				.OrderBy(s => s)
				.Select((s, i) => (Symbol: s, Index: i))
				.ToDictionary(tup => tup.Symbol, tup => idfVector[tup.Index]);

			var tfIdfDocumentSimilarity = new TfIdfDocumentSimilarity(idfDict, linearAlgebra);
			var metaISVProvider = new MetaIncidentSimilarityTfIdfProvider();
			var metaISVs = new List<MetaIncidentSimilarityTfIdf>();
			var batchSize = EDIDataRegistry.Instance.RelatedIncidentsTfIdfBatchSize.Value;
			var decimalPrecision = EDIDataRegistry.Instance.RelatedIncidentsDecimalPrecision.Value;

			var version = similarIncidentRepository.RepositoryVersion;

			logger?.Information(string.Format(CultureInfo.InvariantCulture, "{0} new or updated incidents to process...", newAndUpdatedIncidents.Count));

			var toUpdateTimestamp = new Dictionary<Guid, DateTime>();

			var numNonLatin = 0;

			foreach (var newOrUpdatedIncident in newAndUpdatedIncidents)
			{
				if (!newOrUpdatedIncident.Description.IsLatin())
				{
					numNonLatin++;
					AddToExclusion(newOrUpdatedIncident.PK, factory, newOrUpdatedIncident.SystemLastEditTimeUtc);
					continue;
				}

				var metaISV = new MetaIncidentSimilarityTfIdf()
				{
					IncidentGuid = newOrUpdatedIncident.PK,
					Version = version,
					IncidentLastModified = newOrUpdatedIncident.SystemLastEditTimeUtc
				};

				ProcessIncidentVectors(metaISV, newOrUpdatedIncident.ToString(), tfIdfDocumentSimilarity, decimalPrecision);

				var existingTfIdfQueryObject = metaISVProvider.FindByIncidentGuid(version, newOrUpdatedIncident.PK);

				if (existingTfIdfQueryObject != null)
				{
					var similarity = linearAlgebra.CosineSimilarity(existingTfIdfQueryObject.TFIDF.ToList(), metaISV.TFIDF.ToList());
					var threshold = Math.Pow(0.1, decimalPrecision);
					if (1.0 - similarity < threshold)
					{
						toUpdateTimestamp.Add(newOrUpdatedIncident.PK, newOrUpdatedIncident.SystemLastEditTimeUtc);
						continue;
					}

					similarityMatrixBootstrapperQueryExecutor.DeleteExistingRows(newOrUpdatedIncident.PK, version);
				}

				metaISVs.Add(metaISV);
				if (metaISVs.Count >= batchSize)
				{
					logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Writing {0} new or updated vectors", metaISVs.Count));
					metaISVProvider.Save(metaISVs);
					metaISVs.Clear();
				}
			}

			if (toUpdateTimestamp.Count > 0)
			{
				logger?.Information(string.Format(CultureInfo.InvariantCulture, "Updating the timestamps of {0} TF-IDFs with no substantial changes", toUpdateTimestamp.Count));
				similarIncidentRepository.UpdateTfIdfTimestamps(toUpdateTimestamp);
			}

			if (numNonLatin > 0)
			{
				logger?.Debug(string.Format(CultureInfo.InvariantCulture, "There are {0} non-Latin incidents, which are being ignored", numNonLatin));
			}

			logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Writing {0} new or updated vectors", metaISVs.Count));
			factory.Save();
			metaISVProvider.Save(metaISVs);
		}

		protected void AddToExclusion(ZGuid item, BusinessObjectFactory factory, ZDateTime systemLastEditTimeUtc)
		{
			if (factory == null)
			{
				logger?.Warning(string.Format(CultureInfo.InvariantCulture, "{0} received factory as null", nameof(AddToExclusion)));
				return;
			}
			var filter = new ZQuery();
			filter.AddToFilter(IncidentSimilarityExclusionSchema.ISE_IM, item);
			var record = factory.LoadTop1<IncidentSimilarityExclusion>(filter);
			if (record != null && record.ISE_LastUpdatedUtc != systemLastEditTimeUtc)
			{
				record.ISE_LastUpdatedUtc = systemLastEditTimeUtc;
				return;
			}
			var newRecord = factory.New<IncidentSimilarityExclusion>();
			newRecord.ISE_IM = item;
			newRecord.ISE_LastUpdatedUtc = systemLastEditTimeUtc;
		}

		public void ProcessIncidentVectors(MetaIncidentSimilarityTfIdf metaISV, string incidentString, TfIdfDocumentSimilarity tfIdfDocumentSimilarity, int decimalPrecision)
		{
			var incidentTokens = incidentTokenizer.TokenizeIncident(incidentString);

			var rawTermFrequency = tfIdfDocumentSimilarity.DocumentToBagOfWords(incidentTokens).ToList();
			var augmentedTermFrequency = tfIdfDocumentSimilarity.GetAugmentedTermFrequency(rawTermFrequency).ToList();
			var tfIdf = tfIdfDocumentSimilarity.ComputeTfIdf(augmentedTermFrequency, decimalPrecision).ToArray();

			metaISV.TermFrequency = rawTermFrequency.Select(i => (double)i).ToList();
			metaISV.TFIDF = tfIdf.ToList();
			metaISV.Status = IncidentSimilarityTfIdf.Status.TfIdfComputed;
		}

		public void ProcessNewIncidentSimilarities(int maxToStore, CancellationToken cancellationToken = default)
		{
			ProcessNewIncidentSimilarities(
				new MetaIncidentSimilarityTfIdfProvider(),
				(double)EDIDataRegistry.Instance.RelatedIncidentsMinimumSimilarity.Value,
				maxToStore,
				cancellationToken);
		}

		public void ProcessNewIncidentSimilarities(
			IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfidfProvider,
			double minimumSimilarity,
			int maxToStore,
			CancellationToken cancellationToken = default)
		{
			LogInfo($"Processing new incident similarities.");

			var latestVersion = similarIncidentRepository.GetLatestVersion();
			var repositoryVersion = similarIncidentRepository.RepositoryVersion;
			var numNewIncidents = metaIncidentSimilarityTfidfProvider.CountNewOrUpdatedIncidentTfIdfs(repositoryVersion);
			var numProcessedIncidents = metaIncidentSimilarityTfidfProvider.CountProcesedIncidentTfIdfs(repositoryVersion);
			LogDebug($"Current version: {latestVersion}, number of new incidents: {numNewIncidents}, number of processed incidents: {numProcessedIncidents}.");

			var stopwatch = new Stopwatch();
			var numNewIncidentsProcessed = 0;
			var allDone = false;
			var numSimilaritiesComputed = 0;
			var peakMemoryUsageEstimate = 0L;
			while (!cancellationToken.IsCancellationRequested)
			{
				stopwatch.Restart();
				var newIncidents = FetchNewIncidentEntries(metaIncidentSimilarityTfidfProvider, repositoryVersion, maxToStore);
				if (newIncidents.Count == 0)
				{
					allDone = true;
					break;
				}

				LogInfo($"Computing similarities for new incidents [{numNewIncidentsProcessed} - {numNewIncidentsProcessed + newIncidents.Count}] out of {numNewIncidents}.");

				ProcessSimilaritiesBetweenNewAndOldIncidents(newIncidents, metaIncidentSimilarityTfidfProvider, latestVersion, minimumSimilarity);
				ProcessSimilaritiesAmongNewIncidents(newIncidents, latestVersion, minimumSimilarity);
				foreach (var (incident, _, topSimilarities) in newIncidents)
				{
					similarityMatrixBootstrapperQueryExecutor.Commit(topSimilarities, incident.PK);
					numSimilaritiesComputed += topSimilarities.Count;
				}

				stopwatch.Stop();
				numNewIncidentsProcessed += newIncidents.Count;
				LogDebug($"Took {stopwatch.Elapsed.TotalMinutes} minutes to process {newIncidents.Count} new incidents.");

				var memoryUsage = GC.GetTotalMemory(false);
				peakMemoryUsageEstimate = Math.Max(peakMemoryUsageEstimate, memoryUsage);
				LogDebug($"Current managed heap allocation: {memoryUsage}");
			}

			if (!allDone)
			{
				LogInfo($"The similarity scores for {numNewIncidents - numNewIncidentsProcessed} new/updated incidents are yet to be processed.");
			}

			LogInfo($"Computed {numSimilaritiesComputed} similarity scores for {numNewIncidentsProcessed} new/updated incidents (out of {numNewIncidents}), with peak memory usage estimated at {peakMemoryUsageEstimate}.");
		}

		public int NewIncidentLoadStrideLength { get; internal set; } = 500;

		record struct NewIncidentEntry(IMetaIncidentSimilarityTfIdf incident, double magnitude, TopList<SimilarityMatrixDataTransferObject> topSimilarities);
		record struct OldIncidentEntry(IMetaIncidentSimilarityTfIdf incident, double magnitude);

		List<NewIncidentEntry> FetchNewIncidentEntries(
			IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfidfProvider,
			int repositoryVersion,
			int maxToStore
		)
		{
			return metaIncidentSimilarityTfidfProvider
				.LoadTopNewOrUpdatedIncidentTfIdfs(repositoryVersion, NewIncidentLoadStrideLength)
				.ToList()
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(incident => new NewIncidentEntry(
					incident,
					magnitude: MemoryOptimizedSparseVector<double>.FastMagnitude(incident.TfIdfSparseVector),
					topSimilarities: new TopList<SimilarityMatrixDataTransferObject>(maxToStore, CompareSimilarityMatrixDTO)
				))
				.ToList();
		}

		public int ProcessedIncidentLoadStrideLength { get; internal set; } = 10_000;

		void ProcessSimilaritiesBetweenNewAndOldIncidents(
			List<NewIncidentEntry> newIncidents,
			IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfidfProvider,
			int latestVersion,
			double minimumSimilarity)
		{
			var repositoryVersion = similarIncidentRepository.RepositoryVersion;
			var oldIncidentChunks = metaIncidentSimilarityTfidfProvider.LoadProcessedIncidentTfIdfsInChunks(
				repositoryVersion, ProcessedIncidentLoadStrideLength);

			foreach (var oldIncidentChunk in oldIncidentChunks)
			{
				var oldIncidents = oldIncidentChunk
					.AsParallel()
					.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
					.Select(incident => new OldIncidentEntry(
						incident,
						magnitude: MemoryOptimizedSparseVector<double>.FastMagnitude(incident.TfIdfSparseVector)))
					.ToList();

				var similarities = newIncidents
					.SelectMany(@new => oldIncidents.Select(@old => (@old, @new)))
					.AsParallel()
					.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
					.Select(pair => (
						pair.@new.topSimilarities,
						similarityDTO: ComputeSimilarity(
							latestVersion,
							minimumSimilarity,
							pair.@old.incident,
							pair.@new.incident,
							pair.@old.magnitude,
							pair.@new.magnitude)))
					.Where(x => x.similarityDTO != null);

				foreach (var (topSimilarities, similarityDTO) in similarities)
				{
					_ = topSimilarities.TryAdd(similarityDTO);
				}
			}
		}

		void ProcessSimilaritiesAmongNewIncidents(
			List<NewIncidentEntry> newIncidents,
			int latestVersion,
			double minimumSimilarity)
		{
			var similarities = newIncidents
				.SelectMany((@from, i) => Enumerable.Range(i + 1, newIncidents.Count - i - 1).Select(j => newIncidents[j]).Select(@to => (@from, @to)))
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(pair => (
					topSimilarities: pair.@to.topSimilarities,
					similarityDTO: ComputeSimilarity(
						latestVersion,
						minimumSimilarity,
						pair.@from.incident,
						pair.@to.incident,
						pair.@from.magnitude,
						pair.@to.magnitude)))
				.Where(x => x.similarityDTO != null);

			foreach (var (topSimilarities, similarityDTO) in similarities)
			{
				_ = topSimilarities.TryAdd(similarityDTO);
			}
		}

		SimilarityMatrixDataTransferObject ComputeSimilarity(
			int version,
			double minimumSimilarity,
			IMetaIncidentSimilarityTfIdf incidentOne,
			IMetaIncidentSimilarityTfIdf incidentTwo,
			double incidentVectorMagnitudeOne,
			double incidentVectorMagnitudeTwo)
		{
			var swap = string.Compare(incidentTwo.IncidentGuid.ToString(), incidentOne.IncidentGuid.ToString(), StringComparison.Ordinal) < 0;
			var similarity = MemoryOptimizedSparseVector<double>.FastDotProduct(incidentOne.TfIdfSparseVector, incidentTwo.TfIdfSparseVector) / (incidentVectorMagnitudeOne * incidentVectorMagnitudeTwo);
			if (similarity < minimumSimilarity)
			{
				return null;
			}

			return new SimilarityMatrixDataTransferObject(
				Guid.NewGuid(),
				swap ? incidentTwo.IncidentGuid : incidentOne.IncidentGuid,
				swap ? incidentOne.IncidentGuid : incidentTwo.IncidentGuid,
				version,
				similarity);
		}

		static int CompareSimilarityMatrixDTO(SimilarityMatrixDataTransferObject a, SimilarityMatrixDataTransferObject b)
		{
			var result = b.Similarity.CompareTo(a.Similarity); // NOTE: descending similarity
			if (result != 0)
			{
				return result;
			}

			result = new SqlGuid(a.Incident1.ToGuid()).CompareTo(new SqlGuid(b.Incident1.ToGuid()));
			if (result != 0)
			{
				return result;
			}

			result = new SqlGuid(a.Incident2.ToGuid()).CompareTo(new SqlGuid(b.Incident2.ToGuid()));
			return result;
		}

		void LogInfo(FormattableString message) => logger?.Information(message.ToString(CultureInfo.InvariantCulture));
		void LogDebug(FormattableString message) => logger?.Debug(message.ToString(CultureInfo.InvariantCulture));
	}

	class TopList<T> : IReadOnlyCollection<T>
	{
		readonly List<T> data;
		readonly int maxSize;
		readonly IComparer<T> comparer;

		public TopList(int maxSize, IComparer<T> comparer)
		{
			this.maxSize = maxSize;
			this.comparer = comparer;
			data = new List<T>(maxSize);
		}

		public TopList(int maxSize, Comparison<T> comparison) : this(maxSize, Comparer<T>.Create(comparison))
		{
		}

		public int Count => data.Count;
		public List<T>.Enumerator GetEnumerator() => data.GetEnumerator();
		IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool TryAdd(T item)
		{
			if (Count < maxSize)
			{
				data.Add(item);

				if (Count == maxSize)
				{
					data.Sort(comparer);
				}

				return true;
			}

			var largest = data.Last();
			if (comparer.Compare(largest, item) <= 0)
			{
				return false;
			}

			data.RemoveAt(data.Count - 1);
			var index = data.BinarySearch(item, comparer);
			if (index >= 0)
			{
				data.Insert(index, item);
			}
			else
			{
				data.Insert(~index, item);
			}

			return true;
		}
	}
}
