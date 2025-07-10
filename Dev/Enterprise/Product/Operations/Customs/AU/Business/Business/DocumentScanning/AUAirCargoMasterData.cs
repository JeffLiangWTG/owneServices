using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUAirCargoMasterData),
	Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AUAirCargoMasterData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Customs.Business.CusMAWB); } }
		protected override Type CollectionType
		{
			get { return typeof(CusMAWBCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusMAWBCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.AU.AirCargo; } }
		public override string ReferenceType { get { return Core.Constants.DocManagerCodes.AirCargoMaster; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("913f1c38-f147-4a50-b00c-6f96e5c0effc", "Air Cargo Master"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
