using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class ImportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			using (var control = new ImportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ManufacturerAddressControl", true, control.FindSingle<Control>("ManufacturerAddressControl").Visible);
					AssertEquals("RepresentativeAddressControl", true, control.FindSingle<Control>("RepresentativeAddressControl").Visible);
					AssertEquals("DeclarantOfficeAddressControl", true, control.FindSingle<Control>("DeclarantOfficeAddressControl").Visible);
					AssertEquals("SellerAddressControl", true, control.FindSingle<Control>("SellerAddressControl").Visible);
					AssertEquals("DefermentPartyDocAddressControl", true, control.FindSingle<Control>("DefermentPartyDocAddressControl").Visible);
				});
			}
		}

		public void TestControlNames()
		{
			using (var control = new ImportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ManufacturerAddressControl", "Manufacturer", control.FindSingle<ZAddressControl>("ManufacturerAddressControl").CaptionResourceString.Caption);
					AssertEquals("RepresentativeAddressControl", "Representative", control.FindSingle<ZAddressControl>("RepresentativeAddressControl").CaptionResourceString.Caption);
					AssertEquals("DeclarantOfficeAddressControl", "[14] Declarant", control.FindSingle<ZAddressControl>("DeclarantOfficeAddressControl").CaptionResourceString.Caption);
					AssertEquals("SellerAddressControl", "Seller", control.FindSingle<ZAddressControl>("SellerAddressControl").CaptionResourceString.Caption);
					AssertEquals("DefermentPartyDocAddressControl", "Deferment Party", control.FindSingle<ZDocAddressControl>("DefermentPartyDocAddressControl").CaptionResourceString.Caption);
				});
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
