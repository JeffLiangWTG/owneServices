using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.xTMessaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor : EventMessageProcessor
	{
		public NCTSCustomsServiceErrorUniversalEventResponseMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => NCTSMessageProcessorHelper.GetBranchPkFromJobBO(linkedObject);

		protected override ZString NoteForUnableToFindALinkedBusinessObject(UniversalEventWrapper messageDataProvider) => NCTSMessageProcessorHelper.NoteForUnableToFindALinkedBusinessObject;

		protected override void SetErrorStatusToLinkedObject(BEMessage message)
		{
			var linkedObject = message.EM_LinkedObject;
			NctsHeader nctsHeader;
			if (linkedObject is NctsDepartureMovementHeader moveHeader)
			{
				nctsHeader = moveHeader.Header;
			}
			else
			{
				nctsHeader = (NctsHeader)linkedObject;
			}

			nctsHeader.EffectiveMessageStatus = LogicalStatusList.Codes.Error;
		}
	}
}
