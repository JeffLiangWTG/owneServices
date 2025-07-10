using CargoWise.EntityFramework;
using CargoWise.Types;
using static System.FormattableString;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public abstract class FREDIMessagePrettier
	{
		protected FREDIMessagePrettier(MessageDataObject messageDataObject)
		{
			MessageDataObject = messageDataObject;
		}

		public MessageDataObject MessageDataObject { get; }

		public BusinessObjectFactory Factory => MessageDataObject.Factory;

		public abstract ZString GetMessageInterpretation();

		protected ZString ToH1IfNotEmpty(ZString h1)
		{
			return !h1.IsEmpty
				? (ZString)Invariant($"<H1>{h1}</H1>")
				: ZString.Empty;
		}

		protected ZString ToH3IfNotEmpty(ZString h3)
		{
			return !h3.IsEmpty
				? (ZString)Invariant($"<H3>{h3}</H3>")
				: ZString.Empty;
		}

		protected ZString ToH3IfNotEmpty(ZString description, ZString h3)
		{
			return !h3.IsEmpty
				? (ZString)Invariant($"<H3>{description + h3}</H3>")
				: ZString.Empty;
		}

		protected ZString ToH4IfNotEmpty(ZString h4)
		{
			return !h4.IsEmpty
				? (ZString)Invariant($"<H4>{h4}</H4>")
				: ZString.Empty;
		}

		protected ZString ToH4IfNotEmpty(ZString description, ZString h4)
		{
			return !h4.IsEmpty
				? (ZString)Invariant($"<H4>{description + h4}</H4>")
				: ZString.Empty;
		}

		protected string GetReadableRequestType(string demandAcronym)
		{
			var result = ZString.Empty;
			switch (demandAcronym)
			{
				case RequestTypeList.Codes.INV:
					result = RequestTypeList.Descriptions.INV;
					break;
				case RequestTypeList.Codes.RCT:
					result = RequestTypeList.Descriptions.RCT;
					break;
				case RequestTypeList.Codes.REV:
					result = RequestTypeList.Descriptions.REV;
					break;
			}
			return result;
		}

		protected string GetReadableRequestStatus(string requestStatus)
		{
			var result = ZString.Empty;
			switch (requestStatus)
			{
				case RequestStatusList.Codes.Accepted:
					result = RequestStatusList.Descriptions.Accepted;
					break;
				case RequestStatusList.Codes.Rejected:
					result = RequestStatusList.Descriptions.Rejected;
					break;
				case RequestStatusList.Codes.Pending:
					result = RequestStatusList.Descriptions.Pending;
					break;
			}
			return result;
		}

		protected string GetReadableRequestStatusDate(string requestDate)
		{
			var result = ZDateTime.Empty;
			return ZDateTime.TryParseExact(requestDate, out result, "ddMMyyyy") ? result.ToShortDateString() : "";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Html strings")]
		protected string MakeRequestStatusPrettier(string demandType, string requestStatus, string requestSatusDate, string reason, string justification, string comment, string newDestination, string rejectionReason, string requestNumber, string agentName, string agentOffice)
		{
			var result = ToH3IfNotEmpty($"Response to amendment/cancellation request:");
			result += "<ul>";
			result += ToH4IfNotEmpty($"<li>Demand type: ", $"{GetReadableRequestType(demandType)}");
			result += ToH4IfNotEmpty($"<li>Request status: ", $"{GetReadableRequestStatus(requestStatus)}");
			result += ToH4IfNotEmpty($"<li>Status granted on: ", $"{GetReadableRequestStatusDate(requestSatusDate)}");
			result += ToH4IfNotEmpty($"<li>Your reason: ", $"{reason}");
			result += ToH4IfNotEmpty($"<li>Your justification: ", $"{justification}");
			result += ToH4IfNotEmpty($"<li>Your comment: ", $"{comment}");
			result += ToH4IfNotEmpty($"<li>Your new destination: ", $"{newDestination}");
			result += ToH4IfNotEmpty($"<li>Rejection reason: ", $"{rejectionReason}");
			result += ToH4IfNotEmpty($"<li>Request number: ", $"{requestNumber}");
			result += ToH4IfNotEmpty($"<li>Agent name: ", $"{agentName}");
			result += ToH4IfNotEmpty($"<li>Agent office: ", $"{agentOffice}");
			result += "</ul>";
			return result;
		}
	}

	public abstract class FREDIMessagePrettier<TMessage> : FREDIMessagePrettier
		where TMessage : class
	{
		protected FREDIMessagePrettier(MessageDataObject<TMessage> messageDataObject) : base(messageDataObject)
		{
		}

		public new MessageDataObject<TMessage> MessageDataObject => (MessageDataObject<TMessage>)base.MessageDataObject;
		public TMessage ResponseMessage => MessageDataObject.ResponseMessage;
	}
}
