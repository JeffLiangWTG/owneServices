using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public interface IB3MessageProcessorLinkedObject
	{
		ZDateTime EntryReleaseDate { get; set; }

		void CancelScheduledB3Message();
		void CancelB3LateSendingWarningEvent();
		void AddDocumentsToGeneratorQueue();
	}
}
