using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.ARAP.Invoicing.Testing
{
	[TestedType(typeof(TransactionPendingAllocationApprovalBulkForm))]
	class TransactionPendingAllocationApprovalBulkFormBasherTest :
		TransactionApprovalBulkFormBasherTest<TransactionPendingAllocationApprovalBulkForm, TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public void TestImportedXMLTabVisibilityForMultipleRequests()
		{
			var request1 = Factory.New<TransactionPendingAllocationApprovalRequest>();
			var transaction1 = TestObjectCreator.CreateTransactionPendingAllocation("INV1", TestObjectCreator.Creditor1, 100);
			request1.Initialize(transaction1);
			var request2 = Factory.New<TransactionPendingAllocationApprovalRequest>();
			var transaction2 = TestObjectCreator.CreateTransactionPendingAllocation("INV2", TestObjectCreator.Creditor1, 100);
			request2.Initialize(transaction2, "some xml", true);
			var bulk = new TransactionPendingAllocationApprovalBulk(Factory, null, new TransactionPendingAllocationApprovalRequest[] { request1, request2 });
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();

				var topGrid = form.GetControl<ZGrid>("TopGrid");
				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");

				var expectedTabIndex = 0;
				topGrid.CurrentRowIndex = 0;
				Assert("importedXMLTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
				AssertEquals("Selected tab index", expectedTabIndex, xmlTabControl.SelectedIndex);
				topGrid.CurrentRowIndex = 1;
				Assert("importedXMLTabPage.TabVisible", xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
				AssertEquals("Selected tab index", expectedTabIndex, xmlTabControl.SelectedIndex);

				expectedTabIndex = 1;
				xmlTabControl.SelectedIndex = expectedTabIndex;
				AssertEquals("Precondition: Selected tab index", expectedTabIndex, xmlTabControl.SelectedIndex);
				topGrid.CurrentRowIndex = 0;
				Assert("importedXMLTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
				AssertEquals("Selected tab index", 0, xmlTabControl.SelectedIndex);
				topGrid.CurrentRowIndex = 1;
				Assert("importedXMLTabPage.TabVisible", xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
				AssertEquals("Selected tab index: importedXMLTabPage should be activated again for better user experience", expectedTabIndex, xmlTabControl.SelectedIndex);
			}
		}

		public void TestImportedXMLTabVisibilityForSingleRequest()
		{
			var request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			var bulk = new TransactionPendingAllocationApprovalBulk(Factory, null, new TransactionPendingAllocationApprovalRequest[] { request });
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");
				Assert("importedXMLTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
			}

			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			request.Initialize(transaction);
			Assert("Precondition: HasUniversalTransaction", !transaction.IsImportedFromUniversalXML);
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");
				Assert("importedXMLTabPage.TabVisible", !xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
			}

			request.Initialize(transaction, "some xml", true);
			Assert("Precondition: HasUniversalTransaction", transaction.IsImportedFromUniversalXML);
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();

				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");
				Assert("importedXMLTabPage.TabVisible", xmlTabControl.TabPages.ContainsKey("importedXMLTabPage"));
				var sourceXmlTabPage = form.GetControl<ZTabPage>("importedXMLTabPage");
				Assert("importedXMLTabPage", sourceXmlTabPage.TabVisible);
			}
		}

		public void TestHideControls()
		{
			TransactionPendingAllocationApprovalRequest request = Factory.New<TransactionPendingAllocationApprovalRequest>();
			var transaction = TestObjectCreator.CreateTransactionPendingAllocation("INV", TestObjectCreator.Creditor1, 100);
			request.Initialize(transaction, "some xml", true);
			var bulk = new TransactionPendingAllocationApprovalBulk(Factory, null, new TransactionPendingAllocationApprovalRequest[] { request });
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();
				var numberOfSupportingDocumentsCalcEdit = form.GetControl<ZCalcEdit>("AH_NumberOfSupportingDocumentsCalcEdit"); // inside test case
				Assert("Non China", !numberOfSupportingDocumentsCalcEdit.Visible);

				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");
				xmlTabControl.SelectTab("importedXMLTabPage");
				numberOfSupportingDocumentsCalcEdit = xmlTabControl.GetControl<ZCalcEdit>("xmlNumberOfDocsCalcEdit"); // inside test case
				Assert("Non China", !numberOfSupportingDocumentsCalcEdit.Visible);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			using (var form = new TransactionPendingAllocationApprovalBulkForm(bulk, TransactionApprovalFormModes.View))
			{
				form.Show();
				var numberOfSupportingDocumentsCalcEdit = form.GetControl<ZCalcEdit>("AH_NumberOfSupportingDocumentsCalcEdit"); // inside test case
				Assert("China", numberOfSupportingDocumentsCalcEdit.Visible);

				var xmlTabControl = form.GetControl<ZTabControl>("detailsTabControl");
				xmlTabControl.SelectTab("importedXMLTabPage");
				numberOfSupportingDocumentsCalcEdit = xmlTabControl.GetControl<ZCalcEdit>("xmlNumberOfDocsCalcEdit"); // inside test case
				Assert("China", numberOfSupportingDocumentsCalcEdit.Visible);
			}
		}

		public void TestPlaceOfSupplyTextBoxVisiblity()
		{
			using (PlaceOfSupplyHelper.SetPOSTypesEnabled_ForTestOnly(PlaceOfSupplyTypes.State.Code, PlaceOfSupplyTypes.PredefinedRule.Code))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var placeOfSupplyTextBox = testForm.GetControl<ZTextBox>("placeOfSupplyTextBox");
					Assert(placeOfSupplyTextBox.Visible);
				}
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				using (var testForm = GetFormToBash())
				{
					testForm.Show();
					var placeOfSupplyTextBox = testForm.GetControl<ZTextBox>("placeOfSupplyTextBox");
					Assert(!placeOfSupplyTextBox.Visible);
				}
			}
		}

		protected override TransactionPendingAllocationApprovalRequest GetNewApprovalRequest()
		{
			return Factory.New<TransactionPendingAllocationApprovalRequest>();
		}

		protected override TransactionPendingAllocationApprovalBulkForm GetRequestForm(params TransactionPendingAllocationApprovalRequest[] bizos)
		{
			return new TransactionPendingAllocationApprovalBulkForm(new TransactionPendingAllocationApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}
	}

	[TestedType(typeof(TransactionPendingAllocationApprovalBulkForm))]
	class TransactionPendingAllocationApprovalBulkFormWithMultipleRequestsBasherTest :
		TransactionApprovalBulkFormWithMultipleRequestsBasherTest<TransactionPendingAllocationApprovalBulkForm, TransactionPendingAllocation, TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		protected override TransactionPendingAllocationApprovalRequest GetNewApprovalRequest(bool isValidParent)
		{
			return Factory.New<TransactionPendingAllocationApprovalRequest>();
		}

		protected override TransactionPendingAllocationApprovalBulkForm GetRequestForm(params TransactionPendingAllocationApprovalRequest[] bizos)
		{
			return new TransactionPendingAllocationApprovalBulkForm(new TransactionPendingAllocationApprovalBulk(Factory, new InteractiveSecurityOverrideProvider(), bizos), ApprovalFormMode);
		}

		protected override TransactionApprovalFormModes[] ModesWhenReasonDescriptionShouldNotBeReadOnly
		{
			get { return new TransactionApprovalFormModes[] { TransactionApprovalFormModes.SetDescription, TransactionApprovalFormModes.Reject }; }
		}
	}
}
