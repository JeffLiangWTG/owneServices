using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class ESBranchCustomsMessageProcessor : BranchCustomsMessageProcessor
	{
		public ESBranchCustomsMessageProcessor()
			: base(new ZString[] { ApplicationCodeList.Codes.ESCustomsMessage }, Enumerable.Empty<ZString>())
		{
		}

		protected override List<ApplicationTypeMessageProcessor> GetMessageProcessorsCore()
		{
			var result = base.GetMessageProcessorsCore();
			result.Add(new ExportDeclarationResponseMessageProcessor(Logger, this));
			result.Add(new ComplXExportDeclarationResponseMessageProcessor(Logger, this));
			result.Add(new PreDUAIncompleteImportResponseMessageProcessor(Logger));
			result.Add(new T2LExpeditionResponseMessageProcessor(Logger));
			result.Add(new T2LExpeditionAmendmentResponseMessageProcessor(Logger, this));
			result.Add(new T2LReceptionResponseMessageProcessor(Logger));
			result.Add(new T2LReceptionAmendmentResponseMessageProcessor(Logger));
			result.Add(new T2LAnnexResponseMessageProcessor(Logger));
			result.Add(new T2LClearanceResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationExportResponseMessageProcessor(Logger));
			result.Add(new CompleteImportResponseMessageProcessor(Logger));
			result.Add(new DJPImportResponseMessageProcessor(Logger));
			result.Add(new Box40AmendmentImportResponseMessageProcessor(Logger));
			result.Add(new ImportQueryResponseMessageProcessor(Logger));
			result.Add(new CANPreDUAImportResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationImportResponseMessageProcessor(Logger));
			result.Add(new SimplifiedImportResponseMessageProcessor(Logger));
			result.Add(new ArrivalAtExitResponseMessageProcessor(Logger, this));
			result.Add(new CustomsServiceErrorResponseMessageProcessor(Logger));
			result.Add(new CustomsServiceErrorUniversalEventResponseMessageProcessor(Logger));
			result.Add(new ImportClearanceEmailResponseMessageProcessor(Logger));
			result.Add(new T2LClearanceEmailResponseMessageProcessor(Logger));
			result.Add(new ExportClearanceEmailResponseMessageProcessor(Logger));
			result.Add(new Box44ImportResponseMessageProcessor(Logger));
			result.Add(new DocumentCaptureResponseMessageProcessor(Logger, this));
			result.Add(new EXSResponseMessageProcessor(Logger, this));
			result.Add(new DeclarationAESResponseMessageProcessor(Logger, this));
			result.Add(new AnnexAESResponseMessageProcessor(Logger));
			result.Add(new DeclarationDVDResponseMessageProcessor(Logger, this));
			result.Add(new QueryAESResponseMessageProcessor(Logger, this));
			result.Add(new GoodsNotificationAESResponseMessageProcessor(Logger, this));
			result.Add(new InboxNotificationClearanceAESResponseMessageProcessor(Logger, this));
			result.Add(new InboxNotificationInvalidationAESResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationNonConformityAESResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationExitResultAESResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationCceControlAESResponseMessageProcessor(Logger));
			result.Add(new CancelAESResponseMessageProcessor(Logger));
			result.Add(new AESAmendmentResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationDVDResponseMessageProcessor(Logger, this));
			result.Add(new CancelDVDResponseMessageProcessor(Logger));
			result.Add(new DepartureCertReqAESResponseMessageProcessor(Logger));
			result.Add(new ComplXDVDResponseMessageProcessor(Logger, this));
			result.Add(new ComplXAESResponseMessageProcessor(Logger));
			result.Add(new QueryDVDResponseMessageProcessor(Logger, this));
			result.Add(new CommonAnnexResponseMessageProcessor(Logger));
			result.Add(new RequestAndReceptionT2LPOUSResponseMessageProcessor(Logger));
			result.Add(new PresentationT2LPOUSResponseMessageProcessor(Logger));
			result.Add(new QueryT2LPOUSResponseMessageProcessor(Logger));
			result.Add(new IncompleteImportH1ResponseMessageProcessor(Logger));
			result.Add(new InboxPendingListResponseMessageProcessor(Logger));
			result.Add(new InboxNotificationInvalidationH1ResponseMessageProcessor(Logger));

			var exitControlMessageProcessors = ObjectFactory.Get<IESExitControlMessageProcessorsProvider>().GetProcessors(Logger);
			result.AddRange(exitControlMessageProcessors);

			var nctsMessageProcessors = ObjectFactory.Get<IESNctsMessageProcessorsProvider>().GetProcessors(Logger);
			result.AddRange(nctsMessageProcessors);

			var g3MessageProcessors = ObjectFactory.Get<IESG3MessageProcessorsProvider>().GetProcessors(Logger);
			result.AddRange(g3MessageProcessors);

			var g5MessageProcessors = ObjectFactory.Get<IESG5MessageProcessorsProvider>().GetProcessors(Logger);
			result.AddRange(g5MessageProcessors);

			var h7MessageProcessors = ObjectFactory.Get<IESH7MessageProcessorsProvider>().GetProcessors(Logger);
			result.AddRange(h7MessageProcessors);

			return result;
		}
	}
}
