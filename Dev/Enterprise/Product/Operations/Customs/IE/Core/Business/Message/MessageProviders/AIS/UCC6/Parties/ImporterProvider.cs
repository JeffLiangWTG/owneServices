using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class ImporterProvider : PartyProvider, IImporter
	{
		ImporterProvider(OrgAddress importer) : base(importer) { }

		public static ImporterProvider New(OrgAddress address)
		{
			return address == null ? null : new ImporterProvider(address);
		}

		public IContactDetails ContactDetails => null; // To be completed in a future WI

		public bool ForceIncludeNameAndAddressDetailsInMessage => false;
	}
}
