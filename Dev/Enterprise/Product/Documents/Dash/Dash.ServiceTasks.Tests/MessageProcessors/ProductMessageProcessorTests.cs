using System;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Dash.Business;
using Enterprise.Dash.Integration.Services;
using Enterprise.Dash.ServiceTasks.MessageProcessors;
using Enterprise.DocumentScanning.Business;
using Enterprise.DocumentScanning.Integration;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Moq;

namespace Enterprise.Dash.ServiceTasks.Tests.MessageProcessors
{
	public class ProductMessageProcessorTests : TestCaseWithFactory
	{
		const int BatchSize = 1;
		const byte RetryCount = 5;

		public void TestProcessMessages_EnsureCorrectFilterAndBatchingParameters_ForSuccess()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var dashDocument = Factory.CreateDashDocument();
				var ediMessage = CreateEdiMessage(WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.ProductCodeMatching, dashDocument.PK.ToGuid());

				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument.PK)).Returns(dashDocument);
				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashGlowService = new Mock<IDashGlowService>();
				var dashErrorReporter = new Mock<IDashErrorReporter>();
				dashGlowService.Setup(service => service.MatchProductCodes(It.IsAny<Guid>())).Returns(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("Success") });
				var dashMessageProcessor = new ProductCodeMatchingMessageProcessor(factoryMock.Object, loggerMock.Object, dashGlowService.Object, shipamaxService.Object, dashErrorReporter.Object);

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns([ediMessage])
					.Returns([]);

				loggerMock.Setup(x => x.Log(LogType.Information, It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(4, logCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			}
		}

		public void TestProcessMessages_EnsureCorrectFilterAndBatchingParameters_ForFailuer()
		{
			// We want ShipamaxEdiMessage to be created automatically when StorageDoc is saved
			using (DocManagerRegistry.Instance.EnableCommercialInvoiceDocumentParsing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			{
				// arrange
				var dashDocument = Factory.CreateDashDocument();
				var ediMessage = CreateEdiMessage(WTG.Shared.Dash.Common.Constants.DataProcessingType.Code.ProductCodeMatching, dashDocument.PK.ToGuid());

				var errorMessageText = "Dummy Error";
				var statusCode = HttpStatusCode.BadRequest;
				var logCounter = 0;
				var factoryMock = new Mock<IFactory>();
				factoryMock.Setup(x => x.Load<DashDocument>(dashDocument.PK)).Returns(dashDocument);
				var loggerMock = new Mock<ILogger>();
				var shipamaxService = new Mock<IShipamaxService>();
				var dashGlowService = new Mock<IDashGlowService>();
				var dashErrorReporter = new Mock<IDashErrorReporter>();
				dashGlowService.Setup(service => service.MatchProductCodes(It.IsAny<Guid>())).Returns(new HttpResponseMessage(statusCode) { Content = new StringContent(errorMessageText) });
				var dashMessageProcessor = new ProductCodeMatchingMessageProcessor(factoryMock.Object, loggerMock.Object, dashGlowService.Object, shipamaxService.Object, dashErrorReporter.Object);

				// We're self-filtering/batching; our Load mechanism ignores the filtering that we're providing, so it's a tiny deceit, but we're not testing the actual Factory
				factoryMock.SetupSequence(x => x.Load<DashDocumentDataMessage>(It.IsAny<ZQuery>()))
					.Returns(new[] { ediMessage })
					.Returns(Array.Empty<DashDocumentDataMessage>());

				loggerMock.Setup(x => x.Log(It.IsIn(LogType.Information, LogType.Warning, LogType.Error), It.IsAny<string>())).Callback(() => logCounter++);

				// act
				dashMessageProcessor.ProcessMessages(CancellationToken.None);

				// assert
				factoryMock.Verify(x => x.Load<DashDocumentDataMessage>(It.Is(ExpectedQueryFilter)));
				factoryMock.Verify(x => x.Save(), Times.Exactly(1));

				loggerMock.Verify(x => x.Log(LogType.Information, "Loaded and started processing 1 of DashDocumentDataMessage records."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "1 message(s) successfully processed in current batch."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "No DashDocumentDataMessage records have been loaded to process."), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Error, $"Endpoint unable to process DashDocumentDataMessage record '{ediMessage.PK}': {statusCode}: {errorMessageText}"), Times.Once);
				loggerMock.Verify(x => x.Log(LogType.Information, "Total 1 message(s) successfully processed."), Times.Once);

				AssertEquals(5, logCounter);

				AssertEquals(EDIMessageStatusList.Codes.ProcessedOK, ediMessage.EM_Status);
			}
		}

		DashDocumentDataMessage CreateEdiMessage(string messageType, Guid documentId)
		{
			var ediMessage = Factory.NewWithValidTestData<DashDocumentDataMessage>();
			ediMessage.EM_MessageType = messageType;
			ediMessage.EM_LinkUniqueID = documentId;
			ediMessage.EM_GB = ZGuid.NewZGuid();
			ediMessage.EM_GE = ZGuid.NewZGuid();
			return ediMessage;
		}

		static readonly Expression<Func<ZQuery, bool>> ExpectedQueryFilter = x => x.MaximumRows == BatchSize && x.FilterPartsHashKey.Contains("and EM_MessageType = 'PCM'");
	}
}
