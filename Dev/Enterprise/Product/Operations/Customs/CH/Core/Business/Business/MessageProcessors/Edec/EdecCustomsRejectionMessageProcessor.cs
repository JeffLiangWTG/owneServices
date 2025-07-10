using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecCustomsRejectionMessageProcessor : BaseResponseMessageProcessor<ICustomsRejectionResponseDetail>
{
	public EdecCustomsRejectionMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("E3E518C7-5FBA-40AB-9C34-C54DABAEE2A4", "Customs Rejection Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.CustomsRejected };

	protected override ZString GetEntryHeaderReference(ICustomsRejectionResponseDetail customsResponse) => customsResponse.TraderDeclarationNumber;

	protected override void ProcessResponseMessage(CHEDIMessage message, ICustomsRejectionResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var rejectionDateTime = customsResponse.RejectionDateTime;
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
			entryHeader.Logs.AddNew(AutoEvents.DeclarationRejected, rejectionDateTime);
		}
	}
}
