using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static CargoWise.EventReference.Constants;
using static Enterprise.xTMessaging.Shared.Constants;

namespace Enterprise.Customs.CH.Business;

public class XtErrorResponseMessageProcessor : BaseInboundMessageProcessor<UniversalEventWrapper>
{
	public XtErrorResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => (NoResString)"XT Error Response Message Processor";

	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.CHCustomsEdec;

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.EBD, MessageTypeCodeList.Codes.ECM };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Rejected };

	protected override UniversalEventWrapper DeserializeResponse(CHEDIMessage message) => message.UniversalEventData;

	protected override BusinessObject FindLinkedObject(EDIMessage message, UniversalEventWrapper xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, UniversalEventWrapper customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader cusEntryHeader)
		{
			switch (message.EM_MessageType)
			{
				case MessageTypeCodeList.Codes.Import:
					cusEntryHeader.CH_Status = CHLogicalStatusList.Codes.Failed;
					break;
				case MessageTypeCodeList.Codes.EBD:
					var originalMessage = message.UniversalEventData.GetContextValueByType(xTUniversalEventContextTypes.OriginalMessage);
					Match match = Regex.Match(originalMessage, FileNamePattern);
					if (match.Success)
					{
						var filename = match.Groups[1].Value;
						cusEntryHeader.Logs.AddNew(Events.DocumentNotDelivered, new KeyValuePair<string, string>(EventReferenceParameters.Codes.File, filename));
					}
					break;
				case MessageTypeCodeList.Codes.ECM:
					var oldLastEComplaintStatus = cusEntryHeader.CH_LastEComplaintStatus;
					cusEntryHeader.CH_LastEComplaintStatus = CHLogicalStatusList.Codes.Failed;

					if (oldLastEComplaintStatus != cusEntryHeader.CH_LastEComplaintStatus)
					{
						cusEntryHeader.Logs.AddNew(Events.EComStatusChange, new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, cusEntryHeader.CH_LastEComplaintStatus), new KeyValuePair<string, string>(EventReferenceParameters.Codes.Old, oldLastEComplaintStatus));
					}

					break;
			}
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
	const string FileNamePattern = @"<filename>(.*?)<\/filename>";
}
