using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Moq;
using NUnit.Framework;
using WTG.Numerics;

namespace Enterprise.Client.EDI.Test
{
	class SimilarityMatrixBootstrapperTest : TestCase
	{
		Mock<ISimilarIncidentRepository> MockSimilarIncidentRepository;
		Mock<IMetaIncidentSimilarityTfIdfProvider> MockMetaIncidentSimilarityTFIDFProvider;
		Mock<ISimilarityMatrixBootstrapperQueryExecutor> MockSimilarityMatrixBootstrapperQueryExecutor;
		List<Mock<IMetaIncidentSimilarityTfIdf>> MockMetaIncidentSimilarityList;
		List<IMetaIncidentSimilarityTfIdf> MetaIncidentSimilarityList;
		int CurrentVersion;
		int NewVersion;
		protected override void SetUp()
		{
			CurrentVersion = 2;
			NewVersion = 3;
			MockSimilarIncidentRepository = new Mock<ISimilarIncidentRepository>();
			MockSimilarIncidentRepository.Setup(repository => repository.GetLatestVersion()).Returns(CurrentVersion);
			MockMetaIncidentSimilarityTFIDFProvider = new Mock<IMetaIncidentSimilarityTfIdfProvider>();
			MockSimilarityMatrixBootstrapperQueryExecutor = new Mock<ISimilarityMatrixBootstrapperQueryExecutor>();

			double[] doubleArray = { 1, 0, 1 };
			MockMetaIncidentSimilarityList = Enumerable.Range(0, 4).Select(index => new Mock<IMetaIncidentSimilarityTfIdf>()).ToList();
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.Status).Returns(IncidentSimilarityTfIdf.Status.TfIdfComputed));
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.TermFrequency).Returns(doubleArray));
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.TFIDF).Returns(doubleArray));
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.IncidentGuid).Returns(ZGuid.NewZGuid().ToGuid()));
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.PK).Returns(ZGuid.NewZGuid().ToGuid()));
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.TfIdfSparseVector).Returns(new MemoryOptimizedSparseVector<double>() { 1, 2, 3 }));

			MetaIncidentSimilarityList = MockMetaIncidentSimilarityList.Select(x => x.Object).ToList();
			MockMetaIncidentSimilarityTFIDFProvider.Setup(x => x.Load(It.IsAny<int>())).Returns(MetaIncidentSimilarityList);
			MockSimilarityMatrixBootstrapperQueryExecutor.Setup(x => x.Commit(It.IsAny<IList<SimilarityMatrixDataTransferObject>>(), It.IsAny<ZGuid>(), 1000));
		}

		public void TestBootstrapSimilarityMatrixVersionInvalid()
		{
			//Arrange Act Assert
			AssertExceptionThrown<ArgumentException>(() => new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object)
				.BootstrapSimilarityMatrix(1, 100, 0.25, 100));

			AssertExceptionThrown<ArgumentException>(() => new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object)
				.BootstrapSimilarityMatrix(0, 100, 0.25, 100));

			AssertNoExceptionThrown(() => new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object)
				.BootstrapSimilarityMatrix(3, 100, 0.25, 100));

			AssertNoExceptionThrown(() => new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object)
					.BootstrapSimilarityMatrix(4, 100, 0.25, 100));
		}

		public void TestBootstrapSimilarityMatrixAlreadyComputed()
		{
			//Arrange 
			MockMetaIncidentSimilarityList.ForEach(incident => incident.Setup(x => x.Status).Returns(IncidentSimilarityTfIdf.Status.MatrixComputed));
			MetaIncidentSimilarityList = MockMetaIncidentSimilarityList.Select(x => x.Object).ToList();
			MockMetaIncidentSimilarityTFIDFProvider.Setup(x => x.Load(It.IsAny<int>())).Returns(MetaIncidentSimilarityList);
			var similarityMatrixBootstrapper = new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object);
			//Act
			similarityMatrixBootstrapper.BootstrapSimilarityMatrix(NewVersion, 100, 0.2, 1000000);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(x => x.Load(It.Is<int>(i => i == NewVersion)));
			MockSimilarityMatrixBootstrapperQueryExecutor.Verify(x => x.Commit(It.IsAny<IList<SimilarityMatrixDataTransferObject>>(), It.IsAny<ZGuid>(), 1000), Times.Never);

			Assert(true);
		}

		public void TestBootstrapSimilarityMatrixNotAllTFIDFComputed()
		{
			//Arrange 
			MockMetaIncidentSimilarityList[0].Setup(x => x.Status).Returns(IncidentSimilarityTfIdf.Status.TermFrequencyComputed);
			MetaIncidentSimilarityList = MockMetaIncidentSimilarityList.Select(x => x.Object).ToList();
			MockMetaIncidentSimilarityTFIDFProvider.Setup(x => x.Load(It.IsAny<int>())).Returns(MetaIncidentSimilarityList);
			var similarityMatrixBootstrapper = new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object);
			//Act
			similarityMatrixBootstrapper.BootstrapSimilarityMatrix(NewVersion, 100, 0.2, 1000000);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(x => x.Load(It.Is<int>(i => i == NewVersion)));
			MockSimilarityMatrixBootstrapperQueryExecutor.Verify(x => x.Commit(It.IsAny<IList<SimilarityMatrixDataTransferObject>>(), It.IsAny<ZGuid>(), 1000), Times.Never);

			Assert(true);
		}

		public void TestBootstrapSimilarityMatrix()
		{
			//Arrange 
			MockMetaIncidentSimilarityTFIDFProvider.Setup(x => x.FindByVectorGuids(It.IsAny<IEnumerable<Guid>>())).Returns(MetaIncidentSimilarityList);
			var similarityMatrixBootstrapper = new SimilarityMatrixBootstrapper(MockSimilarIncidentRepository.Object, MockMetaIncidentSimilarityTFIDFProvider.Object, MockSimilarityMatrixBootstrapperQueryExecutor.Object);
			//Act
			similarityMatrixBootstrapper.BootstrapSimilarityMatrix(NewVersion, 100, 0.2, 1000000);
			//Assert
			MockMetaIncidentSimilarityTFIDFProvider.Verify(x => x.Load(It.Is<int>(i => i == NewVersion)));
			MetaIncidentSimilarityList.ForEach(incident => MockSimilarityMatrixBootstrapperQueryExecutor.Verify(x => x.Commit(It.IsAny<IEnumerable<SimilarityMatrixDataTransferObject>>(), It.IsAny<ZGuid>(), 1000)));

			Assert(true);
		}
	}
}
