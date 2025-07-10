using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.Business;

public abstract class DeclarationMessageProcessor<T> : BaseMessageProcessor<T> where T : class, IInboundProvider
{
	protected DeclarationMessageProcessor(LoggingInformation logger) : base(logger) { }

	protected override ZGuid GetBranchPkFromJobBO(BusinessObject linkedObject) =>  DeclarationMessageProcessorHelper.GetBranchPkFromJobBO(linkedObject);

	protected sealed override void ProcessMessageCore(BEMessage message, T messageDataProvider)
	{
		var (jobStatus, messageStatus, errorNote) = UpdateBOAndMessageStatus(message, messageDataProvider);

		if (!jobStatus.IsEmpty)
		{
			((CusEntryHeader)message.EM_LinkedObject).CH_EntryStatus = jobStatus;
		}

		if (!messageStatus.IsEmpty)
		{
			message.EM_Status = messageStatus;
		}

		if (!errorNote.IsEmpty)
		{
			message.Notes.AddNew(true, Constants.MessageProcessingNotes.ProcessingLog, errorNote);
		}
	}

	protected abstract (ZString jobStatus, ZString messageStatus, ZString processLog) UpdateBOAndMessageStatus(BEMessage message, T messageDataProvider);

	protected override ZString NoteForUnableToFindALinkedBusinessObject(T messageDataProvider) => DeclarationMessageProcessorHelper.NoteForUnableToFindALinkedBusinessObject;
}
