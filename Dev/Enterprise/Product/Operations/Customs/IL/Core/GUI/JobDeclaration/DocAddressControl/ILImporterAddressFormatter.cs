using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.GUI
{
	public class ILImporterAddressFormatter : AddressFormatter
	{
		public ILImporterAddressFormatter(BusinessObjectFactory factory, OrgAddress address) : base(factory, address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		protected override void BuildAddress()
		{
			base.BuildAddress();

			var vat = address?.Header?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Israel, new ZString[] { OrgCusCode.CodeTypes.VATCode }) ?? ZString.Empty;
			var vatSummary = vat.IsEmpty ? Res.GetString("57A88365-767A-4E0A-8C54-E2DB81F8D9BC", "VAT #: <Empty>") : Res.GetString("6DA4EECF-AD4A-46F6-A8EF-90927710FF70", "VAT #: {0}", vat);
			FormattedAddress += newLine;
			FormattedAddress += vatSummary;
		}
	}
}
