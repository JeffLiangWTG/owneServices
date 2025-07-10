using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(TaxOverrideGroupData),
	Enterprise.Core.Constants.DocManagerCodes.TaxOverrideGroup)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class TaxOverrideGroupData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccTaxOverrideGroup); } }
		protected override Type CollectionType
		{
			get { return typeof(AccTaxOverrideGroupCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccTaxOverrideGroupCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccTaxOverrideGroup; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("03047374-d105-49a3-9f69-bba4c506699b", "Tax Override Group"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
