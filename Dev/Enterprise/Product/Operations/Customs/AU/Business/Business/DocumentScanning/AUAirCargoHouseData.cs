using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(AUAirCargoHouseData),
	Enterprise.Core.Constants.DocManagerCodes.AirCargoHouse,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class AUAirCargoHouseData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.CusHAWB);

		protected override Type CollectionType => typeof(CusHAWBCollectionNonDependent);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CusHAWBCollectionNonDependent(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.AU.HouseAirCargo;

		public override string ReferenceType => Core.Constants.ReferenceTypes.All;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("2158463f-63b3-4b56-af56-81bc6fa64baa", "Air Cargo House");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
