using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Accounting.GUI.JobInvoicing.JobChargeUserControl;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	public class JobChargeUserControlGeneralTest : TransactionedTestCase
	{
		#region Raw Data 

		public void TestWiseRatesRawDataButton_MessageWhenHasNotAutoRate()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var shipment = Factory.New<ForwardingShipment>();
			job.PlugInData = shipment;

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var jobChargeUserControl = new JobChargeUserControl())
			using (var form = new ZForm())
			{
				form.Controls.Add(jobChargeUserControl);
				jobChargeUserControl.Bind(job);

				AssertEquals(0, job.Charges.Count);
				form.Show();
				var autoratingCostTabPage = jobChargeUserControl.ChargesDetailsTabControl.TabPages["AutoratingCostTabPage"];
				autoratingCostTabPage.Show();
				Assert("autoratingCostTabPage visible", autoratingCostTabPage.Visible);

				var wiseRatesRawDataUserControl = ControlTestHelper.FindControls<WiseRatesRawDataUserControl>(jobChargeUserControl).Single();
				AssertEquals("WiseRatesData button visible", true, wiseRatesRawDataUserControl.Visible);

				var wiseRatesRawDataButton = ControlTestHelper.FindControls<ZButton>(wiseRatesRawDataUserControl).Single();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wiseRatesRawDataButton.PerformClick();
				AssertEquals(
					"GIVEN not autorate yet, WHEN clicking WiseRatesRawDataButton THEN should show info message",
					"Please perform Autorating in this session before you can see Raw Data from Rates Service. This information is available until the form is closed.",
					UnitTestUserNotification.Instance.LastMessage.Text);

				// simulate autorate
				var cache = Factory.GetCachedValue("AutoRatingStarerCore.RateCharges.RawResponse", () => new Dictionary<ZGuid, string>());
				cache.GetOrAdd(shipment.PK, () => "test");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				wiseRatesRawDataButton.PerformClick();
				AssertNullOrEmpty(
					"GIVEN was autorated, WHEN clicking WiseRatesRawDataButton THEN should not show info message",
					UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestWiseRatesRawDataButton_WhenDiagnosticSettingsIncludeRawDataIsTrue_ThenShowWiseRatesDataButton()
		{
			AssertWiseRatesRawDataButton(diagnosticSettingsIncludeRawData: true, expectWiseRatesDataButton: true);
		}

		public void TestWiseRatesRawDataButton_WhenDiagnosticSettingsIncludeRawDataIsFalse_ThenHideWiseRatesDataButton()
		{
			AssertWiseRatesRawDataButton(diagnosticSettingsIncludeRawData: false, expectWiseRatesDataButton: false);
		}

		void AssertWiseRatesRawDataButton(bool diagnosticSettingsIncludeRawData, bool expectWiseRatesDataButton)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var shipment = Factory.New<CommonShipment>();
			job.PlugInData = shipment;

			using (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, diagnosticSettingsIncludeRawData))
			using (var jobChargeUserControl = new JobChargeUserControl())
			{
				jobChargeUserControl.Bind(job);

				AssertEquals(0, job.Charges.Count);

				var autoratingCostTabPage = jobChargeUserControl.ChargesDetailsTabControl.TabPages["AutoratingCostTabPage"];
				autoratingCostTabPage.Show();
				Assert("autoratingCostTabPage visible", autoratingCostTabPage.Visible);

				var wiseRatesRawDataUserControl = ControlTestHelper.FindControls<WiseRatesRawDataUserControl>(jobChargeUserControl).Single();
				AssertEquals("WiseRatesData button visible", expectWiseRatesDataButton, wiseRatesRawDataUserControl.Visible);
			}
		}

		#endregion

		#region Spot Quote Charges

		public void TestSpotQuoteChargesExist()
		{
			Quote testQuote = Factory.New<Quote>();
			testQuote.TH_OneTimeQuote = true;
			testQuote.TH_QuoteNumber = "123456";

			var job = (Job)new JobHeader.Loader(testQuote).TryLoadOrCreate();
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var localClient = Factory.NewWithValidTestData<OrgHeader>();
			job.LocalChargesPK = localClient.PK;

			JobCharge charge1 = job.Charges.AddNew();
			charge1.JR_AC = Env.Registry.FreightChargeCode;
			charge1.JR_LocalSellAmt = 100m;

			Factory.Save();

			Job testJob = Factory.NewJobForTesting<Job>();
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				AssertEquals(0, testJob.Charges.Count);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testJob.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
				AssertEquals("The Quote # has been applied to the Billing Job. Do you want to bring through all quoted charges from this Spot Quotation?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, testJob.Charges.Count);

				testJob.Charges.RemoveAndDeleteAll();
				testJob.JH_TH_NKQuoteNumber = "";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				testJob.JH_TH_NKQuoteNumber = testQuote.TH_QuoteNumber;
				AssertEquals("The Quote # has been applied to the Billing Job. Do you want to bring through all quoted charges from this Spot Quotation?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, testJob.Charges.Count);
			}
		}

		#endregion

		#region Client Contract Number

		public void TestClientContractNumberIsNotVisibleOrEditable()
		{
			Job testJob = Factory.NewJobForTesting<Job>();
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				AssertEquals(false, control.ClientContractNumber_ForTestOnly.Visible);
				AssertEquals(false, control.ClientContractNumber_ForTestOnly.ReadOnly);

				using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Client contract number button should not be visible.", false, control.ClientContractNumberButton_ForTestOnly.Visible);
				}

				using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AssertEquals("Client contract number button should not be visible.", false, control.ClientContractNumberButton_ForTestOnly.Visible);
				}

				var quotesCodeFindBox = control.GetControl<ZCodeFindBox>("QuotesCodeFindBox");
				AssertLessThan(quotesCodeFindBox.Left, control.ClientContractNumber_ForTestOnly.Left + control.ClientContractNumber_ForTestOnly.Width);
			}
		}

		#endregion

		#region Job Local Charge Org Changed

		public void TestJob_LocalChargeOrgChangedShouldCreateReverseCommission()
		{
			SetupTransactionsAndCommissions();
			Job.Close(null, null);

			Factory.Save();

			var originalCommissions = Factory.Load<IAccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Invoice.PK));
			AssertEquals("Precondition", 1, originalCommissions.Length);

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(Job);

				Job.LocalZAddressWithContact.OrgPK = Org1.PK;
				AssertNull("Should not trigger if resetting to original organisation", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Job.LocalZAddressWithContact.OrgPK = Org2.PK;
				AssertEquals("Commission calculations have already been processed for this job. Existing commission transactions will be reversed on Save. To avoid commission reversal, please restore original values for Local Client. Commission for this job will only be reprocessed once the job is reopened and re-closed.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				var commissionQuery = new ZQuery(AccCommissionHeaderSchema.CH0_OH_Customer, Org1.PK);
				commissionQuery.OrderBy = AccCommissionHeaderSchema.CH0_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				var org1Commissions = Factory.Load<IAccCommissionHeader>(commissionQuery);
				AssertEquals(2, org1Commissions.Length);
				AssertEquals("Original commission", Invoice.PK, org1Commissions[1].CH0_AH_Source);
				AssertEquals("Reverse commission", Invoice.PK, org1Commissions[0].CH0_AH_Source);

				var commissionLineGroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, org1Commissions[1].PK);
				var originalCommissionLine = Factory.LoadTop1<IAccCommissionLineGroup>(commissionLineGroupQuery);
				AssertEquals("Original commission", (ZDecimal)100, originalCommissionLine.CLG_TransactionAmount);
				AssertEquals("Original commission", (ZDecimal)100, originalCommissionLine.CLG_TotalCommissionableAmount);

				var reverseCommissionLineGroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, org1Commissions[0].PK);
				var reverseCommissionLine = Factory.LoadTop1<IAccCommissionLineGroup>(reverseCommissionLineGroupQuery);
				AssertEquals("Reverse commission", (ZDecimal)(-100), reverseCommissionLine.CLG_TransactionAmount);
				AssertEquals("Reverse commission", (ZDecimal)(-100), reverseCommissionLine.CLG_TotalCommissionableAmount);
			}
		}

		public void TestJob_LocalChargeOrgChangedMultipleTransactions()
		{
			SetupTransactionsAndCommissions();
			var invoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			invoice2.AH_InvoiceAmount = 1500;
			invoice2.AH_OH = Org1.PK;
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1, 150, 15, 0);
			line2.AL_LineType = TransactionLineTypes.Revenue;
			line2.AL_JH = Job.PK;
			line2.AL_AC = TestObjectCreator.CC1.PK;
			var sellpostedCharge = TestObjectCreator.CreateCharge(line2);

			Factory.Save();

			Job.Close(null, null);

			Factory.Save();

			var originalInvoiceCommissions = Factory.Load<IAccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Invoice.PK));
			AssertEquals("Precondition", 1, originalInvoiceCommissions.Length);
			var originalInvoice2Commissions = Factory.Load<IAccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, invoice2.PK));
			AssertEquals("Precondition", 1, originalInvoice2Commissions.Length);

			Factory.Save();

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(Job);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				Job.LocalZAddressWithContact.OrgPK = Org2.PK;
				AssertEquals("Commission calculations have already been processed for this job. Existing commission transactions will be reversed on Save. To avoid commission reversal, please restore original values for Local Client. Commission for this job will only be reprocessed once the job is reopened and re-closed.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				var commissionQuery = new ZQuery(AccCommissionHeaderSchema.CH0_OH_Customer, Org1.PK);
				var org1Commissions = Factory.Load<IAccCommissionHeader>(commissionQuery);
				AssertEquals(4, org1Commissions.Length);
				var invoice1Commissions = org1Commissions.Where(x => x.CH0_AH_Source == Invoice.PK).OrderByDescending(x => x.CH0_SystemCreateTimeUtc).ToArray();
				var invoice2Commissions = org1Commissions.Where(x => x.CH0_AH_Source == invoice2.PK).OrderByDescending(x => x.CH0_SystemCreateTimeUtc).ToArray();

				var commissionLineGroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, invoice1Commissions[1].PK);
				var originalCommissionLine = Factory.LoadTop1<IAccCommissionLineGroup>(commissionLineGroupQuery);
				AssertEquals("Original commission", (ZDecimal)100, originalCommissionLine.CLG_TransactionAmount);
				AssertEquals("Original commission", (ZDecimal)100, originalCommissionLine.CLG_TotalCommissionableAmount);

				var reverseCommissionLineGroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, invoice1Commissions[0].PK);
				var reverseCommissionLine = Factory.LoadTop1<IAccCommissionLineGroup>(reverseCommissionLineGroupQuery);
				AssertEquals("Reverse commission", (ZDecimal)(-100), reverseCommissionLine.CLG_TransactionAmount);
				AssertEquals("Reverse commission", (ZDecimal)(-100), reverseCommissionLine.CLG_TotalCommissionableAmount);

				var commissionLine2GroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, invoice2Commissions[1].PK);
				var originalCommissionLine2 = Factory.LoadTop1<IAccCommissionLineGroup>(commissionLine2GroupQuery);
				AssertEquals("Original commission", (ZDecimal)150, originalCommissionLine2.CLG_TransactionAmount);
				AssertEquals("Original commission", (ZDecimal)150, originalCommissionLine2.CLG_TotalCommissionableAmount);

				var reverseCommissionLine2GroupQuery = new ZQuery(AccCommissionLineGroupSchema.CLG_CH0, invoice2Commissions[0].PK);
				var reverseCommissionLine2 = Factory.LoadTop1<IAccCommissionLineGroup>(reverseCommissionLine2GroupQuery);
				AssertEquals("Reverse commission", (ZDecimal)(-150), reverseCommissionLine2.CLG_TransactionAmount);
				AssertEquals("Reverse commission", (ZDecimal)(-150), reverseCommissionLine2.CLG_TotalCommissionableAmount);
			}
		}

		public void TestJob_LocalChargeOrgChangedShouldNotCreateReverseCommissionIfAlreadyExists()
		{
			SetupTransactionsAndCommissions();
			Job.Close(null, null);

			Factory.Save();

			var originalCommissions = Factory.Load<IAccCommissionHeader>(new ZQuery(AccCommissionHeaderSchema.CH0_AH_Source, Invoice.PK));
			AssertEquals("Precondition", 1, originalCommissions.Length);

			var commissionCreator = ObjectFactory.Get<ICommissionCreatorProvider>().GetReversalTransactionCommissionCreator(Invoice);
			commissionCreator.CreateReversalCommissions(originalCommissions[0]);

			Factory.Save();

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(Job);

				UnitTestUserNotification.Instance.ClearMessages();
				Job.LocalZAddressWithContact.OrgPK = Org2.PK;
				AssertNull("Should not trigger if outstanding commissions have already been reversed", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		OrgHeader Org1;
		OrgHeader Org2;
		Job Job;
		InvoicingBase Invoice;

		void SetupTransactionsAndCommissions()
		{
			Org1 = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = Org1.Addresses.AddNew();
			address1.OA_Address1 = "72 O'Riordan St";
			Org2 = Factory.NewWithValidTestData<OrgHeader>();
			var address2 = Org2.Addresses.AddNew();
			address2.OA_Address1 = "27 O'Riordan St";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ACL";
			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();

			var commissionAgreement1 = opportunity1.ApprovedCommissionAgreements.AddNew();
			commissionAgreement1.CA0_OH_Customer = Org1.PK;
			commissionAgreement1.FillWithValidTestData();
			commissionAgreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement1.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement1.CA0_EffectiveDate = ZDate.Today;
			var item1 = commissionAgreement1.ProductItems.AddNew(true, "SHP");
			item1.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			var agreementPctRecipient1 = commissionAgreement1.Recipients.AddNew();
			agreementPctRecipient1.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient1.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient1.CAR_Share = 1;
			var agreementPctRecipient1Rate = agreementPctRecipient1.Rates.AddNew();
			agreementPctRecipient1Rate.CAT_CommissionPercentage = 10;

			var commissionAgreement2 = opportunity2.ApprovedCommissionAgreements.AddNew();
			commissionAgreement2.CA0_OH_Customer = Org2.PK;
			commissionAgreement2.FillWithValidTestData();
			commissionAgreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			commissionAgreement2.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;
			commissionAgreement2.CA0_EffectiveDate = ZDate.Today;
			var item2 = commissionAgreement2.ProductItems.AddNew(true, "SHP");
			item2.CAI_Type = OrgCommissionAgreementItemTypes.Codes.Product;
			var agreementPctRecipient2 = commissionAgreement2.Recipients.AddNew();
			agreementPctRecipient2.CAR_GS_NKStaff = "ACL";
			agreementPctRecipient2.CAR_CommissionType = CommissionTypes.Codes.PCT;
			agreementPctRecipient2.CAR_Share = 1;
			var agreementPctRecipient2Rate = agreementPctRecipient2.Rates.AddNew();
			agreementPctRecipient2Rate.CAT_CommissionPercentage = 10;

			var shipment = TestObjectCreator.CreateShipment("1001");
			Job = TestObjectCreator.CreateJob(shipment);
			Job.JH_OA_LocalChargesAddr = address1.PK;
			AssertEquals("Precondition", Org1, Job.LocalCharges);
			Invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1);
			Invoice.AH_InvoiceAmount = 1000;
			Invoice.AH_OH = Org1.PK;
			var line = TestObjectCreator.CreateInvoiceLine(Invoice, TestObjectCreator.AUD, 1, 100, 10, 0);
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_JH = Job.PK;
			line.AL_AC = TestObjectCreator.CC1.PK;
			var sellpostedCharge = TestObjectCreator.CreateCharge(line);

			Factory.Save();
		}

		#endregion

		#region Initialisation

		[ExpectNoExceptions()]
		public void TestInitialise()
		{
			Job job = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			job.PlugInData = parent;

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(job);
				control.Bind(job);  // should blow up for the second time
			}
		}

		#endregion

		#region TestAddExtraDataColumns

		public void TestAddExtraDataColumns()
		{
			var job = Factory.NewJobForTesting<Job>();
			var parent = Factory.New<ParentJobWithAdditionalPropertiesForTest>();
			job.PlugInData = parent;

			using (var form = new ZForm(parent))
			{
				var tabControl = new ZTemplateTabControl();
				form.Controls.Add(tabControl);
				form.PlugIns.Add(ControllerIDs.JobInvoicing);
				form.Show();

				var invoicingPlugin = (InvoicingPluginToFreight)form.PlugIns.Instances[0];
				var grid = ((JobInvoicingUserControl)invoicingPlugin.UserControl).JobChargeUserControl.JobChargeBoundGrid;
				var customVisibleColumn = grid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().First(c => c.ColumnName == "VisibleColumnTest");
				AssertNotNull(customVisibleColumn);
				Assert(customVisibleColumn.IsReadOnly);
				Assert("Visibility should be determined by the Visibility on the property itself.", customVisibleColumn.IsVisible);
				AssertNull("Properties should not be grouped.", customVisibleColumn.GroupName.Caption);

				var customInvisibleColumn = grid.ColumnStyles.Cast<ZTextBoxColumnStyleInfo>().First(c => c.ColumnName == "InvisibleColumnTest");
				AssertNotNull(customInvisibleColumn);
				Assert(customInvisibleColumn.IsReadOnly);
				Assert("Visibility should be determined by the Visibility on the property itself.", !customInvisibleColumn.IsVisible);
				AssertNull("Properties should not be grouped.", customInvisibleColumn.GroupName.Caption);
			}
		}

		class ParentJobWithAdditionalPropertiesForTest : CommonShipment, IJobInvoicingAdditionalData
		{
			public ParentJobWithAdditionalPropertiesForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public CustomPropertyContainer<JobCharge> GetAdditionalProperties()
			{
				var additionalProperties = new CustomPropertyContainer<JobCharge>();
				additionalProperties.AddCustomProperty("VisibleColumnTest", "123", typeof(ZString), true);
				additionalProperties.AddCustomProperty("InvisibleColumnTest", "456", typeof(ZString), false);
				return additionalProperties;
			}
		}

		#endregion

		#region Overseas Agent

		public void TestOverseasAgentHiddenForNonShipment()
		{
			Job job = Factory.NewJobForTesting<Job>();

			CommonShipment shipment = Factory.New<CommonShipment>();
			job.PlugInData = shipment;

			Assert("Overseas agent IS applicable to a freight job", job.OverseasAgentIsApplicable);

			InvoicingParam testParent = new InvoicingParam();
			job = Factory.NewJobForTesting<Job>();
			job.PlugInData = testParent;

			Assert("Overseas agent is NOT applicable to a test job", !job.OverseasAgentIsApplicable);
		}

		public void TestExchangeRatesPlacementTest()
		{
			using (var control = new JobChargeUserControl())
			{
				control.Width = control.MinimumSize.Width;

				void assertEverythingCorrect()
				{
					var invoicingFieldsPanel = control.GetControl<ZPanel>("InvoicingFieldsPanel");
					var exRatesPanel = control.GetControl<ZPanel>("ExchangeRatesZPanel");
					AssertEquals(invoicingFieldsPanel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(5), exRatesPanel.Left);
					AssertCloseEnough(ControlDpiScalingHelper.ScaleToCurrentDpiX(5), control.ClientRectangle.Right - exRatesPanel.Right);
					AssertGreaterThan(exRatesPanel.Width, ControlDpiScalingHelper.ScaleToCurrentDpiX(100));
				}

				control.ToggleOverseasAgentControlsVisibility(false);
				assertEverythingCorrect();
				control.ToggleOverseasAgentControlsVisibility(true);
				assertEverythingCorrect();
			}
		}

		#endregion

		#region GST / WHT Visibility

		public void TestControlsHiding()
		{
			Job testJob = Factory.NewJobWithValidTestDataForTesting<Job>();
			CommonShipment shipment = Factory.New<CommonShipment>();
			testJob.PlugInData = shipment;

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = ZBool.True;
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(true, control.CostGSTPanel.Visible);
				AssertEquals(true, control.CostWHTPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(true, control.SellWHTPanel.Visible);
				AssertEquals(true, control.SellGSTPanel.Visible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.False;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = ZBool.False;
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(false, control.CostGSTPanel.Visible);
				AssertEquals(false, control.CostWHTPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(false, control.SellGSTPanel.Visible);
				AssertEquals(false, control.SellWHTPanel.Visible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.False;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = ZBool.True;
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(false, control.CostGSTPanel.Visible);
				AssertEquals(true, control.CostWHTPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(false, control.SellGSTPanel.Visible);
				AssertEquals(true, control.SellWHTPanel.Visible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = ZBool.True;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = ZBool.False;
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(true, control.CostGSTPanel.Visible);
				AssertEquals(false, control.CostWHTPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(true, control.SellGSTPanel.Visible);
				AssertEquals(false, control.SellWHTPanel.Visible);
			}

			AssertEquals("Company should not be in Canada ", false, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(false, control.CostExtraTaxPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(false, control.SellExtraTaxPanel.Visible);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
			Factory.Save();
			AssertEquals("Company should be in Canada ", true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Canada);
			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
				AssertEquals(true, control.CostExtraTaxPanel.Visible);

				control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
				AssertEquals(true, control.SellExtraTaxPanel.Visible);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
			try
			{
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					control.Bind(testJob);
					control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
					control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
					AssertEquals(true, control.CostExtraTaxPanel.Visible);
					AssertEquals("ExtraTaxCostTaxAmountCalcEdit binding should be changed", "FilteredCharges.LineExtraTaxAmountOnInvoiceForJob",
						control.BindingSource.GetBindingMember(control.CostExtraTaxPanel.Controls["ExtraTaxCostTaxAmountCalcEdit"]));
					string eDUAmountCalcEditCaption = "SGST Amount";
					AssertEquals("ExtraTaxCostTaxAmountCalcEdit caption should be changed", eDUAmountCalcEditCaption,
						control.CostExtraTaxPanel.Controls["ExtraTaxCostTaxAmountCalcEdit"].GetExtension<ILabelCaptionRenderer>().Caption);
					control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
					AssertEquals(true, control.SellExtraTaxPanel.Visible);
					AssertEquals("ExtraTaxSellTaxAmountCalcEdit binding should be changed", "FilteredCharges.JR_Calc_OSSellExtraTaxAmt",
						control.BindingSource.GetBindingMember(control.SellExtraTaxPanel.Controls["ExtraTaxSellTaxAmountCalcEdit"]));
					AssertEquals("ExtraTaxSellTaxAmountCalcEdit caption should be changed", eDUAmountCalcEditCaption,
						control.SellExtraTaxPanel.Controls["ExtraTaxSellTaxAmountCalcEdit"].GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
			AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
			try
			{
				using (JobChargeUserControl control = new JobChargeUserControl())
				{
					control.Bind(testJob);
					control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
					control.ChargesDetailsTabControl.TabPages["CostTabPage"].Show();
					AssertEquals(true, control.CostExtraTaxPanel.Visible);
					AssertEquals("ExtraTaxCostTaxAmountCalcEdit binding should be changed", "FilteredCharges.LineExtraTaxAmountOnInvoiceForJob",
						control.BindingSource.GetBindingMember(control.CostExtraTaxPanel.Controls["ExtraTaxCostTaxAmountCalcEdit"]));
					var rETAmountCalcEditCaption = "RET Amount";
					AssertEquals("ExtraTaxCostTaxAmountCalcEdit caption should be changed", rETAmountCalcEditCaption,
						control.CostExtraTaxPanel.Controls["ExtraTaxCostTaxAmountCalcEdit"].GetExtension<ILabelCaptionRenderer>().Caption);
					control.ChargesDetailsTabControl.TabPages["RevenueTabPage"].Show();
					AssertEquals(true, control.SellExtraTaxPanel.Visible);
					AssertEquals("ExtraTaxSellTaxAmountCalcEdit binding should be changed", "FilteredCharges.JR_Calc_OSSellExtraTaxAmt",
						control.BindingSource.GetBindingMember(control.SellExtraTaxPanel.Controls["ExtraTaxSellTaxAmountCalcEdit"]));
					AssertEquals("ExtraTaxSellTaxAmountCalcEdit caption should be changed", rETAmountCalcEditCaption,
						control.SellExtraTaxPanel.Controls["ExtraTaxSellTaxAmountCalcEdit"].GetExtension<ILabelCaptionRenderer>().Caption);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		#endregion

		#region Periodic Billing

		public void TestExcludeFromPeriodicRatingCheckboxVisible()
		{
			Job testJob = Factory.NewJobForTesting<Job>();
			InvoicingParam parent = new InvoicingParam();
			parent.ConsumerType = JobInvoicingConsumerTypes.WarehouseInwards;
			testJob.PlugInData = parent;

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				AssertEquals(true, control.ExcludeFromPeriodicRatingCheckbox.Visible);
			}

			testJob = Factory.NewJobForTesting<Job>();
			parent = new InvoicingParam();
			parent.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			testJob.PlugInData = parent;

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				AssertEquals(false, control.ExcludeFromPeriodicRatingCheckbox.Visible);
			}

			testJob = Factory.NewJobForTesting<Job>();
			parent = new InvoicingParam();
			parent.ConsumerType = JobInvoicingConsumerTypes.WarehouseOutwards;
			testJob.PlugInData = parent;

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				AssertEquals(true, control.ExcludeFromPeriodicRatingCheckbox.Visible);
			}

			testJob = Factory.NewJobForTesting<Job>();
			parent = new InvoicingParam();
			parent.ConsumerType = JobInvoicingConsumerTypes.WarehouseStorage;
			testJob.PlugInData = parent;

			using (JobChargeUserControl control = new JobChargeUserControl())
			{
				control.Bind(testJob);
				AssertEquals(false, control.ExcludeFromPeriodicRatingCheckbox.Visible);
			}
		}

		#endregion

		#region Shipment And Billing Details pop up

		public void TestRelatedJobFilter()
		{
			var creator = new TestObjectCreator(Factory);
			var setup = creator.CreateGatewayConsolsAndShipments();
			var consol = setup.gC0002;
			Factory.Save();

			using (var control = new JobChargeUserControl())
			using (var form = new ZForm(consol))
			using (var job = creator.CreateJob(consol))
			{
				var chargeWithShipment1RelatedJob = job.Charges.AddNew();
				chargeWithShipment1RelatedJob.JR_Calc_RelatedJobNumber = setup.s0001.JobNumber;
				var chargeWithShipment2RelatedJob = job.Charges.AddNew();
				chargeWithShipment2RelatedJob.JR_Calc_RelatedJobNumber = setup.s0002.JobNumber;
				var chargeWithEmptyRelatedJob = job.Charges.AddNew();
				AssertNullOrEmpty("Precondition: non charge has no related job", chargeWithEmptyRelatedJob.JR_Calc_RelatedJobNumber);

				control.Bind(job);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNotNull("Precondition: details is created", control.ShipmentAndBillingDetails?.DetailsRows);
				AssertNotEquals("Precondition: details has values", 0, control.ShipmentAndBillingDetails.DetailsRows.Count);
				AssertContainsExactElementsInAnyOrder("When form opened, all charges are shown", new[] { chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob, chargeWithEmptyRelatedJob }, job.FilteredCharges);
				Assert("When form opened, charge grid is not readonly", !control.JobChargeBoundGrid.ReadOnly);

				bool? eventValue = null;
				var eventInvokes = 0;
				control.RelatedJobFilterUpdated += (object sender, RelatedJobFilterUpdatedEventArgs args) =>
				{
					eventValue = args.AnyFiltersApplied;
					eventInvokes++;
				};

				var detailsRows = control.ShipmentAndBillingDetails.DetailsRows.Cast<ShipmentAndBillingDetailsRow>();
				detailsRows.First(x => x.RelatedJobNum == setup.s0001.JobNumber).ShowRelatedCharges = true;
				AssertContainsExactElementsInAnyOrder("When ShowRelatedCharges is ticked for row, non related rows are hidden", new[] { chargeWithShipment1RelatedJob }, job.FilteredCharges);
				AssertEquals("When ShowRelatedCharges is ticked for row, event is called with arg showing filter is applied", true, eventValue);
				AssertEquals("When ShowRelatedCharges is ticked for row, event is invoked once", 1, eventInvokes);
				Assert("When filter applied, charge grid is readonly", control.JobChargeBoundGrid.ReadOnly);

				detailsRows.First(x => x.RelatedJobNum == setup.s0002.JobNumber).ShowRelatedCharges = true;
				AssertContainsExactElementsInAnyOrder("When ShowRelatedCharges is ticked for row, non related rows are hidden", new[] { chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob }, job.FilteredCharges);
				AssertEquals("When ShowRelatedCharges is ticked for row, event is called with arg showing filter is applied", true, eventValue);
				AssertEquals("When ShowRelatedCharges is ticked for row, event is invoked once", 2, eventInvokes);
				Assert("When filter applied, charge grid is readonly", control.JobChargeBoundGrid.ReadOnly);

				detailsRows.First(x => x.RelatedJobNum == setup.s0001.JobNumber).ShowRelatedCharges = false;
				detailsRows.First(x => x.RelatedJobNum == setup.s0002.JobNumber).ShowRelatedCharges = false;
				AssertContainsExactElementsInAnyOrder("When filter no longer applied, all charges are shown", new[] { chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob, chargeWithEmptyRelatedJob }, job.FilteredCharges);
				AssertEquals("When filter no longer applied, event is called with arg showing filter is not applied", false, eventValue);
				AssertEquals("When ShowRelatedCharges is clicked twice, event is invoked twice", 4, eventInvokes);
				Assert("When filter no longer applied, charge grid is not readonly", !control.JobChargeBoundGrid.ReadOnly);

				detailsRows.First(x => x.RelatedJobNum == setup.s0001.JobNumber).ShowRelatedCharges = true;
				AssertContainsExactElementsInAnyOrder("When ShowRelatedCharges is ticked for row, non related rows are hidden", new[] { chargeWithShipment1RelatedJob }, job.FilteredCharges);
				AssertEquals("When ShowRelatedCharges is ticked for row, event is invoked once", 5, eventInvokes);

				var detailsForm = OpenedFormCache.GetInstance().GetForm(control.ShipmentAndBillingDetails.PK.ToGuid(), "ShipmentAndBillingDetails");
				detailsForm.Close();
				AssertContainsExactElementsInAnyOrder("When form is closed, filter is reset", new[] { chargeWithShipment1RelatedJob, chargeWithShipment2RelatedJob, chargeWithEmptyRelatedJob }, job.FilteredCharges);
				AssertEquals("When form is closed, event is called with arg showing filter is not applied", false, eventValue);
				AssertEquals("When form is closed, event is invoked once", 6, eventInvokes);
				Assert("When form is closed, charge grid is not readonly", !control.JobChargeBoundGrid.ReadOnly);
			}
		}

		public void TestShipmentAndBillingDetailsLinkLabelVisibility()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateGatewayConsolsAndShipments().gC0001;
			Factory.Save();

			using (var control = new JobChargeUserControl())
			using (var form = new ZForm(consol))
			{
				control.Bind(creator.CreateJob(consol, false));
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				Assert("Invisible by default", !control.ShipmentAndBillingDetailsLinkLabel.Visible);

				control.ToggleShipmentAndBillingDetailsIsActive(true);
				Assert("Visible once toggled", control.ShipmentAndBillingDetailsLinkLabel.Visible);
				AssertNull("Doesn't create details before clicked", control.ShipmentAndBillingDetails);

				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNotNull("Creates details when clicked", control.ShipmentAndBillingDetails);
				Assert("Opens details form when clicked", OpenedFormCache.GetInstance().Contains(control.ShipmentAndBillingDetails.PK.ToGuid(), "ShipmentAndBillingDetails"));

				var cachedDetails = control.ShipmentAndBillingDetails;
				control.ToggleShipmentAndBillingDetailsIsActive(false);
				Assert("Invisible when toggled again", !control.ShipmentAndBillingDetailsLinkLabel.Visible);
				AssertNull("Removes details when link toggled off", control.ShipmentAndBillingDetails);
				Assert("Closes form when link toggled off", !OpenedFormCache.GetInstance().Contains(cachedDetails.PK.ToGuid(), "ShipmentAndBillingDetails"));
			}
		}

		public void TestShipmentAndBillingDetailsLinkLabel_OnClick()
		{
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateGatewayConsolsAndShipments().gC0001;
			Factory.Save();

			using (var control = new JobChargeUserControl())
			using (var form = new ZForm(consol))
			{
				control.Bind(creator.CreateJob(consol, false));
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				control.ToggleShipmentAndBillingDetailsIsActive(true);
				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNotNull("Creates details when clicked", control.ShipmentAndBillingDetails);

				AssertEquals("Details Contains row for each shipment in bound consol", consol.Shipments.Count, control.ShipmentAndBillingDetails.DetailsRows.Count);
				foreach (ForwardingShipment shipment in consol.Shipments)
				{
					Assert("Details Contains row for each shipment in bound consol", control.ShipmentAndBillingDetails.DetailsRows.Cast<ShipmentAndBillingDetailsRow>().Any(x => x.RelatedJobNum == shipment.JobNumber));
				}

				Assert("Opens details form when clicked", OpenedFormCache.GetInstance().Contains(control.ShipmentAndBillingDetails.PK.ToGuid(), "ShipmentAndBillingDetails"));

				var cachedDetails = control.ShipmentAndBillingDetails;
				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNull("Removes details when clicked while form is open", control.ShipmentAndBillingDetails);
				Assert("Closes form when clicked while form is open", !OpenedFormCache.GetInstance().Contains(cachedDetails.PK.ToGuid(), "ShipmentAndBillingDetails"));
			}
		}

		public void TestShipmentAndBillingDetailsHighlightsAccordingToChargeGrid()
		{
			var creator = new TestObjectCreator(Factory);
			var setup = creator.CreateGatewayConsolsAndShipments();
			var consol = setup.gC0001;
			var job = creator.CreateJob(consol);
			Factory.Save();

			var charge1 = job.Charges.AddNew();
			charge1.JR_Calc_RelatedJobNumber = setup.s0002.JobNumber;
			var charge2 = job.Charges.AddNew();
			charge2.JR_Calc_RelatedJobNumber = setup.s0003.JobNumber;

			using (var control = new JobChargeUserControl())
			using (var form = new ZForm(consol))
			{
				control.Bind(job);
				((ICompositeControlBindingSourceProvider)form).BindingSource.SetBindingMember(control, "");
				form.Controls.Add(control);
				form.Show();

				AssertNoExceptionThrown("Should not break when form isn't opened", () =>
				{
					control.JobChargeBoundGrid.SelectSingleElementByPK(charge1.PK);
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(control.JobChargeBoundGrid.CurrentRowIndex, 1);
					Application.DoEvents();
				});

				control.ToggleShipmentAndBillingDetailsIsActive(true);
				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNotNull("Creates details when clicked", control.ShipmentAndBillingDetails);
				var formPK = control.ShipmentAndBillingDetails.PK.ToGuid();
				var detailsForm = OpenedFormCache.GetInstance().GetForm(formPK, "ShipmentAndBillingDetails") as ShipmentAndBillingDetailsForm;
				AssertNotNull("Opens details form when clicked", detailsForm);

				control.JobChargeBoundGrid.SelectSingleElementByPK(charge2.PK);
				control.JobChargeBoundGrid.CurrentCell = new DataGridCell(control.JobChargeBoundGrid.CurrentRowIndex, 2);
				Application.DoEvents();
				var selectedDetails = detailsForm.DetailsGrid.GetFirstSelectedRow() as ShipmentAndBillingDetailsRow;
				AssertNotNull("A row should be selected", selectedDetails);
				AssertEquals("Select row matching selected charge", setup.s0003.JobNumber, selectedDetails.RelatedJobNum);

				control.JobChargeBoundGrid.SelectSingleElementByPK(charge1.PK);
				control.JobChargeBoundGrid.CurrentCell = new DataGridCell(control.JobChargeBoundGrid.CurrentRowIndex, 3);
				Application.DoEvents();
				selectedDetails = detailsForm.DetailsGrid.GetFirstSelectedRow() as ShipmentAndBillingDetailsRow;
				AssertNotNull("A row should be selected", selectedDetails);
				AssertEquals("Select row matching selected charge", setup.s0002.JobNumber, selectedDetails.RelatedJobNum);

				control.ShipmentAndBillingDetailsLinkLabel.OnLinkClicked_Exposed(null);
				AssertNull(" Removes details when clicked", control.ShipmentAndBillingDetails);
				Assert("Closes details form when clicked", !OpenedFormCache.GetInstance().Contains(formPK, "ShipmentAndBillingDetails"));

				AssertNoExceptionThrown("Should not break when form isn't opened", () =>
				{
					control.JobChargeBoundGrid.SelectSingleElementByPK(charge2.PK);
					control.JobChargeBoundGrid.CurrentCell = new DataGridCell(control.JobChargeBoundGrid.CurrentRowIndex, 4);
					Application.DoEvents();
				});
			}
		}

		#endregion

		#region RelatedJobNumber and InvoiceTarget Column Visibility

		public void TestRelatedJobNumberAndInvoiceTargetColumnVisibility()
		{
			var creator = new TestObjectCreator(Factory);
			var gatewayConsol = creator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var consolJobHeader = creator.CreateJob(gatewayConsol);

			var shipment = creator.CreateShipment("S0001");
			gatewayConsol.Shipments.Add(shipment);
			var shipmentJobHeader = creator.CreateJob(shipment);
			Factory.Save();

			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.Controls.OfType<ZGrid>().Single(x => x.Name == "JobChargeBoundGrid");

				control.Bind(consolJobHeader);
				Assert(!chargesGrid.GetColumnStyle(nameof(BaseCharge.JR_Calc_RelatedJobNumber)).IsUnavailable);
				Assert(!chargesGrid.GetColumnStyle(nameof(BaseCharge.JR_Calc_InvoiceTarget)).IsUnavailable);
			}

			using (var control = new JobChargeUserControl())
			{
				var chargesGrid = control.Controls.OfType<ZGrid>().Single(x => x.Name == "JobChargeBoundGrid");

				control.Bind(shipmentJobHeader);
				Assert(chargesGrid.GetColumnStyle(nameof(BaseCharge.JR_Calc_RelatedJobNumber)).IsUnavailable);
				Assert(chargesGrid.GetColumnStyle(nameof(BaseCharge.JR_Calc_InvoiceTarget)).IsUnavailable);
			}
		}

		#endregion

		public void TestTaxExpenseTotalsPanelVisibility()
		{
			var job = TestObjectCreator.Job1;
			var taxConfig = Factory.New<AccTaxConfiguration>();
			taxConfig.ETC_ParentTableCode = GlbCompanySchema.Constants.Prefix;

			taxConfig.ETC_ParentId = job.Company.PK;
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				Assert(control.TaxExpenseTotalsPanel.Visible);
			}

			taxConfig.ETC_ParentId = ZGuid.Empty;
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				Assert(!control.TaxExpenseTotalsPanel.Visible);
			}
		}

		public void TestJobDependentCaptionResourceStrings_NonGateway()
		{
			using (var control = new JobChargeUserControl())
			{
				control.Bind(TestObjectCreator.Job1);
				var caption = control.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString;
				AssertEquals("Overseas Agent", caption.Caption);
				AssertEquals(string.Empty, caption.FullDescription);

				caption = control.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString;
				AssertEquals("Local Client", caption.Caption);
				AssertEquals(string.Empty, caption.FullDescription);

				caption = control.JR_JH_InternalJobGuidFindBox.CaptionResourceString;
				AssertEquals(ResourceStringData.Empty, caption);
			}
		}

		public void TestJobDependentCaptionResourceStrings_Gateway()
		{
			var consol = TestObjectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			using (var job = TestObjectCreator.CreateJob(consol))
			using (var control = new JobChargeUserControl())
			{
				control.Bind(job);
				var caption = control.JH_OH_AgentCollectBoundOrgCard.CaptionResourceString;
				AssertEquals("Collect Agent", caption.Caption);
				AssertEquals("Agent who collects revenue for this gateway billing job and/or handles the consol at destination.", caption.FullDescription);

				caption = control.JH_OH_LocalChargesBoundOrgCard.CaptionResourceString;
				AssertEquals("Prepaid Agent", caption.Caption);
				AssertEquals("Agent who collects revenue for this gateway billing job and/or handles the consol at origin.", caption.FullDescription);

				caption = control.JR_JH_InternalJobGuidFindBox.CaptionResourceString;
				AssertEquals("Internal Job", caption.Caption);
				AssertEquals(@"Please enter or select an internal job related to this charge. To record gateway revenue as cost on a specific job, enter or select this job number.
To apportion gateway revenue as cost on multiple shipments attached to this consol, set Internal Job to the current consol number and review the Gateway Sell Apportionment tab.", caption.FullDescription);
			}
		}

		public void TestResourceString_JR_OSCostGSTAmtAndJR_OSSellGSTAmt()
		{
			var testJob = Factory.NewJobForTesting<Job>();
			var parent = new InvoicingParam();
			parent.ConsumerType = JobInvoicingConsumerTypes.Shipment;
			testJob.PlugInData = parent;

			using (var control = new JobChargeUserControl())
			{
				control.Bind(testJob);

				var jR_OSCostAmtResString = new ResourceStringKeyCalculator(control.JobChargeBoundGrid, "JR_OSCostAmt").DataString.FullDescription;
				var jR_OSSellAmtResString = new ResourceStringKeyCalculator(control.JobChargeBoundGrid, "JR_OSSellAmt").DataString.FullDescription;

				AssertEquals("Please enter the cost amount in the nominated cost currency.", jR_OSCostAmtResString);
				AssertEquals("Please enter the sell amount in the nominated sell currency.", jR_OSSellAmtResString);
			}
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new BusinessObjectFactory();
				}

				return fFactory;
			}
		}
		BusinessObjectFactory fFactory;

		TestObjectCreator TestObjectCreator
		{
			get
			{
				return fCreator ?? (fCreator = new TestObjectCreator(Factory));
			}
		}
		TestObjectCreator fCreator;

		#endregion
	}
}
