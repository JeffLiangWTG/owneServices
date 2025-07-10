using System;
using System.Data;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.BatchProcessor;
using Enterprise.Customs.CA.Registry;
using Enterprise.Registry.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CAEDIInterchange : Enterprise.Messaging.Business.EDIInterchange, Integration.Customs.CA.IEDIInterchange
	{
		public CAEDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type GetMessageTypeToCreate(ZString messageText)
		{
			if (EI_ApplicationCode == ApplicationCodes.CAIMP)
			{
				var messageType = BatchProcessorUtilities.GetMessageType(EI_HeaderText, EI_BodyText);
				switch (messageType)
				{
					case MessageTypeList.Codes.EDIRelease:
						return typeof(EDIReleaseMessage);
					case MessageTypeList.Codes.B3CUSDEC:
						return typeof(B3Message);
					case MessageTypeList.Codes.SyntaxError:
						return typeof(SyntaxErrorMessage);
					case MessageTypeList.Codes.Query:
						return typeof(QueryMessage);
					case MessageTypeList.Codes.K84Report:
						return typeof(K84Message);
					case MessageTypeList.Codes.TradeChainPartner:
						return typeof(TCPMessage);
					case MessageTypeList.Codes.CSARevenueSummaryForm:
						return typeof(RSFMessage);
				}
			}
			return base.GetMessageTypeToCreate(messageText);
		}

		internal Enterprise.Messaging.Business.EDIMessage CreateMessageFromInterchange(Type messageType, string messageSubType = "")
		{
			var newEDIMessage = ContainedMessages.AddNew(messageType);
			newEDIMessage.EM_GB = EI_GB;
			newEDIMessage.EM_ReceiveTransmit = Direction.Receive;
			newEDIMessage.EM_MessageText = EI_BodyText;
			newEDIMessage.EM_MessageNum = EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
			if (!messageSubType.IsEmpty())
			{
				newEDIMessage.EM_MessageSubType = messageSubType;
			}
			return newEDIMessage;
		}

		protected override ZString GetInterchangeNumber()
		{
			return PreparationDateTime.ToString("yyyyMMddHHmm") + base.GetInterchangeNumber().PadLeft(14, '0');
		}

		protected override ZString GetInterchangeNumberReplacementString(ZString interchangeNumber)
		{
			return interchangeNumber.Right(14).TrimStart('0');
		}

		protected override bool ShouldSendViaEHubCore
		{
			get { return eHubMessagingRegistry.Instance.SendCAViaEHub.Value; }
		}

		protected override bool ShouldBatchNumberBeByInterchange
		{
			get { return CACustomsDataRegistry.Instance.ShouldBatchNumberBeByInterchange.Value; }
		}
	}
}
