using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.GoodsDeclarations;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecRuleErrorMessageProcessor : BaseResponseMessageProcessor<IRuleErrorResponseDetail>
{
	public EdecRuleErrorMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("F180CEB2-D88E-4619-8BD8-9C942551944C", "Rule Error Message Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.Import, MessageTypeCodeList.Codes.Export };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.RuleError };

	protected override ZString GetEntryHeaderReference(IRuleErrorResponseDetail customsResponse) => customsResponse.TraderDeclarationNumber;

	protected override void ProcessResponseMessage(CHEDIMessage message, IRuleErrorResponseDetail customsResponse)
	{
		if (customsResponse != null && message.EM_LinkedObject is CusEntryHeader entryHeader)
		{
			var rejectionDateTime = customsResponse.RejectionDateTime;
			entryHeader.CH_Status = CHLogicalStatusList.Codes.Invalid;
			entryHeader.Logs.AddNew(AutoEvents.DeclarationRejected, rejectionDateTime);
		}
	}
}
