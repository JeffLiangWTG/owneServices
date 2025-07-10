using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	abstract class AUSupplierHeaderUserControlTest : TestCaseWithFactory
	{
		public abstract void TestGetColumnOrderForInvoiceHeaderGrid();

		public abstract void TestDeclarationModes();

		public abstract void TestLockingShipmentData();

		public void TestBaseGroupChargesGridColumns()
		{
			using (var control = new AUSupplierHeaderUserControl())
			{
				AssertNotNull(control.BaseGroupChargesGrid.GetColumnStyle("J7_IsCalculated"));
			}
		}

		public void TestChangeColumnNameForChargeGrid()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertEquals("For import, IsGSTapplicable is relevant", "GST Apply", supplierHeader.BaseGroupChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
				AssertEquals("For import, IsDutiable is relevant", "Dutiable", supplierHeader.BaseGroupChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertEquals("For Export, IsGSTapplicable should read 'Add To CIF'", "Add to CIF?", supplierHeader.BaseGroupChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsGSTApplicable].ColumnStyle.HeaderText);
				AssertEquals("For Export, IsDutiable should read 'Add to FOB'", "Add to FOB?", supplierHeader.BaseGroupChargesGrid.Columns[Common.AutoJobComInvHeaderCharge.Schema.J7_IsDutiable].ColumnStyle.HeaderText);
			}
		}

		public void TestDeclarationModesForIncoTerm()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "IMP";
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertEquals("Inv IncoTerm visible", false, supplierHeader.IncoTermTextBoxInternal.Visible);
				AssertEquals("ITOT incoTerm invisible", true, supplierHeader.ITOTIncoTermTextBox.Visible);
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = "EXP";
				testForm.CustomsBrokerageUserControl.LoadInvoicesTabPage();
				supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertEquals("Inv IncoTerm invisible", true, supplierHeader.IncoTermTextBoxInternal.Visible);
				AssertEquals("ITOT incoTerm visible", false, supplierHeader.ITOTIncoTermTextBox.Visible);
			}
		}

		protected abstract ZString AppCode { get; }

		public void TestLockingGrid()
		{
			JobDeclaration jobDecBizObj = Factory.New<JobDeclaration>();
			jobDecBizObj.JE_ApplicationCode = AppCode;
			jobDecBizObj.SetReadOnlyIncludingChildren(true);
			jobDecBizObj.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(jobDecBizObj))
			{
				const bool ReadOnlyState = true;
				testForm.Show();
				AUDeclarationUserControl decUserControl = testForm.DeclarationUserControl as AUDeclarationUserControl;
				testForm.CustomsBrokerageUserControl.SetDeclarationReadOnly(ReadOnlyState);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUSupplierHeaderUserControl supplierHeaderControl = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertEquals("JobComInvoiceHeadersBoundGrid ReadOnly", ReadOnlyState, supplierHeaderControl.InvoiceHeadersBoundGrid.ReadOnly);
			}
		}

		public void TestJobComInvoiceHeadersGridSkipsGroupInvoiceColumn()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AssertEquals("JobComInvoiceHeadersBoundGrid should has columns to skip", 1, testForm.SupplierUserControl.InvoiceHeadersBoundGrid.ColumnsToSkip.Count);
				testForm.Close();
			}
		}

		public virtual void TestGreyOutFieldsForNature30()
		{
			using (AUCustomsDeclarationFormForTest testForm = new AUCustomsDeclarationFormForTest(Declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				AUSupplierHeaderUserControl supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertWarehouseVisibility(supplierHeader, false);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.InvoicesTabPage;
				supplierHeader = testForm.SupplierUserControl as AUSupplierHeaderUserControl;
				AssertWarehouseVisibility(supplierHeader, true);
			}
		}

		protected abstract ZInt NumberOfGridColumnsForNature20(bool enabled);

		protected abstract JobDeclaration Declaration { get; }

		protected void AssertColumnExists(ZGridColumns columns, string columnName)
		{
			AssertNotNull("Column exists - " + columnName, columns[columnName]);
		}

		void AssertWarehouseVisibility(AUSupplierHeaderUserControl supplierHeader, bool enabled)
		{
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_InvoiceNumberBoundTextBoxInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_InvoiceCurrExRateCalcEditInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_IncoTermBoundDropDownEditInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.IncoTermExplainButtonInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_IncoTermPlaceTextBoxInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.GrossWeightCalcDropEditInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.NetWeightCalcDropEditInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.ITOTIncoTermTextBox.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.LineTotalBoundConvertToLocalCurrencyControlInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_FOBAmountBoundCurrencyControlInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_CIFAmountBoundCurrencyControlInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.JZ_Calc_TNIBoundInvoiceCurrencyControlInternal.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.ChargesGroupBox.Visible);
			AssertEquals("Control Enabled", enabled, supplierHeader.BaseGroupChargesGroupBoxInternal.Visible);
			AssertEquals("Col Count", NumberOfGridColumnsForNature20(enabled), supplierHeader.JobComInvoiceHeadersBoundGrid.InnerGrid.Columns.Count);
		}

		protected sealed class AUCustomsDeclarationFormForTest : ZAUCustomsDeclarationForm
		{
			public AUCustomsDeclarationFormForTest(JobDeclaration jobDeclaration) : base(jobDeclaration)
			{
			}

			public AUCustomsDeclarationFormForTest() : this(null)
			{
			}

			internal BaseCustomsDeclarationUserControl DeclarationUserControl => CustomsBrokerageUserControl.DeclarationUserControlForTesting;

			internal BaseCustomsSupplierHeaderUserControl SupplierUserControl => CustomsBrokerageUserControl.SupplierHeaderUserControl;
		}
	}
}
