using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CusSCAOceanBillData),
	Enterprise.Core.Constants.DocManagerCodes.SCAOceanBill,
	Country = "AU")]

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAOceanBillData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(Customs.Business.BaseCusSCAOceanBill);

		protected override Type CollectionType => typeof(CusSCAOceanBillCollection);

		public override string ReferenceType => Core.Constants.ReferenceTypes.All;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("13f909ee-2261-492a-90f0-ff8641c29853", "SCA Ocean Bill");

		public override CargoWise.EntityFramework.IBusinessObjectCollection GetBusinessObjectCollection(CargoWise.EntityFramework.BusinessObjectFactory factory)
		{
			return new CusSCAOceanBillCollection(factory);
		}

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
