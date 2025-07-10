using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.Chief
{
	public class CusDecFunctionChooser : Business.Messaging.CusDecFunctionChooser
	{
		public CusDecFunctionChooser(CusEntryHeader entry, Customs.Business.CusdecMessageFunction declarationMessageFunctionFromUserClick)
			: base(entry, declarationMessageFunctionFromUserClick)
		{
		}

		protected override bool IsSendingSecondMessageUnderFallback()
		{
			var isReallyAnAmendment = false;
			if ((entryHeader.Declaration.IsExport && GBCustomsDataRegistry.Instance.ChiefFallbackExports.Value)
				||
				(entryHeader.Declaration.IsImport && GBCustomsDataRegistry.Instance.ChiefFallbackImports.Value))
			{
				// We're under fallback
				if (!entryHeader.CH_CustomsMessageRemarks.IsEmpty)
				{
					if (entryHeader.Declaration.ZG_Gateway == GatewayList.Codes.CCSUKviaNTMsgGW)
					{   // we are using ccsuk							
						if ((from EDIMessage m in entryHeader.Messages where m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && m.EM_Status == EDIMessage.Status.Acknowledged && m.EM_MessageText.Contains("CUSDEC") select m).Any())
						{   // we have one acknowledged sent CUSDEC message
							if ((from EDIMessage m in entryHeader.Messages where m.EM_ReceiveTransmit == EDIMessage.Direction.Receive && m.EM_MessageType == "CTL" && m.EM_MessageSubType == EDIMessage.Status.Acknowledged select m).Any())
							{   // we have one received CONTRL ack message
								isReallyAnAmendment = true;
							}
						}
					}
				}
			}
			return isReallyAnAmendment;
		}

		protected override string GetMessageTypeForDeleteEdiMessage()
		{
			return ChiefConstants.CusDecTypeXTC;
		}

		public ZString MessageSubType
		{
			get
			{
				switch (Function)
				{
					case Business.Messaging.CusDecMessageTypeFunction.Original:
						return GBMessageTypeList.Codes.New;
					case Business.Messaging.CusDecMessageTypeFunction.Replacement:
						return GBMessageTypeList.Codes.Amend;
					case Business.Messaging.CusDecMessageTypeFunction.Delete:
						return GBMessageTypeList.Codes.Cancel;
					default:
						return ZString.Empty;
				}
			}
		}
	}
}
