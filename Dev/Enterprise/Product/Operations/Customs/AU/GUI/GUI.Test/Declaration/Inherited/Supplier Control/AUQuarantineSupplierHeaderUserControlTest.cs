using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUQuarantineSupplierHeaderUserControlTest : AUSupplierHeaderUserControlTest
	{
		public void TestGridId()
		{
			using (var control = new AUQuarantineSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutl4kHxyoLJZjbddFGTuFelw==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestInvoiceHeaderGridContext()
		{
			using (AUQuarantineSupplierHeaderUserControl control = new AUQuarantineSupplierHeaderUserControl())
			{
				AssertEquals("Context is set", nameof(DeclarationType.Quarantine), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestBottomPanelCannotBeReducedOffForm()
		{
			using (AUQuarantineSupplierHeaderUserControl control = new AUQuarantineSupplierHeaderUserControl())
			{
				Assert("BottomPanel cannot be minimized off the form", control.FindSingle<SplitContainer>(x => x.Name == "Splitter").Panel2MinSize == 330);
				Assert("BottomPanel cannot completely cover the invoices grid", control.BottomPanel.MaximumSize.Height < control.Size.Height);
			}
		}

		public override void TestGetColumnOrderForInvoiceHeaderGrid()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUQuarantineSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUQuarantineSupplierHeaderUserControl;
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceNumber);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Supplier);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.SupplierName);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_IncoTerm);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_IncoTermPlace);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_RX_NKInvoice_Currency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_BalanceString);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_ORG);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_PRF);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Buyer);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_ValuationBasis);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_FOBCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.BaseJobComInvoiceHeader.Schema.JZ_Calc_CIFCurrency);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.ZA_GSTE);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_PaymentNo);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_PaymentAmount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_PaymentDate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_PaymentExRate);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_Volume);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_VolumeUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_Weight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_WeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_NetWeight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_NetWeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_CU_RelatedHouseBill);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_ValuationDateOverride);
				AssertEquals("Inco Term should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_IncoTerm].IsVisible);
				AssertEquals("Agreed Place should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_IncoTermPlace].IsVisible);
				AssertEquals("InvoiceLineTotal should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.BaseJobComInvoiceHeader.Schema.InvoiceLineTotal].IsVisible);
				AssertEquals("Buyer should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Buyer].IsVisible);
				AssertEquals("Invoice Curr Ex Rate should be visible", true, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_InvoiceCurrExRate].IsVisible);
				AssertEquals("Payment date is not visible", false, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_PaymentDate].IsVisible);
			}
		}

		public override void TestDeclarationModes()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUQuarantineSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUQuarantineSupplierHeaderUserControl;
				AssertEquals("JZ_ValuationBasesBoundDropDownEdit Visible", false, supplierHeader.JZ_ValuationBasisBoundDropDownEdit.Visible);
				AssertEquals("JZ_PreferenceBoundDropDownEdit Visible", false, supplierHeader.Controls.Find("JZ_PreferenceBoundDropDownEdit", true).First().Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", false, supplierHeader.Controls.Find("GSTEDropEdit", true).First().Visible);
				AssertEquals("JZ_AddInfoBoundTextBox Visible", false, supplierHeader.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("Origin FindBox Visible", false, supplierHeader.InvoiceOriginCodeFindBox.Visible);
			}
		}

		public override void TestLockingShipmentData()
		{
			JobDeclaration jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.JE_ApplicationCode = "LEG";
			jobDecBizObj.ReadOnly = true;
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUQuarantineSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUQuarantineSupplierHeaderUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				testForm.CustomsBrokerageUserControl.SetDeclarationReadOnly(true);
				AssertEquals("JZ_AddInfoBoundTextBox Visible", false, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", false, supplierHeaderControl.Controls.Find("GSTEDropEdit", true).First().Visible);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", true, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_PreferenceBoundDropDownEdit Visible", false, supplierHeaderControl.Controls.Find("JZ_PreferenceBoundDropDownEdit", true).First().Visible);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", true, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit Visible", false, supplierHeaderControl.JZ_ValuationBasisBoundDropDownEdit.Visible);
			}

			jobDecBizObj = Declaration;
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.AU.AUJobMessageTypeList.Codes.Quarantine;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUQuarantineSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUQuarantineSupplierHeaderUserControl;
				UserIdleWorker.Flush();
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				AssertEquals("JZ_AddInfoBoundTextBox Visible", false, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", false, supplierHeaderControl.Controls.Find("GSTEDropEdit", true).First().Visible);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", false, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_PreferenceBoundDropDownEdit Visible", false, supplierHeaderControl.Controls.Find("JZ_PreferenceBoundDropDownEdit", true).First().Visible);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", false, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit Visible", false, supplierHeaderControl.JZ_ValuationBasisBoundDropDownEdit.Visible);
			}
		}

		public override void TestGreyOutFieldsForNature30()
		{
			Assert("This is for export only there is no N30 information", true);
		}

		protected override ZInt NumberOfGridColumnsForNature20(bool enabled) => enabled ? 33 : 9;

		protected override JobDeclaration Declaration => declaration;

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "LEG";
		}

		protected override ZString AppCode => "LEG";
	}
}
