using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.GUI
{
	public class FRAddressFormatter : AddressFormatter
	{
		public FRAddressFormatter(BusinessObjectFactory factory, OrgAddress address) : base(factory, address)
		{
			this.address = address;
		}

		readonly OrgAddress address;

		protected override void BuildAddress()
		{
			base.BuildAddress();

			var eori = address?.GetEORI() ?? ZString.Empty;
			var eoriSummary = "EORI: " + (eori.IsEmpty ? (ZString)(NoResString)"<Empty>" : eori);
			FormattedAddress += newLine;
			FormattedAddress += eoriSummary;
		}
	}
}
