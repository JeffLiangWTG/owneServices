using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Common.EU;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BE.Business;

public class CustomsServiceErrorUniversalEventResponseMessageProcessor : EventMessageProcessor
{
	public CustomsServiceErrorUniversalEventResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => DeclarationMessageProcessorHelper.GetBranchPkFromJobBO(linkedObject);

	protected override ZString NoteForUnableToFindALinkedBusinessObject(UniversalEventWrapper messageDataProvider) => DeclarationMessageProcessorHelper.NoteForUnableToFindALinkedBusinessObject;

	protected override void SetErrorStatusToLinkedObject(BEMessage message)
	{
		var entryHeader = (CusEntryHeader)message.EM_LinkedObject;
		entryHeader.CH_EntryStatus = LogicalStatusList.Codes.Error;
	}
}
