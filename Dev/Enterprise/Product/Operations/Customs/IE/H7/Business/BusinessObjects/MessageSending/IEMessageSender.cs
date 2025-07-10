using CargoWise.Customs.IE.MessageContracts.AIS;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using EDIMessage = Enterprise.Messaging.Business.EDIMessage;
using H7V1 = CargoWise.Customs.IE.MessageContracts.MessageBuilders.AIS.H7V1;
using MessageSender = Enterprise.Customs.EU.H7.Business.MessageSender;

namespace Enterprise.Customs.IE.H7.Business
{
	public class IEMessageSender : MessageSender
	{
		public IEMessageSender(IH7MessageSendingObject sendingObject)
			: base(sendingObject)
		{
		}

		protected override void SetBillMessageStatus(EU.H7.Business.AsycudaBill bill)
		{
			bill.ABL_MessageStatus = LogicalStatusList.Codes.Sent;
		}

		protected override IXmlMessageBuilder CreateMessageBuilder(EDIMessage message)
		{
			return IsV1 ? CreateV1MessageBuilder(message) : CreateV2MessageBuilder(message);
		}

		IXmlMessageBuilder CreateV1MessageBuilder(EDIMessage message)
		{
			var result = default(IXmlMessageBuilder);
			switch (SendingObject.Action)
			{
				case AISOutgoingMessageTypeList.Codes.AmendmentRequest:
					result = new H7V1.IM413MessageBuilder(new Messaging.V1.IM413HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.InvalidationRequest:
					result = new H7V1.IM414MessageBuilder(new Messaging.V1.IM414HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.CustomsDeclaration:
					result = new H7V1.IM415MessageBuilder(new Messaging.V1.IM415HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.PresentationNotification:
					result = new H7V1.IM432MessageBuilder(new Messaging.V1.IM432HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.DocumentsReceived:
					var im483HeaderProvider = new Messaging.V1.IM483HeaderProvider(SendingObject as UploadDocumentsSendingAction);
					CreateAttachments(message, im483HeaderProvider);
					result = new H7V1.IM483MessageBuilder(im483HeaderProvider);
					break;
				case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15:
					result = new H7V1.Rf415MessageBuilder(new Messaging.V1.RF415HeaderProvider(SendingObject as RF415MessageSendingObject));
					break;
			}

			return result;
		}

		IXmlMessageBuilder CreateV2MessageBuilder(EDIMessage message)
		{
			var result = default(IXmlMessageBuilder);
			switch (SendingObject.Action)
			{
				case AISOutgoingMessageTypeList.Codes.InvalidationRequest:
					result = new IM414MessageBuilder(new IM414HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.CustomsDeclaration:
					result = new IM415MessageBuilder(new IM415HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.AmendmentRequest:
					result = new IM413MessageBuilder(new IM413HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.PresentationNotification:
					result = new IM432MessageBuilder(new IM432HeaderProvider(SendingObject as MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.ApplicationForRemissionOfCustomsDebtF15:
					result = new RF415MessageBuilder(new RF415HeaderProvider(SendingObject as RF415MessageSendingObject));
					break;
				case AISOutgoingMessageTypeList.Codes.DocumentsReceived:
					var im483HeaderProvider = new IM483HeaderProvider(SendingObject as UploadDocumentsSendingAction);
					CreateAttachments(message, im483HeaderProvider);
					result = new IM483MessageBuilder(im483HeaderProvider);
					break;
			}

			return result;
		}

		protected override void SetupBranchSpecificInfo(EDIMessage message, GlbBranch branch)
		{
			base.SetupBranchSpecificInfo(message, branch);
			message.EM_GP = branch.Company.GetCredentialPK();
		}

		protected override EDIMessage CreateOutboundEDIMessage(BusinessObjectFactory factory) => factory.New<AISOutboundEDIMessage>();

		bool IsV1 => SendingObject.Bill.Header.AMA_ApplicationCode == ApplicationCodeTypeList.Codes.EuH7V1;

		void CreateAttachments(EDIMessage message, IEDocsToAttach eDocsProvider)
		{
			foreach (var eDoc in eDocsProvider.EDocsToAttach)
			{
				var attachment = message.MessageAttachments.AddNew();
				attachment.EG_StorageDocsGuid = eDoc.Key;
				attachment.EG_FileName = eDoc.Value;
			}
		}
	}
}
