using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	abstract class NCTSLinkedGuaranteeMessageProcessorAbstractTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider> :
		MessageAttacheeMessageProcessorTest<TMessageProcessor, TInboundEDIMessage, TOutboundEDIMessage, TDataProvider, CusGuaranteeHeader, CusGuaranteeHeader>
	where TMessageProcessor : MessageAttacheeMessageProcessor<TInboundEDIMessage, TDataProvider>
	where TInboundEDIMessage : InboundEDIMessage
	where TOutboundEDIMessage : OutboundEDIMessage
	{
		protected override (CusGuaranteeHeader declaration, CusGuaranteeHeader messageAttachee, EDIMessage outgoingMessage, TInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var header = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.Ireland, grn, ZDate.BrettsBirthday, ZDate.BrettsBirthday.AddYears(1));
			var incomingMessage = CreateNewIncomingMessage(incomingMessageText);
			var outgoingMessage = Factory.New<TOutboundEDIMessage>();
			incomingMessage.EM_SystemCreateTimeUtc = ZDateTime.UtcNow;
			outgoingMessage.EM_ApplicationReference = TransactionID;
			outgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
			outgoingMessage.EM_SystemCreateUser = Staff.GS_Code;
			header.Messages.Add(outgoingMessage);
			return (header, header, null, incomingMessage);
		}

		protected override TInboundEDIMessage CreateNewIncomingMessage(string incomingMessageText = null)
		{
			var result = base.CreateNewIncomingMessage(incomingMessageText);
			result.EM_GB = Branch.PK;
			return result;
		}

		protected CusGuaranteeHeader CreateCusGuaranteeHeader(ZString countryCode, ZString grn, ZDate startDate, ZDate endDate)
		{
			var header = Factory.New<CusGuaranteeHeader>();
			header.CPH_RN_NKCountryCode = countryCode;
			header.CPH_Number = grn;
			header.CPH_StartDate = startDate;
			header.CPH_EndDate = endDate;
			header.CPH_IsActive = true;
			return header;
		}

		protected const string grn = "12GRNCC055C012345A678901";
	}
}
