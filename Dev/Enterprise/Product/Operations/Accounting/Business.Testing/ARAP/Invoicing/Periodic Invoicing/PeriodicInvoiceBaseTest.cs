using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.Periodic_Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class PeriodicInvoiceBaseTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected OrgHeader TestOrg
		{
			get { return fTestOrg ?? (fTestOrg = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader fTestOrg;

		protected TestObjectCreator TestObjectCreator
		{
			get { return TestObjectCreator_internal ?? (TestObjectCreator_internal = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator TestObjectCreator_internal;

		protected override void SetUp()
		{
			ReleaseFactory();
			RowFactory.LoadedFetchHintRecordingEnabled = true;

			base.SetUp();

			testPeriodicInvoice_internalValue = (PeriodicInvoiceBase)GetNewBusinessObject();
		}

		protected override void TearDown()
		{
			RowFactory.LoadedFetchHintRecordingEnabled = false;

			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = base.GetNewBusinessObject();
			result.Factory.RefreshEnabled = true;
			return result;
		}

		PeriodicInvoiceBase TestPeriodicInvoice
		{
			get { return testPeriodicInvoice_internalValue; }
		}
		protected PeriodicInvoiceBase testPeriodicInvoice_internalValue;

		protected virtual void SetupPeriodicInvoiceWithJobsAndMiscInvoices(PeriodicInvoiceBase invoice, ZGuid[] jobPks, ZGuid[] chargePks, ZGuid[] invoicePks)
		{
			if (jobPks != null && chargePks != null)
			{
				invoice.LoadJobs();
			}
			if (invoicePks != null)
			{
				invoice.LoadMiscInvoices();
			}
		}

		protected Charge CreateCharge(Job job, AccChargeCode chargeCode, RefCurrency sellCurrency, ZDecimal sellAmount, OrgHeader debtor, AccTaxRate taxRate, ZString invoiceType)
		{
			var charge = TestObjectCreator.CreateCharge(job, chargeCode, "", sellCurrency, sellAmount, null, sellCurrency, sellAmount, debtor);
			charge.JR_AT_SellGSTRate = taxRate.PK;
			charge.JR_InvoiceType = invoiceType;
			return charge;
		}

		#endregion

		#region Validation

		public void TestOSTaxAmount()
		{
			var warning = "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules.";
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			AssertEquals("precondition, triggering validation", 0m, TestPeriodicInvoice.OSTaxAmount);
			AssertNoWarning(TestPeriodicInvoice.OSTaxAmountInfo, warning);
			AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			TestPeriodicInvoice.ClearMiscInvoices(); // clear cache
			AssertEquals("precondition, triggering validation", 0m, TestPeriodicInvoice.OSTaxAmount);
			AssertNoWarning(TestPeriodicInvoice.OSTaxAmountInfo, warning);

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1.1234m);
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching);
				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1.1234m, 30.00m, 3.71m, 0.11m, 26.70m, 3.30m, 0.10m);
				AssertAmountsOnCharge("charge2", charge2, 1.1234m, 300.00m, 37.08m, 1.08m, 267.05m, 33.01m, 0.96m);

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				}
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, Array.Empty<ZGuid>()); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);

				AssertEquals("OSExTaxAmount", 330m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 40.79m, TestPeriodicInvoice.OSTaxAmount);

				AssertNoWarning(TestPeriodicInvoice.OSTaxAmountInfo, warning);

				AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				TestPeriodicInvoice.ClearMiscInvoices(); // clear cache
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, Array.Empty<ZGuid>()); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.
				AssertEquals("OSTaxAmount", 40.79m, TestPeriodicInvoice.OSTaxAmount);
				AssertHasWarning(TestPeriodicInvoice.OSTaxAmountInfo, warning);
			}
		}

		public void TestValidateCurrencyNK()
		{
			TestPeriodicInvoice.CurrencyNK = ZString.Empty;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasErrors(TestPeriodicInvoice.CurrencyNKInfo);

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);
		}

		public void TestValidateInvoiceDate()
		{
			TestPeriodicInvoice.InvoiceDate = ZDateTime.Empty;
			TestPeriodicInvoice.ValidateInvoiceDate();
			AssertHasErrors(TestPeriodicInvoice.InvoiceDateInfo);

			TestPeriodicInvoice.InvoiceDate = ZDateTime.Invalid;
			TestPeriodicInvoice.ValidateInvoiceDate();
			AssertHasErrors(TestPeriodicInvoice.InvoiceDateInfo);

			TestPeriodicInvoice.InvoiceDate = ZDateTime.BrettsBirthday;
			TestPeriodicInvoice.RunPreSaveValidation();
			AssertHasErrors(TestPeriodicInvoice.InvoiceDateInfo);

			TestPeriodicInvoice.InvoiceDate = ZDateTime.Now;
			TestPeriodicInvoice.ValidateInvoiceDate();
			AssertNoErrors(TestPeriodicInvoice.InvoiceDateInfo);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Portugal))
			{
				TestPeriodicInvoice.InvoiceDate = ZDateTime.Today.AddDays(3);
				TestPeriodicInvoice.ValidateInvoiceDate();
				AssertHasWarnings(TestPeriodicInvoice.InvoiceDateInfo);
				AssertEquals("Invoice Date is in the future. Please check the Invoice Date against the current system date and time. If this invoice is posted, future invoices cannot use current or previous date.", TestPeriodicInvoice.InvoiceDateInfo.Notifications.ToMessageListString());
			}

			AccountingMasterFilesRegistry.Instance.DisallowPostingInvoicesWithAFutureInvoiceDate.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestPeriodicInvoice.InvoiceDate = ZDateTime.Today.AddDays(3);
			TestPeriodicInvoice.ValidateInvoiceDate();
			AssertHasError(TestPeriodicInvoice.InvoiceDateInfo, "The invoice date cannot be in the future because the registry 'Accounting > Receivable > Default Settings > Disallow Posting Invoices With A Future Invoice Date' is set to Yes.");
		}

		public void TestValidateInvoiceDate_IInvoiceDateValidation()
		{
			var validationMock = new Mock<IInvoiceDateValidation>();
			var countryFactoryMock = new Mock<IAccountingCountryFactory>();
			countryFactoryMock.As<IInstanceProvider<IInvoiceDateValidation>>().Setup(x => x.Get()).Returns(validationMock.Object);
			var factoryMock = new Mock<IGlobalAccountingCountryFactory>();
			factoryMock.Setup(c => c.GetCountryFactory(It.IsAny<ZString>())).Returns(countryFactoryMock.Object);

			using (ObjectFactory.Substitute(factoryMock.Object))
			{
				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<ZDateTime>())).Returns((ResourceString)null);
				TestPeriodicInvoice.InvoiceDate = ZDateTime.Now;
				AssertNoErrors(TestPeriodicInvoice.InvoiceDateInfo);

				validationMock.Reset();

				validationMock.Setup(x => x.ValidateInvoiceDate(It.IsAny<ZDateTime>())).Returns(ResString.GetMultilingualString("Test", "Dummy Error"));
				TestPeriodicInvoice.InvoiceDate = ZDateTime.Now.AddDays(1);
				AssertHasError(TestPeriodicInvoice.InvoiceDateInfo, "Dummy Error");
			}
		}

		public void TestRunPreSaveValidation()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				TestPeriodicInvoice.InvoiceDate = ZDateTime.Empty;
				TestPeriodicInvoice.CurrencyNK = ZString.Empty;
				TestPeriodicInvoice.PostDate = ZDateTime.Empty;

				TestPeriodicInvoice.RunPreSaveValidation();

				AssertEquals(true, TestPeriodicInvoice.HasErrors);
				AssertHasErrors(TestPeriodicInvoice.InvoiceDateInfo);
				AssertHasErrors(TestPeriodicInvoice.CurrencyNKInfo);
				AssertHasErrors(TestPeriodicInvoice.PostDateInfo);
			}
		}

		[TestDate(2010, 09, 15)]
		public void TestValidatePostDate()
		{
			TestPeriodicInvoice.Factory.RefreshEnabled = true; //to allow to test period validations with saving periods in different factory

			TestPeriodicInvoice.PostDate = ZDateTime.Empty;
			AssertHasErrors(TestPeriodicInvoice.PostDateInfo);

			TestPeriodicInvoice.PostDate = ZDateTime.Invalid;
			TestPeriodicInvoice.ValidatePostDate();
			AssertHasErrors(TestPeriodicInvoice.PostDateInfo);

			TestPeriodicInvoice.PostDate = ZDateTime.BrettsBirthday;
			TestPeriodicInvoice.ValidatePostDate();
			AssertHasErrors(TestPeriodicInvoice.PostDateInfo);

			BusinessObjectFactory periodFactory = new BusinessObjectFactory();
			AccountingPeriodCalculator periodCalculator = new AccountingPeriodCalculator(periodFactory);

			AccPeriodManagement accPeriod = periodCalculator.GetPeriodManagementFromDate(ZDateTime.Today);
			AssertNull("Precondition: There is no period for today.", accPeriod);
			TestPeriodicInvoice.PostDate = ZDateTime.Today;
			AssertHasErrors(TestPeriodicInvoice.PostDateInfo);

			accPeriod = periodFactory.New<AccPeriodManagement>();
			accPeriod.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			accPeriod.AM_Period = periodCalculator.GetPeriodFromDate(ZDateTime.Today);
			accPeriod.AM_Year = (short)(accPeriod.AM_Period / 100);
			accPeriod.AM_StartDate = new ZDateTime(ZDateTime.Today.Year, ZDateTime.Today.Month, 1);
			accPeriod.AM_EndDate = accPeriod.AM_StartDate.AddMonths(1).AddDays(-1);
			periodFactory.Save();

			accPeriod.AM_IsSubLedgerClosed = true;
			accPeriod.AM_IsGeneralLedgerClosed = true;
			periodFactory.Save();
			TestPeriodicInvoice.ValidatePostDate();
			AssertHasErrors(TestPeriodicInvoice.PostDateInfo);

			accPeriod.AM_IsSubLedgerClosed = false;
			accPeriod.AM_IsGeneralLedgerClosed = false;
			periodFactory.Save();
			TestPeriodicInvoice.RunPreSaveValidation();
			AssertNoErrors(TestPeriodicInvoice.PostDateInfo);

			TestPeriodicInvoice.PostDate = ZDateTime.Now.AddDays(-1);
			AssertHasWarning(TestPeriodicInvoice.PostDateInfo, TransactionHeaderValidation.PreviousPostDateWarning);

			TestPeriodicInvoice.PostDate = ZDateTime.Now.AddDays(1);
			AssertHasError(TestPeriodicInvoice.PostDateInfo, TransactionHeaderValidation.FuturePostDateError);
		}

		public virtual void TestValidateTotalAmountWhenNotAllowZeroValueARInvoices()
		{
			AccountingConfigurationRegistry.Instance.AllowZeroValueARInvoices.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertHasErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSTotalAmount = 10M;
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			TestPeriodicInvoice.ClearMiscInvoices();
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertHasErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				Factory.Save();
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, null);
				TestPeriodicInvoice.ValidateTotalAmount();
				AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);
			}
		}

		public virtual void TestValidateTotalAmount()
		{
			var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OSTotalAmount = 10M;
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			TestPeriodicInvoice.ClearMiscInvoices();
			TestPeriodicInvoice.ValidateTotalAmount();
			AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				Charge charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 10M, TestObjectCreator.ABIGAS);
				Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, -10M, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				Factory.Save();
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge1.PK, charge2.PK }, null);
				TestPeriodicInvoice.ValidateTotalAmount();
				AssertNoErrors(TestPeriodicInvoice.OSTotalAmountInfo);
			}
		}

		public void TestValidateOSTotalAmountLocalAmountHasDifferentSigns()
		{
			AssertValidateOSTotalAmountAndLocalAmount(12M, "OS Total Amount and Local Total Amount must be in the same sign.", TestPeriodicInvoice.OSTotalAmountInfo);
		}

		public void TestValidateOnlyOSTotalAmountIsZero()
		{
			AssertValidateOSTotalAmountAndLocalAmount(10M, "OS Total Amount cannot be zero when Local Total Amount is not zero.", TestPeriodicInvoice.OSTotalAmountInfo);
		}

		public void TestValidateOnlyLocalAmountIsZero()
		{
			AssertValidateOSTotalAmountAndLocalAmount(20M, "Local Total Amount cannot be zero when OS Total Amount is not zero.", TestPeriodicInvoice.LocalTotalAmountInfo, "S002");
		}

		void AssertValidateOSTotalAmountAndLocalAmount(ZDecimal osSellAmount, string errorMsg, ZPropertyInfo propertyInfo, string jobNum = "S001")
		{
			var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
			}
			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment(jobNum)))
			{
				var charge1 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Desc", TestObjectCreator.USD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.USD, osSellAmount, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC2, "Desc", TestObjectCreator.USD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.USD, -10M, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				charge1.JR_OSSellExRate = 2M;
				charge2.JR_OSSellExRate = 1M;
				Factory.Save();
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge1.PK, charge2.PK }, null);
				TestPeriodicInvoice.ValidateTotalAmount();
				TestPeriodicInvoice.ValidateLocalTotalAmount();
				AssertHasError(propertyInfo, errorMsg);
			}
		}

		#endregion

		#region Overrides

		public virtual void TestSetDefaultValues()
		{
			AssertEquals("InvoiceDate", ZDateTime.Now.Date, TestPeriodicInvoice.InvoiceDate.Date);
			AssertEquals("PostDate", ZDateTime.Now.Date, TestPeriodicInvoice.PostDate.Date);
			AssertEquals("CurrencyNK", ZString.Empty, TestPeriodicInvoice.CurrencyNK);
		}

		#endregion

		#region Lookups

		public void TestInvoiceTypeLookUp()
		{
			Assert(!TestPeriodicInvoice.JobTypeLookUp.ContainsCode("MSC"));
			Assert(!TestPeriodicInvoice.JobTypeLookUp.ContainsCode(JobInvoicingConsumerTypes.ImporterSecurityFiling));
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Enterprise.Core.Constants.CountryCodes.UnitedStates;
			PeriodicInvoiceBase testPeriodicInvoiceLocal = (PeriodicInvoiceBase)GetNewBusinessObject();
			Assert(testPeriodicInvoiceLocal.JobTypeLookUp.ContainsCode(InvoiceTypeModuleList.Codes.ISF));
			Assert(!TestPeriodicInvoice.JobTypeLookUp.ContainsCode(JobInvoicingConsumerTypes.TransportBooking));
		}

		#endregion

		#region Properties

		public void TestCurrencyNK()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();

			TestPeriodicInvoice.Jobs.Add(job);
			TestPeriodicInvoice.MiscInvoices.Add(invoice);

			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPeriodicInvoice.CurrencyNK);
			Assert(!TestPeriodicInvoice.CurrencyNKInfo.HasErrors());

			TestPeriodicInvoice.CurrencyNK = ZString.Empty;

			AssertEquals(ZString.Empty, TestPeriodicInvoice.CurrencyNK);
			Assert(TestPeriodicInvoice.CurrencyNKInfo.HasErrors());
		}

		public void TestSetCurrencyNKClearsJobsAndMiscInvoices()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			ARInvoice arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);
			TestPeriodicInvoice.MiscInvoices.Add(arInvoice);
			Assert("Precondition: Jobs must not be empty", TestPeriodicInvoice.Jobs.Count > 0);
			Assert("Precondition: MiscInvoices must not be empty", TestPeriodicInvoice.MiscInvoices.Count > 0);

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
			Assert("Jobs must be empty", TestPeriodicInvoice.Jobs.Count == 0);
			Assert("MiscInvoices not be empty", TestPeriodicInvoice.MiscInvoices.Count == 0);
		}

		public void TestSelectedJobTypeCodes()
		{
			TestPeriodicInvoice.JobTypeList[0].Value = true;
			TestPeriodicInvoice.JobTypeList[2].Value = true;

			AssertEquals(2, TestPeriodicInvoice.SelectedJobTypeCodes.Count);
			AssertEquals(TestPeriodicInvoice.GetJobTypeCodeFromJobTypeListDescription(TestPeriodicInvoice.JobTypeList[0].Description), TestPeriodicInvoice.SelectedJobTypeCodes[0]);
			AssertEquals(TestPeriodicInvoice.GetJobTypeCodeFromJobTypeListDescription(TestPeriodicInvoice.JobTypeList[2].Description), TestPeriodicInvoice.SelectedJobTypeCodes[1]);
		}

		public void TestSelectedJobs()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Job job2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);
			TestPeriodicInvoice.Jobs.Add(job2);

			TestPeriodicInvoice.Jobs[0].IncludeInThePeriodicInvoice = false;

			AssertEquals(1, TestPeriodicInvoice.SelectedJobs.Count());
			Assert(!TestPeriodicInvoice.SelectedJobs.Contains(job));
			Assert(TestPeriodicInvoice.SelectedJobs.Contains(job2));
		}

		public void TestSelectedMiscInvoices()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			ARInvoice invoice2 = Factory.NewWithValidTestData<ARInvoice>();
			TestPeriodicInvoice.MiscInvoices.Add(invoice);
			TestPeriodicInvoice.MiscInvoices.Add(invoice2);

			invoice.IncludeInThePeriodicInvoice = false;

			AssertEquals(1, TestPeriodicInvoice.SelectedMiscInvoices.Count());
			Assert(!TestPeriodicInvoice.SelectedMiscInvoices.Contains(invoice));
			Assert(TestPeriodicInvoice.SelectedMiscInvoices.Contains(invoice2));
		}

		public void TestCalculateTotalsWhenPostingForeignCurrencyChargesOnForeignCurrencyInvoice()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1.1234m);
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching);
				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1.1234m, 30.00m, 3.71m, 0.11m, 26.70m, 3.30m, 0.10m);
				AssertAmountsOnCharge("charge2", charge2, 1.1234m, 300.00m, 37.08m, 1.08m, 267.05m, 33.01m, 0.96m);

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				}
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.USD.RX_Code;
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, Array.Empty<ZGuid>()); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);

				AssertEquals("OSExTaxAmount", 330m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 40.79m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSExtraTaxAmount", 1.19m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("OSTotalAmount", 370.79m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("LocalExTaxAmount", 293.75m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 36.31m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalExtraTaxAmount", 1.06m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertEquals("LocalTotalAmount", 330.06m, TestPeriodicInvoice.LocalTotalAmount);
				AssertAmountsOnPeriodicInvoice(330m, 40.79m, 370.79m, 1.19m, 293.75m, 36.31m, 330.06m, 1.06m);

				AssertEquals("Job.Currency", TestObjectCreator.USD.RX_Code, TestPeriodicInvoice.Jobs[0].Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", 40.79m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalAmountForPeriodicBilling", 293.75m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", 36.31m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], TestObjectCreator.USD, 330m, 40.79m, 1.19m, 293.75m, 36.31m, 1.06m);
			}
		}

		public void TestCalculateTotalsWhenPostingLocalCurrencyChargesOnLocalCurrencyInvoice()
		{
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1m, 30m, 3.71m, 0.11m, 30m, 3.71m, 0.11m);
				AssertAmountsOnCharge("charge2", charge2, 1m, 300m, 37.08m, 1.08m, 300m, 37.08m, 1.08m);

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				AssertEquals("OSExTaxAmount", 330m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 40.79m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 370.79m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.19m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 330m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 40.79m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 370.79m, TestPeriodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.19m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertAmountsOnPeriodicInvoice(330m, 40.79m, 370.79m, 1.19m, 330m, 40.79m, 370.79m, 1.19m);

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPeriodicInvoice.Jobs[0].Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", 40.79m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", 40.79m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], GlbCompany.CurrentCompany.LocalCurrency, 330m, 40.79m, 1.19m, 330m, 40.79m, 1.19m);
			}
		}

		public void TestCalculateTotalsWhenPostingSellInvoiceCurrencyChargesOnLocalCurrencyInvoice()
		{
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes.AddNew();
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_Module = JobInvoicingConsumerTypes.Shipment.Code;
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_ServiceDirection = "ALL";
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_TransportMode = "ALL";
			TestObjectCreator.ABIGAS.CompanyData.InvoiceTypes[0].PI_SecondaryType = "INV";

			Factory.Save();

			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1.1234m);
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.EUR, 1.2345m);

				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);

				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1.1234m, 30.00m, 3.71m, 0.11m, 26.70m, 3.30m, 0.10m);
				AssertAmountsOnCharge("charge2", charge2, 1.1234m, 300.00m, 37.08m, 1.08m, 267.05m, 33.01m, 0.96m);

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.Code;
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				AssertEquals("OSExTaxAmount", 293.75m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 36.31m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSExtraTaxAmount", 1.06m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("OSTotalAmount", 330.06m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("LocalExTaxAmount", 293.75m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 36.31m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalExtraTaxAmount", 1.06m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertEquals("LocalTotalAmount", 330.06m, TestPeriodicInvoice.LocalTotalAmount);
				AssertAmountsOnPeriodicInvoice(293.75m, 36.31m, 330.06m, 1.06m, 293.75m, 36.31m, 330.06m, 1.06m);

				AssertEquals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPeriodicInvoice.Jobs[0].Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", 293.75m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", 36.31m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalAmountForPeriodicBilling", 293.75m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", 36.31m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], GlbCompany.CurrentCompany.LocalCurrency, 293.75m, 36.31m, 1.06m, 293.75m, 36.31m, 1.06m);

				// Set Sell Invoice Currency
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.Code;
				charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.Code;
				Assert(charge.BillInInvoiceCurrency);
				Assert(charge2.BillInInvoiceCurrency);

				Factory.Save();
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(0, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(0, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.EUR.Code;
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				AssertEquals("OSExTaxAmount", 362.63m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 44.82m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSExtraTaxAmount", 1.31m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("OSTotalAmount", 407.45m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("LocalExTaxAmount", 293.75m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 36.31m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalExtraTaxAmount", 1.06m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertEquals("LocalTotalAmount", 330.06m, TestPeriodicInvoice.LocalTotalAmount);
				AssertAmountsOnPeriodicInvoice(362.63m, 44.82m, 407.45m, 1.31m, 293.75m, 36.31m, 330.06m, 1.06m);

				AssertEquals(TestPeriodicInvoice.CurrencyNK, TestPeriodicInvoice.Jobs[0].Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", 362.63m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", 44.82m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalAmountForPeriodicBilling", 293.75m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", 36.31m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], TestObjectCreator.EUR, 362.63m, 44.82m, 1.31m, 293.75m, 36.31m, 1.06m);

				// Set Sell Currency to Local
				charge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.Code;
				charge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.Code;
				charge.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.Code;
				charge2.JR_RX_NKSellInvoiceCurrency = TestObjectCreator.EUR.Code;
				Assert(charge.BillInInvoiceCurrencyWithLocalSellCurrency);
				Assert(charge2.BillInInvoiceCurrencyWithLocalSellCurrency);
				// Provide CFX uplift
				charge.JR_LineCFX = 10m;
				charge2.JR_LineCFX = 10m;

				AssertEquals(TestObjectCreator.EUR.Code, TestPeriodicInvoice.CurrencyNK);
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified.

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				AssertEquals("OSExTaxAmount", 398.91m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 49.30m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSExtraTaxAmount", 1.45m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("OSTotalAmount", 448.21m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("LocalExTaxAmount", 323.13m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 39.94m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalExtraTaxAmount", 1.17m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertEquals("LocalTotalAmount", 363.07m, TestPeriodicInvoice.LocalTotalAmount);
				AssertAmountsOnPeriodicInvoice(398.91m, 49.30m, 448.21m, 1.45m, 323.13m, 39.94m, 363.07m, 1.17m);

				AssertEquals(TestPeriodicInvoice.CurrencyNK, TestPeriodicInvoice.Jobs[0].Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", 398.91m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", 49.30m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalAmountForPeriodicBilling", 323.13m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", 39.94m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], TestObjectCreator.EUR, 398.91m, 49.30m, 1.45m, 323.13m, 39.94m, 1.17m);
			}
		}

		public void TestCalculateTotals_ChargeTaxBranches()
		{
			var testBranch1 = TestObjectCreator.CreateBranch("001", GlbCompany.CurrentCompany);
			var testBranch2 = TestObjectCreator.CreateBranch("002", GlbCompany.CurrentCompany);
			var testBranch3 = TestObjectCreator.CreateBranch("003", GlbCompany.CurrentCompany);

			Factory.Save();

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1.1636m);
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(20);

				var charge1 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge3 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				charge1.JR_GB_SellTaxBranch = testBranch1.PK;
				charge2.JR_GB_SellTaxBranch = testBranch2.PK;
				charge3.JR_GB_SellTaxBranch = testBranch3.PK;

				Factory.Save();

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.Code;

				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge1.PK, charge2.PK, charge3.PK }, Array.Empty<ZGuid>()); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);

				_ = TestPeriodicInvoice.LocalExtraTaxAmount;

				var chargeTaxBranches = TestPeriodicInvoice.Jobs[0].ChargeTaxBranches;

				Assert(chargeTaxBranches.Contains("001"));
				Assert(chargeTaxBranches.Contains("002"));
				Assert(chargeTaxBranches.Contains("003"));
			}
		}

		public void TestCalculateTotalsWhenPostingForeignCurrencyChargesOnLocalCurrencyInvoice()
		{
			CoreTestCalculateTotalsWhenPostingForeignCurrencyChargesOnLocalCurrencyInvoice(false);
		}

		public void TestCalculateTotalsWhenPostingForeignCurrencyChargesOnLocalCurrencyInvoice_CalculateTaxAtHeaderLevel()
		{
			CoreTestCalculateTotalsWhenPostingForeignCurrencyChargesOnLocalCurrencyInvoice(true);
		}

		void CoreTestCalculateTotalsWhenPostingForeignCurrencyChargesOnLocalCurrencyInvoice(bool isTestingTaxAtHeaderLevel)
		{
			using (AccountingConfigurationRegistry.Instance.CalculateTaxAtHeaderLevel.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, isTestingTaxAtHeaderLevel))
			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 1.1636m);
				TestObjectCreator.GST1.SetRateNumerator_ForTestOnly(20);

				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge3 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.USD, 144.38m, TestObjectCreator.ABIGAS, TestObjectCreator.GST1, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1.1636m, 144.38m, 28.88m, 0.00m, 124.08m, 24.82m, 0.00m);
				AssertAmountsOnCharge("charge2", charge2, 1.1636m, 144.38m, 28.88m, 0.00m, 124.08m, 24.82m, 0.00m);
				//we no longer do adjustments for charge
				AssertAmountsOnCharge("charge3", charge3, 1.1636m, 144.38m, isTestingTaxAtHeaderLevel ? 28.88m : 28.88m, 0.00m, 124.08m, isTestingTaxAtHeaderLevel ? 24.82m : 24.82m, 0.00m);
				if (isTestingTaxAtHeaderLevel)
				{
					var warning = "This value is not precise and is for reference only. It will be recalculated during posting with higher precision due to Calculate Tax at Header Level rules.";
					charge3.Validation.ValidateAll();
					AssertHasWarning(charge3.JR_OSSellGSTAmt_CalcInfo, warning);
				}

				var oSExTaxAmount = 3 * 124.08m;
				var oSTaxAmount = isTestingTaxAtHeaderLevel ? 74.46m : 3 * 24.82m;
				var oSTotalAmount = oSExTaxAmount + oSTaxAmount;
				var oSExtraTaxAmount = 0m;

				var localExTaxAmount = 3 * 124.08m;
				var localTaxAmount = isTestingTaxAtHeaderLevel ? 74.46m : 3 * 24.82m;
				var localTotalAmount = localExTaxAmount + localTaxAmount;
				var localExtraTaxAmount = 0m;

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK, charge3.PK }, Array.Empty<ZGuid>()); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(3, TestPeriodicInvoice.Charges.Count);
				AssertAmountsOnPeriodicInvoice(oSExTaxAmount, oSTaxAmount, oSTotalAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localTotalAmount, localExtraTaxAmount);
				AssertAmountsOnJob(TestPeriodicInvoice.Jobs[0], TestObjectCreator.AUD, oSExTaxAmount, oSTaxAmount, oSExtraTaxAmount, localExTaxAmount, localTaxAmount, localExtraTaxAmount);
			}
		}

		void AssertAmountsOnCharge(string desc, Charge charge, decimal exchangeRate,
			decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount,
			decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount)
		{
			CombineAssertions("Precondition: check values for " + desc, delegate
			{
				AssertEquals("Exchange Rate", exchangeRate, charge.JR_OSSellExRate);

				AssertEquals("OSExTaxAmount", oSExTaxAmount, charge.JR_OSSellAmt);
				AssertEquals("OSTaxAmount", oSTaxAmount, charge.JR_OSSellGSTAmt_Calc);
				AssertEquals("OSExtraTaxAmount", oSExtraTaxAmount, charge.JR_Calc_OSSellExtraTaxAmt);

				AssertEquals("LocalExTaxAmount", localExTaxAmount, charge.JR_LocalSellAmt);
				AssertEquals("LocalTaxAmount", localTaxAmount, charge.JR_Sell_LocalGSTAmount);
				AssertEquals("LocalExtraTaxAmount", localExtraTaxAmount, charge.JR_Calc_LocalSellExtraTaxAmt);
			});
		}

		void AssertAmountsOnPeriodicInvoice(decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSTotalAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localTotalAmount, decimal localExtraTaxAmount)
		{
			CombineAssertions("Periodic Invoice values", delegate
			{
				AssertEquals("OSExTaxAmount", oSExTaxAmount, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", oSTaxAmount, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", oSTotalAmount, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", oSExtraTaxAmount, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", localExTaxAmount, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", localTaxAmount, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", localTotalAmount, TestPeriodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", localExtraTaxAmount, TestPeriodicInvoice.LocalExtraTaxAmount);
			});
		}

		void AssertAmountsOnJob(PeriodicInvoiceSelectableJob periodicInvoiceJob, RefCurrency currency, decimal oSExTaxAmount, decimal oSTaxAmount, decimal oSExtraTaxAmount, decimal localExTaxAmount, decimal localTaxAmount, decimal localExtraTaxAmount)
		{
			CombineAssertions("Amounts on periodic invoice selectable job", delegate
			{
				AssertEquals("Currency", currency.RX_Code, periodicInvoiceJob.Currency);
				AssertEquals("JH_OSAmountForPeriodicBilling", oSExTaxAmount, periodicInvoiceJob.JH_OSAmountForPeriodicBilling);
				AssertEquals("JH_OSTaxAmountForPeriodicBilling", oSTaxAmount, periodicInvoiceJob.JH_OSTaxAmountForPeriodicBilling);
				AssertEquals("JH_OSExtraTaxAmount", oSExtraTaxAmount, periodicInvoiceJob.JH_OSExtraTaxAmount);
				AssertEquals("JH_LocalAmountForPeriodicBilling", localExTaxAmount, periodicInvoiceJob.JH_LocalAmountForPeriodicBilling);
				AssertEquals("JH_LocalTaxAmountForPeriodicBilling", localTaxAmount, periodicInvoiceJob.JH_LocalTaxAmountForPeriodicBilling);
				AssertEquals("JH_LocalExtraTaxAmount", localExtraTaxAmount, periodicInvoiceJob.JH_LocalExtraTaxAmount);
			});
		}

		public void TestCalculateTotalsWithReverseTaxType()
		{
			AccTaxRate reverseTaxRate = TestObjectCreator.CreateTaxRate("RVS", "Reverse Tax Rate", "RVS", 5, "", 0, 1);
			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			ARInvoice invoice = TestObjectCreator.CreateARInvoice<ARInvoice>("0000001", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			ARInvoice invoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("0000002", TestObjectCreator.AUD, 1M, TestObjectCreator.AALSHI);
			invoice.AH_TransactionCategory = invoice2.AH_TransactionCategory = "FIN";
			ARInvoiceLine invoiceline1 = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 50M);
			invoiceline1.AL_OSTaxAmount = 60M;
			invoiceline1.AL_AT = reverseTaxRate.PK;
			ARInvoiceLine invoiceline2 = (ARInvoiceLine)TestObjectCreator.CreateInvoiceLine(invoice2, TestObjectCreator.AUD, 1M, 500M);
			invoiceline2.AL_OSTaxAmount = 600M;
			invoiceline2.AL_AT = reverseTaxRate.PK;

			using (Job job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S001")))
			{
				Charge charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 30M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 30M, TestObjectCreator.AALSHI);
				Charge charge2 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "desc", TestObjectCreator.AUD, 300M, TestObjectCreator.ABIGAS, TestObjectCreator.AUD, 300M, TestObjectCreator.AALSHI);
				charge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge.JR_AT_SellGSTRate = reverseTaxRate.PK;
				charge2.JR_AT_SellGSTRate = reverseTaxRate.PK;

				Factory.Save();

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.AALSHI.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { invoice.PK, invoice2.PK }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals("There should be one job", 1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals("There should be two charges", 2, TestPeriodicInvoice.Charges.Count);
				if (TestPeriodicInvoice is PeriodicInvoiceLightForCheckSecurityRights)
				{
					AssertEquals("There should be zero misc invoices", 0, TestPeriodicInvoice.MiscInvoices.Count);
					AssertEquals("OSExTaxAmount", 330m, TestPeriodicInvoice.OSExTaxAmount);
					AssertEquals("OSExtraTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.OSExtraTaxAmount);
					AssertEquals("OSTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.OSTaxAmount);
					AssertEquals("OSTotalAmount", 330m, TestPeriodicInvoice.OSTotalAmount);
					AssertEquals("LocalExTaxAmount", 330m, TestPeriodicInvoice.LocalExTaxAmount);
					AssertEquals("LocalExtraTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.LocalExtraTaxAmount);
					AssertEquals("LocalTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.LocalTaxAmount);
					AssertEquals("LocalTotalAmount", 330m, TestPeriodicInvoice.LocalTotalAmount);
					AssertEquals("Local Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPeriodicInvoice.Jobs[0].Currency);
					AssertEquals("JH_OSAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
					AssertEquals("JH_OSTaxAmountForPeriodicBilling = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
					AssertEquals("JH_LocalAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
					AssertEquals("JH_LocalTaxAmountForPeriodicBilling = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				}
				else
				{
					AssertEquals("There should be two misc invoices", 2, TestPeriodicInvoice.MiscInvoices.Count);
					AssertEquals("OSExTaxAmount", 880m, TestPeriodicInvoice.OSExTaxAmount);
					AssertEquals("OSExtraTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.OSExtraTaxAmount);
					AssertEquals("OSTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.OSTaxAmount);
					AssertEquals("OSTotalAmount", 880m, TestPeriodicInvoice.OSTotalAmount);
					AssertEquals("LocalExTaxAmount", 880m, TestPeriodicInvoice.LocalExTaxAmount);
					AssertEquals("LocalExtraTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.LocalExtraTaxAmount);
					AssertEquals("LocalTaxAmount = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.LocalTaxAmount);
					AssertEquals("LocalTotalAmount", 880m, TestPeriodicInvoice.LocalTotalAmount);
					AssertEquals("Local Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, TestPeriodicInvoice.Jobs[0].Currency);
					AssertEquals("JH_OSAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_OSAmountForPeriodicBilling);
					AssertEquals("JH_OSTaxAmountForPeriodicBilling = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.Jobs[0].JH_OSTaxAmountForPeriodicBilling);
					AssertEquals("JH_LocalAmountForPeriodicBilling", 330m, TestPeriodicInvoice.Jobs[0].JH_LocalAmountForPeriodicBilling);
					AssertEquals("JH_LocalTaxAmountForPeriodicBilling = 0 when Reverse Tax Rate used", 0m, TestPeriodicInvoice.Jobs[0].JH_LocalTaxAmountForPeriodicBilling);
				}
			}
		}

		public void TestPostDateReadonly()
		{
			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals(false, TestPeriodicInvoice.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = true;
			AssertEquals(true, TestPeriodicInvoice.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals(true, TestPeriodicInvoice.PostDateInfo.ReadOnly);

			AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed = false;
			AssertEquals(true, TestPeriodicInvoice.PostDateInfo.ReadOnly);
		}

		#endregion

		#region Lines Methods

		public void TestLoadJobs()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Factory.Save();
			TestPeriodicInvoice.Jobs.Add(job);

			Charge charge = Factory.NewWithValidTestData<Charge>();
			TestPeriodicInvoice.Charges.Add(charge);
			charge.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			charge.JR_LocalSellAmt = 30;
			charge.JR_RX_NKSellCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			charge.JR_RX_NKCostCurrency = Enterprise.Core.Constants.CurrencyCodes.UnitedStates;
			charge.JR_JH = job.PK;

			if (TestPeriodicInvoice is PeriodicInvoice)
			{
				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.Charges.Count);
				AssertEquals(30m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(30m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(30m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals(30m, TestPeriodicInvoice.LocalTotalAmount);
			}

			TestPeriodicInvoice.CurrencyNK = "AUD";
			TestPeriodicInvoice.LoadJobs();

			AssertEquals(0, TestPeriodicInvoice.Jobs.Count);
			AssertEquals(0, TestPeriodicInvoice.Charges.Count);
			AssertEquals(0m, TestPeriodicInvoice.OSExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalTotalAmount);

			var jobHeader = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader.Parent = TestObjectCreator.CreateShipment("S001");
			jobHeader.JH_ParentID = jobHeader.Parent.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var jobHeader2 = Factory.NewJobWithValidTestDataForTesting<Job>();
			jobHeader2.Parent = TestObjectCreator.CreateShipment("S002");
			jobHeader2.JH_ParentID = jobHeader2.Parent.PK;
			jobHeader2.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			var jobCharge = Factory.NewWithValidTestData<Charge>();
			var jobCharge2 = Factory.NewWithValidTestData<Charge>();
			var jobCharge3 = Factory.NewWithValidTestData<Charge>();
			var jobCharge4 = Factory.NewWithValidTestData<Charge>();
			var jobCharge5 = Factory.NewWithValidTestData<Charge>();
			var jobCharge6 = Factory.NewWithValidTestData<Charge>();
			jobCharge.JR_OH_SellAccount = TestOrg.PK;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_OSSellAmt = 10;
			jobCharge.JR_LocalSellAmt = 30;
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			TestObjectCreator.CreateWIP(jobCharge);
			jobCharge.JR_InvoiceType = "FID";

			jobCharge2.JR_OH_SellAccount = TestOrg.PK;
			jobCharge2.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge2.JR_JH = jobHeader2.PK;
			jobCharge2.JR_OSSellAmt = 100;
			jobCharge2.JR_LocalSellAmt = 300;
			jobCharge2.JR_GB = GlbBranch.CurrentBranch.PK;
			TestObjectCreator.CreateWIP(jobCharge2);
			jobCharge2.JR_InvoiceType = "FID";

			jobCharge3.JR_OH_SellAccount = TestOrg.PK;
			jobCharge3.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge3.JR_JH = jobHeader.PK;
			jobCharge3.JR_OSSellAmt = 0;
			jobCharge3.JR_LocalSellAmt = 0;
			jobCharge3.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge3.JR_InvoiceType = "FID";

			jobCharge4.JR_OH_SellAccount = TestOrg.PK;
			jobCharge4.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge4.JR_JH = jobHeader2.PK;
			jobCharge4.JR_OSSellAmt = 0;
			jobCharge4.JR_LocalSellAmt = 0;
			jobCharge4.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge4.JR_InvoiceType = "FID";

			jobCharge5.JR_OH_SellAccount = TestOrg.PK;
			jobCharge5.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge5.JR_JH = jobHeader2.PK;
			jobCharge5.JR_OSSellAmt = 0;
			jobCharge5.JR_LocalSellAmt = 0;
			jobCharge5.JR_AC = TestObjectCreator.CreateChargeCode("CMT", "Comment", Constants.ChargeType.Comment, 0, null, null, "ALL").PK;
			jobCharge5.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge5.JR_InvoiceType = "FID";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, TestObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				jobCharge6.JR_OH_SellAccount = TestOrg.PK;
				jobCharge6.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
				jobCharge6.JR_JH = jobHeader.PK;
				jobCharge6.JR_OSSellAmt = 10;
				jobCharge6.JR_LocalSellAmt = 30;
				jobCharge6.JR_GB = Env.CurrentBranchPK;
				TestObjectCreator.CreateWIP(jobCharge6);
				jobCharge6.JR_InvoiceType = "FID";
				Factory.Save();
			}

			ZQuery cacheOnlyFilter = new ZQuery();
			cacheOnlyFilter.FetchOnlyFromLocalCache = true;
			AssertEquals("Count of GenericJob in Factory", 0, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
			int genericJobDBHits = Factory.GetTableHitCount(GenericJob.GenericJob.Schema.TableName);
			AssertEquals("Precondition: GetLoadedFetchHintCountForTable GenericJob", 0, Factory.GetLoadedFetchHintCountForTable(GenericJob.GenericJob.Schema.TableName));

			var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestOrg.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { jobHeader.PK, jobHeader2.PK }, new ZGuid[] { jobCharge.PK, jobCharge2.PK, jobCharge3.PK }, null); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

			var loadToTrigerFetchHint = Factory.LoadGenericJob(jobHeader);

			AssertEquals("GenericJob DB Hits", genericJobDBHits + 1, Factory.GetTableHitCount(GenericJob.GenericJob.Schema.TableName));
			if (TestPeriodicInvoice is PeriodicInvoiceLightForCheckSecurityRights)
			{
				AssertEquals("Count of GenericJob in Factory", 1, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
				AssertEquals("GetLoadedFetchHintCountForTable GenericJob", 0, Factory.GetLoadedFetchHintCountForTable(GenericJob.GenericJob.Schema.TableName));
			}
			else
			{
				AssertEquals("Count of GenericJob in Factory", 2, Factory.Load<GenericJob.GenericJob>(cacheOnlyFilter).Length);
				AssertEquals("GetLoadedFetchHintCountForTable GenericJob", 2, Factory.GetLoadedFetchHintCountForTable(GenericJob.GenericJob.Schema.TableName));
			}

			AssertEquals(2, TestPeriodicInvoice.Jobs.Count);
			AssertEquals(3, TestPeriodicInvoice.Charges.Count);
			AssertEquals(330m, TestPeriodicInvoice.OSExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
			AssertEquals(330m, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals(330m, TestPeriodicInvoice.LocalExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
			AssertEquals(330m, TestPeriodicInvoice.LocalTotalAmount);
		}

		public void TestLoadMasterHouseConsolNumbers()
		{
			ForwardingConsol consol = TestObjectCreator.CreateConsol("AUSYD", "KRSEL", "C1");
			consol.JK_MasterBillNum = "M9876543210";
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "H0123456789";

			JobHeader jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			jobHeader.Parent = shipment;
			jobHeader.JH_ParentID = jobHeader.Parent.PK;
			jobHeader.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;

			JobCharge jobCharge = Factory.NewWithValidTestData<JobCharge>();
			jobCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			jobCharge.JR_OH_SellAccount = TestOrg.PK;
			jobCharge.JR_RX_NKSellCurrency = TestObjectCreator.AUD.RX_Code;
			jobCharge.JR_JH = jobHeader.PK;
			jobCharge.JR_OSSellAmt = 30;
			jobCharge.JR_LocalSellAmt = 30;
			jobCharge.JR_InvoiceType = "FID";

			Factory.Save();

			var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
			if (periodicInvoice != null)
			{
				periodicInvoice.DebtorPK = TestOrg.PK;
			}
			TestPeriodicInvoice.CurrencyNK = "AUD";
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { jobHeader.PK }, new ZGuid[] { jobCharge.PK }, null); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

			AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
			AssertEquals(1, TestPeriodicInvoice.Charges.Count);
			AssertEquals("Consol #", "C1", TestPeriodicInvoice.Jobs[0].JH_ConsolNo);
			AssertEquals("Master Bill #", "M9876543210", TestPeriodicInvoice.Jobs[0].JH_MasterBillNo);
			AssertEquals("House BIll #", "H0123456789", TestPeriodicInvoice.Jobs[0].JH_HouseBillNo);
		}

		public void TestLoadMiscInvoices()
		{
			ARInvoice invoice = new BusinessObjectFactory().NewWithValidTestData<ARInvoice>();
			TestPeriodicInvoice.MiscInvoices.Add(invoice);
			invoice.AH_OSExTaxAmount = 50;
			invoice.AH_OSTaxAmount = 60;
			invoice.AH_InvoiceAmount = 70;

			AssertEquals(1, TestPeriodicInvoice.MiscInvoices.Count);

			AssertEquals(50m, TestPeriodicInvoice.OSExTaxAmount);
			AssertEquals(60m, TestPeriodicInvoice.OSTaxAmount);
			AssertEquals(110m, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals(70m, TestPeriodicInvoice.LocalExTaxAmount);
			AssertEquals(60m, TestPeriodicInvoice.LocalTaxAmount);
			AssertEquals(130m, TestPeriodicInvoice.LocalTotalAmount);

			TestPeriodicInvoice.LoadMiscInvoices();

			AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);
			AssertEquals(0m, TestPeriodicInvoice.OSExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalExTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
			AssertEquals(0m, TestPeriodicInvoice.LocalTotalAmount);

			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice1.AH_TransactionCategory = "FIN";
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			aRInvoice2.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice2.AH_TransactionCategory = "FIN";
			TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1M, 70).AL_OSTaxAmount = 60M;
			TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1M, 30).AL_OSTaxAmount = 20M;

			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			aRCreditNote.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRCreditNote.AH_TransactionCategory = "FIN";
			var line = (InvoicingLineBase)aRCreditNote.Lines.AddNew();
			line.AL_LocalExTaxAmount = 30m;
			line.AL_LocalTaxAmount = 20m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;

			Factory.Save();

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, null, null, new ZGuid[] { aRInvoice1.PK, aRInvoice2.PK, aRCreditNote.PK }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

			if (TestPeriodicInvoice is PeriodicInvoiceLightForCheckSecurityRights)
			{
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals(0m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.LocalTotalAmount);
			}
			else
			{
				AssertEquals(3, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals(70m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(60m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(130m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(70m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals(60m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals(130m, TestPeriodicInvoice.LocalTotalAmount);
			}
		}

		public virtual void TestLoadMiscInvoicesWithOtherTaxes()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.AH_OH = TestOrg.PK;
			aRInvoice1.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice1.AH_TransactionCategory = "FIN";
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice2.AH_OH = TestOrg.PK;
			aRInvoice2.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRInvoice2.AH_TransactionCategory = "FIN";
			TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1M, 70).AL_OSTaxAmount = 60M;
			TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1M, 30).AL_OSTaxAmount = 20M;
			aRInvoice1.AH_OSTaxAmountOtherTaxes = 12;
			aRInvoice1.AH_LocalTaxAmountOtherTaxes = 12;
			aRInvoice2.AH_OSTaxAmountOtherTaxes = 17;
			aRInvoice2.AH_LocalTaxAmountOtherTaxes = 17;

			var aRCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
			aRCreditNote.AH_OH = TestOrg.PK;
			aRCreditNote.AH_RX_NKTransactionCurrency = TestObjectCreator.AUD.RX_Code;
			aRCreditNote.AH_TransactionCategory = "FIN";
			var line = (InvoicingLineBase)aRCreditNote.Lines.AddNew();
			line.AL_LocalExTaxAmount = 30m;
			line.AL_LocalTaxAmount = 20m;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			aRCreditNote.AH_OSTaxAmountOtherTaxes = 6;
			aRCreditNote.AH_LocalTaxAmountOtherTaxes = 6;

			Factory.Save();

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;
			TestPeriodicInvoice.LoadMiscInvoices();

			AssertEquals(3, TestPeriodicInvoice.MiscInvoices.Count);
			AssertEquals(70m, TestPeriodicInvoice.OSExTaxAmount);
			AssertEquals(60m, TestPeriodicInvoice.OSTaxAmount);
			AssertEquals(130m, TestPeriodicInvoice.OSTotalAmount);
			AssertEquals(70m, TestPeriodicInvoice.LocalExTaxAmount);
			AssertEquals(60m, TestPeriodicInvoice.LocalTaxAmount);
			AssertEquals(130m, TestPeriodicInvoice.LocalTotalAmount);
		}

		public void TestLoadMiscInvoices_NoJobInvoiceNumber()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				var miscInvoice1 = Factory.New<ARInvoice>();
				miscInvoice1.AH_GC = GlbCompany.CurrentCompany.PK;
				miscInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
				var miscInvoiceLine1 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
				miscInvoiceLine1.GenericCharge = TestObjectCreator.GLHeader1.PK;
				miscInvoiceLine1.AL_OSExTaxAmount = 100m;
				miscInvoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				miscInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				var miscInvoice2 = Factory.New<ARInvoice>();
				miscInvoice2.AH_GC = GlbCompany.CurrentCompany.PK;
				miscInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				var miscInvoiceLine2 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
				miscInvoiceLine2.GenericCharge = TestObjectCreator.GLHeader1.PK;
				miscInvoiceLine2.AL_OSExTaxAmount = 100m;
				miscInvoiceLine2.AL_AT = TestObjectCreator.GST1.PK;
				miscInvoice2.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				miscInvoice1.AH_ConsolidatedInvoiceRef = "Test123";
				miscInvoice2.AH_ConsolidatedInvoiceRef = string.Empty;

				Factory.Save();

				TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestPeriodicInvoice.LoadMiscInvoices();
				AssertEquals("Should only be 1 misc invoice in the collection", 1, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals("should only contain misc invoice 2 because misc invoice 2 don't have job invoice number.", true, TestPeriodicInvoice.MiscInvoices.Contains(miscInvoice2.PK));
			}
		}

		public virtual void TestPeriodicInvoiceDoesNotLoadMiscInvoicesWithOtherTaxes()
		{
			var aRInvoice1 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m, TestOrg);
			var aRInvoice2 = TestObjectCreator.CreateInvoice(typeof(ARInvoice), GlbCompany.CurrentCompany.LocalCurrency, 1m, TestOrg);
			TestObjectCreator.CreateInvoiceLine(aRInvoice1, TestObjectCreator.AUD, 1M, 70);
			TestObjectCreator.CreateInvoiceLine(aRInvoice2, TestObjectCreator.AUD, 1M, 30);

			var taxTransaction = Factory.NewWithValidTestData<AccTaxTransaction>();
			taxTransaction.ATT_AH = aRInvoice1.PK;

			Factory.Save();

			TestPeriodicInvoice.CurrencyNK = TestObjectCreator.AUD.RX_Code;

			TestPeriodicInvoice.LoadMiscInvoices();
			AssertEquals(1, TestPeriodicInvoice.MiscInvoices.Count);
			AssertEquals(aRInvoice2.PK, TestPeriodicInvoice.MiscInvoices[0].PK);
		}

		#endregion

		public virtual void TestDontRetrieveInvoicesFromAnotherCompany()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				InvoicingBase miscInvoice1 = Factory.New<ARInvoice>();
				miscInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
				InvoicingLineBase miscInvoiceLine1 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
				miscInvoiceLine1.GenericCharge = TestObjectCreator.GLHeader1.PK;
				miscInvoiceLine1.AL_OSExTaxAmount = 100m;
				miscInvoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
				miscInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				InvoicingBase miscInvoice2 = Factory.New<ARInvoice>();
				miscInvoice2.AH_OH = TestObjectCreator.ABIGAS.PK;
				InvoicingLineBase miscInvoiceLine2 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
				miscInvoiceLine2.GenericCharge = TestObjectCreator.GLHeader1.PK;
				miscInvoiceLine2.AL_OSExTaxAmount = 100m;
				miscInvoiceLine2.AL_AT = TestObjectCreator.GST1.PK;
				miscInvoice2.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				miscInvoice1.AH_GC = GlbCompany.CurrentCompany.PK;
				miscInvoice2.AH_GC = TestObjectCreator.NonCurrentCompany.PK;
				miscInvoice2.AH_GB = TestObjectCreator.NonCurrentCompany.Branches[0].PK;

				Factory.Save();

				TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
				TestPeriodicInvoice.LoadMiscInvoices();
				AssertEquals("Should only be 1 misc invoice in the collection", 1, TestPeriodicInvoice.MiscInvoices.Count);
				AssertEquals("should only contain misc invoice 1", true, TestPeriodicInvoice.MiscInvoices.Contains(miscInvoice1.PK));
			}
		}

		public virtual void TestDontRetrievePeriodicInvoicesAsMiscInvoices()
		{
			InvoicingBase miscInvoice1 = Factory.New<ARInvoice>();
			miscInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			InvoicingLineBase miscInvoiceLine1 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
			miscInvoiceLine1.GenericCharge = TestObjectCreator.GLHeader1.PK;
			miscInvoiceLine1.AL_OSExTaxAmount = 100m;
			miscInvoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
			miscInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

			InvoicingBase fakePeriodicInvoice1 = Factory.New<ARInvoice>();
			fakePeriodicInvoice1.AH_OH = TestObjectCreator.ABIGAS.PK;
			InvoicingLineBase fakePeriodicInvoiceLine1 = (InvoicingLineBase)miscInvoice1.Lines.AddNew();
			fakePeriodicInvoiceLine1.GenericCharge = TestObjectCreator.GLHeader1.PK;
			fakePeriodicInvoiceLine1.AL_OSExTaxAmount = 100m;
			fakePeriodicInvoiceLine1.AL_AT = TestObjectCreator.GST1.PK;
			fakePeriodicInvoice1.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice_Batching;

			Factory.Save();
			AssertEquals("Hoping the transaction category hasn't changed automatatically", InvoiceTypesList.Codes.FinalInvoice, miscInvoice1.AH_TransactionCategory);
			AssertEquals("Hoping the transaction category hasn't changed automatatically", InvoiceTypesList.Codes.FinalInvoice_Batching, fakePeriodicInvoice1.AH_TransactionCategory);

			TestPeriodicInvoice.CurrencyNK = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			TestPeriodicInvoice.LoadMiscInvoices();
			AssertEquals("Should only be 1 misc invoice in the collection", 1, TestPeriodicInvoice.MiscInvoices.Count);
		}

		public virtual void TestReloadChargesByJob()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001234")))
			{
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 30m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				var charge2 = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 300m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				AssertAmountsOnCharge("charge", charge, 1m, 30m, 3.71m, 0.11m, 30m, 3.71m, 0.11m);
				AssertAmountsOnCharge("charge2", charge2, 1m, 300m, 37.08m, 1.08m, 300m, 37.08m, 1.08m);

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK, charge2.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertEquals(0, TestPeriodicInvoice.MiscInvoices.Count);

				AssertEquals("OSExTaxAmount", 330m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 40.79m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 370.79m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.19m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 330m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 40.79m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 370.79m, TestPeriodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.19m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertAmountsOnPeriodicInvoice(330m, 40.79m, 370.79m, 1.19m, 330m, 40.79m, 370.79m, 1.19m);

				var newFactory = new BusinessObjectFactory();
				var loadedCharge = newFactory.Load<JobCharge>(charge2.PK);
				loadedCharge.JR_OSSellAmt = 500;
				loadedCharge.JR_LocalSellAmt = 500;
				newFactory.Save();

				TestPeriodicInvoice.ReloadChargesByJob(job.PK);

				AssertEquals("OSExTaxAmount", 530m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals("OSTaxAmount", 65.51m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals("OSTotalAmount", 595.51m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals("OSExtraTaxAmount", 1.91m, TestPeriodicInvoice.OSExtraTaxAmount);
				AssertEquals("LocalExTaxAmount", 530m, TestPeriodicInvoice.LocalExTaxAmount);
				AssertEquals("LocalTaxAmount", 65.51m, TestPeriodicInvoice.LocalTaxAmount);
				AssertEquals("LocalTotalAmount", 595.51m, TestPeriodicInvoice.LocalTotalAmount);
				AssertEquals("LocalExtraTaxAmount", 1.91m, TestPeriodicInvoice.LocalExtraTaxAmount);
				AssertAmountsOnPeriodicInvoice(530m, 65.51m, 595.51m, 1.91m, 530m, 65.51m, 595.51m, 1.91m);
			}
		}

		public virtual void TestReloadChargesByJob_ExcludeAutoJRJ()
		{
			var company = TestObjectCreator.CreateNewCompany("DMO");
			var organizationAU = TestObjectCreator.CreateOrgHeader("DAU", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationAU, "AU", "ABN", "41065894724");
			var organizationRO = TestObjectCreator.CreateOrgHeader("DRO", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationRO, "RO", "ABN", "00001");
			Factory.Save();

			var branchA01 = TestObjectCreator.CreateBranch("A01", company);
			var branchA04 = TestObjectCreator.CreateBranch("A04", company);
			branchA01.GB_OH_OrgProxy = organizationAU.PK;
			branchA04.GB_OH_OrgProxy = organizationRO.PK;
			company.GC_OH_OrgProxy = organizationAU.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA01.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment = testObjectCreatorInNewFactory.CreateShipment("S00001");
				var job = testObjectCreatorInNewFactory.CreateJob(shipment);
				var charge1 = testObjectCreatorInNewFactory.CreateCharge(job, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge1.JR_OH_SellAccount = organizationAU.PK;
				charge1.JR_GB = branchA04.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge1.JR_AT_SellGSTRate = testObjectCreatorInNewFactory.GST1WithDates.PK;

				var charge2 = testObjectCreatorInNewFactory.CreateCharge(job, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge2.JR_OH_SellAccount = organizationAU.PK;
				charge2.JR_GB = branchA01.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				AssertEquals("Charge is NOT valid for Auto JRJ due to Tax Number is different.", false, charge1.IsValidForAutoRevenuePosting);
				AssertEquals("Charge is valid for Auto JRJ but should not post due to Internal Branch is empty.", true, charge2.IsValidForAutoRevenuePosting);
				AssertEquals(2m, charge1.JR_Sell_LocalGSTAmount);
				AssertEquals(0m, charge2.JR_Sell_LocalGSTAmount);
				AssertEquals(false, charge1.JR_IsRevenuePosted);
				AssertEquals(false, charge2.JR_IsRevenuePosted);

				newFactory.Save();
				AssertEquals(false, charge1.JR_IsRevenuePosted);
				AssertEquals(false, charge2.JR_IsRevenuePosted);

				if (TestPeriodicInvoice is PeriodicInvoice periodicInvoice)
				{
					periodicInvoice.DebtorPK = organizationAU.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.Charges.Count);
				AssertEquals(charge1.PK, TestPeriodicInvoice.Charges[0].PK);
				AssertEquals(job.PK, TestPeriodicInvoice.Jobs[0].PK);
				AssertEquals(20m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(2m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(22m, TestPeriodicInvoice.OSTotalAmount);

				TestPeriodicInvoice.ReloadChargesByJob(job.PK);
				AssertEquals(1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(1, TestPeriodicInvoice.Charges.Count);
				AssertEquals(charge1.PK, TestPeriodicInvoice.Charges[0].PK);
				AssertEquals(job.PK, TestPeriodicInvoice.Jobs[0].PK);
				AssertEquals(20m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(2m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(22m, TestPeriodicInvoice.OSTotalAmount);

				job.Dispose();
			}
		}

		public virtual void TestReloadChargesByJobWithNoCharge()
		{
			using (var job = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("S00001235")))
			{
				var charge = CreateCharge(job, TestObjectCreator.CC1, TestObjectCreator.AUD, 50m, TestObjectCreator.ABIGAS, TestObjectCreator.GSTWithExtraRate, InvoiceTypesList.Codes.FinalInvoice_Batching);
				Factory.Save();

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				SetupPeriodicInvoiceWithJobsAndMiscInvoices(TestPeriodicInvoice, new ZGuid[] { job.PK }, new ZGuid[] { charge.PK }, new ZGuid[] { Guid.Empty, Guid.Empty }); // This method is used here as an action rather than as a setup. Please call appropriate TestPeriodicInvoice.LoadXXX() method when this test is next modified. 

				AssertAmountsOnPeriodicInvoice(50m, 6.18m, 56.18m, 0.18m, 50m, 6.18m, 56.18m, 0.18m);

				var newFactory = new BusinessObjectFactory();
				var loadedCharge = newFactory.Load<JobCharge>(charge.PK);
				loadedCharge.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice;
				newFactory.Save();

				TestPeriodicInvoice.ReloadChargesByJob(job.PK);

				AssertAmountsOnPeriodicInvoice(0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m);
			}
		}

		public void TestGetJobsQueryShouldContainJobsFilterWhenFiltersAreEmpty()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				var shipment1 = TestObjectCreator.CreateShipment("S0001");
				var shipment2 = TestObjectCreator.CreateShipment("S0002");
				var job1 = TestObjectCreator.CreateJob(shipment1);
				var job2 = TestObjectCreator.CreateJob(shipment2);
				var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge1", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Charge2", TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 200m, TestObjectCreator.ABIGAS);
				charge1.JR_OSSellExRate = charge2.JR_OSSellExRate = 1m;
				charge1.JR_InvoiceType = charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				Factory.Save();

				shipment1.JS_IsForwardRegistered = true;
				shipment1.JS_IsCancelled = true;
				Factory.Save();

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.JobTypeList.First(x => x.Description.Equals("[" + JobInvoicingConsumerTypes.Shipment.Code + "]" + JobInvoicingConsumerTypes.Shipment.Description)).Value = true;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("Expecting collection not to contain Job1", false, TestPeriodicInvoice.Jobs.Contains(job1));

				shipment1.JS_IsCancelled = false;
				Factory.Save();

				TestPeriodicInvoice.Factory.ClearQueryCache();

				TestPeriodicInvoice.LoadJobs();
				AssertEquals("Expecting collection to contain Job1", true, TestPeriodicInvoice.Jobs.Contains(job1));
			}
		}

		public void TestGetMiscInvoicesQueryShouldContainMiscInvoicesFilterWhenFiltersAreEmpty()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				var eURInvoice = Factory.New<ARInvoice>();
				eURInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
				eURInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.EUR.RX_Code;
				var eURInvoiceLine = (InvoicingLineBase)eURInvoice.Lines.AddNew();
				eURInvoiceLine.GenericCharge = TestObjectCreator.GLHeader1.PK;
				eURInvoiceLine.AL_OSExTaxAmount = 100m;
				eURInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				eURInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				var uSDInvoice = Factory.New<ARInvoice>();
				uSDInvoice.AH_OH = TestObjectCreator.ABIGAS.PK;
				uSDInvoice.AH_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
				var uSDInvoiceLine = (InvoicingLineBase)eURInvoice.Lines.AddNew();
				uSDInvoiceLine.GenericCharge = TestObjectCreator.GLHeader1.PK;
				uSDInvoiceLine.AL_OSExTaxAmount = 100m;
				uSDInvoiceLine.AL_AT = TestObjectCreator.GST1.PK;
				uSDInvoice.AH_TransactionCategory = InvoiceTypesList.Codes.FinalInvoice;

				Factory.Save();

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
					periodicInvoice.InvoiceType = InvoiceTypesList.Codes.ForeignCurrencyInvoice_Batching;
				}

				TestPeriodicInvoice.CurrencyNK = TestObjectCreator.EUR.RX_Code;
				TestPeriodicInvoice.LoadMiscInvoices();
				AssertEquals("Expecting collection not to contain USD Invoice", false, TestPeriodicInvoice.MiscInvoices.Contains(uSDInvoice));
				AssertEquals("Expecting collection to contain EUR Invoice", true, TestPeriodicInvoice.MiscInvoices.Contains(eURInvoice));
			}
		}

		public void TestCachedChargeFilter()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				var shipment1 = TestObjectCreator.CreateShipment("S0001");
				var shipment2 = TestObjectCreator.CreateShipment("S0002");
				var shipment3 = TestObjectCreator.CreateShipment("S0003");
				var job1 = TestObjectCreator.CreateJob(shipment1);
				var job2 = TestObjectCreator.CreateJob(shipment2);
				var job3 = TestObjectCreator.CreateJob(shipment3);
				var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge1", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
				var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC3, "Charge2", TestObjectCreator.AUD, 200m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 200m, TestObjectCreator.ABIGAS);
				var charge3 = TestObjectCreator.CreateCharge(job3, TestObjectCreator.CC3, "Charge3", TestObjectCreator.AUD, 300m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 300m, TestObjectCreator.ABIGAS);
				charge1.JR_InvoiceType = charge2.JR_InvoiceType = charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge1.JR_OSSellExRate = charge2.JR_OSSellExRate = charge3.JR_OSSellExRate = 1m;

				Factory.Save();

				ModuleGuidFilter filter = (ModuleGuidFilter)TestPeriodicInvoice.JobsFilter["Charge Code"];

				filter.Property = TestObjectCreator.CC1.PK;
				filter.IsActive = true;

				var periodicInvoice = TestPeriodicInvoice as PeriodicInvoice;
				if (periodicInvoice != null)
				{
					periodicInvoice.DebtorPK = TestObjectCreator.ABIGAS.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.LoadJobs();

				AssertEquals("Expecting collection to contain Job1", true, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection not to contain job2", false, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection not to contain job3", false, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 100m, TestPeriodicInvoice.OSTotalAmount);

				filter.Property = TestObjectCreator.CC3.PK;
				filter.IsActive = true;

				var jobsToUnTick = TestPeriodicInvoice.Jobs[0];
				var bulkPeriodicInvoice = TestPeriodicInvoice as PeriodicInvoiceBulk;
				if (bulkPeriodicInvoice != null)
				{
					jobsToUnTick = bulkPeriodicInvoice.PeriodicInvoices[0].Jobs[0];
				}

				jobsToUnTick.IncludeInThePeriodicInvoice = false;

				AssertEquals("Expecting collection to contain Job1", true, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection not to contain Job2", false, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection not to contain Job3", false, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 0m, TestPeriodicInvoice.OSTotalAmount);

				jobsToUnTick.IncludeInThePeriodicInvoice = true;

				AssertEquals("Expecting collection to contain Job1", true, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection not to contain Job2", false, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection not to contain Job3", false, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 100m, TestPeriodicInvoice.OSTotalAmount);

				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.LoadJobs();

				AssertEquals("Expecting collection to contain Job1", false, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection not to contain Job2", true, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection not to contain Job3", true, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 500m, TestPeriodicInvoice.OSTotalAmount);

				jobsToUnTick = TestPeriodicInvoice.Jobs[0];
				bulkPeriodicInvoice = TestPeriodicInvoice as PeriodicInvoiceBulk;
				if (bulkPeriodicInvoice != null)
				{
					jobsToUnTick = bulkPeriodicInvoice.PeriodicInvoices[0].Jobs[0];
				}

				jobsToUnTick.IncludeInThePeriodicInvoice = false;
				var excludedAmount = jobsToUnTick.Parent.PK == job2.PK ? charge2.JR_OSSellAmt : charge3.JR_OSSellAmt;

				AssertEquals("Expecting collection not to contain Job1", false, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection to contain Job2", true, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection to contain Job3", true, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 500m - excludedAmount, TestPeriodicInvoice.OSTotalAmount);

				jobsToUnTick.IncludeInThePeriodicInvoice = true;

				AssertEquals("Expecting collection not to contain Job1", false, TestPeriodicInvoice.Jobs.Contains(job1));
				AssertEquals("Expecting collection to contain Job2", true, TestPeriodicInvoice.Jobs.Contains(job2));
				AssertEquals("Expecting collection to contain Job3", true, TestPeriodicInvoice.Jobs.Contains(job3));
				AssertEquals("Invoice Total", 500m, TestPeriodicInvoice.OSTotalAmount);
			}
		}

		public void TestPeriodicInvoiceFactoriesSetup()
		{
			var factory = new BusinessObjectFactory();
			Assert("Precondition: by default the factory's refresh is enabled", factory.RefreshEnabled);
			Assert("Precondition: by default the factory context is not applied", !factory.HasContext(BusinessContext.PeriodicInvoicePosting));

			var periodicInvoice = (PeriodicInvoiceBase)Activator.CreateInstance(GetExpectedBusinessObjectType(), new object[] { factory });

			Assert("Data refresh bus must be disabled to disable charge reloader among other things", !factory.RefreshEnabled);
			Assert("Periodic Invoice Factory Context must be applied to disable functionality that is not required during periodic invoicing", factory.HasContext(BusinessContext.PeriodicInvoicePosting));
		}

		public virtual void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllChargesShouldNotPost()
		{
			var company = TestObjectCreator.CreateNewCompany("DMO");
			var organizationAU = TestObjectCreator.CreateOrgHeader("DAU", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationAU, "AU", "ABN", "41065894724");
			var organizationRO = TestObjectCreator.CreateOrgHeader("DRO", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationRO, "RO", "ABN", "00001");
			Factory.Save();

			var branchA01 = TestObjectCreator.CreateBranch("A01", company);
			var branchA02 = TestObjectCreator.CreateBranch("A02", company);
			var branchA04 = TestObjectCreator.CreateBranch("A04", company);
			branchA01.GB_OH_OrgProxy = organizationAU.PK;
			branchA02.GB_OH_OrgProxy = organizationAU.PK;
			branchA04.GB_OH_OrgProxy = organizationRO.PK;
			company.GC_OH_OrgProxy = organizationAU.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA01.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1 = testObjectCreatorInNewFactory.CreateShipment("S00001");
				var job1 = testObjectCreatorInNewFactory.CreateJob(shipment1);
				var charge1 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge1.JR_OH_SellAccount = organizationAU.PK;
				charge1.JR_GB = branchA01.PK;
				charge1.JR_GB_InternalBranch = branchA02.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				var charge2 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge2.JR_OH_SellAccount = organizationAU.PK;
				charge2.JR_GB = branchA01.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				AssertEquals("Charge is valid for Auto JRJ.", true, charge1.IsValidForAutoRevenuePosting);
				AssertEquals("Charge is valid for Auto JRJ but should not post due to Internal Branch is empty.", true, charge2.IsValidForAutoRevenuePosting);
				AssertEquals(0m, charge1.JR_Sell_LocalGSTAmount);
				AssertEquals(0m, charge2.JR_Sell_LocalGSTAmount);
				AssertEquals(false, charge1.JR_IsRevenuePosted);
				AssertEquals("Charge should not be posted due to Internal Branch is empty.", false, charge2.JR_IsRevenuePosted);

				newFactory.Save();
				AssertEquals(true, charge1.JR_IsRevenuePosted);
				AssertEquals(false, charge2.JR_IsRevenuePosted);

				var shipment2 = testObjectCreatorInNewFactory.CreateShipment("S00002");
				var job2 = testObjectCreatorInNewFactory.CreateJob(shipment2);
				var charge3 = testObjectCreatorInNewFactory.CreateCharge(job2, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge3.JR_OH_SellAccount = organizationAU.PK;
				charge3.JR_GB = branchA01.PK;
				charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				AssertEquals("Charge is valid for Auto JRJ but should not post due to Internal Branch is empty.", true, charge3.IsValidForAutoRevenuePosting);
				AssertEquals(0m, charge3.JR_Sell_LocalGSTAmount);
				AssertEquals(false, charge3.JR_IsRevenuePosted);

				newFactory.Save();
				AssertEquals("Charge should not be posted due to Internal Branch is empty.", false, charge3.JR_IsRevenuePosted);

				if (TestPeriodicInvoice is PeriodicInvoice periodicInvoice)
				{
					periodicInvoice.DebtorPK = organizationAU.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("No Job should be loaded due to all the JobCharge have already auto posted.", 0, TestPeriodicInvoice.Jobs.Count);
				if (TestPeriodicInvoice is PeriodicInvoiceBulk periodicInvoiceBulk)
				{
					AssertEquals("No Periodic Invoices should be loaded due to Jobs is empty.", 0, periodicInvoiceBulk.PeriodicInvoices.Count);
				}
				AssertEquals(0m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(0m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(0, TestPeriodicInvoice.Charges.Count);

				job1.Dispose();
				job2.Dispose();
			}
		}

		public virtual void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_PartOfChargesShouldPost()
		{
			var company = TestObjectCreator.CreateNewCompany("DMO");
			var organizationAU = TestObjectCreator.CreateOrgHeader("DAU", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationAU, "AU", "ABN", "41065894724");
			var organizationRO = TestObjectCreator.CreateOrgHeader("DRO", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationRO, "RO", "ABN", "00001");
			Factory.Save();

			var branchA01 = TestObjectCreator.CreateBranch("A01", company);
			var branchA02 = TestObjectCreator.CreateBranch("A02", company);
			var branchA03 = TestObjectCreator.CreateBranch("A03", company);
			branchA01.GB_OH_OrgProxy = organizationAU.PK;
			branchA02.GB_OH_OrgProxy = organizationAU.PK;
			branchA03.GB_OH_OrgProxy = organizationRO.PK;
			company.GC_OH_OrgProxy = organizationAU.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA01.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1 = testObjectCreatorInNewFactory.CreateShipment("S00001");
				var job1 = testObjectCreatorInNewFactory.CreateJob(shipment1);
				var charge1 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge1.JR_OH_SellAccount = organizationAU.PK;
				charge1.JR_GB = branchA01.PK;
				charge1.JR_GB_InternalBranch = branchA02.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				var charge2 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge2.JR_OH_SellAccount = organizationAU.PK;
				charge2.JR_GB = branchA03.PK;
				charge2.JR_GB_InternalBranch = branchA02.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge2.JR_AT_SellGSTRate = testObjectCreatorInNewFactory.GST1WithDates.PK;

				var charge3 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge3.JR_OH_SellAccount = organizationAU.PK;
				charge3.JR_GB = branchA01.PK;
				charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;

				AssertEquals("Charge is valid for Auto JRJ.", true, charge1.IsValidForAutoRevenuePosting);
				AssertEquals("Charge is NOT valid for Auto JRJ but should not post due to Tax Number is different.", false, charge2.IsValidForAutoRevenuePosting);
				AssertEquals("Charge is valid for Auto JRJ but should not post due to Internal Branch is empty.", true, charge3.IsValidForAutoRevenuePosting);
				AssertEquals(0m, charge1.JR_Sell_LocalGSTAmount);
				AssertEquals(2m, charge2.JR_Sell_LocalGSTAmount);
				AssertEquals(false, charge1.JR_IsRevenuePosted);
				AssertEquals(false, charge2.JR_IsRevenuePosted);
				AssertEquals(false, charge3.JR_IsRevenuePosted);
				AssertEquals(false, charge1.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals("JR_GB_InternalBranch should be read-only due to Tax Number is different", true, charge2.JR_GB_InternalBranch_ReadOnly_ForTestOnly);

				newFactory.Save();
				AssertEquals(true, charge1.JR_IsRevenuePosted);
				AssertEquals("Charge2 should not auto post due to Tax Number is different.", false, charge2.JR_IsRevenuePosted);
				AssertEquals("Charge3 should not auto post due to Internal Branch is null.", false, charge3.JR_IsRevenuePosted);

				var shipment2 = testObjectCreatorInNewFactory.CreateShipment("S00002");
				var job2 = testObjectCreatorInNewFactory.CreateJob(shipment2);
				var charge4 = testObjectCreatorInNewFactory.CreateCharge(job2, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge4.JR_OH_SellAccount = organizationAU.PK;
				charge4.JR_GB = branchA01.PK;
				charge4.JR_GB_InternalBranch = branchA02.PK;
				charge4.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				AssertEquals("Charge is valid for Auto JRJ.", true, charge4.IsValidForAutoRevenuePosting);
				AssertEquals(false, charge4.JR_IsRevenuePosted);

				newFactory.Save();
				AssertEquals(true, charge4.JR_IsRevenuePosted);

				if (TestPeriodicInvoice is PeriodicInvoice periodicInvoice)
				{
					periodicInvoice.DebtorPK = organizationAU.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("Job1 should be loaded due to some of the JobCharge is not auto posted.", 1, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(job1.PK, TestPeriodicInvoice.Jobs[0].PK);
				AssertEquals(20m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(2m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(22m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(1, TestPeriodicInvoice.Charges.Count);
				AssertEquals(charge2.PK, TestPeriodicInvoice.Charges[0].PK);

				job1.Dispose();
				job2.Dispose();
			}
		}

		public virtual void TestRemoveJobsWhichAllChargesShouldPostAutoJRJ_AllOfChargesShouldPost()
		{
			var company = TestObjectCreator.CreateNewCompany("DMO");
			var organizationAU = TestObjectCreator.CreateOrgHeader("DAU", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationAU, "AU", "ABN", "41065894724");
			var organizationRO = TestObjectCreator.CreateOrgHeader("DRO", true, true);
			TestObjectCreator.CreateCustomsCodes(organizationRO, "RO", "ABN", "00001");
			Factory.Save();

			var branchA01 = TestObjectCreator.CreateBranch("A01", company);
			var branchA03 = TestObjectCreator.CreateBranch("A03", company);
			branchA01.GB_OH_OrgProxy = organizationAU.PK;
			branchA03.GB_OH_OrgProxy = organizationRO.PK;
			company.GC_OH_OrgProxy = organizationAU.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var testObjectCreatorInNewFactory = new TestObjectCreator(newFactory);
			using (AccountingConfigurationRegistry.Instance.EnableAutoJobRevenueJournals.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.AutoJobRevenueJournalCodes.Tax))
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branchA01.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var shipment1 = testObjectCreatorInNewFactory.CreateShipment("S00001");
				var job1 = testObjectCreatorInNewFactory.CreateJob(shipment1);
				var charge1 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge1.JR_OH_SellAccount = organizationAU.PK;
				charge1.JR_GB = branchA03.PK;
				charge1.JR_GB_InternalBranch = branchA01.PK;
				charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				charge1.JR_AT_SellGSTRate = testObjectCreatorInNewFactory.GST1WithDates.PK;
				AssertEquals(2m, charge1.JR_Sell_LocalGSTAmount);

				var charge2 = testObjectCreatorInNewFactory.CreateCharge(job1, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge2.JR_OH_SellAccount = organizationAU.PK;
				charge2.JR_GB = branchA01.PK;
				charge2.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				AssertEquals("Charge is NOT valid for Auto JRJ but should not post due to Tax Number is different.", false, charge1.IsValidForAutoRevenuePosting);
				AssertEquals("Charge is valid for Auto JRJ but should not post due to Internal Branch is empty.", true, charge2.IsValidForAutoRevenuePosting);
				AssertEquals(false, charge1.JR_IsRevenuePosted);
				AssertEquals(false, charge2.JR_IsRevenuePosted);
				AssertEquals("JR_GB_InternalBranch should be read-only due to Tax Number is different", true, charge1.JR_GB_InternalBranch_ReadOnly_ForTestOnly);
				AssertEquals(false, charge2.JR_GB_InternalBranch_ReadOnly_ForTestOnly);

				newFactory.Save();
				AssertEquals("Charge1 should not auto post due to Tax Number is different.", false, charge1.JR_IsRevenuePosted);
				AssertEquals("Charge2 should not auto post due to Internal Branch is null.", false, charge2.JR_IsRevenuePosted);

				var shipment2 = testObjectCreatorInNewFactory.CreateShipment("S00002");
				var job2 = testObjectCreatorInNewFactory.CreateJob(shipment2);
				var charge3 = testObjectCreatorInNewFactory.CreateCharge(job2, testObjectCreatorInNewFactory.FRT, 0m, 20m);
				charge3.JR_OH_SellAccount = organizationAU.PK;
				charge3.JR_GB = branchA03.PK;
				charge3.JR_GB_InternalBranch = branchA01.PK;
				charge3.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
				AssertEquals("Charge is NOT valid for Auto JRJ but should not post due to Tax Number is different.", false, charge3.IsValidForAutoRevenuePosting);
				AssertEquals(false, charge3.JR_IsRevenuePosted);
				AssertEquals("JR_GB_InternalBranch should be read-only due to Tax Number is different", true, charge3.JR_GB_InternalBranch_ReadOnly_ForTestOnly);

				newFactory.Save();
				AssertEquals("charge3 should not auto post due to Tax Number is different.", false, charge3.JR_IsRevenuePosted);

				if (TestPeriodicInvoice is PeriodicInvoice periodicInvoice)
				{
					periodicInvoice.DebtorPK = organizationAU.PK;
				}
				TestPeriodicInvoice.CurrencyNK = "AUD";
				TestPeriodicInvoice.JobTypeList.Single(x => x.Description.Contains(JobInvoicingConsumerTypes.Shipment.Code)).Value = true;
				TestPeriodicInvoice.LoadJobs();
				AssertEquals("All Jobs should be loaded due to all of the JobCharge is not auto posted.", 2, TestPeriodicInvoice.Jobs.Count);
				AssertEquals(40m, TestPeriodicInvoice.OSExTaxAmount);
				AssertEquals(2m, TestPeriodicInvoice.OSTaxAmount);
				AssertEquals(42m, TestPeriodicInvoice.OSTotalAmount);
				AssertEquals(2, TestPeriodicInvoice.Charges.Count);
				AssertCollectionContains(charge1.PK, TestPeriodicInvoice.Charges.Select(x => x.PK));
				AssertCollectionContains(charge3.PK, TestPeriodicInvoice.Charges.Select(x => x.PK));

				job1.Dispose();
				job2.Dispose();
			}
		}

		[TestDate(2015, 1, 1)]
		public void TestGetDefaultARInvoiceAndPostDate()
		{
			Func<PeriodicInvoiceBase> validationAction = () =>
			{
				TestPeriodicInvoice.InvoiceDate = ZDateTime.Now;
				return TestPeriodicInvoice;
			};

			var today = ZDateTime.Now;

			//registry not configed
			var invoice = validationAction();
			AssertEquals("should be today", today, invoice.InvoiceDate);
			AssertEquals(false, invoice.InvoiceDateInfo.ReadOnly);
			AssertEquals("should be today", today, invoice.PostDate);

			//default registry
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.Default.Code);
			invoice = validationAction();
			AssertEquals("should be today", today, invoice.InvoiceDate);
			AssertEquals(false, invoice.InvoiceDateInfo.ReadOnly);
			AssertEquals("should be today", today, invoice.PostDate);

			var invoiceDateWarningMsg = @"Invoice Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";
			var postDateWarningMsg = @"Post Date is non-editable as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'.";

			//MonthEndSuspension configed but hidden registry CurrentInvoiceDate not configed
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviour.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, AccountingConstants.InvAndPstDateDefaultingRuleTypes.MonthEndSuspension.Code);
			AssertEquals(AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.Value, DateTime.MinValue);
			invoice = validationAction();
			AssertEquals("should be today", today, invoice.InvoiceDate);
			AssertEquals(true, invoice.InvoiceDateInfo.ReadOnly);
			AssertHasWarning(invoice.InvoiceDateInfo, invoiceDateWarningMsg);
			AssertEquals("should be today", today, invoice.PostDate);
			AssertEquals(true, invoice.PostDate_ReadOnly);
			AssertHasWarning(invoice.PostDateInfo, postDateWarningMsg);

			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as current year/month
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, today.AddDays(2).ToDateTime());
			invoice = validationAction();
			AssertEquals("should be today", today, invoice.InvoiceDate);
			AssertEquals(true, invoice.InvoiceDateInfo.ReadOnly);
			AssertHasWarning(invoice.InvoiceDateInfo, invoiceDateWarningMsg);
			AssertEquals("should be today", today, invoice.PostDate);
			AssertEquals(true, invoice.PostDate_ReadOnly);
			AssertHasWarning(invoice.PostDateInfo, postDateWarningMsg);

			//MonthEndSuspension configed, hidden registry CurrentInvoiceDate configed as not current year/month
			var configDateTime = today.AddMonths(1).ToDateTime();
			AccountingConfigurationRegistry.Instance.InvAndPstDateDefaultingBehaviourInstatedDate.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, configDateTime);
			invoice = validationAction();
			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, invoice.InvoiceDate);
			AssertEquals("should be the last day of the configed year/month",
				new ZDateTime(configDateTime.Year, configDateTime.Month, DateTime.DaysInMonth(configDateTime.Year, configDateTime.Month))
				, invoice.PostDate);

			AssertEquals(true, invoice.InvoiceDateInfo.ReadOnly);
			AssertHasWarning(invoice.InvoiceDateInfo, invoiceDateWarningMsg);
			AssertEquals(true, invoice.PostDate_ReadOnly);
			AssertHasWarning(invoice.PostDateInfo, postDateWarningMsg);

			TestPeriodicInvoice.ValidateBeforeFindingJobs();
			AssertEquals(true, invoice.InvoiceDateInfo.ReadOnly);
			AssertHasWarning(invoice.InvoiceDateInfo, invoiceDateWarningMsg);
			AssertEquals(true, invoice.PostDate_ReadOnly);
			AssertHasWarning(invoice.PostDateInfo, postDateWarningMsg);
		}

		[TestDate(2015, 5, 1)]
		public void TestValidateCurrencyNKWithPostingExchangeRateOption()
		{
			ExchangeRateReader.GetReaderInstance().ClearCache();

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "DEF");
			TestPeriodicInvoice.InvoiceDate = new ZDateTime(2015, 4, 10);
			TestPeriodicInvoice.PostDate = new ZDateTime(2015, 4, 11);
			TestPeriodicInvoice.CurrencyNK = "USD";
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TOD");
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasWarning(TestPeriodicInvoice.CurrencyNKInfo, @"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Today Exchange Rate"".
But the USD exchange rate is not set for the date 01-May-15. Please check your data and try again.");
			TestObjectCreator.CreateUSDBuyRate(0.951m, new DateTime(2015, 5, 1));
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "INV");
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasWarning(TestPeriodicInvoice.CurrencyNKInfo, @"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Invoice Date"".
But the USD exchange rate is not set for the date 10-Apr-15. Please check your data and try again.");
			TestObjectCreator.CreateUSDBuyRate(0.941m, new DateTime(2015, 4, 10));
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);

			PostingExRateRegistryAR.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "PST");
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertHasWarning(TestPeriodicInvoice.CurrencyNKInfo, @"The ""AR Invoice Posting Exchange Rate Option"" has been set to ""Exchange Rate based on Post Date"".
But the USD exchange rate is not set for the date 11-Apr-15. Please check your data and try again.");
			TestObjectCreator.CreateUSDBuyRate(0.945m, new DateTime(2015, 4, 11));
			TestPeriodicInvoice.ValidateCurrencyNK();
			AssertNoErrors(TestPeriodicInvoice.CurrencyNKInfo);

			ExchangeRateReader.GetReaderInstance().ClearCache();
		}

		public void FAT_TestReloadChargesCoreNoExceptionThrownWhenTooManyJobs()
		{
			if (TestPeriodicInvoice is PeriodicInvoiceLight)
			{
				Assert(true);
			}
			else
			{
				for (int i = 1; i < 30001; i++)
				{
					var shipment1 = TestObjectCreator.CreateShipment(i.ToString());
					var job1 = TestObjectCreator.CreateJob(shipment1);
					var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge1", TestObjectCreator.AUD, 100m, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100m, TestObjectCreator.ABIGAS);
					charge1.JR_OSSellExRate = 1m;
					charge1.JR_InvoiceType = InvoiceTypesList.Codes.FinalInvoice_Batching;
					if (i % 100 == 0)
					{
						Factory.Save();
					}
				}
				AssertNoExceptionThrown(() => TestPeriodicInvoice.LoadJobs());
			}
		}

		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAP => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP;
		protected InvoicePostingExRateOptionRegistryItem PostingExRateRegistryAR => AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR;
	}
}
