using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business
{
	public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.EU.ICusVehicle
	{
		public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);

		protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);
	}
}
