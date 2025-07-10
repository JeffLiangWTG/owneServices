using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
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
					AssertEquals("SupervisingOfficeDocAddress", true, control.FindSingle<Control>("SupervisingOfficeDocAddress").Visible);
					AssertEquals("SpoffSetterButton", true, control.FindSingle<Control>("SpoffSetterButton").Visible);
				});
			}
		}

		public void TestSpoffSetterButtonHooked()
		{
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				AssertEquals("SpoffSetterButton", true, control.FindSingle<ZButton>("SpoffSetterButton").ClickHasBeenHooked);
			}
		}

		public void TestControlsNames_Chief()
		{
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("DeclarantOfficeAddressControl", "[14] Declarant", control.FindSingle<Control>("DeclarantOfficeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SupervisingOfficeDocAddress", "[44] Supervising Office", control.FindSingle<Control>("SupervisingOfficeDocAddress").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("RepresentativeAddressControl", "Representative", control.FindSingle<Control>("RepresentativeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SellerAddressControl", "Seller", control.FindSingle<Control>("SellerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("ManufacturerAddressControl", "Manufacturer", control.FindSingle<Control>("ManufacturerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
				});

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
				control.SetCaptions();
				CombineAssertions(() =>
				{
					AssertEquals("DeclarantOfficeAddressControl", "[UCC 3/18] Declarant", control.FindSingle<Control>("DeclarantOfficeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SupervisingOfficeDocAddress", "[UCC 5/27] Supervising Office", control.FindSingle<Control>("SupervisingOfficeDocAddress").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("RepresentativeAddressControl", "[UCC 3/20] Representative", control.FindSingle<Control>("RepresentativeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SellerAddressControl", "[UCC 3/24] Seller", control.FindSingle<Control>("SellerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("ManufacturerAddressControl", "[UCC 3/37] Manufacturer", control.FindSingle<Control>("ManufacturerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
				});
			}
		}

		public void TestControlNames_CDS()
		{
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			using (var control = new ExportOrganizationUserControl())
			{
				control.SetDataBinding(declaration, "");
				CombineAssertions(() =>
				{
					AssertEquals("DeclarantOfficeAddressControl", "[UCC 3/18] Declarant", control.FindSingle<Control>("DeclarantOfficeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SupervisingOfficeDocAddress", "[UCC 5/27] Supervising Office", control.FindSingle<Control>("SupervisingOfficeDocAddress").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("RepresentativeAddressControl", "[UCC 3/20] Representative", control.FindSingle<Control>("RepresentativeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SellerAddressControl", "[UCC 3/24] Seller", control.FindSingle<Control>("SellerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("ManufacturerAddressControl", "[UCC 3/37] Manufacturer", control.FindSingle<Control>("ManufacturerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("ConsignorAddressControl", "[UCC 3/7] Consignor", control.FindSingle<Control>("ConsignorAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
				});

				declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
				control.SetCaptions();

				CombineAssertions(() =>
				{
					AssertEquals("DeclarantOfficeAddressControl", "[14] Declarant", control.FindSingle<Control>("DeclarantOfficeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SupervisingOfficeDocAddress", "[44] Supervising Office", control.FindSingle<Control>("SupervisingOfficeDocAddress").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("RepresentativeAddressControl", "Representative", control.FindSingle<Control>("RepresentativeAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("SellerAddressControl", "Seller", control.FindSingle<Control>("SellerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
					AssertEquals("ManufacturerAddressControl", "Manufacturer", control.FindSingle<Control>("ManufacturerAddressControl").GetExtension<ILabelCaptionRenderer>().Caption);
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
