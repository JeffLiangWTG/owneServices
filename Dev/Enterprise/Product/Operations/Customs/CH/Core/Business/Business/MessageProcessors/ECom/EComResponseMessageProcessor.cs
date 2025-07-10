using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public class EComResponseMessageProcessor : BaseResponseMessageProcessor<IEdecComplaintResponseDetail>
{
	public EComResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("285960E1-5041-4A82-B463-23A78BE2241D", "Customs ECom Message Response Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.ECM };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Accepted, MessageSubTypeCodeList.Codes.RuleError, MessageSubTypeCodeList.Codes.XmlSchemaError };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEdecComplaintResponseDetail xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEdecComplaintResponseDetail customsResponse)
	{
		var entryHeader = FindLinkedObject(message, customsResponse) as CusEntryHeader;
		if (entryHeader != null)
		{
			var oldLastEComplaintStatus = entryHeader.CH_LastEComplaintStatus;

			switch (message.EM_MessageSubType)
			{
				case MessageSubTypeCodeList.Codes.Accepted:
					entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Accepted;
					break;
				case MessageSubTypeCodeList.Codes.RuleError:
				case MessageSubTypeCodeList.Codes.XmlSchemaError:
					entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Rejected;
					break;
			}

			if (oldLastEComplaintStatus != entryHeader.CH_LastEComplaintStatus)
			{
				entryHeader.Logs.AddNew(Events.EComStatusChange, new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, entryHeader.CH_LastEComplaintStatus), new KeyValuePair<string, string>(EventReferenceParameters.Codes.Old, oldLastEComplaintStatus));
			}
		}
	}
}
