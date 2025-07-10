using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(GBAirCargoHouseData),
	Enterprise.Core.Constants.DocManagerCodes.AirCargoHouse,
	Country = "GB")]

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class GBAirCargoHouseData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(Customs.Business.CusHAWB); } }
		protected override Type CollectionType
		{
			get { return typeof(CusHAWBCollectionNonDependent); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusHAWBCollectionNonDependent(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Customs.EU.GB.CcsukAirInventoryHouse; } }
		public override string ReferenceType { get { return Constants.ReferenceTypes.SupplyChainLogistics; } }  // SCL - to allow the RRA docs to be printed and scanned, and to be allocated as type RRA (rerlease/removal authority) instead of type PUB (public docuiment).  It gives them a less crappy description (but it's still pretty crappy).
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("136f5d10-c5eb-457c-9bfd-b876a57efa85", "CCSUK House Air Waybill"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override bool AllowLookupOfBizOFromPk { get { return true; } }
	}
}
