using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusSCAHouseBillData),
	Enterprise.Core.Constants.DocManagerCodes.SCAHouseBill,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseBillData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(CusSCAHouse);
		protected override Type CollectionType
		{
			get { return typeof(CusSCAHouseCollectionNonDependent); }
		}

		public override string ReferenceType => Core.Constants.ReferenceTypes.All;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("73C71C02-B9F5-4EE8-A313-3549B4D052DE", "SCA House Bill");

		public override CargoWise.EntityFramework.IBusinessObjectCollection GetBusinessObjectCollection(CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			return new CusSCAHouseCollectionNonDependent(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
