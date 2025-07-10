using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUOtherSupplierHeaderUserControlTest : AUSupplierHeaderUserControlTest
	{
		public void TestGridId()
		{
			using (var control = new AUOtherSupplierHeaderUserControl())
			{
				AssertEquals("GridLayoutZqATrs1wYGJY91xTMxFaKQ==", control.JobComInvoiceHeadersBoundGrid.InnerGrid.GridId);
			}
		}

		public void TestInvoiceHeaderGridContext()
		{
			using (AUOtherSupplierHeaderUserControl control = new AUOtherSupplierHeaderUserControl())
			{
				AssertEquals("Context is set", nameof(DeclarationType.EdificeImport), control.InvoiceHeadersBoundGrid.ColumnLayoutContext);
			}
		}

		public void TestSupplierNoVisibleForExport()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertNull("Supplier should NOT exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Supplier]);
				AssertNull("Supplier Address should NOT exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress]);
			}

			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.MiscellaneousCustoms;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				var supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertNotNull("Supplier should exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OH_Supplier]);
				AssertNotNull("Supplier Address should exist", supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns[Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_OA_SupplierAddress]);
			}
		}

		public override void TestGetColumnOrderForInvoiceHeaderGrid()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUOtherSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertEquals("Column Order array list has 50 elements", 50, supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Count);
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
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_PiecesForRelease);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_Nature10PackCount);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_PiecesToBond);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, JobComInvoiceHeader.Schema.JZ_BondPackCount);
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
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, "AddInfo+ZA_PermitNumbers_Hidden");
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_Volume);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_VolumeUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_Weight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_WeightUQ);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_NetWeight);
				AssertColumnExists(supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns, Customs.Business.AutoJobComInvoiceHeader.Schema.JZ_NetWeightUQ);
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
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUOtherSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertEquals("JZ_ValuationBasesBoundDropDownEdit Visible", false, supplierHeader.JZ_ValuationBasisBoundDropDownEdit.Visible);
				AssertEquals("JZ_PreferenceBoundDropDownEdit Visible", false, supplierHeader.jZ_PreferenceBoundDropDownEdit.Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", false, supplierHeader.gSTEDropEdit.Visible);
				AssertEquals("JZ_AddInfoBoundTextBox Visible", false, supplierHeader.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("Origin FindBox Visible", false, supplierHeader.InvoiceOriginCodeFindBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				Declaration.JE_ApplicationCode = "LEG";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertEquals("JZ_AddInfoBoundTextBox Visible", true, supplierHeader.JZ_AddInfoBoundAddInfoControl.Visible);
				AssertEquals("JZ_ValuationBasesBoundDropDownEdit Visible", true, supplierHeader.JZ_ValuationBasisBoundDropDownEdit.Visible);
				AssertEquals("JZ_PreferenceBoundDropDownEdit Visible", true, supplierHeader.jZ_PreferenceBoundDropDownEdit.Visible);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox Visible", true, supplierHeader.gSTEDropEdit.Visible);
				AssertEquals("Origin Visible", true, supplierHeader.InvoiceOriginCodeFindBox.Visible);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				AssertNoExceptionThrown(() => Declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import);
			}
		}

		public override void TestLockingShipmentData()
		{
			JobDeclaration jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.JE_ApplicationCode = "LEG";
			jobDecBizObj.SetReadOnlyIncludingChildren(true);
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUOtherSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				testForm.CustomsBrokerageUserControl.SetDeclarationReadOnly(true);
				AssertEquals("JZ_AddInfoBoundTextBox ReadOnly", true, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.ReadOnly);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox ReadOnly", true, supplierHeaderControl.gSTEDropEdit.ReadOnly);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", true, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_PreferenceBoundDropDownEdit ReadOnly", true, supplierHeaderControl.jZ_PreferenceBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", true, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit ReadOnly", true, supplierHeaderControl.JZ_ValuationBasisBoundDropDownEdit.ReadOnly);
			}

			jobDecBizObj = Declaration;
			jobDecBizObj.Invoices.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				testForm.Show();
				UserIdleWorker.Flush();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUOtherSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				AssertEquals("JZ_AddInfoBoundTextBox ReadOnly", false, supplierHeaderControl.JZ_AddInfoBoundAddInfoControl.ReadOnly);
				AssertEquals("JZ_Calc_GSTExemptBoundTextBox ReadOnly", false, supplierHeaderControl.gSTEDropEdit.ReadOnly);
				AssertEquals("JZ_InvoiceNumberBoundTextBox ReadOnly", false, supplierHeaderControl.InvoiceNumberBoundTextBox.ReadOnly);
				AssertEquals("JZ_PreferenceBoundDropDownEdit ReadOnly", false, supplierHeaderControl.jZ_PreferenceBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_IncoTermBoundDropDownEdit ReadOnly", false, supplierHeaderControl.IncoTermBoundDropDownEdit.ReadOnly);
				AssertEquals("JZ_ValuationBasisBoundDropDownEdit ReadOnly", false, supplierHeaderControl.JZ_ValuationBasisBoundDropDownEdit.ReadOnly);
			}
		}

		public override void TestGreyOutFieldsForNature30()
		{
			base.TestGreyOutFieldsForNature30();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUOtherSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertEquals("Control Enabled", false, supplierHeader.addInfoPermitNumberBoundTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.LoadInvoicesTabPage();
				supplierHeader = testForm.SupplierUserControl as AUOtherSupplierHeaderUserControl;
				AssertEquals("Control Enabled", true, supplierHeader.addInfoPermitNumberBoundTextBox.Visible);
			}
		}

		protected override ZInt NumberOfGridColumnsForNature20(bool enabled) => enabled ? 50 : 25;

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
