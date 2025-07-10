using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusOutturnHeaderJobData),
	Enterprise.Core.Constants.DocManagerCodes.CusOutturnHeader)]

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderJobData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(CusOutturnHeader); }
		}

		protected override Type CollectionType
		{
			get { return typeof(CusOutturnHeader); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusOutturnHeaderCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.AU.SeaCargoDepot; }
		}

		public override string ReferenceType
		{
			get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("1ec680a1-1c54-4b90-93c1-b390a4abeee1", "Sea Cargo Outturn"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
