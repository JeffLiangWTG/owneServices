using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRCIncomingMessageProcessorTest : TestCase
	{
		public void TestGetMessageProcessors()
		{
			CombineAssertions(() =>
			{
				var processors = new BRCIncomingMessageProcessorForTesting(new LoggingInformationForTesting()).GetMessageProcessorsExposed();
				AssertEquals(29, processors.Count);
				var index = 0;
				AssertType<BRCFirstReturnMessageProcessor>(processors[index++]);
				AssertType<BRCSecondReturnErrorMessageProcessor>(processors[index++]);
				AssertType<BRCSecondReturnSuccessMessageProcessor>(processors[index++]);
				AssertType<BRCResponseErrorMessageProcessor>(processors[index++]);
				AssertType<BRCImportLicenseAcceptMessageProcessor>(processors[index++]);
				AssertType<BRCImportLicenseStatusMessageProcessor>(processors[index++]);
				AssertType<BRCExportPushNotificationMessageProcessor>(processors[index++]);
				AssertType<BRCLPCOSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCLPCOErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCSubscriptionErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCSubscriptionSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCImportPushNotificationMessageProcessor>(processors[index++]);
				AssertType<BRCLPCOPushNotificationMessageProcessor>(processors[index++]);
				AssertType<BRCExportCompleteConsultMessageProcessor>(processors[index++]);
				AssertType<BRCDuimpHeaderSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCDuimpHeaderErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCDuimpLinesSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCDuimpLinesErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogPushNotificationMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogZipFileResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogOperatorZipFileResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogManufacturerZipFileResponseMessageProcessor>(processors[index++]);
				AssertType<BRCCatalogManufacturerLinkResponseMessageProcessor>(processors[index++]);
				AssertType<BRCForeignOperatorSuccessResponseMessageProcessor>(processors[index++]);
				AssertType<BRCForeignOperatorErrorResponseMessageProcessor>(processors[index++]);
				AssertType<BRCMandatoryTreatmentAttributesResponseMessageProcessor>(processors[index++]);
				AssertType<BRCOptionalTreatmentAttributesResponseMessageProcessor>(processors[index++]);
			});
		}

		protected class BRCIncomingMessageProcessorForTesting : BRCIncomingMessageProcessor
		{
			public BRCIncomingMessageProcessorForTesting(LoggingInformation logger) : base(logger)
			{
			}

			public List<ApplicationTypeMessageProcessor> GetMessageProcessorsExposed() => GetMessageProcessors();
		}
	}
}
