using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GLAccountsData),
	Enterprise.Core.Constants.DocManagerCodes.GLAccounts)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class GLAccountsData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccGLHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(AccGLHeaderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccGLHeaderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccGLHeader; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("8c6376ea-5682-4e61-a8a3-6c06bdac2ea5", "GL Accounts"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
