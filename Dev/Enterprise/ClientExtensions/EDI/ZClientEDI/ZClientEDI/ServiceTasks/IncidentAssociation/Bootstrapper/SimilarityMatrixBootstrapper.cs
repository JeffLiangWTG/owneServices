using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using WTG.Numerics;

namespace Enterprise.Client.EDI
{
	public class SimilarityMatrixBootstrapper
	{
		readonly ILogger logger;
		readonly ISimilarIncidentRepository similarIncidentRepository;
		readonly IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfIdfProvider;
		readonly ISimilarityMatrixBootstrapperQueryExecutor similarityMatrixBootstrapperQueryExecutor;

		public SimilarityMatrixBootstrapper(ILogger logger = null) : this(
			new SimilarIncidentRepository(),
			new MetaIncidentSimilarityTfIdfProvider(),
			new SimilarityMatrixBootstrapperQueryExecutor(),
			logger)
		{ }

		public SimilarityMatrixBootstrapper(
			ISimilarIncidentRepository similarIncidentRepository,
			IMetaIncidentSimilarityTfIdfProvider metaIncidentSimilarityTfIdfProvider,
			ISimilarityMatrixBootstrapperQueryExecutor similarityMatrixBootstrapperQueryExecutor,
			ILogger logger = null)
		{
			this.logger = logger;
			this.similarIncidentRepository = similarIncidentRepository ?? throw new ArgumentNullException(nameof(similarIncidentRepository));
			this.metaIncidentSimilarityTfIdfProvider = metaIncidentSimilarityTfIdfProvider ?? throw new ArgumentNullException(nameof(metaIncidentSimilarityTfIdfProvider));
			this.similarityMatrixBootstrapperQueryExecutor = similarityMatrixBootstrapperQueryExecutor ?? throw new ArgumentNullException(nameof(similarityMatrixBootstrapperQueryExecutor));
		}

		public bool BootstrapSimilarityMatrix(int newVersion, int maxToStore, double minimumSimilarity, int strideLength)
		{
			var latestVersion = similarIncidentRepository.GetLatestVersion();
			if (latestVersion > newVersion)
			{
				throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The latest version ({0}) is not older than version {1}", latestVersion, newVersion));
			}

			var incidentVectors = metaIncidentSimilarityTfIdfProvider
				.Load(newVersion)
				.ToList();

			if (incidentVectors.All(isv => isv.Status == IncidentSimilarityTfIdf.Status.MatrixComputed))
			{
				logger?.Information("Similarity matrix has already been computed.");
				return true;
			}
			if (incidentVectors.Any(ism => ism.Status != IncidentSimilarityTfIdf.Status.TfIdfComputed && ism.Status != IncidentSimilarityTfIdf.Status.MatrixComputed))
			{
				logger?.Warning("There are some support incident vectors that are not ready for similarity matrix computation yet. Similarity matrix will not be computed.");
				return false;
			}

			logger?.Debug("Computing vector magnitudes");
			var vectorMagnitudes = PregenerateVectorMagnitudes(incidentVectors).ToList();
			List<SimilarityMatrixDataTransferObject> sqlValues = null;
			List<SimilarityMatrixDataTransferObject> completedValues = null;

			GenerateSimilarityMatrixByRows(incidentVectors, vectorMagnitudes, sqlValues, completedValues, minimumSimilarity, newVersion, maxToStore, strideLength);

			return metaIncidentSimilarityTfIdfProvider
				.Load(newVersion)
				.All(isv => isv.Status == IncidentSimilarityTfIdf.Status.MatrixComputed);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		void GenerateSimilarityMatrixByRows(
			IList<IMetaIncidentSimilarityTfIdf> incidentVectors,
			IList<double> vectorMagnitudes,
			IList<SimilarityMatrixDataTransferObject> sqlBuffer,
			IList<SimilarityMatrixDataTransferObject> sqlReadyToCommit,
			double minimumSimilarity,
			int newVersion,
			int maxToStore,
			int strideLength)
		{
#pragma warning disable CA1502 // Avoid excessive complexity
			Task computeTask;
			var completedIndex = -1;

			var minIndex = 0;
			while (minIndex < incidentVectors.Count)
			{
				var maxIndex = Math.Min(minIndex + strideLength, incidentVectors.Count);
				var updatedVectors = metaIncidentSimilarityTfIdfProvider
					.FindByVectorGuids(incidentVectors.Skip(minIndex).Take(maxIndex - minIndex).Select(v => v.PK))
					.ToList();

				for (var i = 0; i < updatedVectors.Count; i++)
				{
					var idx = incidentVectors.IndexOf(v => v.PK == updatedVectors[i].PK);
					if (idx >= 0)
					{
						incidentVectors[idx] = updatedVectors[i];
					}
				}

				if (updatedVectors.All(isv => isv.Status == IncidentSimilarityTfIdf.Status.MatrixComputed))
				{
					minIndex += strideLength;
					continue;
				}

				var lockName = string.Format(CultureInfo.InvariantCulture, "ISM_{0}", minIndex);
				logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Trying to lock on {0}", lockName));
				if (Db.Connection.TryGetLock(lockName, out SqlApplicationLock appLock))
				{
					logger?.Debug(string.Format(CultureInfo.InvariantCulture, "Obtained lock on {0}", lockName));
					using (appLock)
					{
						// We must first check if another processor has finished this batch by comparing with what's now in DB:
						updatedVectors = metaIncidentSimilarityTfIdfProvider
							.FindByVectorGuids(incidentVectors.Skip(minIndex).Take(maxIndex - minIndex).Select(v => v.PK))
							.ToList();

						for (var i = 0; i < updatedVectors.Count; i++)
						{
							var idx = incidentVectors.IndexOf(v => v.PK == updatedVectors[i].PK);
							if (idx >= 0)
							{
								incidentVectors[idx] = updatedVectors[i];
							}
						}

						if (updatedVectors.All(isv => isv.Status == IncidentSimilarityTfIdf.Status.MatrixComputed))
						{
							logger?.Debug("This subset has already been processed.");
							minIndex += strideLength;
							continue;
						}

						logger?.Information(string.Format(CultureInfo.InvariantCulture, "{0}\\PID{1} processing rows {2} to {3}", System.Environment.MachineName, Process.GetCurrentProcess().Id, minIndex, maxIndex - 1));

						for (var i = minIndex; i < maxIndex; i++)
						{
							logger?.Debug(string.Format(CultureInfo.InvariantCulture, "{0}/{1}", i + 1, incidentVectors.Count));
							if (incidentVectors[i].Status == IncidentSimilarityTfIdf.Status.MatrixComputed) { continue; }

							var taskIndex = i;
							computeTask = new Task(() =>
								sqlBuffer = GenerateSimilarityCells(taskIndex, minimumSimilarity, incidentVectors, vectorMagnitudes, newVersion, maxToStore).ToList()
							);
							computeTask.Start();

							if (sqlReadyToCommit != null)
							{
								similarityMatrixBootstrapperQueryExecutor.Commit(sqlReadyToCommit, incidentVectors[completedIndex].PK);
							}

							computeTask?.Wait();

							sqlReadyToCommit = sqlBuffer;
							completedIndex = i;
						}

						if (sqlReadyToCommit != null)
						{
							similarityMatrixBootstrapperQueryExecutor.Commit(sqlReadyToCommit, incidentVectors[completedIndex].PK);
						}
					}
				}

				minIndex += strideLength;
				sqlBuffer = null;
				sqlReadyToCommit = null;
			}
#pragma warning restore CA1502
		}

		IEnumerable<double> PregenerateVectorMagnitudes(IEnumerable<IMetaIncidentSimilarityTfIdf> incidentVectors)
		{
			return incidentVectors
				.AsParallel()
				.AsOrdered()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(isv => MemoryOptimizedSparseVector<double>.FastMagnitude(isv.TfIdfSparseVector));
		}

		IEnumerable<SimilarityMatrixDataTransferObject> GenerateSimilarityCells(
			int taskIndex,
			double minimumSimilarity,
			IList<IMetaIncidentSimilarityTfIdf> incidentVectors,
			IList<double> vectorMagnitudes,
			int newVersion,
			int maxToStore)
		{
			return Enumerable.Range(taskIndex + 1, incidentVectors.Count - taskIndex - 1)
				.AsParallel()
				.WithDegreeOfParallelism(IncidentAssociationStatic.MaxDegreeOfParallelism)
				.Select(j => (J: j, Similarity: MemoryOptimizedSparseVector<double>.FastDotProduct(incidentVectors[taskIndex].TfIdfSparseVector, incidentVectors[j].TfIdfSparseVector) / (vectorMagnitudes[taskIndex] * vectorMagnitudes[j])))
				.Where(tup => tup.Similarity >= minimumSimilarity)
				.Select(cell => new SimilarityMatrixDataTransferObject(Guid.NewGuid(), incidentVectors[taskIndex].IncidentGuid, incidentVectors[cell.J].IncidentGuid, newVersion, cell.Similarity))
				.OrderBy(cell => -cell.Similarity)
				.Take(maxToStore);
		}
	}
}