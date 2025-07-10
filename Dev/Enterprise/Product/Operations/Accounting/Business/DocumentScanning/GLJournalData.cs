using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GLJournalData),
	Enterprise.Core.Constants.DocManagerCodes.GLJournal)]

namespace Enterprise.Accounting.Business
{
	public class GLJournalData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GLJournal); } }
		protected override Type CollectionType
		{
			get { return typeof(GLJournalCollection); }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("2977180d-1c79-4e1c-99f3-b5c8cede2bbc", "GL Journal"); } }
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GLJournal; } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			GLJournalCollection collection = new GLJournalCollection(factory);
			return collection;
		}
	}
}
