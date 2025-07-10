using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.DIF.Business.UniversalDataTransfer;
using Enterprise.Customs.Common.CA.DIF;

namespace Enterprise.Customs.CA.DIF.Business
{
	public class MessageManager : DISMessageManagerBase
	{
		public MessageManager(DIFDocument difDocument)
			: base(difDocument)
		{
			this.difDocument = difDocument;
			this.messagingHelper = new DIFDocumentUniversalMessagingHelper();
		}

		readonly DIFDocument difDocument;
		readonly DIFDocumentUniversalMessagingHelper messagingHelper;

		protected override void SendSubmissionCore()
		{
			if (StatusList.HasBeenLodgedAtCustoms(difDocument.Status) || StatusList.HasResponseFromCBSA(difDocument.Status))
			{
				var difHost = difDocument.HostWrapper?.DISHost;
				if (difHost?.ShouldSendChangeMessageAsAmendment ?? false)
				{
					messagingHelper.SendEventViaEHub(difDocument, DIFConstants.UniversalEventFunctionCode.Amendment);
					SetStatus(StatusList.Codes.AwaitingAmendment);
				}
				else
				{
					messagingHelper.SendEventViaEHub(difDocument, DIFConstants.UniversalEventFunctionCode.Change);
					SetStatus(StatusList.Codes.AwaitingChange);
				}
			}
			else
			{
				messagingHelper.SendEventViaEHub(difDocument, DIFConstants.UniversalEventFunctionCode.Original);
				SetStatus(StatusList.Codes.AwaitingOriginal);
			}
		}

		protected override void SendWithdrawlCore()
		{
			messagingHelper.SendEventViaEHub(difDocument, DIFConstants.UniversalEventFunctionCode.Cancel);
			SetStatus(StatusList.Codes.AwaitingWithdrawal);
		}

		void SetStatus(ZString status)
		{
			var requiredDocumentAddInfo = difDocument.RequiredDocumentAddInfo;
			if (requiredDocumentAddInfo != null)
			{
				requiredDocumentAddInfo.EX_Status = status;
			}
		}
	}
}
