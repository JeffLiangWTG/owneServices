using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	[DependentBusinessObject(typeof(ClientBISIShipmentHeader), "Charges")]
	public class ClientBISIShipmentCharge : AutoClientBISIShipmentCharge
	{
		public ClientBISIShipmentCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
