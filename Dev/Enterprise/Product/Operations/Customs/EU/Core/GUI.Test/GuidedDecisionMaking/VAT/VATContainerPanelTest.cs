using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class VATContainerPanelTest : TestCaseWithFactory
	{
		public void TestPopulateVATDetailControls_NotInSummary()
		{
			var gdmBasic = SetUpGDMBasicWithVATForTest(Factory);
			using (var control = new VATContainerPanel())
			{
				control.PopulateVATControl(gdmBasic);
				var vatControls = control.FindAll<VATControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("When not in summary, one VATControl should be generated.", 1, vatControls.Length);
				AssertEquals("When not in summary, VATControlGroupBox.Text should be empty.", ZString.Empty, (vatControls.First().Controls.Find("VATControlGroupBox", true).Single() as ZGroupBox).Text);

				var vatDetailControls = control.FindAll<VATDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("When not in summary, VATDetailControls are generated for all VATs.", 3, vatDetailControls.Length);
			}
		}

		public void TestPopulateVATDetailControls_InSummary()
		{
			var gdmBasic = SetUpGDMBasicWithVATForTest(Factory);

			using (var control = new VATContainerPanel())
			{
				control.InSummary = true;
				control.PopulateVATControl(gdmBasic);
				var vatControls = control.FindAll<VATControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("When in summary, No VATControls should be populated as no VAT is ticked.", 0, vatControls.Length);
			}

			gdmBasic.VATApplicabilities[0].IsTicked = true;
			gdmBasic.VATApplicabilities[2].IsTicked = true;
			using (var control = new VATContainerPanel())
			{
				control.InSummary = true;
				control.PopulateVATControl(gdmBasic);
				var vatControls = control.FindAll<VATControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("When in summary, one VATControl should be populated as there is one ticked VAT.", 1, vatControls.Length);
				AssertEquals("When in summary, VATControlGroupBox.Text should be VAT.", "VAT", (vatControls.First().Controls.Find("VATControlGroupBox", true).Single() as ZGroupBox).Text);

				var vatDetailControls = control.FindAll<VATDetailControl>().OrderBy(c => c.Top).ToArray();
				AssertEquals("When in summary, VATDetailControls are generated for ticked VATs only.", 2, vatDetailControls.Length);
			}
		}

		internal static GuidedDecisionMakingBasic SetUpGDMBasicWithVATForTest(BusinessObjectFactory factory, GuidedDecisionMakingBasic originalGDMBasic = null)
		{
			var gdmBasic = originalGDMBasic ?? GuidedDecisionMakingTestHelper.CreateGuidedDecisionMakingBasicForTest(factory, false, false);

			var vat1 = gdmBasic.VATApplicabilities.AddNew();
			vat1.AdditionalCode = "VAT1";
			vat1.AdditionalCodeDescription = "VAT1 Desc";
			vat1.Category = "A001";
			vat1.VATCode = "RED";
			vat1.Description = "RED DESC";
			vat1.VATRateValue = 0.033m;
			vat1.IsTicked = false;
			var vat2 = gdmBasic.VATApplicabilities.AddNew();
			vat2.AdditionalCode = "VAT2";
			vat2.AdditionalCodeDescription = "VAT2 Desc";
			vat2.Category = "A001";
			vat2.VATCode = "STD";
			vat2.Description = "STD DESC";
			vat2.VATRateValue = 0.055m;
			vat2.IsTicked = false;
			var vat3 = gdmBasic.VATApplicabilities.AddNew();
			vat3.AdditionalCode = "VAT3";
			vat3.AdditionalCodeDescription = "VAT3 Desc";
			vat3.Category = "A005";
			vat3.VATCode = "STD";
			vat3.Description = "STD DESC";
			vat3.VATRateValue = 0.088m;
			vat3.IsTicked = false;
			return gdmBasic;
		}
	}
}
