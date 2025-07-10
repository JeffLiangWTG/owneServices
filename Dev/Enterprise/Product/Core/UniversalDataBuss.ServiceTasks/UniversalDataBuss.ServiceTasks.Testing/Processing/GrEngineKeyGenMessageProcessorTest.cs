using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Moq;

namespace Enterprise.UniversalDataBuss.ServiceTasks.Testing
{
	class GrEngineKeyGenMessageProcessorTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGrEngineKeyGenMessageProcessorInitializesDisposableManagerOnShortCircuit()
		{
			var message = GetQueuedUniversalEventMessage(@"<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalEvent xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Event>
	</Event>
</UniversalEvent>");

			var mock = new Mock<IInterchangeAcknowledgement>();
			mock.Setup(ia => ia.Send(It.IsAny<BusinessObjectFactory>(), It.IsAny<INotifications>(), It.IsAny<IEDIMessage>(), It.IsAny<string>(), It.IsAny<IEnumerable<IValidationRule>>(), It.IsAny<InterchangeAcknowledgementType>()))
				.Callback((BusinessObjectFactory factory, INotifications notification, IEDIMessage ediMessage, string dataImportLog, IEnumerable<IValidationRule> _, InterchangeAcknowledgementType type) =>
				{
					factory.SubscribeForDispose(new MemoryStream());
					throw new Exception("Success");
				});

			ObjectFactory.Substitute(mock.Object);

			try
			{
				var processor = new GrEngineKeyGenMessageProcessor(null, new LoggingInformation(), Array.Empty<string>(), new FactoryService());
				processor.ProcessMessage((EDIMessage)message);
			}
			catch (Exception ex) when (ex.Message == "Success")
			{
				AssertEquals("No Error Should Have Been Reported", 0, ErrorReporter.TotalErrorCount);
				return;
			}

			Assert(false);
		}
	}
}
