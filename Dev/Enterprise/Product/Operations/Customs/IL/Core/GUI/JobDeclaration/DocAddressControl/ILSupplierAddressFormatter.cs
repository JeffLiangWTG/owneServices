using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.GUI
{
	public class ILSupplierAddressFormatter : AddressFormatter
	{
		public ILSupplierAddressFormatter(BusinessObjectFactory factory, OrgAddress address) : base(factory, address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		protected override void BuildAddress()
		{
			base.BuildAddress();

			var csc = address?.Header?.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(Core.Constants.CountryCodes.Israel, new ZString[] { OrgCusCode.CodeTypes.SupplierCode }) ?? ZString.Empty;
			var ccpSummary = csc.IsEmpty ? Res.GetString("0EF3BB82-261A-4052-9359-FC169FE15D43", "Customs #: <Empty>") : Res.GetString("99E69501-485A-4384-B5C1-F869570FEBC3", "Customs #: {0}", csc);
			FormattedAddress += newLine;
			FormattedAddress += ccpSummary;
		}
	}
}
