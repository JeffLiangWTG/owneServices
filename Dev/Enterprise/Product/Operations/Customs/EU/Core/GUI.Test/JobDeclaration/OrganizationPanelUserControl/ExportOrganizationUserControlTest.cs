using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public class ExportOrganizationUserControlTest : TestCaseWithFactory
	{
		public void TestControlVisibility()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ManufacturerAddressControl", true, control.FindSingle<Control>("ManufacturerAddressControl").Visible);
					AssertEquals("RepresentativeAddressControl", true, control.FindSingle<Control>("RepresentativeAddressControl").Visible);
					AssertEquals("DeclarantOfficeAddressControl", true, control.FindSingle<Control>("DeclarantOfficeAddressControl").Visible);
					AssertEquals("SellerAddressControl", true, control.FindSingle<Control>("SellerAddressControl").Visible);
				});
			}
		}

		public void TestControlNames()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("ManufacturerAddressControl", "Manufacturer", control.FindSingle<ZAddressControl>("ManufacturerAddressControl").CaptionResourceString.Caption);
					AssertEquals("RepresentativeAddressControl", "Representative", control.FindSingle<ZAddressControl>("RepresentativeAddressControl").CaptionResourceString.Caption);
					AssertEquals("DeclarantOfficeAddressControl", "[14] Declarant", control.FindSingle<ZAddressControl>("DeclarantOfficeAddressControl").CaptionResourceString.Caption);
					AssertEquals("SellerAddressControl", "Seller", control.FindSingle<ZAddressControl>("SellerAddressControl").CaptionResourceString.Caption);
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
