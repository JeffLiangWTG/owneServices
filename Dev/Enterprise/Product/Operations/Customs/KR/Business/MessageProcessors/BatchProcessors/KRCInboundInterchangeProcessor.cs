using System.Xml;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class KRCInboundInterchangeProcessor : InboundInterchangeProcessor
	{
		public KRCInboundInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}
		protected override string[] ApplicationCodes => new string[] { EDIMessage.ApplicationCodes.KRCustoms };

		protected override IInboundMessageCreator GetMessageCreator(Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			IInboundMessageCreator result = null;
			switch (interchange.EI_InterchangeType)
			{
				case EDIInterchangeType.ESR:
					result = new ESRInboundMessageCreator();
					break;
				case EDIInterchangeType.RSP:
					result = new RSPInboundMessageCreator();
					break;
				case EDIInterchangeType.SSR:
					interchange.GetOutgoingInterchange().SetMessageAndInterchangeStatusAsDiscarded();
					break;
				default:
					result = new InboundMessageCreator();
					break;
			}
			return result;
		}

		protected override ZString GetStatusForProcessFailure(Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			return interchange.EI_InterchangeType == EDIInterchangeType.SSR ? EDIInterchangeStatusList.Codes.Discarded : EDIInterchangeStatusList.Codes.Failed;
		}

		protected override bool IsNoBranchFilter
		{
			get { return true; }
		}

		#region InboundMessageCreator Class

		class InboundMessageCreator : IInboundMessageCreator
		{
			public void CreateMessagesForInterchange(Enterprise.Messaging.Business.EDIInterchange interchange)
			{
				interchange.UpdateEI_GB();
				var message = interchange.CreateMessage();
				message.SetEM_MessageDataSource(new StreamSource(interchange.GetEI_BodyDataReader()));
				var messageDataAccessedToReadInBodyDataTillEnd = message.EM_MessageData;
				message.EM_MessageType = interchange.EI_InterchangeType;
			}
		}
		class ESRInboundMessageCreator : IInboundMessageCreator
		{
			public void CreateMessagesForInterchange(Enterprise.Messaging.Business.EDIInterchange interchange)
			{
				interchange.UpdateEI_GB();

				var message = interchange.CreateMessage();
				message.EM_MessageType = interchange.EI_InterchangeType;
				message.EM_MessageText = interchange.EI_HeaderText;
			}
		}
		class RSPInboundMessageCreator : IInboundMessageCreator
		{
			public void CreateMessagesForInterchange(Enterprise.Messaging.Business.EDIInterchange interchange)
			{
				const string signedNodeName = "Response/Signature";
				const string typecodeNodeName = "Response/TypeCode";

				interchange.UpdateEI_GB();

				var message = interchange.CreateMessage();
				var xmldoc = new XmlDocument();
				xmldoc.PreserveWhitespace = true;
				xmldoc.LoadXml(MessageEncoding.UTF8WithoutBOM.GetString(interchange.EI_BodyData));

				var xPathTypeCode = StringUtils.ConvertToXPath(typecodeNodeName);
				var typecode_info = xmldoc.SelectSingleNode(xPathTypeCode)?.InnerXml ?? ZString.Empty;
				if (typecode_info.Length == 9)
				{
					message.EM_MessageType = typecode_info.Substring(6, 3);
				}

				var xPathSignature = StringUtils.ConvertToXPath(signedNodeName);
				var signed_info = xmldoc.SelectSingleNode(xPathSignature);
				xmldoc.DocumentElement.RemoveChild(signed_info);
				message.EM_MessageData = MessageEncoding.UTF8WithoutBOM.GetBytes(xmldoc.InnerXml);
				interchange.GetOutgoingInterchange().SetMessageAndInterchangeStatusAsDiscarded();
			}
		}

		#endregion
	}
	static class InterchangeExtensionMethods
	{
		public static EDIMessage CreateMessage(this Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			var result = interchange.Factory.New<EDIMessage>();
			result.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
			result.EM_Status = EDIMessageStatusList.Codes.Queued;
			interchange.ContainedMessages.Add(result);
			return result;
		}

		public static void UpdateEI_GB(this Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			var outgoingInterChange = interchange.GetOutgoingInterchange();
			if (outgoingInterChange != null)
			{
				interchange.EI_GB = outgoingInterChange.EI_GB;
			}
		}

		public static EDIInterchange GetOutgoingInterchange(this Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			var query = new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
			query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, interchange.EI_SessionGUID);
			return interchange.Factory.LoadTop1<EDIInterchange>(query);
		}

		public static void SetMessageAndInterchangeStatusAsDiscarded(this Enterprise.Messaging.Business.EDIInterchange interchange)
		{
			if (interchange != null && CanInterchangeTypeBeSetAsDiscarded(interchange.EI_InterchangeType))
			{
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Discarded;
				if (interchange.ContainedMessages.Count > 0)
				{
					var outMessage = interchange.ContainedMessages[0];
					outMessage.EM_Status = EDIMessageStatusList.Codes.Discarded;
				}
			}
		}

		static bool CanInterchangeTypeBeSetAsDiscarded(ZString interchangeType)
		{
			return interchangeType == EDIInterchangeType.DLT ||
					interchangeType == EDIInterchangeType.DOC ||
					interchangeType == EDIInterchangeType.SSR;
		}
	}
}
