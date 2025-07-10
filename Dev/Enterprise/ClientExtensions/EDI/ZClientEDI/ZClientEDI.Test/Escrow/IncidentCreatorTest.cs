using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Escrow;
using Enterprise.Client.EDI.Escrow.Interfaces;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Integration;
using Moq;
using NUnit.Framework;

namespace ZClientEDI.Test.Escrow
{
	class IncidentCreatorTest : TestCaseWithFactory
	{
		public void TestWrongParamsCall()
		{
			var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new IncidentCreator(null));
			AssertEquals("incidentConfigurationRegistry", result.ParamName);
		}

		public void TestCreateLogsException()
		{
			// Arrange
			incidentConfigurationRegistry.SetupGet(i => i.IncidentProduct).Returns("INVALID");

			// Act & Assert
			AssertExceptionThrown<Exception>(() => incidentCreator.Create(exportResultMock.Object, loggerMock.Object));
			loggerMock.Verify(l => l.Log(LogType.Error, It.IsAny<string>()), Times.Once);

			ErrorReporter.Clear();
		}

		[TestDate(2024, 10, 11)]
		public void TestCreateCreatesIncidentWithExpectedInfo()
		{
			// Arrange
			exportResultMock.SetupGet(e => e.RemotePath).Returns("https://a.com/endpoints/EscrowExport/Escrow202409/");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentProduct).Returns("PPP");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentModule).Returns("MMM");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentPriority).Returns("CR0");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentPriority).Returns("CR0");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentMessage).Returns("Test Message");

			// Act
			incidentCreator.Create(exportResultMock.Object, loggerMock.Object);

			// Assert
			var result = Factory.Load<SupportIncident>(new ZQuery());
			AssertEquals(1, result.Length);
			var request = result[0].Request;
			AssertEquals("CR0", request.INC_Criticality);
			AssertEquals("PPP", request.INC_Type);
			AssertEquals("MMM", request.INC_SubType);
			AssertEquals("Source Code Escrow", request.INC_Summary);
			AssertEquals("Test Message", request.INC_Details);
		}

		[ExpectNoExceptions]
		public void TestCreateLogsAsExpectedAfterIncidentIsCreated()
		{
			// Arrange
			exportResultMock.SetupGet(e => e.RemotePath).Returns("https://a.com/b/EscrowExport/Escrow202409/");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentProduct).Returns("PPP");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentModule).Returns("MMM");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentPriority).Returns("CR0");
			incidentConfigurationRegistry.SetupGet(i => i.IncidentMessage).Returns("Test Message");

			// Act
			incidentCreator.Create(exportResultMock.Object, loggerMock.Object);

			// Assert
			var result = Factory.Load<SupportIncident>(new ZQuery());
			AssertEquals(1, result.Length);
			var incident = result[0];
			loggerMock.Verify(l => l.Log(LogType.Information, $"Incident created, incident number {incident.IM_IncidentNumber}"), Times.Once);
		}

		protected override void SetUp()
		{
			base.SetUp();
			incidentConfigurationRegistry = new Mock<IIncidentConfigurationRegistry>();
			incidentCreator = new IncidentCreator(incidentConfigurationRegistry.Object);
			loggerMock = new Mock<ILogger>();
			exportResultMock = new Mock<IExportResult>();
		}

		IncidentCreator incidentCreator;
		Mock<IIncidentConfigurationRegistry> incidentConfigurationRegistry;
		Mock<ILogger> loggerMock;
		Mock<IExportResult> exportResultMock;
	}
}
