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
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APEnquiryModule))]
	class APEnquiryModuleTest : APTransactionModuleTest
	{
		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.APEnquiry;
		}

		protected new APEnquiryModule TestTransactionModule
		{
			get { return (APEnquiryModule)base.TestTransactionModule; }
		}

		#endregion

		#region EInvoicing Requeue test cases

		public override void TestResetStatusToQueuedMenuItem_IsOnlyVisibleIfEnableEInvoicingFunctionalityRegistryIsOn()
		{
			var registryItem = AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables;
			Assert("Registry is off by default", !registryItem.Value);

			var currCompany = GlbCompany.CurrentCompany;
			foreach (var country in new[] { CountryCodes.Italy, CountryCodes.Spain })
			{
				foreach (bool funcEnabled in new[] { false, true })
				{
					using (currCompany.TemporarilySetCountry(country))
					using (registryItem.SetTemporaryValue(currCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, funcEnabled))
					using (var testModule = (APEnquiryModule)ZModuleFactory.Instance.Create(ModuleID))
					{
						AssertEquals("Registry is on/off", funcEnabled, registryItem.Value);
						var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
						AssertNull("'Reset Status to Queued' Menu Item should not be visibile in AP Enquiry module", resetStatusToQueuedMenuItem);
					}
				}
			}
		}

		public override void TestResetStatusToQueuedMenuItem_IsOnlyAvailableOnForCountriesSupportingEInvoicing()
		{
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
				using (var testModule = (APEnquiryModule)ZModuleFactory.Instance.Create(ModuleID))
				{
					var resetStatusToQueuedMenuItem = testModule.GetNewActionMenuItems_ForTestOnly().FindByText(testModule.ResetStatusToQueuedText_ForTestOnly);
					AssertNull("AP Enquiry module should not have 'Reset Status to Queued' Menu Item for any country", resetStatusToQueuedMenuItem);
				}
			}
		}

		public override void TestResetStatusToQueuedMenuItem_TransactionPivotStatusConcurrency_WithReloadingTransactionsInNewFactory()
		{
			Assert("AP Enquiry module does not have 'Reset Status to Queued' Menu Item", true);
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

		protected override void SetupTransactionFilter()
		{
			base.SetupTransactionFilter();
			ModuleGuidFilter orgFilter = (ModuleGuidFilter)(TestTransactionModule.FilterBusinessObject)["Organisation"];
			orgFilter.IsActive = true;
			orgFilter.Property = InactiveOrg.PK;
			TestTransactionModule.PerformSearch_ForTest();
		}

		public void TestCommandsRunOnPerformSearch()
		{
			TestTransactionModule.Dispose();
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			for (int index = 0; index < 100; index++)
			{
				creator.CreateInvoiceWithLine(typeof(APInvoice), index.ToString(), creator.AUD, 1m, 100m, 0, 100m, 0m, creator.AALSHI, creator.GLHeader1.PK);
			}
			factory.Save();

			foreach (bool isWarmUp in new[] { true, false })
			{
				using (var testModule = (APEnquiryModule)ZModuleFactory.Instance.Create(GetModuleID()))
				{
					AssertCommandsRunOnPerformSearch(isWarmUp, creator.AALSHI.PK, testModule);
				}
			}
		}

		void AssertCommandsRunOnPerformSearch(bool isWarmUp, ZGuid organisationPK, APEnquiryModule testModule)
		{
			var orgFilter = (ModuleGuidFilter)((APEnquiryFilterBusinessObject)testModule.FilterBusinessObject)["Organisation"];
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

		public override void TestSecurityCheckpoint()
		{
			AssertEquals("Should have correct Security CheckPoint", Env.Security.PayablesAccountEnquiry, TestTransactionModule.SecurityCheckpoint);
		}

		public void TestAH_OH_EventHandler()
		{
			using (APEnquiryModule testMod = (APEnquiryModule)ZModuleFactory.Instance.Create(ModuleIDs.APEnquiry))
			{
				APEnquiryFilterBusinessObject filterBizO = (APEnquiryFilterBusinessObject)testMod.FilterBusinessObject;

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = ZGuid.NewZGuid();
				AssertEquals("The orgGuid of the BizOCollection should be set when AH_OH on the FilterBizO is set", orgFilter.Property, testMod.CollectionForDefault.OrganizationGuid);
			}
		}

		public void TestToolbarReadOnlyForInactiveOrganizations()
		{
			OrgHeader testOrg = Factory.NewWithValidTestData<OrgHeader>();
			testOrg.OH_IsActive = false;
			Factory.Save();

			using (APEnquiryModule testMod = (APEnquiryModule)ZModuleFactory.Instance.Create(ModuleIDs.APEnquiry))
			{
				APEnquiryFilterBusinessObject filterBizO = (APEnquiryFilterBusinessObject)testMod.FilterBusinessObject;

				ModuleGuidFilter orgFilter = (ModuleGuidFilter)filterBizO["Organisation"];
				orgFilter.IsActive = true;
				orgFilter.Property = testOrg.PK;

				Assert("Reverse button should be disabled because current organization is inactive", !testMod.FormActionMenu.FindByText("&Reverse").Enabled);
				Assert("Copy button should be disabled because current organization is inactive", !testMod.FormActionMenu.FindByText("&Copy").Enabled);
			}
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
				var orgFilter = (ModuleGuidFilter)(TestTransactionModule.FilterBusinessObject)["Organisation"];
				orgFilter.Property = organization.PK;
				var paymentStatusFilter = (ModuleTextFilter)(TestTransactionModule.FilterBusinessObject)["Payment Status"];
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

		public void TestFilterWithColorSchemeGridAndSureThatIsFilterWorkingProperly()
		{
			using (APEnquiryModule testModule = TestTransactionModule)
			{
				using (ZForm form = new ZForm())
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					ZString filterDataLegacy =
						"<NewDataSet>" + System.Environment.NewLine + "  " +
							"<ColourStrips>" + System.Environment.NewLine + "    " +
								"<RulePK>0</RulePK>" + System.Environment.NewLine + "    " +
								"<RuleName>rule1</RuleName>" + System.Environment.NewLine + "    " +
								"<BGColor>-6972</BGColor>" + System.Environment.NewLine + "  " +
							"</ColourStrips>" + System.Environment.NewLine + "  " +
							"<ColourStrips>" + System.Environment.NewLine + "    " +
								"<RulePK>1</RulePK>" + System.Environment.NewLine + "    " +
								"<RuleName>rule2</RuleName>" + System.Environment.NewLine + "    " +
								"<BGColor>-16776961</BGColor>" + System.Environment.NewLine + "  " +
							"</ColourStrips>" + System.Environment.NewLine +
						"</NewDataSet>";

					GridColourScheme gridColourScheme = Factory.New<GridColourScheme>();
					gridColourScheme.S9_FilterData = System.Text.Encoding.ASCII.GetBytes(filterDataLegacy);
					gridColourScheme.S9_FilterName = "test_scheme";
					//Call OnLoaded to load the 'filterDataLegacy' data into the variable, application automatically loading
					//this values when Factory is loading
					gridColourScheme.OnLoaded();
					gridColourScheme.SetStripsFromFilter(testModule.FilterBusinessObject, typeof(APEnquiryFilterBusinessObject));

					APEnquiryFilterBusinessObject aPEnquiryFilterBusinessObject = (APEnquiryFilterBusinessObject)testModule.FilterBusinessObject;
					ModuleGuidFilter orgFilter = (ModuleGuidFilter)aPEnquiryFilterBusinessObject["Organisation"];

					AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(ZArchitecture.Core.TransactionTypes.Invoice);
					aPInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
					aPInvoice.AH_Ledger = ZArchitecture.Core.LedgerTypes.AccountsPayable;
					aPInvoice.AH_TransactionNum = TestObjectCreator.GetRandomString(10);
					Factory.Save();

					orgFilter.IsActive = true;
					orgFilter.Property = TestObjectCreator.AALSHI.PK;

					testModule.PerformSearch_ForTest();

					AssertEquals(false, aPEnquiryFilterBusinessObject.Filter.IsNoResultQuery);
					AssertEquals(true, testModule.GridCollection.ToArray().Any(a => a.PK == aPInvoice.PK));
				}
			}
		}

		public void TestForActiveStatusFilterInTheFilterCollection()
		{
			using (APEnquiryModule testModule = TestTransactionModule)
			{
				ModuleTextFilter filter = (ModuleTextFilter)testModule.FilterBusinessObject["Active Status"];
				AssertNotNull("The 'Active Status' should be exists in the APEnquiry module", filter);
				Assert("The 'Active Status' should be visible always", filter.Visibility == FilterVisibility.Visible);
			}
		}

		public override void TestTransactionPKsAreRegisteredForWHTAmountCalculation()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var bizo = objectCreator.CreateAPInvoice<APInvoice>("0001", objectCreator.AUD, 1.0M, 250M, 10M, 0M, 250M, 10M, 0M);
			Factory.Save();

			using (APEnquiryModule testModule = TestTransactionModule)
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var orgFilter = (ModuleGuidFilter)((APEnquiryFilterBusinessObject)testModule.FilterBusinessObject)["Organisation"];
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
			Assert("No RegenerateJournalEntries for AP Enquiry", true);
		}

		public override void TestRegenerateJournalEntries()
		{
			Assert("No RegenerateJournalEntries for AP Enquiry", true);
		}
		protected override APTransactionModuleStrip GetNewModuleObject(IWithholdingJournalCreatorForMultipleInvoices withholdingJournalCreatorForMultipleInvoices, IWithholdingJournalRealizerForMultipleInvoices withholdingJournalRealizerForMultipleInvoices)
		{
			return new APEnquiryModule((withholdingJournalCreatorForMultipleInvoices, withholdingJournalRealizerForMultipleInvoices));
		}
	}
}
