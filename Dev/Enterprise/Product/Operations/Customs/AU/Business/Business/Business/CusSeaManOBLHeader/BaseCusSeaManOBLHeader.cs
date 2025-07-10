using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class BaseCusSeaManOBLHeader : Customs.Business.CusSeaManOBLHeader
	{
		protected BaseCusSeaManOBLHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			ShipmentCalculator = new CusSeaManOBLHeaderShipmentStatusCalculator(this);
		}

		public readonly CusSeaManOBLHeaderShipmentStatusCalculator ShipmentCalculator;

		#region ShipmentStatus

		public CargoCusStatus ShipmentStatus
		{
			get
			{
				if (fShipmentStatus == null)
				{
					fShipmentStatus = new CargoCusStatus(BO_ShipmentStatusInfo, ShipmentCalculator);
				}
				return fShipmentStatus;
			}
		}
		CargoCusStatus fShipmentStatus;

		#endregion
	}
}
