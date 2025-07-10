using System.Collections.Generic;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.BR.Business
{
	public class BRCIncomingMessageProcessor : BranchMessageProcessor
	{
		public BRCIncomingMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool MessageShouldBeProcessedInASeparateFactory => true;

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors()
		{
			var result = base.GetMessageProcessors();
			result.Add(new BRCFirstReturnMessageProcessor(Logger));
			result.Add(new BRCSecondReturnErrorMessageProcessor(Logger));
			result.Add(new BRCSecondReturnSuccessMessageProcessor(Logger));
			result.Add(new BRCResponseErrorMessageProcessor(Logger));
			result.Add(new BRCImportLicenseAcceptMessageProcessor(Logger));
			result.Add(new BRCImportLicenseStatusMessageProcessor(Logger));
			result.Add(new BRCExportPushNotificationMessageProcessor(Logger));
			result.Add(new BRCLPCOSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCLPCOErrorResponseMessageProcessor(Logger));
			result.Add(new BRCSubscriptionErrorResponseMessageProcessor(Logger));
			result.Add(new BRCSubscriptionSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCImportPushNotificationMessageProcessor(Logger));
			result.Add(new BRCLPCOPushNotificationMessageProcessor(Logger));
			result.Add(new BRCExportCompleteConsultMessageProcessor(Logger));
			result.Add(new BRCDuimpHeaderSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCDuimpHeaderErrorResponseMessageProcessor(Logger));
			result.Add(new BRCDuimpLinesSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCDuimpLinesErrorResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogPushNotificationMessageProcessor(Logger));
			result.Add(new BRCCatalogErrorResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogZipFileResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogOperatorZipFileResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogManufacturerZipFileResponseMessageProcessor(Logger));
			result.Add(new BRCCatalogManufacturerLinkResponseMessageProcessor(Logger));
			result.Add(new BRCForeignOperatorSuccessResponseMessageProcessor(Logger));
			result.Add(new BRCForeignOperatorErrorResponseMessageProcessor(Logger));
			result.Add(new BRCMandatoryTreatmentAttributesResponseMessageProcessor(Logger));
			result.Add(new BRCOptionalTreatmentAttributesResponseMessageProcessor(Logger));
			return result;
		}
	}
}
