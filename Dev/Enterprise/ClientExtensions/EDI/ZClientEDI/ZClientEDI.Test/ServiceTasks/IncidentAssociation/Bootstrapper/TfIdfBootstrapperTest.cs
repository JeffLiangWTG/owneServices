using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;
using WTG.Numerics.LinearAlgebra;

namespace Enterprise.Client.EDI.Test
{
	class TfIdfBootstrapperTest : TestCase
	{
		Mock<ISimilarIncidentRepository> MockSimilarIncidentRepository;
		Mock<ILinearAlgebra> MockLinearAlgebra;
		Mock<IIncidentTokenizer> MockIncidentTokenizer;
		Mock<ILogger> MockLogger;
		Mock<ITfIdfBootstrapperQueryExecutor> MockITfIdfBootstrapperQueryExecutor;
		Mock<IMetaIncidentSimilarityTfIdfProvider> MockMetaIncidentSimilarityTFIDFProvider;
		Mock<IMetaSupportIncidentProvider> MockMetaSupportIncidentProvider;
		List<MetaIncidentSimilarityTfIdf> MetaIncidentSimilarityList;
		List<Mock<IMetaSupportIncident>> MockMetaSupportIncidentsList;
		List<IMetaSupportIncident> MetaSupportIncidentsList;
		int CurrentVersion;
		int NewVersion;

		protected override void SetUp()
		{
			CurrentVersion = 2;
			NewVersion = 3;
			MockSimilarIncidentRepository = new Mock<ISimilarIncidentRepository>();
			MockSimilarIncidentRepository.Setup(repository => repository.GetLatestVersion()).Returns(CurrentVersion);
			var dict = new Dictionary<string, int>() { { "a", 1 }, { "b", 2 }, { "c", 1 }, { "d", 3 }, };
			MockSimilarIncidentRepository.Setup(repository => repository.GetTokenPairs()).Returns(dict);

			MockLinearAlgebra = new Mock<ILinearAlgebra>();
			MockIncidentTokenizer = new Mock<IIncidentTokenizer>();
			MockLogger = new Mock<ILogger>();
			MockITfIdfBootstrapperQueryExecutor = new Mock<ITfIdfBootstrapperQueryExecutor>();
			MockMetaIncidentSimilarityTFIDFProvider = new Mock<IMetaIncidentSimilarityTfIdfProvider>();
			MockMetaSupportIncidentProvider = new Mock<IMetaSupportIncidentProvider>();

			double[] doubleArray = { 1, 0, 1 };
			MetaIncidentSimilarityList = Enumerable.Range(0, 4).Select(index =>
				 new MetaIncidentSimilarityTfIdf()
				 {
					 PK = Guid.NewGuid(),
					 IncidentGuid = Guid.NewGuid(),
					 Version = CurrentVersion,
					 Status = IncidentSimilarityTfIdf.Status.TermFrequencyComputed,
					 IncidentLastModified = ZDateTime.Now.AddDays(-1).ToDateTime(),
					 TermFrequency = doubleArray.ToList()
				 }).ToList();
			MockMetaIncidentSimilarityTFIDFProvider.Setup(x => x.Load(It.IsAny<int>())).Returns(MetaIncidentSimilarityList);
			MockMetaSupportIncidentsList = Enumerable.Range(0, 4).Select(index => new Mock<IMetaSupportIncident>()).ToList();
			MockMetaSupportIncidentsList.Select((v, i) => new { value = v, index = i })
				.ToList()
				.ForEach(incidnet => incidnet.value.Setup(x => x.PK).Returns(MetaIncidentSimilarityList[incidnet.index].IncidentGuid));
			MockMetaSupportIncidentsList.ForEach(incidnet => incidnet.Setup(x => x.SystemLastEditTimeUtc).Returns(ZDateTime.Now.ToDateTime()));
			MockMetaSupportIncidentsList.ForEach(incidnet => incidnet.Setup(x => x.ToString()).Returns("mock to string"));
			MockMetaSupportIncidentsList.ForEach(incidnet => incidnet.Setup(x => x.Description).Returns("mock string"));

			MetaSupportIncidentsList = MockMetaSupportIncidentsList.Select(x => x.Object).ToList();
			MockMetaSupportIncidentProvider.Setup(x => x.FromGuids(It.IsAny<IList<Guid>>())).Returns(MetaSupportIncidentsList);
			MockIncidentTokenizer.Setup(x => x.TokenizeIncident(It.IsAny<string>())).Returns(new List<string>() { "a", "b", "c", "d" });
			var a = MetaIncidentSimilarityList.Select(metaIncident => metaIncident.IncidentGuid).ToList();

			MockITfIdfBootstrapperQueryExecutor.Setup(executor => executor.GetIncidentGuidsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(MetaIncidentSimilarityList.Select(metaIncident => metaIncident.IncidentGuid).ToList());
		}

		public void TestBootstrapSimilarityMatrixVersionInvalid()
		{
			//Arrange
			//Act
			//Assert
			AssertExceptionThrown<ArgumentException>(
				() => new TfIdfBootstrapper(
					-1,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentException>(
				() => new TfIdfBootstrapper(
					0,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentException>(
				() => new TfIdfBootstrapper(
					1,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertNoExceptionThrown(
				() => new TfIdfBootstrapper(
					3,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertNoExceptionThrown(
				() => new TfIdfBootstrapper(
					4,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
		}

		public void TestBootstrapSimilarityMatrixNullArguments()
		{
			//Arrange
			//Act
			//Assert
			AssertExceptionThrown<ArgumentNullException>(
				() => new TfIdfBootstrapper(
					NewVersion,
					null,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentNullException>(
				() => new TfIdfBootstrapper(
					NewVersion,
					MockSimilarIncidentRepository.Object,
					null,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentNullException>(
				() => new TfIdfBootstrapper(
					NewVersion,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					null,
					MockITfIdfBootstrapperQueryExecutor.Object,
					MockMetaIncidentSimilarityTFIDFProvider.Object,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentNullException>(
				() => new TfIdfBootstrapper(
					NewVersion,
					MockSimilarIncidentRepository.Object,
					MockLinearAlgebra.Object,
					MockIncidentTokenizer.Object,
					MockITfIdfBootstrapperQueryExecutor.Object,
					null,
					MockMetaSupportIncidentProvider.Object,
					MockLogger.Object));
			AssertExceptionThrown<ArgumentNullException>(
				 () => new TfIdfBootstrapper(
					 NewVersion,
					 MockSimilarIncidentRepository.Object,
					 MockLinearAlgebra.Object,
					 MockIncidentTokenizer.Object,
					 MockITfIdfBootstrapperQueryExecutor.Object,
					 MockMetaIncidentSimilarityTFIDFProvider.Object,
					 null,
					 MockLogger.Object));
		}

		public void TestBootstapTfIdfBootstrapAllReadyAtMatrixStage()// long name old
		{
			//Arrange
			var now = ZDateTime.UtcNow;
			MetaIncidentSimilarityList[2].Status = IncidentSimilarityTfIdf.Status.MatrixComputed;
			MockMetaIncidentSimilarityTFIDFProvider.Setup(
					tfidfProvider => tfidfProvider.Load(It.IsAny<int>())
				).Returns(MetaIncidentSimilarityList);
			var tfIdfBootstrapper = new TfIdfBootstrapper(
				NewVersion,
				MockSimilarIncidentRepository.Object,
				MockLinearAlgebra.Object,
				MockIncidentTokenizer.Object,
				MockITfIdfBootstrapperQueryExecutor.Object,
				MockMetaIncidentSimilarityTFIDFProvider.Object,
				MockMetaSupportIncidentProvider.Object,
				MockLogger.Object);
			//Act
			tfIdfBootstrapper.BootstrapBetween(now.AddDays(-2), now);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(tfidfProvider => tfidfProvider.Load(NewVersion));

			Assert(true);
		}

		public void TestBootstapTfIdfBootstrapAlreadyComputed()
		{
			//Arrange
			var now = ZDateTime.UtcNow;
			MetaIncidentSimilarityList[2].Status = IncidentSimilarityTfIdf.Status.Uninitialized;
			MockMetaIncidentSimilarityTFIDFProvider.Setup(
					tfidfProvider => tfidfProvider.Load(It.IsAny<int>())
				).Returns(MetaIncidentSimilarityList);
			var tfIdfBootstrapper = new TfIdfBootstrapper(
				NewVersion,
				MockSimilarIncidentRepository.Object,
				MockLinearAlgebra.Object,
				MockIncidentTokenizer.Object,
				MockITfIdfBootstrapperQueryExecutor.Object,
				MockMetaIncidentSimilarityTFIDFProvider.Object,
				MockMetaSupportIncidentProvider.Object,
				MockLogger.Object);
			//Act
			tfIdfBootstrapper.BootstrapBetween(now.AddDays(-2), now);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(tfidfProvider => tfidfProvider.Load(NewVersion));
			MockITfIdfBootstrapperQueryExecutor.Verify(x => x.GetIncidentGuidsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()));
			MockMetaIncidentSimilarityTFIDFProvider.Verify(tfidfProvider => tfidfProvider.Save(It.Is<IEnumerable<IMetaIncidentSimilarityTfIdf>>(x => x.Count() == 4), It.IsAny<int>()));
			MockIncidentTokenizer.Verify(x => x.TokenizeIncident("mock to string"), Times.Exactly(4));
			MockSimilarIncidentRepository.Verify(x => x.SetIdfVector(It.IsAny<ICollection<Double>>()));
			Assert(true);
		}

		public void TestBootstrapTfIdfBootstrapBetween()
		{
			//Arrange
			var now = ZDateTime.UtcNow;
			var tfIdfBootstrapper = new TfIdfBootstrapper(
				NewVersion,
				MockSimilarIncidentRepository.Object,
				MockLinearAlgebra.Object,
				MockIncidentTokenizer.Object,
				MockITfIdfBootstrapperQueryExecutor.Object,
				MockMetaIncidentSimilarityTFIDFProvider.Object,
				MockMetaSupportIncidentProvider.Object,
				MockLogger.Object);
			//Act
			tfIdfBootstrapper.BootstrapBetween(now.AddDays(-2), now);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(tfidfProvider => tfidfProvider.Load(NewVersion));
			MockITfIdfBootstrapperQueryExecutor.Verify(x => x.GetIncidentGuidsBetween(It.IsAny<DateTime>(), It.IsAny<DateTime>()));
			MockSimilarIncidentRepository.Verify(x => x.SetIdfVector(It.IsAny<ICollection<Double>>()));
			Assert(true);
		}
	}
}
