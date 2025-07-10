using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	[TestedType(typeof(PeriodicInvoicingBulkForm))]
	public class PeriodicInvoicingBulkFormTest : AccountingZFormBasherTest
	{
		#region Implementation

		protected virtual PeriodicInvoiceBulk GetFormBizO()
		{
			return new PeriodicInvoiceBulk(Factory);
		}

		protected virtual PeriodicInvoicingBulkForm GetForm(PeriodicInvoiceBulk periodicInvoiceBulk)
		{
			return new PeriodicInvoicingBulkForm(periodicInvoiceBulk);
		}

		protected override Form GetFormToBashCore()
		{
			return GetForm(GetFormBizO());
		}

		void PreparePeriodicInvoiceTestData(TestObjectCreator testObjectCreator)
		{
			var type = testObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			var shipment1 = testObjectCreator.CreateShipment("S1");
			shipment1.JS_RS_NKServiceLevel = "STD";
			Job1 = testObjectCreator.CreateJob(shipment1);
			Job1.LocalChargesPK = testObjectCreator.ABIGAS.PK;
			Job1.JH_Status = JobHeaderStatus.Working.Code;

			var shipment2 = testObjectCreator.CreateShipment("S2");
			shipment2.JS_RS_NKServiceLevel = "STD";
			Job2 = testObjectCreator.CreateJob(shipment2);
			Job2.LocalChargesPK = testObjectCreator.ABIGAS.PK;
			Job2.JH_Status = JobHeaderStatus.Working.Code;

			var charge1 = Job1.Charges.AddNew();
			charge1.FillWithValidTestData();
			charge1.JR_AC = testObjectCreator.CC1.PK;
			charge1.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			charge1.JR_OSSellAmt = 100m;
			charge1.JR_LocalSellAmt = 100m;
			charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge1.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var charge2 = Job2.Charges.AddNew();
			charge2.FillWithValidTestData();
			charge2.JR_AC = testObjectCreator.CC1.PK;
			charge2.JR_OH_SellAccount = testObjectCreator.ABIGAS.PK;
			charge2.JR_OSSellAmt = 100m;
			charge2.JR_LocalSellAmt = 100m;
			charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge2.JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			charge2.JR_RX_NKSellCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		Job Job1, Job2;

		#endregion

		public override void TestFormVerb()
		{
			using (AccountingZForm testForm = (AccountingZForm)GetFormToBashCore())
			{
				testForm.DisplayMode = ODisplayMode.New;
				AssertEquals("Verb should be 'New'", "New", testForm.FormVerb);
			}
		}

		new public void TestPromptReversingReason()
		{
			Assert(true);
		}

		public void TestMicsInvoicesTabShowsOnlyWhenRegistrySetYes()
		{
			AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			using (PeriodicInvoicingBulkForm testForm = GetFormToBash() as PeriodicInvoicingBulkForm)
			{
				testForm.Show();

				Assert(!testForm.PeriodicInvoiceBulkControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceBulkControl_ForTestOnly.MiscInvoicesTabPage));
				Assert(testForm.PeriodicInvoiceBulkControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceBulkControl_ForTestOnly.JobsTabPage));
			}

			AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			using (PeriodicInvoicingBulkForm testForm = GetFormToBash() as PeriodicInvoicingBulkForm)
			{
				testForm.Show();

				Assert(testForm.PeriodicInvoiceBulkControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceBulkControl_ForTestOnly.MiscInvoicesTabPage));
				Assert(testForm.PeriodicInvoiceBulkControl_ForTestOnly.TabControl.Controls.Contains(testForm.PeriodicInvoiceBulkControl_ForTestOnly.JobsTabPage));
			}
		}

		public void TestDialogResult()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			OrgInvoiceType type = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
			type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
			type.PI_RS_NKServiceLevel = "STD";

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment);
			job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "DESC", TestObjectCreator.AUD, 100m, null, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);

			charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			Factory.Save();

			using (PeriodicInvoicingBulkForm form = (PeriodicInvoicingBulkForm)GetFormToBashCore())
			{
				form.Show();
				form.BusinessEntity.JobTypeList.Where(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).ToList().ForEach(x => x.Value = true);
				form.BusinessEntity.CurrencyNK = TestObjectCreator.AUD.RX_Code;

				form.BusinessEntity.LoadJobs();
				AssertEquals(1, form.BusinessEntity.Jobs.Count);
				AssertEquals(1, form.BusinessEntity.PeriodicInvoices.Count);

				form.BusinessEntity.RunPreSaveValidation();
				AssertNoErrors("Precondition: form bizo should allow to call Save method.", form.BusinessEntity);
				form.FireSaveButton();
				AssertEquals("DialogResult after pressing Save button", DialogResult.OK, form.DialogResult);
			}

			using (AccountingZForm form = (AccountingZForm)GetFormToBashCore())
			{
				form.Show();
				Application.DoEvents();
				form.CancelButton.PerformClick();
				Application.DoEvents();
				AssertNotEquals("DialogResult after pressing Cancel button", DialogResult.OK, form.DialogResult);
			}
		}

		public void TestShowPreSaveDialogs()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			TestObjectCreator.CreateJobShipmentWithFIDCharge("S001", TestObjectCreator.LocalClient, TestObjectCreator.CC1, 100M, -1000M);

			var testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
			testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testPeriodicInvoice.LoadJobs();
			TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			using (var form = new PeriodicInvoicingBulkForm(testPeriodicInvoice))
			{
				form.Show();
				Assert("Expect user has rights", !testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
				AssertEquals("Expect continueWithSave.Yes when user has rights", ContinueWithSave.Yes, form.ShowPreSaveDialogs_ForTestOnly());
			}

			var originalFirstAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed;
			var originalSecondAmountLevelAllows = Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed;
			try
			{
				var setting = TestObjectCreator.CreateAuthorizationModeAndSettings(100m, 200m);
				AccountingConfigurationRegistry.Instance.ReceivableAuthorizationModeAndSettings.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, setting);
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = false;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = false;
				using (var form = new PeriodicInvoicingBulkForm(testPeriodicInvoice))
				{
					form.Show();
					Assert("Expect user doesn't have rights", testPeriodicInvoice.CheckLevelSecurityRightsForPeriodicCreditNotes());
					AssertEquals("Expect continueWithSave.No when user doesn't have rights", ContinueWithSave.No, form.ShowPreSaveDialogs_ForTestOnly());
				}
			}
			finally
			{
				Env.Security.CreditAdjustmentNotePostingApprovalFirstLevelApproval.IsAllowed = originalFirstAmountLevelAllows;
				Env.Security.CreditAdjustmentNotePostingApprovalSecondLevelApproval.IsAllowed = originalSecondAmountLevelAllows;
			}
		}

		public void TestOnClosedIsCalledWhenClickSaveAndCloseButton()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);
			var shipment = TestObjectCreator.CreateShipment("S001");
			shipment.JS_RS_NKServiceLevel = "STD";
			using (Job job = TestObjectCreator.CreateJob(shipment))
			{
				job.JH_OA_LocalChargesAddr = TestObjectCreator.LocalClient.Addresses.MainAddress.PK;
				Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100M, 120M); //Factory.NewWithValidTestData<Charge>();
				TestObjectCreator.LocalClient.CompanyData.InvoiceTypes.AddNew();
				TestObjectCreator.LocalClient.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				TestObjectCreator.LocalClient.CompanyData.InvoiceTypes[0].PI_RS_NKServiceLevel = "STD";
				charge.JR_OH_SellAccount = TestObjectCreator.LocalClient.PK;
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge.JR_AT_SellGSTRate = TestObjectCreator.GST1.PK;
				Factory.Save();
			}

			PeriodicInvoiceBulk testPeriodicInvoice = new PeriodicInvoiceBulk(Factory);
			testPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			testPeriodicInvoice.LoadJobs();
			TestObjectCreator.LocalClient.CompanyData.ClearInvoiceTypeCache_ForTestOnly();

			using (PeriodicInvoicingBulkForm form = new PeriodicInvoicingBulkForm(testPeriodicInvoice))
			{
				bool isOnClosedCalled = false;
				form.Closed += delegate(object sender, EventArgs e)
				{ isOnClosedCalled = true; };
				form.Show();
				form.BusinessEntity.RunPreSaveValidation();
				AssertNoErrors("Precondition: form bizo should allow to call Save method.", form.BusinessEntity);
				form.FindAll<ZPostOrCancelButton>().First(b => b.Text == "P&ost && Close").PerformClick();
				Assert("Form OnClosed should be called", isOnClosedCalled);
			}
		}

		[TestDate(2016, 10, 20)]
		public void TestShowErrorMessageWhenPostInvoiceWithoutProperReasonCode()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			TestObjectCreator.CreateTestPeriodsForEntireYear(2016);

			var jobReasonCollection = new JobProfitLossReasonCodeCollection();
			var code = jobReasonCollection.AddNew();
			code.Code = "TST";

			var jobReasonParam = new JobProfitLossRequiringReasonParameters();
			jobReasonParam.LossThreshold = 0.05;
			jobReasonParam.ProfitThreshold = 0.05;
			var jobStatus = jobReasonParam.JobStatusCollection.AddNew();
			jobStatus.Code = JobHeaderStatus.JobInvoiced.Code;

			var changeStatusCollection = new CodeDescriptionBoolDisallowNewCollection();
			var changeStatus = changeStatusCollection.AddNew();
			changeStatus.Code = JobHeaderStatus.Working.Code;
			changeStatus.Bool = false;

			Factory.Save();

			PreparePeriodicInvoiceTestData(testObjectCreator);
			Factory.Save();

			using (AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonCollection))
			using (AccountingConfigurationRegistry.Instance.JobProfitLossRequiringReasonParameters.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				jobReasonParam))
			using (AccountingConfigurationRegistry.Instance.SetJobStatusToInvoicedWhenFirstARInvoicePosted.SetTemporaryValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty,
				changeStatusCollection))
			{
				var periodicInvoiceBulk = new PeriodicInvoiceBulk(new BusinessObjectFactory());
				periodicInvoiceBulk.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				periodicInvoiceBulk.LoadJobs();

				using (var form = new PeriodicInvoicingBulkForm(periodicInvoiceBulk))
				{
					form.ValidateAndSave_ForTestOnly();

					var notifications = periodicInvoiceBulk.GetErrors();
					AssertEquals("Should has Reason Code errors.", 2, notifications.Count());
					AssertContainsExactElementsInAnyOrder(new List<string>()
								{
									@"Error - JobReasonCode: Job S1 status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered. ",
									@"Error - JobReasonCode: Job S2 status will be changed to INV after posting the first AR Invoice. The Profit/Loss threshold settings require Profit/Loss reason to be set on this job before posting any AR invoices. Posting is prohibited and Invoice preview not available until Profit / Loss reason has been entered. "
								},
						notifications.Select(x => x.Message));

					Job1.JH_ProfitLossReasonCode = "TST";
					Job2.JH_ProfitLossReasonCode = "TST";
					Factory.Save();

					form.ValidateAndSave_ForTestOnly();
					AssertEquals("No errors with valid Reason Code.", false, periodicInvoiceBulk.HasErrors);
				}
			}
		}
	}
}
