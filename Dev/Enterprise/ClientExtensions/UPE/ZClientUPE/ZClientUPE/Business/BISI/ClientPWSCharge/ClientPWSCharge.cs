using System.Data;

using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	[DependentBusinessObject(typeof(ClientPWSHeader), "Charges")]
	public class ClientPWSCharge : AutoClientPWSCharge
	{
		public ClientPWSCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
