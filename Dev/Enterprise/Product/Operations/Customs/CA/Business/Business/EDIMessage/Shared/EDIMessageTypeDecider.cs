using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CA.Business
{
	public class EDIMessageTypeDecider : TypeDecider, Integration.Customs.CA.IEDIMessageTypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			switch (row[EDIMessage.Schema.EM_MessageType].ToString().Trim())
			{
				case MessageTypeList.Codes.SupplementaryCargoReport:
					return typeof(SUPRPTMessage);
				case MessageTypeList.Codes.G7Export:
					return typeof(EX1STPMessage);
				case MessageTypeList.Codes.GenericACIResponse:
					return typeof(ACIEDIMessage);
				case MessageTypeList.Codes.GenericEXPResponse:
					return typeof(EXPEDIMessage);
				case MessageTypeList.Codes.EDIRelease:
					return typeof(EDIReleaseMessage);
				case MessageTypeList.Codes.B3CUSDEC:
					return typeof(B3Message);
				case MessageTypeList.Codes.CommercialAccountingDeclaration:
					return typeof(CADMessage);
				case MessageTypeList.Codes.XTypeEntry:
					return typeof(B3XMessage);
				case MessageTypeList.Codes.Query:
					return typeof(QueryMessage);
				case MessageTypeList.Codes.SyntaxError:
					return typeof(SyntaxErrorMessage);
				case MessageTypeList.Codes.RNSRequest:
					return typeof(RNSRequestMessage);
				case MessageTypeList.Codes.DataLoadingModule:
					return typeof(DLMMessage);
				case MessageTypeList.Codes.K84Report:
					return typeof(K84Message);
				case MessageTypeList.Codes.TradeChainPartner:
					return typeof(TCPMessage);
				case MessageTypeList.Codes.CSARevenueSummaryForm:
					return typeof(RSFMessage);
				case MessageTypeList.Codes.ACIHouseBill:
					return typeof(ACIHouseBillMessage);
				case MessageTypeList.Codes.ACIForwarderClose:
					return typeof(ACIForwarderCloseMessage);
				case MessageTypeList.Codes.AVSQuery:
					return typeof(AVSQueryMessage);
				case MessageTypeList.Codes.IntegratedImportDeclaration:
					return typeof(IIDUniversalShipmentMessage);
				case EDIMessageTypeList.Codes.XDC:
					switch (row[EDIMessage.Schema.EM_MessageSubType].ToString().Trim())
					{
						case EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch:
						case ARLMessageTypes.Codes.StatementOfAccount:
						case ARLMessageTypes.Codes.DailyNotice:
							return typeof(ARLMessage);
						case EDIMessageSubTypeList.Codes.XmlUniversalEvent:
						case UniversalEventMessageTypes.Codes.IIDResponses:
						case UniversalEventMessageTypes.Codes.D4Notices:
							return typeof(UniversalEventMessage);
						default:
							return typeof(EDIMessage);
					}
				case MessageTypeList.Codes.CARMDailyNotice:
					return typeof(CARMDailyNoticeMessage);
				case MessageTypeList.Codes.CARMStatementOfAccount:
					return typeof(CARMStatementOfAccountMessage);
				default:
					//TODO: Write transformation to set EM_MessageType = 'DLM' where EM_MessageSubType = 'DLM' and get rid of it
					switch (row[EDIMessage.Schema.EM_MessageSubType].ToString().Trim())
					{
						case MessageTypeList.Codes.DataLoadingModule:
							return typeof(DLMMessage);
						default:
							return typeof(EDIMessage);
					}
			}
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}

		public override Type GetTypeForNew()
		{
			return null;
		}
	}
}
