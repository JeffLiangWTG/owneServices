using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.ECom;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using static CargoWise.EventReference.Constants;

namespace Enterprise.Customs.CH.Business;

public class EComRequestMessageProcessor : BaseResponseMessageProcessor<IEdecComplaintRequestDetail>
{
	public EComRequestMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("E12E8969-7107-4E37-B550-B214F2391C32", "Customs ECom Message Request Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.ECM };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Request };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEdecComplaintRequestDetail xmlObject) => FindLinkedEntryHeaderByEntryNum(message, xmlObject.CustomsDeclarationNumber, anyVersion: true);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEdecComplaintRequestDetail customsResponse)
	{
		if (message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var oldLastEComplaintStatus = entryHeader.CH_LastEComplaintStatus;
			if (oldLastEComplaintStatus != EComplaintStatusList.Codes.Received)
			{
				entryHeader.CH_LastEComplaintStatus = EComplaintStatusList.Codes.Received;
				entryHeader.Logs.AddNew(Events.EComStatusChange, new KeyValuePair<string, string>(EventReferenceParameters.Codes.New, entryHeader.CH_LastEComplaintStatus), new KeyValuePair<string, string>(EventReferenceParameters.Codes.Old, oldLastEComplaintStatus));
			}
		}
	}
}
