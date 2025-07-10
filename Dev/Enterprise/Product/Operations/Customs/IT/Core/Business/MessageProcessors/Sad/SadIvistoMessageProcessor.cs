using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.MessageStructure.IVISTO;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class SadIvistoMessageProcessor : SadCustomsExitAndReleaseMessageProcessor<Ivisto, CustomsApplicationResponse>
{
	public SadIvistoMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZString MessageTypeToInclude => SADConstants.CustomsInterchangeType.Ivisto;
	protected override ZString MessageInfo => nameof(Ivisto);

	protected override bool CusEntryNumAlreadyExists(ISadCustomsLinkedObjectAdapter entryAdapter) => entryAdapter.IvistoCusEntryNum != null;

	protected override void UpdateEntryStatus(ISadCustomsLinkedObjectAdapter entryAdapter, CustomsApplicationResponse applicationResponse) => entryAdapter.SetEntryCustomsStatus(ITEntryStatusList.Codes.Exit);

	protected override void PopulateCusEntryNum(CusEntryNumber entryNumber, CustomsApplicationResponse applicationResponse)
	{
		entryNumber.CE_EntryNum = string.Empty;
		entryNumber.CE_EntryType = CusEntryNumberConstants.EntryTypes.Ivisto;
		entryNumber.CE_EntryLineReference = applicationResponse.EffectiveExitCustomsOffice;
		entryNumber.CE_EntryStatus = ConvertCustomsOfficeResult(applicationResponse.ExitCustomsOfficeResult);
		entryNumber.CE_IssueDate = applicationResponse.ExitOrRejectedExitDate;
	}

	const string UDF = "UDF";
	readonly CodeDescriptionPairList exitStatus = new ExitStatusList();
	protected string ConvertCustomsOfficeResult(ZString customsOfficeResult)
	{
		return exitStatus.GetCodeFromDescription(customsOfficeResult) ?? UDF;
	}
}
