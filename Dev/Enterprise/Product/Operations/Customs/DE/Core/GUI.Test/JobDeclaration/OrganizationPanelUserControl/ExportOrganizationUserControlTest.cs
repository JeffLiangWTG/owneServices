using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class ExportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestControlNames()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("RepresentativeAddressControl", "[14] Representative", control.FindSingle<ZAddressControl>("RepresentativeAddressControl").CaptionResourceString.Caption);
					AssertEquals("SellerAddressControl", "[2] Subcontractor", control.FindSingle<ZAddressControl>("SellerAddressControl").CaptionResourceString.Caption);
					AssertEquals("ExporterDocAddressControl", "Exporter", control.FindSingle<ZDocAddressControl>("ExporterDocAddressControl").CaptionResourceString.Caption);
					AssertEquals("ContractualPartnerDocAddressControl", "Contractual Partner", control.FindSingle<ZDocAddressControl>("ContractualPartnerDocAddressControl").CaptionResourceString.Caption);
					AssertEquals("CarrierEUBorderDocAddressControl", "Carrier EU Border", control.FindSingle<ZDocAddressControl>("CarrierEUBorderDocAddressControl").CaptionResourceString.Caption);
				});
			}
		}

		public void TestControlVisibilities()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				AssertEquals("ManufacturerAddressControl", false, control.FindSingle<ZAddressControl>("ManufacturerAddressControl").Visible);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}
		JobDeclaration declaration;
	}
}
