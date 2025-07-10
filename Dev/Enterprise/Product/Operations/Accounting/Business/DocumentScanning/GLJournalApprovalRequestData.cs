using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GLJournalApprovalRequestData),
	Enterprise.Core.Constants.DocManagerCodes.GLJournalApprovalRequest)]

namespace Enterprise.Accounting.Business
{
	public class GLJournalApprovalRequestData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GLJournalApprovalRequest); } }
		protected override Type CollectionType
		{
			get { return typeof(GLJournalApprovalRequestCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("da41f353-b982-4dec-baac-eb833d0f0777", "GL Journal Approval Request"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GLJournalApproval; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			GLJournalApprovalRequestCollection collection = new GLJournalApprovalRequestCollection(factory);
			return collection;
		}
	}
}
