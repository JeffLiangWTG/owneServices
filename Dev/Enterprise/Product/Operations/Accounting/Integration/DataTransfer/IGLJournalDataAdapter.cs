using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Integration
{
	public interface IGLJournalDataAdapter
	{
		void Initialize(GLJournalDataAdapterSettings settings);
	}

	public struct GLJournalDataAdapterSettings
	{
		public AccTransactionHeader journalToPopulate;
		public BusinessObjectFactory factoryForNewJournal;
		public bool useJournalNumber;
		public bool useLinePKs;
	}
}
