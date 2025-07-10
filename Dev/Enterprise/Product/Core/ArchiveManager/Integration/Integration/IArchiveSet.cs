using System;
using System.Collections.ObjectModel;
using CargoWise.EntityFramework;

namespace Enterprise.ArchiveManager.Integration
{
	public interface IArchiveSet
	{
		IArchiveItem MainArchiveItem { get; }

		string MainArchiveItemNK { get; }

		Guid IgnoredPK { get; set; }

		int Count { get; }

		IArchiveSystemDescriptor SystemDescriptor { get; }

		string StageName { get; }

		Guid ScheduleIdentifier { get; }

		Guid MainItemFK { get; }

		ZQuery MainArchiveableTypeFilter { get; }

		IArchiveStepResult Load(IArchiveLogger logger);

		void MarkToIgnoreForThisRun();

		ReadOnlyCollection<IArchiveItem> GetArchiveItems();

		IArchiveItem GetArchiveItem(Guid itemPK);

		ReadOnlyDictionary<string, ITableProcessingInfo> GetTotalDocumentsDeletedFromArchiveItems();

		int TotalNumberOfDocumentsGeneratedInSet { get; set; }

		long TimeTakenToGenerateDocuments { get; set; }

		long TimeTakenToDelete { get; set; }

		long TimeTakenToDeleteAllDocuments { get; set; }
	}
}
