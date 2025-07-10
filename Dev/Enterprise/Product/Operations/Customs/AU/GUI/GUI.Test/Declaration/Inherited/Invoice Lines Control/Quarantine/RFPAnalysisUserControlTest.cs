using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.GUI;
using Enterprise.Customs.AU.Declaration.GUI.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI.Testing
{
	sealed class RFPAnalysisUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibilityWhenQH_ProductTypeChanged()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, false))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				var invoice = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
				using (var testForm = new TestAUCustomsDeclarationForm(declaration))
				{
					testForm.Show();
					testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
					var quarantineInvoiceLineUserControl = testForm.InvoiceControl as AUQuarantineInvoiceLineUserControl;
					quarantineInvoiceLineUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceLineUserControl.FindSingle<ZTabPage>("RFPAnalysisTabPage");

					AssertEquals("OtherGroupBox.Visible", false, quarantineInvoiceLineUserControl.FindSingle<ZGroupBox>("OtherGroupBox").Visible);
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Dairy;
					AssertEquals("OtherGroupBox.Visible", true, quarantineInvoiceLineUserControl.FindSingle<ZGroupBox>("OtherGroupBox").Visible);

					AssertEquals("EggGroupBox.Visible", false, quarantineInvoiceLineUserControl.FindSingle<ZGroupBox>("EggGroupBox").Visible);
					invoice.QuarantineExDocHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.Eggs;
					AssertEquals("EggGroupBox.Visible", true, quarantineInvoiceLineUserControl.FindSingle<ZGroupBox>("EggGroupBox").Visible);
				}
			}
		}

		public void TestControlsBinding()
		{
			using (var testControl = new AUQuarantineInvoiceLineUserControl())
			{
				AssertEquals("QL_FishWaterIndicatorDropEdit.BindingMember", "QuarantineExDocLine.QL_FishWaterIndicator", testControl.FindSingle<ZDropEdit>("QL_FishWaterIndicatorDropEdit").GetBindingMember());
			}
		}

		public void TestUpdateDairyFieldsIfRequired_WithExDocMessage()
		{
			var dec = CreateQuarantineDecWithMessage(EDIInterchange.ApplicationCodes.EXDOC);
			AssertDairyControlsVisibility(null, dec, true);
		}

		public void TestUpdateDairyFieldsIfRequired_WithoutExDocMessage()
		{
			var dec = CreateQuarantineDecWithMessage(EDIInterchange.ApplicationCodes.CAACI);
			AssertDairyControlsVisibility(null, dec, false);
		}

		void AssertDairyControlsVisibility(string message, JobDeclaration declaration, bool shouldBeVisible)
		{
			using (var testForm = new TestAUCustomsDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var quarantineInvoiceUserControl = (AUQuarantineInvoiceLineUserControl)testForm.InvoiceControl;
				quarantineInvoiceUserControl.LineDetailTabControl.SelectedTab = quarantineInvoiceUserControl.FindSingle<ZTabPage>("RFPAnalysisTabPage");
				CombineAssertions(message, () =>
				{
					AssertEquals("QL_PercentOfMilkProteinCalcEdit.Visible", shouldBeVisible, quarantineInvoiceUserControl.FindSingle<ZCalcEdit>("QL_PercentOfMilkProteinCalcEdit").Visible);
					AssertEquals("QL_TotalWeightOfMilkProteinInMixturesCalcEdit.Visible", shouldBeVisible, quarantineInvoiceUserControl.FindSingle<ZCalcEdit>("QL_TotalWeightOfMilkProteinInMixturesCalcEdit").Visible);
					AssertEquals("QL_PercentOfMilkFatCalcEdit.Visible", shouldBeVisible, quarantineInvoiceUserControl.FindSingle<ZCalcEdit>("QL_PercentOfMilkFatCalcEdit").Visible);
					AssertEquals("QL_TotalWeightOfMilkFatInMixturesCalcEdit.Visible", shouldBeVisible, quarantineInvoiceUserControl.FindSingle<ZCalcEdit>("QL_TotalWeightOfMilkFatInMixturesCalcEdit").Visible);
				});
			}
		}

		JobDeclaration CreateQuarantineDecWithMessage(string messageApplicationCode)
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
			var inv = dec.Invoices.AddNew();
			var quarantineInv = inv.QuarantineExDocHeader;
			var msg = quarantineInv.Messages.AddNew();
			msg.EM_ApplicationCode = messageApplicationCode;
			return dec;
		}
	}
}
