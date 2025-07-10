using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.CusTempStorage
{
	public class WarehouseIdentificationProvider : IWarehouseIdentification
	{
		public static WarehouseIdentificationProvider New(ZString type, ZString id) => new WarehouseIdentificationProvider(type, id);

		WarehouseIdentificationProvider(ZString type, ZString id)
		{
			this.type = type;
			this.id = id;
		}
		readonly ZString type;
		readonly ZString id;

		public string Type => type;

		public string ID => id;
	}
}
