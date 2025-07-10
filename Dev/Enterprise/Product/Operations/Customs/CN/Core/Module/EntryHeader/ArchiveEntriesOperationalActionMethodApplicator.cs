using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CN.Module
{
	public class ArchiveEntriesOperationalActionMethodApplicator : OperationalActionMethodApplicator
	{
		public ArchiveEntriesOperationalActionMethodApplicator(BusinessObjectFactory factory) : base((NoResString)"Archive Entries", factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			log.SetSectionProgressMax(targets.Length);

			foreach (var bo in targets)
			{
				if (bo is CusEntryHeader entryHeader)
				{
					entryHeader.AddRecordArchivedLog();

					log.BumpSectionProgress();
				}
			}
		}
	}
}
