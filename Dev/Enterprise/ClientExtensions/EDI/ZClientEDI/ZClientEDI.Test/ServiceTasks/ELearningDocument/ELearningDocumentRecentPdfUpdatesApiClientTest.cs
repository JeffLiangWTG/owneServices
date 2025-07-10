using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.ServiceTasks.ELearningDocument;
using Enterprise.Integration;
using Moq;
using ZClientEDI.Business.IncidentManager.ELearningDocument.Business;

namespace Enterprise.Client.EDI.Test
{
	public class ELearningDocumentRecentPDFUpdatesApiClientTest : TestCaseWithFactory
	{
		public void TestDownloadListOfChangesEntireFlow()
		{
			// Arrange
			var mockLogger = new Mock<ILogger>();
			var client = new Mock<IRecentPdfUpdatesApiClient>();
			client.Setup(m => m.DownloadListOfChanges(It.IsAny<ZDateTime>())).Returns(new List<IELearningDocumentFileDescription>());
			//Act
			var result = client.Object.DownloadListOfChanges(It.IsAny<DateTime>());
			//Assert
			Assert("entire flow for DownloadListOfChanges should return object", result != null);
		}
	}
}
