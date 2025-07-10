using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class GACUserControlTestCase : TestCaseWithFactory
	{
		public void TestAvailableLPCOGridFields()
		{
			using (var filterControl = new GACUserControl())
			{
				filterControl.Show();
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_CommodityTypeCode).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactEmail).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactName).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderContactPhone).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_DIFRefNumberOrLocation).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_ApplicantType).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.LPCOApplicantOrgPK).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_ApplicantName).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_OA_Applicant).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderType).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_HolderName).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_OA_Holder).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.LPCOHolderOrgPK).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_IssueDate).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_StartDate).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_ApplicantContactPhone).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_AlternativeQuotaQuantity).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_SecondaryRefNo).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_Type).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_AlternativeQuotaUQ).IsUnavailable);
				Assert(!filterControl.LPCOGridUserControl.LpcoGrid.GetColumnStyle(CusCALPCO.Schema.CLP_RN_NKSmeltAndPourCountryCode).IsUnavailable);
			}
		}

		public void TestIsVisibleForBindingStringOnTPLPermitLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CA_PermitApplication = false;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			var pgaHeader = invoiceLine.GACPGAHeader;
			pgaHeader.CA_AllProgramInd = Customs.Business.YesNoList.Codes.Yes;
			pgaHeader.CA_CommodityCode = string.Empty;

			invoiceLine.JI_Tariff = "50000012";

			pgaHeader.CA_FTACode = FTAProcessingCodes.Codes.FA01;
			AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);
			AssertEquals(true, pgaHeader.IsFTAProcessingCodeFA01);

			using (var form = new ZForm())
			using (var control = new GACUserControl())
			{
				control.SetDataBinding(pgaHeader, "");
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();

				Assert("ClothingAndTextileDetailsGroup should be visible", control.ClothingAndTextileDetailsGroup.Visible);
				Assert("TPLPermitLabel should be visible", control.TPLPermitLabel.Visible);

				pgaHeader.CA_FTACode = FTAProcessingCodes.Codes.FA02;
				AssertEquals(true, pgaHeader.AreClothingAndTextileDetailsVisibility);
				AssertEquals(false, pgaHeader.IsFTAProcessingCodeFA01);
				Assert("ClothingAndTextileDetailsGroup should be visible", control.ClothingAndTextileDetailsGroup.Visible);
				Assert("TPLPermitLabel should NOT be visible", !control.TPLPermitLabel.Visible);
			}
		}

		public void TestAutoScroll()
		{
			using (var filterControl = new GACUserControl())
			{
				filterControl.Show();
				Assert(filterControl.AutoScroll);
				AssertEquals(525, filterControl.AutoScrollMinSize.Height);
				AssertEquals(1080, filterControl.AutoScrollMinSize.Width);
			}
		}

		public void TestPanel1MinSize()
		{
			using (var filterControl = new GACUserControl())
			{
				filterControl.Show();
				var splitContainer = filterControl.Controls.Find("GACSplitContainer", true)[0] as CargoWise.Windows.UI.KSplitContainer;
				AssertEquals(175, splitContainer.Panel1MinSize);
			}
		}

		public void TestDefaultDataSourceBindingMemberAttributeAndDefaultBindingPropertyAttribute()
		{
			var defaultDataSourceBindingMember = typeof(GACUserControl).GetCustomAttributes(typeof(DefaultDataSourceBindingMemberAttribute), false).First();
			AssertNotNull(defaultDataSourceBindingMember);
			AssertNull(((DefaultDataSourceBindingMemberAttribute)defaultDataSourceBindingMember).DefaultBindingMember);

			var defaultBindingProperty = typeof(GACUserControl).GetCustomAttributes(typeof(DefaultBindingPropertyAttribute), false).First();
			AssertNotNull(defaultBindingProperty);
			AssertEquals("GACPGAHeader", ((DefaultBindingPropertyAttribute)defaultBindingProperty).Name);
		}
	}
}
