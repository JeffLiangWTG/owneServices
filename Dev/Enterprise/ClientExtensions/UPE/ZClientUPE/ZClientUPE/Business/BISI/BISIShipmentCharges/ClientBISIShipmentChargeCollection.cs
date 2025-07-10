
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class ClientBISIShipmentChargeCollection : DependentBusinessObjectCollection<ClientBISIShipmentCharge, ClientBISIShipmentHeader>
	{
		public ClientBISIShipmentChargeCollection(ClientBISIShipmentHeader master)
			: base(master)
		{
		}

		public ClientBISIShipmentCharge FindByChargeType(ZString chargeType)
		{
			ClientBISIShipmentCharge[] result = (ClientBISIShipmentCharge[])Find(new ZQuery(ClientBISIShipmentChargeSchema.T9_ChargeType, chargeType));
			return result.Length >= 1 ? result[0] : null;
		}
	}
}
