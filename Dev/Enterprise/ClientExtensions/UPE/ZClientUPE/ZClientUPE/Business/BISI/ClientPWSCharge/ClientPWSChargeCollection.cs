
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class ClientPWSChargeCollection : DependentBusinessObjectCollection<ClientPWSCharge, ClientPWSHeader>
	{
		public ClientPWSChargeCollection(ClientPWSHeader master)
			: base(master)
		{
		}

		public ClientPWSCharge FindByChargeDescription(ZString description)
		{
			ClientPWSCharge[] result = (ClientPWSCharge[])Find(new ZQuery(ClientPWSChargeSchema.U2_ChargeDescription, description));
			return result.Length >= 1 ? result[0] : null;
		}
	}
}
