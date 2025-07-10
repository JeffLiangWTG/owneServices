using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GBAirCargoMasterData),
	Enterprise.Core.Constants.DocManagerCodes.AirCargoMaster,
	Country = "GB")]

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class GBAirCargoMasterData : GBAirCargoHouseData
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
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.EU.GB.CcsukAirInventory; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("045d3289-27b5-449d-9c89-00f4eb7e45e0", "CCSUK Master Air Waybill"); } }
	}
}
