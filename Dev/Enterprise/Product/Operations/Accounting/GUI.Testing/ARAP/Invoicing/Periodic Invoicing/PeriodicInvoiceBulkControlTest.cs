using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Printing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.DocumentEngine;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Accounting.GUI.Testing
{
	public class PeriodicInvoiceBulkControlTest : PeriodicInvoiceControlTest
	{
		public void TestErrorMessageShownWhenRelatedChargeHasCriticalPostErrorAfterPreviewPeriodicInvoice()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ZArchitecture.Modules.ISecurityCheckpoint>()))
				.Returns(false);

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				var debtor = TestObjectCreator.CreateOrgHeader("TSTORG1", false, true);
				debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				OrgInvoiceType type = debtor.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";

				var creditor = TestObjectCreator.CreateOrgHeader("TSTORG2", true, false);
				creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;

				var shipment = TestObjectCreator.CreateShipment("S001001");
				var job = TestObjectCreator.CreateJob(shipment, false, false);
				job.JH_OA_LocalChargesAddr = TestObjectCreator.ABIGAS.Addresses.MainAddress.PK;

				var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 100M, 120M);
				charge.JR_OH_SellAccount = debtor.PK;
				charge.JR_InvoiceType = "FID";
				charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				charge.JR_JH = job.PK;
				charge.JR_GB = GlbBranch.CurrentBranch.PK;
				charge.JR_AT_CostGSTRate = Guid.Empty;
				charge.JR_OH_CostAccount = creditor.PK;
				Factory.Save();

				creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
				Factory.Save();

				try
				{
					using (AccountingZForm form = GetFormForTest())
					{
						((PeriodicInvoiceBase)form.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
						((PeriodicInvoiceBase)form.BusinessEntity).CurrencyNK = TestObjectCreator.AUD.RX_Code;

						var control = (PeriodicInvoiceBulkControl)GetControlForTest();
						form.Controls.Add(control);
						form.Show();

						control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
						AssertEquals(1, ((PeriodicInvoiceBulk)form.BusinessEntity).Jobs.Count);
						AssertEquals(1, ((PeriodicInvoiceBulk)form.BusinessEntity).PeriodicInvoices.Count);

						control.PeriodicInvoicesOnJobsGrid_ForTestOnly.SelectAllElements();
						var periodicInvoiceSelected = (PeriodicInvoice)control.PeriodicInvoicesOnJobsGrid_ForTestOnly.SelectedElements[0];
						control.PeriodicInvoicesOnJobsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Preview Invoice").PerformClick();

						var expectedErrorMsg = @"Error occurred for the following Job(s): S001001

Summary of the error(s):
Error - Cost Tax ID: Tax IDs on unposted charges conflict with the creditor ""Tax is Applicable"" flag.
One possible way to resolve this is to go into the ""Job Invoicing"" menu and click ""Reset Unposted lines Tax Default"" option.";
						AssertEquals("Should show error message when related charge contain critical post error.", expectedErrorMsg, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					job.Dispose();
				}
			}
		}

		public void TestPreviewInvoiceContextMenu()
		{
			var mockIPrintTaskUIProvider = new Mock<IPrintTaskUIProvider>();
			mockIPrintTaskUIProvider.Setup(m => m.ShowDocDeliveryUI(It.IsAny<PrintTask>(), It.IsAny<DeliveryInstructions>(), It.IsAny<ZArchitecture.Modules.ISecurityCheckpoint>()))
				.Returns(false);

			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (new PrintTaskUIProviderFactory.OverriderForTesting(mockIPrintTaskUIProvider.Object))
			{
				OrgInvoiceType type = TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
				type.PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
				type.PI_Interval = InvoiceTypeBillingInterval.Codes.MTH;
				type.PI_Type = InvoiceTypeLayoutList.Codes.CHG;
				type.PI_RS_NKServiceLevel = "STD";

				var shipment = TestObjectCreator.CreateShipment("S1");
				var job = TestObjectCreator.CreateJob(shipment);
				job.LocalChargesPK = TestObjectCreator.ABIGAS.PK;

				var charge = job.Charges.AddNew();
				try
				{
					charge.FillWithValidTestData();
					charge.JR_AC = TestObjectCreator.CC1.PK;
					charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
					charge.JR_OSSellAmt = 100m;
					charge.JR_LocalSellAmt = 100m;
					charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					Factory.Save();

					using (AccountingZForm form = GetFormForTest())
					{
						((PeriodicInvoiceBase)form.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
						((PeriodicInvoiceBase)form.BusinessEntity).CurrencyNK = TestObjectCreator.AUD.RX_Code;

						var control = (PeriodicInvoiceBulkControl)GetControlForTest();
						form.Controls.Add(control);
						form.Show();

						control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
						AssertEquals(1, ((PeriodicInvoiceBulk)form.BusinessEntity).Jobs.Count);
						AssertEquals(1, ((PeriodicInvoiceBulk)form.BusinessEntity).PeriodicInvoices.Count);

						control.PeriodicInvoicesOnJobsGrid_ForTestOnly.SelectAllElements();
						var periodicInvoiceSelected = (PeriodicInvoice)control.PeriodicInvoicesOnJobsGrid_ForTestOnly.SelectedElements[0];
						control.PeriodicInvoicesOnJobsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Preview Invoice").PerformClick();
						var invoiceCreated = periodicInvoiceSelected.previewPostManager_ForTestOnly.Poster.PostedInvoices[0];
						AssertNotNull(invoiceCreated);

						var printTask = new InvoicePrintTask(new InvoicePrintTask.Configuration(invoiceCreated));
						AssertEquals(1, printTask.TaskCount);

						AssertEquals("Previewing doesn't post a transaction via PostManager", 0, periodicInvoiceSelected.PostManager.Poster.PostedInvoices.Count);

						var newFactory = new BusinessObjectFactory();
						var reloadCharge = newFactory.Load<Charge>(charge.PK);
						reloadCharge.Delete();
						newFactory.Save();

						control.PeriodicInvoicesOnJobsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Preview Invoice").PerformClick();
						AssertEquals("Deleted charge should trigger an error of preview.",
							@"This invoice can't be previewed due to the following:
Since this invoice screen was opened, one of the billing lines included in this invoice has been changed.
Error - record: Charge count for the invoice was changed from 1 to 0.
Error - record: Total Local Ex Tax Amount of this invoice has changed from $100.00 to $0.00.
You will need to cancel this screen and begin the periodic invoice process again.",
							UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}
				finally
				{
					job.Dispose();
				}
			}
		}

		public new void TestJobsPerformSearchAndClear()
		{
			PrepareData();

			using (AccountingZForm testForm = GetFormForTest())
			{
				((PeriodicInvoiceBulk)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				((PeriodicInvoiceBulk)testForm.BusinessEntity).CurrencyNK = TestObjectCreator.USD.RX_Code;

				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(1, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertEquals("Found 2 record(s) that match your criteria. Created 1 Periodic Invoice(s).", control.JobsInfoLabel_ForTestOnly.Text);

				((PeriodicInvoiceBulk)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = false);
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertContains("At least one Job Type should be selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				((PeriodicInvoiceBulk)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(1, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				control.JobsFilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertEquals("Found 0 record(s) that match your criteria. Created 0 Periodic Invoice(s).", control.JobsInfoLabel_ForTestOnly.Text);
			}
		}

		public new void TestMicsInvoicesPerformSearchAndClear()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			aRInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			aRInvoice2.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			aRInvoice2.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).MiscInvoices.Count);
				((PeriodicInvoiceBulk)testForm.BusinessEntity).CurrencyNK = TestObjectCreator.USD.RX_Code;

				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBulk)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals(1, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertEquals("Found 2 record(s) that match your criteria. Created 1 Periodic Invoice(s).", control.MiscInvoicesInfoLabel_ForTestOnly.Text);

				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBulk)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals(1, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertEquals("Found 2 record(s) that match your criteria. Created 1 Periodic Invoice(s).", control.MiscInvoicesInfoLabel_ForTestOnly.Text);
				control.MiscInvoicesFilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals(0, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);
				AssertEquals("Found 0 record(s) that match your criteria. Created 0 Periodic Invoice(s).", control.MiscInvoicesInfoLabel_ForTestOnly.Text);
			}
		}

		public new void TestHideNonApplicableControls()
		{
			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				System.Windows.Forms.Application.DoEvents();
				control.PreparePeriodicInvoiceControl();

				AssertEquals("OSExtraTaxAmount should not be avaliable", true, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount").IsUnavailable);
				AssertEquals("LocalExtraTaxAmount should not be avaliable", true, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount").IsUnavailable);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("OSExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount").IsUnavailable);
					AssertEquals("LocalExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount").IsUnavailable);
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount"), null, "SGST Amount", "SGST Amt");
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount"), null, "SGST Local", "");
				}
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("OSExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount").IsUnavailable);
					AssertEquals("LocalExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount").IsUnavailable);
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount"), null, "RET Amount", "RET Amt");
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount"), null, "RET Local", "");
				}
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				Factory.Save();
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("OSExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount").IsUnavailable);
					AssertEquals("LocalExtraTaxAmount should not be unavaliable", false, control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount").IsUnavailable);
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("OSExtraTaxAmount"), null, "QST Amount", "QST Amt");
					AssertColumnCaption(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("LocalExtraTaxAmount"), null, "QST Local", "");
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestTaxBranchAvailable()
		{
			AssertTaxBranchAvailable(true, true);
			AssertTaxBranchAvailable(true, false);
			AssertTaxBranchAvailable(false, true);
			AssertTaxBranchAvailable(false, false);

			void AssertTaxBranchAvailable(bool isEnableRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnableRegistry))
				using (var testForm = GetFormForTest())
				{
					var control = (PeriodicInvoiceBulkControl)GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					var expectedAvailable = isEnableRegistry && isGSTRegistered;

					var isAvailable = !(control.PeriodicInvoicesOnJobsGrid_ForTestOnly.GetColumnStyle("TaxBranch").IsUnavailable);
					AssertEquals(expectedAvailable, isAvailable);
				}
			}
		}

		public void TestDecimalPlaces()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();

				((PeriodicInvoiceBulk)testForm.BusinessEntity).CurrencyNK = "JPY";
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 2;
				AssertEquals(control.OSExTaxAmountCalcFindBox_ForTestOnly.Decimals, 0);
				AssertEquals(control.LocalExTaxAmountCalcFindBox_ForTestOnly.Decimals, 2);
				AssertEquals(control.OSExtraTaxAmount_ForTestOnly.Decimals, 0);
				AssertEquals(control.LocalExtraTaxAmount_ForTestOnly.Decimals, 2);

				((PeriodicInvoiceBulk)testForm.BusinessEntity).CurrencyNK = "TND";
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
				AssertEquals(control.OSTotalAmountCalcFindBox_ForTestOnly.Decimals, 3);
				AssertEquals(control.LocalTotalAmountCalcFindBox_ForTestOnly.Decimals, 0);
				AssertEquals(control.OSTaxAmountCalcFindBox_ForTestOnly.Decimals, 3);
				AssertEquals(control.LocalTaxAmountCalcFindBox_ForTestOnly.Decimals, 0);
			}
		}

		public void TestReadOnlyColumns()
		{
			PrepareData();

			using (AccountingZForm testForm = GetFormForTest())
			{
				((PeriodicInvoiceBulk)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				PeriodicInvoiceBulkControl control = (PeriodicInvoiceBulkControl)GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();

				((PeriodicInvoiceBulk)testForm.BusinessEntity).CurrencyNK = TestObjectCreator.USD.RX_Code;
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBulk)testForm.BusinessEntity).Jobs.Count);
				AssertEquals(1, ((PeriodicInvoiceBulk)testForm.BusinessEntity).PeriodicInvoices.Count);

				var currentAsBusinessObject = control.PeriodicInvoicesOnJobsGrid_ForTestOnly.ListManager.List[0] as IAccessBusinessObject;
				AssertNotNull(currentAsBusinessObject);
				AssertEquals("IncludeInThePeriodicInvoice", false, currentAsBusinessObject.IsPropertyReadOnly("IncludeInThePeriodicInvoice"));
				AssertEquals("DebtorPK", true, currentAsBusinessObject.IsPropertyReadOnly("DebtorPK"));
				AssertEquals("InvoiceType", true, currentAsBusinessObject.IsPropertyReadOnly("InvoiceType"));
				AssertEquals("InvoiceDate", true, currentAsBusinessObject.IsPropertyReadOnly("InvoiceDate"));
				AssertEquals("DueDate", true, currentAsBusinessObject.IsPropertyReadOnly("DueDate"));
				AssertEquals("InvoiceTerm", true, currentAsBusinessObject.IsPropertyReadOnly("InvoiceTerm"));
				AssertEquals("InvoiceTermDays", true, currentAsBusinessObject.IsPropertyReadOnly("InvoiceTermDays"));
				AssertEquals("CurrencyReadonlyLocalNK", true, currentAsBusinessObject.IsPropertyReadOnly("CurrencyReadonlyLocalNK"));
				AssertEquals("LocalExTaxAmount", true, currentAsBusinessObject.IsPropertyReadOnly("LocalExTaxAmount"));
				AssertEquals("LocalTaxAmount", true, currentAsBusinessObject.IsPropertyReadOnly("LocalTaxAmount"));
				AssertEquals("LocalTotalAmount", true, currentAsBusinessObject.IsPropertyReadOnly("LocalTotalAmount"));
				AssertEquals("CurrencyReadonlyLocalNK", true, currentAsBusinessObject.IsPropertyReadOnly("CurrencyReadonlyLocalNK"));
				AssertEquals("OSExTaxAmount", true, currentAsBusinessObject.IsPropertyReadOnly("OSExTaxAmount"));
				AssertEquals("OSTaxAmount", true, currentAsBusinessObject.IsPropertyReadOnly("OSTaxAmount"));
				AssertEquals("OSTotalAmount", true, currentAsBusinessObject.IsPropertyReadOnly("OSTotalAmount"));
			}
		}

		void PrepareData()
		{
			var periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();

			OrgInvoiceType invoiceType = TestOrg.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = "SHP";
			invoiceType.PI_Type = "CHG";
			IJobInvoicingPlugIn shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			IJobInvoicingPlugIn shipment2 = Factory.NewWithValidTestData<ForwardingShipment>();
			var jobHeader = new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader(shipment2).TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jobCharge = Factory.NewWithValidTestData<Charge>();
			var jobCharge2 = Factory.NewWithValidTestData<Charge>();
			var jobCharge3 = Factory.NewWithValidTestData<Charge>();
			var jobCharge4 = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge.JR_OH_SellAccount = TestOrg.PK;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge.JR_OSSellAmt = 100M;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_InvoiceType = "CUD";
			TestObjectCreator.CreateWIP(jobCharge);

			jobCharge2.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge2.JR_OH_SellAccount = TestOrg.PK;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge2.JR_OSSellAmt = 100M;
			jobCharge2.JR_JH = jobHeader2.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_InvoiceType = "CUD";
			TestObjectCreator.CreateWIP(jobCharge2);

			jobCharge3.JR_AC = TestObjectCreator.CC1.PK;
			jobCharge3.JR_OH_SellAccount = TestOrg.PK;
			jobCharge3.JR_InvoiceType = "CUD";
			jobCharge3.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge3.JR_JH = jobHeader.PK;
			jobCharge3.JR_GB = GlbBranch.CurrentBranch.PK;

			jobCharge4.JR_AC = TestObjectCreator.CC2.PK;
			jobCharge4.JR_OH_SellAccount = TestOrg.PK;
			jobCharge4.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge4.JR_JH = jobHeader2.PK;
			jobCharge4.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge4.JR_InvoiceType = "CUD";

			Factory.Save();
		}

		#region Implementation

		protected override PeriodicInvoiceControl GetControlForTest()
		{
			return new PeriodicInvoiceBulkControl();
		}

		protected override AccountingZForm GetFormForTest()
		{
			return new PeriodicInvoicingBulkForm(new PeriodicInvoiceBulk(Factory));
		}

		#endregion
	}
}
