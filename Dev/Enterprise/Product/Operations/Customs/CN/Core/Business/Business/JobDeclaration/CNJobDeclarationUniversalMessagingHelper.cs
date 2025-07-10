using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CNJobDeclarationUniversalMessagingHelper : JobDeclarationUniversalMessagingHelper
	{
		public CNJobDeclarationUniversalMessagingHelper(IJobDeclarationMessageSendingObjectParent messageSendingObject) : base(messageSendingObject) { }

		protected override ZString UniversalCustomsMessagingRecipientID => "CNCustoms";

		protected override ZString InstructionForRecipientIDSetup => ZString.Empty;

		protected override ZBool UseMessageNumberAsRerenceNumber => true;

		protected override ZString GetMessageTypeForEventReference(BusinessObject header)
		{
			return Constants.JobDeclarationUniversalMessagingEventTypes.SingleWindow
				+ ((header is CusEntryHeader entryHeader) ? entryHeader.GetDeclarationType() : ZString.Empty);
		}

		protected override void UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(BusinessObject businessObject, ZString status)
		{
			base.UpdateSendingObjectHeaderAfterCreatingEDIEnterchage(businessObject, status);

			if (businessObject is CusEntryHeader entryHeader)
			{
				entryHeader.Logs.AddNew(AutoEvents.MessageSent, $"Send to Customs;{entryHeader.CH_BGMReference}");
			}
		}

		protected override ZString GetMessageAwaitingStatus(BusinessObject header)
		{
			return (header is CusEntryHeader cusEntryHeader) ? JobMessageStatusList.GetAwaitingStatusByDeclarationType(cusEntryHeader.GetDeclarationType()) : base.GetMessageAwaitingStatus(header);
		}

		protected override void SetMessageInterpretion(EDIMessage message)
		{
			var cusEntryHeader = message.EM_LinkedObject as CusEntryHeader;
			message.EM_MessageInterpretation = cusEntryHeader == null ? ZString.Empty : cusEntryHeader.HtmlFormatEntryData;
		}

		protected override ZBool ShouldPopulateAttachedDocumentCollection
			=> MessageSendingObjectParent.SendingObjectsCollection.Cast<CNJobDeclarationMessageSendingObject>().Any(x => x.ShouldSend && x.DeclarationType != DeclarationTypeList.Codes.PreliminaryDeclaration);
	}
}
