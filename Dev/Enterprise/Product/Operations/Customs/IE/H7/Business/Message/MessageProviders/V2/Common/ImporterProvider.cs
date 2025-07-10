using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IE.H7.Business
{
	public class ImporterProvider : BillPartyProvider, IImporter
	{
		public ImporterProvider(AsycudaBill bill) : base(bill, AsycudaBillAddress.AddressType.Consignee)
		{
		}

		public IContactDetails ContactDetails => null;

		public bool ForceIncludeNameAndAddressDetailsInMessage => Id == NotRegistered;
	}
}
