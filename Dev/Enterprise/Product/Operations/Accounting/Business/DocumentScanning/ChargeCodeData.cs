using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ChargeCodeData),
	Enterprise.Core.Constants.DocManagerCodes.ChargeCode)]

namespace Enterprise.Accounting.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;

	public class ChargeCodeData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccChargeCode); } }
		protected override Type CollectionType
		{
			get { return typeof(AccChargeCodeCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccChargeCodeCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccChargeCode; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("da10e13c-0480-47a4-bffa-832b3caacb6d", "Charge Code"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
