using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.H7V1;
using CargoWise.EntityFramework;
using Enterprise.Customs.ManifestBase;
using IParty = CargoWise.Customs.IE.MessageContracts.AIS.Interfaces.IParty;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1
{
	public class Parties02Provider : IParties02
	{
		public Parties02Provider(AsycudaBill bill)
		{
			this.bill = bill;
		}
		readonly AsycudaBill bill;

		public IParty Importer => CachedValueHelper.GetValue(ref importerCached, () => new BillPartyProvider(bill, AsycudaBillAddress.AddressType.Consignee));
		CachedValue<IParty> importerCached;

		public IParty Exporter => CachedValueHelper.GetValue(ref exporterCached, () => MessageProviderHelper.GetExporter(bill));
		CachedValue<IParty> exporterCached;
	}
}
