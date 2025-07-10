using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals.Testing
{
	class ImportedInvoiceXMLControlTest : TestCaseWithFactory
	{
		public void TestWHTColumnsVisiblity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = false;
			using (var form = new ZForm(transaction))
			{
				var control = new ImportedInvoiceXMLControl();
				control.SetDataBinding(transaction, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
				form.Controls.Add(control);
				form.Show();
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("WithholdingTaxID", false, xmlLinesGrid.Columns.Contains("WithholdingTaxID"));
				AssertEquals("OSWHTAmount", false, xmlLinesGrid.Columns.Contains("OSWHTAmount"));
				AssertEquals("LocalWHTAmount", false, xmlLinesGrid.Columns.Contains("LocalWHTAmount"));
			}

			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
			using (var form = new ZForm(transaction))
			{
				var control = new ImportedInvoiceXMLControl();
				control.SetDataBinding(transaction, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
				form.Controls.Add(control);
				form.Show();
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("WithholdingTaxID", true, xmlLinesGrid.Columns.Contains("WithholdingTaxID"));
				AssertEquals("WithholdingTaxID default visibility", false, xmlLinesGrid.GetColumnStyle("WithholdingTaxID").IsVisible);
				AssertEquals("OSWHTAmount", true, xmlLinesGrid.Columns.Contains("OSWHTAmount"));
				AssertEquals("OSWHTAmount default visibility", false, xmlLinesGrid.GetColumnStyle("OSWHTAmount").IsVisible);
				AssertEquals("LocalWHTAmount", true, xmlLinesGrid.Columns.Contains("LocalWHTAmount"));
				AssertEquals("LocalWHTAmount default visibility", false, xmlLinesGrid.GetColumnStyle("LocalWHTAmount").IsVisible);
			}
		}

		public void TestPlaceOfSupplyVisiblity()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
				using (var form = new ZForm(transaction))
				{
					var control = new ImportedInvoiceXMLControl();
					control.SetDataBinding(transaction, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
					form.Controls.Add(control);
					form.Show();
					var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
					AssertNotNull(xmlLinesGrid.Columns["PlaceOfSupply"]);
					var placeOfSupplyTextBox = form.GetControl<ZTextBox>("placeOfSupplyTextBox");
					Assert(placeOfSupplyTextBox.Visible);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
				using (var form = new ZForm(transaction))
				{
					var control = new ImportedInvoiceXMLControl();
					control.SetDataBinding(transaction, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
					form.Controls.Add(control);
					form.Show();
					var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
					AssertNull(xmlLinesGrid.Columns["PlaceOfSupply"]);
					var placeOfSupplyTextBox = form.GetControl<ZTextBox>("placeOfSupplyTextBox");
					Assert(!placeOfSupplyTextBox.Visible);
				}
			}
		}

		public void TestSubAccountsColumnsVisiblity()
		{
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);

			using (var form = new ZForm(transaction))
			{
				var control = new ImportedInvoiceXMLControl();
				control.SetDataBinding(transaction, "TransactionApprovalRequest.PostingDetails.UniversalTransaction");
				form.Controls.Add(control);
				form.Show();
				var xmlLinesGrid = form.GetControl<ZGrid>("xmlLinesGrid");
				AssertEquals("SubAccounts", true, xmlLinesGrid.Columns.Contains("SubAccounts"));
				AssertEquals("SubAccounts default visibility", false, xmlLinesGrid.GetColumnStyle("SubAccounts").IsVisible);
			}
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;
	}
}
