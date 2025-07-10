using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public abstract class NCTSMessageProcessor<T> : BE.Business.BaseMessageProcessor<T> where T : class, IInboundProvider
	{
		protected NCTSMessageProcessor(LoggingInformation logger) : base(logger) { }

		protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) => NCTSMessageProcessorHelper.GetBranchPkFromJobBO(linkedObject);

		protected override ZString StatusForUnableToFindALinkedBusinessObject => EDIMessage.Status.Failed;

		protected override ZString NoteForUnableToFindALinkedBusinessObject(T messageDataProvider) => NCTSMessageProcessorHelper.NoteForUnableToFindALinkedBusinessObject;
	}
}
