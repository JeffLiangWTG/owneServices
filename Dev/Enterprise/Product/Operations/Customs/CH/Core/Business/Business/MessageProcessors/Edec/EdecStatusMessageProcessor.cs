using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.CH.Business;

public class EdecStatusMessageProcessor : BaseResponseMessageProcessor<IStatusResponseDetail>
{
	public EdecStatusMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("6AA1E0C8-9BEE-49A6-B498-749447E80A08", "Status Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.Status };

	protected override ZString GetEntryHeaderReference(IStatusResponseDetail customsResponse) => customsResponse.TraderDeclarationNumber;

	protected override void ProcessResponseMessage(CHEDIMessage message, IStatusResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			entryHeader.CH_EntryStatus = customsResponse.Status;
			if (entryHeader.CH_EntryStatus == SwissCustomsConstants.CustomsStatusCodes.CustomsDeclarationReceived)
			{
				entryHeader.CH_Status = CHLogicalStatusList.Codes.Acknowledged;
			}

			entryHeader.CH_EntryReleaseDate = customsResponse.IsGoodsDeclarationRelease ? customsResponse.StatusDate.DateTime : entryHeader.CH_EntryReleaseDate;
		}
	}
}
