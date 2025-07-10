using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Statement = Enterprise.Accounting.Business.ARAP.Invoicing.Statement;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AREnquiryModule))]
	class AREnquiryModuleTest : ARTransactionModuleStripTest
	{
		#region EInvoicing Requeue test cases

		public override void TestResetStatusToQueuedMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			var registryItem = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality;
			Assert("Registry is off by default", !registryItem.Value);

			var currCompany = GlbCompany.CurrentCompany;
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				foreach (bool funcEnabled in new[] { false, true })
				{
					using (currCompany.TemporarilySetCountry(country))
					using (registryItem.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, funcEnabled))
					using (var testModule = (AREnquiryModule)ZModuleFactory.Instance.Create(ModuleID))
					{
						AssertEquals("Registry is on/off", funcEnabled, registryItem.Value);
						var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
						AssertNull("'Reset Status to Queued' Menu Item should not be visibile in AR Enquiry module", resetStatusToQueuedMenuItem);
					}
				}
			}
		}

		public override void TestResetStatusToQueuedMenuItem_IsOnlyAvailableOnForCountriesSupportingEInvoicing()
		{
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var testModule = (AREnquiryModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
					AssertNull("AR Enquiry module should not have 'Reset Status to Queued' Menu Item for any country", resetStatusToQueuedMenuItem);
				}
			}
		}

		public override void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory()
		{
			Assert("AR Enquiry module does not have 'Reset Status to Queued' Menu Item", true);
		}

		public override void AssertResetStatusToDeliveredMenuItem(MenuItem menuItem)
		{
			AssertNull("AR Enquiry module does not have 'Reset Status to Delivered' Menu Item", menuItem);
		}

		public override void TestClickResetStatusToDelivered()
		{
			Assert("AR Enquiry module does not have 'Reset Status to Delivered' Menu Item", true);
		}

		#endregion

		public override void TestSignElectronicInvoiceActionMenuItemVisibility()
		{
			var countriesQuery = new ZQuery();
			countriesQuery.OrderBy = RefCountrySchema.Constants.RN_Code;
			var countries = Factory.Load<RefCountry>(countriesQuery);

			foreach (var country in countries)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country.Code))
				{
					var actionMenuItems = TestTransactionModule.GetNewActionMenuItems_ForTestOnly();
					var menuItem = actionMenuItems.FindByText("Sign Electronic Invoice");
					AssertNull("Sign Electronic Invoice menu item only exists on AR Transaction module", menuItem);
				}
			}
		}

		public void TestCommandsRunOnPerformSearch()
		{
			TestTransactionModule.Dispose();
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			for (int index = 0; index < 100; index++)
			{
				creator.CreateInvoiceWithLine(typeof(ARInvoice), index.ToString(), creator.AUD, 1m, 100m, 0, 100m, 0m, creator.AALSHI, creator.CC1.PK);
			}
			factory.Save();

			foreach (bool isWarmUp in new[] { true, false })
			{
				using (var testModule = (AREnquiryModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					AssertRecordsLoaded(isWarmUp, creator.AALSHI.PK, testModule);
				}
			}
		}

		void AssertRecordsLoaded(bool isWarmUp, ZGuid organisationPK, AREnquiryModule testModule)
		{
			var orgFilter = (ModuleGuidFilter)((AREnquiryFilterBusinessObject)testModule.FilterBusinessObject)["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = organisationPK;

			if (!isWarmUp)
			{
				AssertEquals("No records shown in the grid yet.", 0, testModule.GridCollection.Count);
			}

			var countbefore = Db.Connection.ExecutedCommandCount;
			testModule.PerformSearch_ForTest();
			var countafter = Db.Connection.ExecutedCommandCount;

			if (!isWarmUp)
			{
				var executedCommandCount = countafter - countbefore;
				var isExecutedCommandCountWithinExpectedRange = executedCommandCount > 0 && executedCommandCount < 15;
				Assert($"Command count should be greater than 0 and less than 15, but command count was {executedCommandCount}", isExecutedCommandCountWithinExpectedRange);
				AssertEquals("Records shown in the grid.", 100, testModule.GridCollection.Count);
			}
		}

		public void TestAH_OH_EventHandler()
		{
			using (AREnquiryModule testMod = (AREnquiryModule)ZModuleFactory.Instance.Create(ModuleIDs.AREnquiry))
			{
				AREnquiryFilterBusinessObject filterBizO = (AREnquiryFilterBusinessObject)testMod.FilterBusinessObject;

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = ZGuid.NewZGuid();

				AssertEquals("The orgGuid of the BizOCollection should be set when AH_OH on the FilterBizO is set", orgFilter.Property, testMod.CollectionForDefault.OrganizationGuid);
			}
		}

		public override void TestSecurityCheckpoint()
		{
			AssertEquals("Should have correct Security CheckPoint", Env.Security.ReceivablesAccountEnquiry, ARModule.SecurityCheckpoint);
		}

		public new void TestHandleMatch()
		{
			new AccountingPeriodTestHelper(Factory).SetupPeriods();

			var organization = Factory.NewWithValidTestData<OrgHeader>();

			var journal1 = CreateJournal();
			journal1.AH_OH = organization.PK;
			journal1.AH_OSTotal = -500M;
			journal1.AH_InvoiceAmount = -500M;
			journal1.AH_OutstandingAmount = -500M;

			var journal2 = CreateJournal();
			journal2.AH_OH = organization.PK;
			journal2.AH_FullyPaidDate = ZDateTime.Today;

			var journal3 = CreateJournal();
			journal3.AH_OH = organization.PK;
			journal3.AH_OSTotal = 500M;
			journal3.AH_InvoiceAmount = 500M;
			journal3.AH_OutstandingAmount = 500M;

			Factory.Save();

			using (var form = new ZForm())
			{
				form.Controls.Add(TestTransactionModule.EmbeddedControl);
				form.Show();
				var orgFilter = (ModuleGuidFilter)((APEnquiryFilterBusinessObject)TestTransactionModule.FilterBusinessObject)["Organisation"];
				orgFilter.Property = organization.PK;
				var paymentStatusFilter = (ModuleTextFilter)((APEnquiryFilterBusinessObject)TestTransactionModule.FilterBusinessObject)["Payment Status"];
				paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.All;

				TestTransactionModule.PerformSearch_ForTest();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "Please select transaction(s) to match", UnitTestUserNotification.Instance.LastMessage.Text);

				TestTransactionModule.MatchCheckpoint_ForTestOnly.IsAllowed = false;
				TestTransactionModule.DisplayGrid.SelectAllElements();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				Assert("The information should read as follows: ",
					UnitTestUserNotification.Instance.LastMessage.Text.Contains("You do not have the appropriate security rights to run this function."));

				TestTransactionModule.MatchCheckpoint_ForTestOnly.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertNotNull(UnitTestUserNotification.Instance.LastMessage);
				AssertEquals("The information should read as follows: ", "One or more transaction(s) has zero outstanding amount. Matching process terminated",
					UnitTestUserNotification.Instance.LastMessage.Text);

				TestTransactionModule.DisplayGrid.UnSelectAll();
				paymentStatusFilter.Property = AccountingUtils.PaymentStatusTypes.Unpaid;
				TestTransactionModule.PerformSearch_ForTest();
				TestTransactionModule.DisplayGrid.SelectAllElements();

				AssertEquals("Journal 1 is unpaid, should be selected", 1, TestTransactionModule.DisplayGrid.SelectedElements.Count(x => x.PK == journal1.PK));
				AssertEquals("Journal 2 is paid, should not be available in the grid.", 0, TestTransactionModule.DisplayGrid.SelectedElements.Count(x => x.PK == journal2.PK));
				AssertEquals("Journal 3 is unpaid, should be selected", 1, TestTransactionModule.DisplayGrid.SelectedElements.Count(x => x.PK == journal3.PK));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				TestTransactionModule.HandleMatch_ForTestOnly(this, EventArgs.Empty);
				AssertEquals("The information should read as follows: ", "Transactions Matched Successfully: Match Group Number M00001000", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region TestShowStatementForm

		public void TestShowStatementForm()
		{
			OrgHeader newOrganisation = Factory.NewWithValidTestData<OrgHeader>();

			using (AREnquiryModuleForTest aREnqModule = new AREnquiryModuleForTest())
			{
				AREnquiryFilterBusinessObject filterBizO = (AREnquiryFilterBusinessObject)aREnqModule.FilterBusinessObject;

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = ZGuid.Invalid;

				aREnqModule.ShowStatementForm_ForTestOnly(false);
				AssertEquals("Message should be shown", aREnqModule.OrganisationIsInvalidMessageText_ForTestOnly, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("Should not be any form shown", ZFormModaliser.LastFormShownDialogForTest);

				orgFilter.Property = newOrganisation.PK;
				aREnqModule.ShowStatementForm_ForTestOnly(false);
				AssertNotNull("Statement form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Organisation should be defaulted", newOrganisation.PK, aREnqModule.StatementObjectAsResultOfTest.OH_PK);
				AssertEquals("CreditStatements property should be defaulted", Statement.CreditOptions.AllDocuments, aREnqModule.StatementObjectAsResultOfTest.CreditStatements);
				AssertNotEquals("IssueStatementPack property shouldn't be defaulted to ALL", AccountingConstants.IssueStatementPackType.StatementAndInvoices, aREnqModule.StatementObjectAsResultOfTest.IssueStatementPack);
				ZFormModaliser.LastFormShownDialogForTest = null;

				aREnqModule.ShowStatementForm_ForTestOnly(true);
				AssertNotNull("Statement form should be shown", ZFormModaliser.LastFormShownDialogForTest);
				AssertEquals("Organisation should be defaulted", newOrganisation.PK, aREnqModule.StatementObjectAsResultOfTest.OH_PK);
				AssertEquals("CreditStatements property should be defaulted", Statement.CreditOptions.AllDocuments, aREnqModule.StatementObjectAsResultOfTest.CreditStatements);
				AssertEquals("IssueStatementPack property should be defaulted to ALL", AccountingConstants.IssueStatementPackType.StatementAndInvoices, aREnqModule.StatementObjectAsResultOfTest.IssueStatementPack);
			}
		}

		class AREnquiryModuleForTest : AREnquiryModule
		{
			public Statement StatementObjectAsResultOfTest
			{
				get { return fStatementObjectAsResultOfTest; }
			}

			public Statement fStatementObjectAsResultOfTest;

			protected override Statement GetNewStatementBusinessObject(ZBool statementPack, ZGuid currentOrganisationPK)
			{
				fStatementObjectAsResultOfTest = base.GetNewStatementBusinessObject(statementPack, currentOrganisationPK);
				return fStatementObjectAsResultOfTest;
			}
		}

		#endregion

		#region TestCurrentOrgIsInactiveAndValid

		public void TestCurrentOrgIsInactiveAndValid()
		{
			OrgHeader inactiveOrg = Factory.NewWithValidTestData<OrgHeader>();
			inactiveOrg.OH_IsActive = false;

			OrgHeader activeOrg = Factory.NewWithValidTestData<OrgHeader>();
			activeOrg.OH_IsActive = true;

			Factory.Save();

			using (AREnquiryModule aREnqModule = (AREnquiryModule)ZModuleFactory.Instance.Create(ModuleIDs.AREnquiry))
			{
				AREnquiryFilterBusinessObject filterBizO = (AREnquiryFilterBusinessObject)aREnqModule.FilterBusinessObject;
				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;

				orgFilter.Property = inactiveOrg.PK;
				Assert("IsCurrentOrgInactiveAndValid_ForTestOnly should be true", aREnqModule.IsCurrentOrgInactiveAndValid_ForTestOnly);

				orgFilter.Property = activeOrg.PK;
				Assert("IsCurrentOrgInactiveAndValid_ForTestOnly should be false", !aREnqModule.IsCurrentOrgInactiveAndValid_ForTestOnly);
			}
		}

		#endregion

		public new void TestPrintingToolBarButtons()
		{
			ToolBarButton[] buttons = TestTransactionModule.ToolBarButtons;
			ToolBarButton printButton = buttons.FindByText("Print");
			AssertNotNull("There should be a Button named Print", printButton);
			AssertNotNull("The Print button should have a dropdown menu", printButton.DropDownMenu);

			AssertNotNull("There should be a suboption called 'PrintTransaction'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintTransactionMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintMatchDoc'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintMatchDocMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintStatement'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintStatementMenuText_ForTestOnly));
			AssertNotNull("There should be a suboption called 'PrintStatementPack'", printButton.DropDownMenu.MenuItems.FindByText(TestTransactionModule.PrintStatementPackMenuText_ForTestOnly));
		}

		#region TestNewNotAllowedIfCurrentOrgIsInactive

		public void TestNewNotAllowedIfCurrentOrgIsInactive()
		{
			SetupDataForTest();

			using (AREnquiryModule aREnqModule = (AREnquiryModule)ZModuleFactory.Instance.Create(ModuleIDs.AREnquiry))
			{
				AREnquiryFilterBusinessObject filterBizO = (AREnquiryFilterBusinessObject)aREnqModule.FilterBusinessObject;
				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = InactiveDebtor.PK;
				aREnqModule.HandleNew_ForTestOnly(ZArchitecture.Core.TransactionTypes.Invoice);

				Assert("Last message should be information", UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Should prevent user from creating a new invoice", "New Transactions cannot be created because the current organization is inactive",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		public void TestForActiveStatusFilterInTheFilterCollection()
		{
			using (AREnquiryModule testModule = new AREnquiryModule())
			{
				ModuleTextFilter filter = (ModuleTextFilter)testModule.FilterBusinessObject["Active Status"];
				AssertNotNull("The 'Active Status' should be exists in the APEnquiry module", filter);
				Assert("The 'Active Status' should be visible always", filter.Visibility == FilterVisibility.Visible);
			}
		}

		public void TestSettlementGroup()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);

			fTransactionModule.Dispose();
			using (var form = new Form())
			using (fTransactionModule = (TransactionModuleStrip)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				form.Controls.Add(fTransactionModule.EmbeddedControl);
				form.Show();

				creator.AALSHI.OH_IsDebtor = true;
				creator.AALSHI.MiscServ.OM_ARCreditLimit = 1000m;
				creator.AALSHI.CompanyData.OB_ARCreditApproved = true;

				creator.ABIGAS.OH_IsDebtor = true;
				creator.ABIGAS.CompanyData.OB_ARUseSettlementGroupCreditLimit = true;
				creator.ABIGAS.CompanyData.OB_ARCreditApproved = true;
				creator.ABIGAS.ARSettlementGroupPK = creator.AALSHI.PK;

				creator.ZECTRA.OH_IsDebtor = true;
				creator.ZECTRA.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
				creator.ZECTRA.MiscServ.OM_ARCreditLimit = 80m;
				creator.ZECTRA.CompanyData.OB_ARCreditApproved = true;
				creator.ZECTRA.ARSettlementGroupPK = creator.AALSHI.PK;

				creator.XLINDU.OH_IsDebtor = true;
				creator.XLINDU.CompanyData.OB_ARUseSettlementGroupCreditLimit = false;
				creator.XLINDU.MiscServ.OM_ARCreditLimit = 120m;
				creator.XLINDU.CompanyData.OB_ARCreditApproved = true;
				creator.XLINDU.ARSettlementGroupPK = ZGuid.Empty;

				var arInvoice1 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, creator.ABIGAS, creator.CC3.PK);
				var arInvoice2 = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "0002", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, creator.XLINDU, creator.CC3.PK);

				factory.Save();

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)((AREnquiryFilterBusinessObject)fTransactionModule.FilterBusinessObject)["Organisation"];
				orgFilter.IsActive = true;
				var control = fTransactionModule.EmbeddedControl as AREnquiryFilterControl;
				var codeTextBox = control.Controls.Find("SettlementGroupTextBox", true)[0] as ZTextBox;
				var creditLimitCheckBox = control.Controls.Find("UseSettlementGroupCreditLimitCheckBox", true)[0] as ZCheckBox;
				var infoLabel = control.Controls.Find("SettlementGroupInfoLabel", true)[0] as ZLabel;
				var creditLimit = control.Controls.Find("CreditLimitCalcEdit", true)[0] as ZCalcEdit;
				var creditAvail = control.Controls.Find("CreditBalanceCalcEdit", true)[0] as ZCalcEdit;
				AssertNotNull("SettlementGroupCodeTextBox", codeTextBox);
				AssertNotNull("UseSettlementGroupCreditLimitCheckBox", creditLimitCheckBox);
				AssertNotNull("SettlementGroupInfoLabel", infoLabel);
				AssertNotNull("CreditLimitCalcEdit", creditLimit);
				AssertNotNull("CreditBalanceCalcEdit", creditAvail);

				orgFilter.Property = creator.AALSHI.PK;
				AssertEquals("Should be the settlement group org", creator.AALSHI.OH_Code, codeTextBox.Text);
				Assert("Shouldn't tick Use Settlement Group Credit Limit", !creditLimitCheckBox.Checked);
				AssertEquals("An debtor uses the credit limit of this one", "Other debtors are using the credit limit of this Settlement Group.", infoLabel.Text);
				AssertEquals(1000m, Convert.ToDecimal(creditLimit.Text));
				AssertEquals("1000 - 100 = 900m", 900m, Convert.ToDecimal(creditAvail.Text));

				orgFilter.Property = creator.ABIGAS.PK;
				AssertEquals("Should be the settlement group org", creator.AALSHI.OH_Code, codeTextBox.Text);
				Assert("Should tick Use Settlement Group Credit Limit", creditLimitCheckBox.Checked);
				AssertEquals("No debtor uses the credit limit of this one", "", infoLabel.Text);
				AssertEquals("Should be equal to the settlement group org's", 1000m, Convert.ToDecimal(creditLimit.Text));
				AssertEquals("Should be equal to the settlement group org's", 900m, Convert.ToDecimal(creditAvail.Text));

				orgFilter.Property = creator.ZECTRA.PK;
				AssertEquals("Should be the settlement group org", creator.AALSHI.OH_Code, codeTextBox.Text);
				Assert("Shouldn't tick Use Settlement Group Credit Limit", !creditLimitCheckBox.Checked);
				AssertEquals("No debtor uses the credit limit of this one", "", infoLabel.Text);
				AssertEquals(80m, Convert.ToDecimal(creditLimit.Text));
				AssertEquals(80m, Convert.ToDecimal(creditAvail.Text));

				orgFilter.Property = creator.XLINDU.PK;
				AssertEquals("Should be the settlement group org", creator.XLINDU.OH_Code, codeTextBox.Text);
				Assert("Shouldn't tick Use Settlement Group Credit Limit", !creditLimitCheckBox.Checked);
				AssertEquals("No debtor uses the credit limit of this one", "", infoLabel.Text);
				AssertEquals(120m, Convert.ToDecimal(creditLimit.Text));
				AssertEquals("120 - 100 = 20m", 20m, Convert.ToDecimal(creditAvail.Text));
			}
		}

		public override void TestTransactionPKsAreRegisteredForWHTAmountCalculation()
		{
			var creator = new TestObjectCreator(Factory);
			var bizo = (ARInvoice)creator.CreateInvoiceWithLine(typeof(ARInvoice), "0001", TestObjectCreator.AUD, 1m, 100m, 0m, 100m, 0m, creator.ABIGAS, creator.CC3.PK);
			Factory.Save();

			using (var testModule = new AREnquiryModule())
			using (var form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var orgFilter = (ModuleGuidFilter)((AREnquiryFilterBusinessObject)testModule.FilterBusinessObject)["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = bizo.AH_OH;

				var loaderMock = new Mock<IWHTAmountLoader>();
				loaderMock.SetupProperty(x => x.IsWHTRealizationInProgress);
				loaderMock.Setup(x => x.RegisterForLoadingWHTAmounts(bizo.PK));
				TestTransactionModule.WHTAmountLoaderSubstituter_ForTestOnly = (factory) => TaxFrameworkObjectFactory.SubstituteWHTAmountLoader_ForTestOnly(factory, loaderMock.Object);
				TestTransactionModule.PerformSearch_ForTest();
				loaderMock.VerifySet(x => x.IsWHTRealizationInProgress = false);
				loaderMock.Verify(x => x.RegisterForLoadingWHTAmounts(bizo.PK));
				AssertContainsExactElementsInAnyOrder(new ZGuid[] { bizo.PK }, TestTransactionModule.GridCollection.Cast<BusinessObject>().Select(b => b.PK));
			}
		}

		public override void TestRegenerateJournalEntriesActionMenuItem()
		{
			Assert("No RegenerateJournalEntries for AR Enquiry", true);
		}

		public override void TestRegenerateJournalEntries()
		{
			Assert("No RegenerateJournalEntries for AR Enquiry", true);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AREnquiry;
		}

		protected new AREnquiryModule TestTransactionModule
		{
			get { return (AREnquiryModule)base.TestTransactionModule; }
		}

		void SetupDataForTest()
		{
			InactiveDebtor = Factory.NewWithValidTestData<OrgHeader>();
			InactiveDebtor.OH_IsDebtor = true;
			InactiveDebtor.OH_IsActive = false;
			Factory.Save();

			ARInv = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
		}

		protected override void SetupTransactionFilter()
		{
			base.SetupTransactionFilter();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)((AREnquiryFilterBusinessObject)TestTransactionModule.FilterBusinessObject)["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = InactiveOrg.PK;
		}

		OrgHeader InactiveDebtor;

		#endregion
	}
}
