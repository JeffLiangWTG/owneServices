using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class MessageValidationPassedMessageProcessor : CAUniversalEventMessageProcessor
	{
		public MessageValidationPassedMessageProcessor(IXmlSessionTracker logger, UniversalEvent universalEvent, UniversalEventMessage message, CusEntryHeader entryHeader)
			: base(logger, universalEvent, message, entryHeader)
		{
		}

		protected override ZGuid NotifyEmailGroup => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgementsToGroup);

		protected override ZString NotifyEmailMode => GetFallBackValueFromRegistry(() => CACustomsDataRegistry.Instance.SendDeclarationMessageAcknowledgements);

		protected override ZString GetMessageTypeDescription() => AutoEvents.MessageValidationPassed.Description;

		protected override ZString GetResponseTypeDescription() => Res.GetString("e1d46c29-ae4e-48f4-a104-8a32366b4fab", "A 'Message Acknowledged' response");

		protected override void UpdateEntryStatusIfNeeded(CusEntryHeader entryHeader)
		{
			var declaration = entryHeader?.Declaration;
			if (declaration != null)
			{
				var cusEntryNum = CusEntryNumber.Load(declaration, CusEntryNumber.EntryType.CATransactionNumber, Core.Constants.CountryCodes.Canada);
				if (cusEntryNum != null && cusEntryNum.CE_EntryStatus != IsOnFile)
				{
					cusEntryNum.CE_EntryStatus = IsOnFile;
				}
			}
		}

		internal const string IsOnFile = "IOF";

		protected override ZBool ShouldUpdateMessageStatus(CusEntryHeader entryHeader) => ZBool.False;
	}
}
