using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Integration;
using Moq;

namespace Enterprise.Client.EDI.Test
{
	public class IncidentAssociationDatasetBuilderRunnerTest : TestCaseWithFactory
	{
		public void TestBlobToFromDateTime()
		{
			// Arrange
			var dateTime = DateTime.Now;

			// Act
			var dtBlob = IncidentAssociationDatasetBuilderRunner.BlobFromDateTime(dateTime);
			var dtFromBlob = IncidentAssociationDatasetBuilderRunner.DateTimeFromBlob(dtBlob);

			// Assert
			AssertEquals(dateTime.ToBinary(), dtFromBlob.ToBinary());
		}

		public void TestBuildDatasetCancellationTokenNoCancellation()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var mockTokenBootstrapper = new Mock<ITokenBootstrapper>();
			var mockTfIdfBootstrapper = new Mock<ITfIdfBootstrapper>();

			var mockBootstrapperProvider = new Mock<IBootstrapperProvider>();
			mockBootstrapperProvider.Setup(m => m.GetTokenBootstrapper(It.IsAny<int>()))
				.Returns(mockTokenBootstrapper.Object);
			mockBootstrapperProvider.Setup(m => m.GetTfIdfBootstrapper(It.IsAny<int>(), It.IsAny<IDictionary<string, int>>()))
				.Returns(mockTfIdfBootstrapper.Object);

			var mockSimilarIncidentRepository = new Mock<ISimilarIncidentRepository>();
			mockSimilarIncidentRepository.Setup(s => s.GetLatestVersion())
				.Returns(2);

			var runner = new IncidentAssociationDatasetBuilderRunner(
				mockLogger.Object,
				Factory,
				mockBootstrapperProvider.Object,
				mockSimilarIncidentRepository.Object);

			// Act
			// Assert
			Assert("Should return true if no cancellation requested.", runner.BuildDataset(
				new Dictionary<string, int>(),
				mockTokenBootstrapper.Object,
				mockTfIdfBootstrapper.Object,
				24));
		}

		public void TestBuildDatasetCancellationTokenWithCancellation()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var mockTokenBootstrapper = new Mock<ITokenBootstrapper>();
			mockTokenBootstrapper.Setup(t => t.BootstrapTokens(It.IsAny<IDictionary<string, int>>()))
				.Throws(new OperationCanceledException());
			var mockTfIdfBootstrapper = new Mock<ITfIdfBootstrapper>();

			var mockBootstrapperProvider = new Mock<IBootstrapperProvider>();
			mockBootstrapperProvider.Setup(m => m.GetTokenBootstrapper(It.IsAny<int>()))
				.Returns(mockTokenBootstrapper.Object);
			mockBootstrapperProvider.Setup(m => m.GetTfIdfBootstrapper(It.IsAny<int>(), It.IsAny<IDictionary<string, int>>()))
				.Returns(mockTfIdfBootstrapper.Object);

			var mockSimilarIncidentRepository = new Mock<ISimilarIncidentRepository>();
			mockSimilarIncidentRepository.Setup(s => s.GetLatestVersion())
				.Returns(2);

			var runner = new IncidentAssociationDatasetBuilderRunner(
				mockLogger.Object,
				Factory,
				mockBootstrapperProvider.Object,
				mockSimilarIncidentRepository.Object);

			// Act
			// Assert
			Assert("Should return false if cancellation requested.", !runner.BuildDataset(
				new Dictionary<string, int>(),
				mockTokenBootstrapper.Object,
				mockTfIdfBootstrapper.Object,
				24));
		}
	}
}
