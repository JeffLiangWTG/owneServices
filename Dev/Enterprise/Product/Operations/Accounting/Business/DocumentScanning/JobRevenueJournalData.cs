using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(JobRevenueJournalData),
	Enterprise.Core.Constants.DocManagerCodes.JobRevenueJournal)]

namespace Enterprise.Accounting.Business
{
	public class JobRevenueJournalData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(JobRevenueJournal); } }
		protected override Type CollectionType
		{
			get { return typeof(JobRevenueJournalCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("3611900c-ffc2-48e7-875a-c3b555de23f8", "Job Revenue Journal"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.JobRevenueJournal; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			JobRevenueJournalCollection collection = new JobRevenueJournalCollection(factory);
			return collection;
		}
	}
}
