using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class PeriodicInvoiceControlTest : TestCaseWithFactory
	{
		public void TestRefreshMenuItem()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			var invoiceType = TestOrg.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.CFSLoadList.Code;
			invoiceType.PI_Type = "CHG";

			var consol = (CommonConsol)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.CFSLoadList.BizoType);
			Assert(consol.JK_IsCFS);

			var jobHeader = new JobHeader.Loader((IJobHeaderParent)consol).TryCreateWithoutMutexForTestOnly();

			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jobCharge = Factory.NewWithValidTestData<Charge>();

			jobCharge.JR_OH_SellAccount = TestOrg.PK;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge.JR_OSSellAmt = 100M;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_InvoiceType = "CUD";
			TestObjectCreator.CreateWIP(jobCharge);

			Factory.Save();

			using (AccountingZForm testForm = GetFormForTest())
			{
				var periodicInvoiceBase = (PeriodicInvoiceBase)testForm.BusinessEntity;
				periodicInvoiceBase.JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code))).ToList().ForEach(x => x.Value = true);
				var control = GetControlForTest();

				testForm.Controls.Add(control);
				testForm.Show();

				var refreshMenuItem = control.JobsGrid_ForTestOnly.ContextMenu.MenuItems.FindByText("Refresh");
				AssertNotNull(refreshMenuItem);

				if (testForm.BusinessEntity is PeriodicInvoice)
				{
					var periodicInvoice = (PeriodicInvoice)testForm.BusinessEntity;
					periodicInvoice.DebtorPK = TestOrg.PK;
					periodicInvoice.InvoiceType = "CUD";
				}
				periodicInvoiceBase.CurrencyNK = TestObjectCreator.USD.RX_Code;
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, periodicInvoiceBase.Jobs.Count);

				AssertEquals(100m, periodicInvoiceBase.Jobs[0].JH_OSAmountForPeriodicBilling);

				jobCharge.JR_OSSellAmt = 200M;
				Factory.Save();

				AssertEquals(100m, periodicInvoiceBase.Jobs[0].JH_OSAmountForPeriodicBilling);

				refreshMenuItem.PerformClick();
				AssertEquals("Please select message", "Please select one Job to refresh.", UnitTestUserNotification.Instance.LastMessage.Text);

				control.JobsGrid_ForTestOnly.Select(0);
				refreshMenuItem.PerformClick();
				Application.DoEvents();

				AssertEquals(200m, periodicInvoiceBase.Jobs[0].JH_OSAmountForPeriodicBilling);
			}
		}

		public void TestClearJobsNotificationsWhenPerformingSearch()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgInvoiceType invoiceType = TestOrg.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = "SHP";
			invoiceType.PI_Type = "CHG";

			var shipment = TestObjectCreator.CreateShipment("S001");
			var job = TestObjectCreator.CreateJob(shipment);
			var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge1", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestOrg);
			var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Charge2", TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 200m, null);
			charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
			charge1.JR_OSSellExRate = charge2.JR_OSSellExRate = 1m;
			Factory.Save();

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceControl control = GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				var periodicInvoice = testForm.BusinessEntity as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestOrg.PK;
					periodicInvoice.InvoiceType = "FID";
				}
				((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = TestObjectCreator.AUD.RX_Code;
				((PeriodicInvoiceBase)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code))).ToList().ForEach(x => x.Value = true);
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);

				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(1, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);

				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);
			}
		}

		public void TestCurrencyNKInfo_ValueChanged()
		{
			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceControl control = GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();

				((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = Factory.NewWithValidTestData<RefCurrency>().RX_Code;
				Assert(control.LocalExTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(control.LocalTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(control.LocalTotalAmountCalcFindBox_ForTestOnly.Visible);
				Assert(control.LocalExtraTaxAmount_ForTestOnly.Visible);

				((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				Assert(!control.LocalExTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalTotalAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalExtraTaxAmount_ForTestOnly.Visible);

				((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = ZString.Empty;
				Assert(!control.LocalExTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalTaxAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalTotalAmountCalcFindBox_ForTestOnly.Visible);
				Assert(!control.LocalExtraTaxAmount_ForTestOnly.Visible);
			}
		}

		public void TestJobsPerformSearchAndClear()
		{
			var periodHelper = new AccountingPeriodTestHelper(Factory);
			periodHelper.SetupPeriods();

			OrgInvoiceType invoiceType = TestOrg.CompanyData.InvoiceTypes.AddNew();
			invoiceType.PI_Module = JobInvoicingConsumerTypes.CFSLoadList.Code;
			invoiceType.PI_Type = "CHG";

			var consol = (CommonConsol)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.CFSLoadList.BizoType);
			var consol2 = (CommonConsol)Factory.NewWithValidTestData(JobInvoicingConsumerTypes.CFSLoadList.BizoType);
			Assert(consol.JK_IsCFS);
			Assert(consol2.JK_IsCFS);

			var jobHeader = new JobHeader.Loader((IJobHeaderParent)consol).TryCreateWithoutMutexForTestOnly();
			var jobHeader2 = new JobHeader.Loader((IJobHeaderParent)consol2).TryCreateWithoutMutexForTestOnly();
			jobHeader.JH_GE = GlbDepartment.CurrentDepartment.PK;
			jobHeader2.JH_GE = GlbDepartment.CurrentDepartment.PK;
			var jobCharge = Factory.NewWithValidTestData<Charge>();
			var jobCharge2 = Factory.NewWithValidTestData<Charge>();
			var jobCharge3 = Factory.NewWithValidTestData<Charge>();
			var jobCharge4 = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OH_SellAccount = TestOrg.PK;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge.JR_OSSellAmt = 100M;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_InvoiceType = "CUD";
			TestObjectCreator.CreateWIP(jobCharge);

			jobCharge2.JR_OH_SellAccount = TestOrg.PK;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge2.JR_OSSellAmt = 200M;
			jobCharge2.JR_JH = jobHeader2.PK;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge2.JR_InvoiceType = "CUD";
			TestObjectCreator.CreateWIP(jobCharge2);

			jobCharge3.JR_OH_SellAccount = TestOrg.PK;
			jobCharge3.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge3.JR_JH = jobHeader.PK;
			jobCharge3.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge3.JR_InvoiceType = "CUD";

			jobCharge4.JR_OH_SellAccount = TestOrg.PK;
			jobCharge4.JR_RX_NKSellCurrency = TestObjectCreator.USD.RX_Code;
			jobCharge4.JR_JH = jobHeader2.PK;
			jobCharge4.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge4.JR_InvoiceType = "CUD";

			Factory.Save();

			using (AccountingZForm testForm = GetFormForTest())
			{
				((PeriodicInvoiceBase)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code))).ToList().ForEach(x => x.Value = true);
				PeriodicInvoiceControl control = GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				((PeriodicInvoice)testForm.BusinessEntity).DebtorPK = TestOrg.PK;
				((PeriodicInvoice)testForm.BusinessEntity).InvoiceType = "CUD";
				((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = TestObjectCreator.USD.RX_Code;
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);

				((PeriodicInvoiceBase)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code))).ToList().ForEach(x => x.Value = true);
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);
				AssertEquals("Found 2 records that match your criteria.", control.JobsInfoLabel_ForTestOnly.Text);

				((PeriodicInvoiceBase)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code))).ToList().ForEach(x => x.Value = false);
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);
				AssertContains("At least one Job Type should be selected.", UnitTestUserNotification.Instance.LastMessage.Text);

				((PeriodicInvoiceBase)testForm.BusinessEntity).JobTypeList.Where(x => (x.Description.Contains(JobInvoicingConsumerTypes.CFSLoadList.Code))).ToList().ForEach(x => x.Value = true);
				control.JobsFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);
				control.JobsFilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).Jobs.Count);
				AssertEquals("Found 0 records that match your criteria.", control.JobsInfoLabel_ForTestOnly.Text);
			}
		}

		public void TestMicsInvoicesPerformSearchAndClear()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice1.AH_OH = TestOrg.PK;
			aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			ARInvoice aRInvoice2 = Factory.NewWithValidTestData(typeof(ARInvoice)) as ARInvoice;
			aRInvoice2.AH_OH = TestOrg.PK;
			aRInvoice2.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice2.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			Factory.Save();

			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceControl control = GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();

				var periodicInvoice = (PeriodicInvoice)testForm.BusinessEntity;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.DestinationChargesInvoice_Batching;

				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals("Error Message", "Error - DebtorPK: Please enter a value.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).MiscInvoices.Count);

				periodicInvoice.DebtorPK = TestOrg.PK;
				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).MiscInvoices.Count);

				((PeriodicInvoice)testForm.BusinessEntity).InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBase)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals("Found 2 records that match your criteria.", control.MiscInvoicesInfoLabel_ForTestOnly.Text);

				control.MiscInvoicesFilterControl_PerformSearch_ForTestOnly(this, new EventArgs());
				AssertEquals(2, ((PeriodicInvoiceBase)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals("Found 2 records that match your criteria.", control.MiscInvoicesInfoLabel_ForTestOnly.Text);

				control.MiscInvoicesFilterControl_ClearButtonClicked_ForTestOnly(this, new EventArgs());
				AssertEquals(0, ((PeriodicInvoiceBase)testForm.BusinessEntity).MiscInvoices.Count);
				AssertEquals("Found 0 records that match your criteria.", control.MiscInvoicesInfoLabel_ForTestOnly.Text);
			}
		}

		public void TestTaxBranchRelatedColumnsVisibility()
		{
			AssertTaxBranchColumnVisibility(true, true);
			AssertTaxBranchColumnVisibility(true, false);
			AssertTaxBranchColumnVisibility(false, true);
			AssertTaxBranchColumnVisibility(false, false);

			void AssertTaxBranchColumnVisibility(bool isEnaleRegistry, bool isGSTRegistered)
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = isGSTRegistered;
				using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isEnaleRegistry))
				using (AccountingZForm testForm = GetFormForTest())
				{
					var control = GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					var expectedVisible = isEnaleRegistry && isGSTRegistered;

					AssertEquals(expectedVisible, control.JobsGrid_ForTestOnly.Columns.Contains(PeriodicInvoiceSelectableJob.Schema.JH_GB_TaxBranch));
					AssertEquals(expectedVisible, control.JobsGrid_ForTestOnly.Columns.Contains(PeriodicInvoiceSelectableJob.Schema.ChargeTaxBranches));
				}
			}
		}

		public void TestHideNonApplicableControls()
		{
			using (AccountingZForm testForm = GetFormForTest())
			{
				PeriodicInvoiceControl control = GetControlForTest();
				testForm.Controls.Add(control);
				testForm.Show();
				System.Windows.Forms.Application.DoEvents();
				control.PreparePeriodicInvoiceControl();

				AssertEquals("JH_OSExtraTaxAmount should not be avaliable", true, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).IsUnavailable);
				AssertEquals("JH_LocalExtraTaxAmount should not be avaliable", true, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).IsUnavailable);
				AssertEquals(control.InternalLocalExtraTaxAmount.Visible, false);
				AssertEquals(control.InternalOSExtraTaxAmount.Visible, false);
			}

			string oldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			try
			{
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.India);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.India);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceControl control = GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("JH_OSExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).IsUnavailable);
					AssertEquals("JH_LocalExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).IsUnavailable);
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount), null, "SGST Amount", "SGST Amt");
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount), null, "SGST Local", "");
				}
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Canada);
				Factory.Save();
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceControl control = GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("JH_OSExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).IsUnavailable);
					AssertEquals("JH_LocalExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).IsUnavailable);
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount), null, "QST Amount", "QST Amt");
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount), null, "QST Local", "");
				}
				GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Mexico);
				AssertEquals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Core.Constants.CountryCodes.Mexico);
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceControl control = GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();
					System.Windows.Forms.Application.DoEvents();
					control.PreparePeriodicInvoiceControl();

					AssertEquals("JH_OSExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount).IsUnavailable);
					AssertEquals("JH_LocalExtraTaxAmount should be avaliable", false, control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount).IsUnavailable);
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_OSExtraTaxAmount), null, "RET Amount", "RET Amt");
					AssertColumnCaption(control.InternalJobsGrid.GetColumnStyle(Job.Schema.JH_LocalExtraTaxAmount), null, "RET Local", "");
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.SetCountry(oldCountry);
			}
		}

		public void TestAmountDecimalPlaces()
		{
			var prevValue = GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio;
			try
			{
				using (AccountingZForm testForm = GetFormForTest())
				{
					PeriodicInvoiceControl control = GetControlForTest();
					testForm.Controls.Add(control);
					testForm.Show();

					((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = "JPY";
					GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 2;
					AssertEquals(control.OSExTaxAmountCalcFindBox_ForTestOnly.Decimals, 0);
					AssertEquals(control.LocalExTaxAmountCalcFindBox_ForTestOnly.Decimals, 2);
					AssertEquals(control.OSExtraTaxAmount_ForTestOnly.Decimals, 0);
					AssertEquals(control.LocalExtraTaxAmount_ForTestOnly.Decimals, 2);

					((PeriodicInvoiceBase)testForm.BusinessEntity).CurrencyNK = "TND";
					GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 0;
					AssertEquals(control.OSTotalAmountCalcFindBox_ForTestOnly.Decimals, 3);
					AssertEquals(control.LocalTotalAmountCalcFindBox_ForTestOnly.Decimals, 0);
					AssertEquals(control.OSTaxAmountCalcFindBox_ForTestOnly.Decimals, 3);
					AssertEquals(control.LocalTaxAmountCalcFindBox_ForTestOnly.Decimals, 0);
				}
			}
			finally
			{
				GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = prevValue;
			}
		}

		public void TestColumnsAddedCorrectly()
		{
			AssertColumnAddedCorrectly("ServiceLevel");
		}

		void AssertColumnAddedCorrectly(string columnName)
		{
			using (var form = GetFormForTest())
			{
				using (var userControl = GetControlForTest())
				{
					form.Controls.Add(userControl);
					form.Show();

					var expectedColumn = columnName;
					var columns = userControl.JobsGrid_ForTestOnly.ColumnStyles.Cast<ZGridColumnInfo>();
					Assert("New column should be added.", columns.Any(x => x.ColumnName == expectedColumn));
					Assert("New column should be visibled.", columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsVisible);
					Assert("New column should not be read only.", !columns.FirstOrDefault(x => x.ColumnName == expectedColumn).IsReadOnly);
				}
			}
		}

		protected void AssertColumnCaption(ZGridColumnInfo zGridColumnInfo, string caption, string captionResourceStringCaption, string captionResourceStringShortCaption)
		{
			AssertEquals(caption, zGridColumnInfo.Caption);
			AssertEquals(captionResourceStringCaption, zGridColumnInfo.CaptionResourceString.Caption);
			AssertEquals(captionResourceStringShortCaption, zGridColumnInfo.CaptionResourceString.ShortCaption);
		}

		#region Implementation

		protected virtual PeriodicInvoiceControl GetControlForTest()
		{
			return new PeriodicInvoiceControl();
		}

		protected virtual AccountingZForm GetFormForTest()
		{
			return new PeriodicInvoicingForm(new PeriodicInvoice(Factory));
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}
		protected TestObjectCreator fTestObjectCreator;

		protected OrgHeader TestOrg
		{
			get { return fTestOrg ?? (fTestOrg = TestObjectCreator.ABIGAS); }
		}
		OrgHeader fTestOrg;

		#endregion
	}
}
