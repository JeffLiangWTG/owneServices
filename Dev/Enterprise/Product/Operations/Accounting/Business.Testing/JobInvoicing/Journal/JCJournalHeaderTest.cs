using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Interfaces;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JCJournalHeader))]
	public class JCJournalHeaderTest : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		public virtual void TestTransactionNumberGenerator()
		{
			JCJournalHeader header = Factory.NewWithValidTestData<JCJournalHeader>();
			TransactionNumberSequenceCustomisationCollection customisation = new TransactionNumberSequenceCustomisationCollection();
			TransactionNumberSequenceCustomisation element = customisation.AddNew();
			element.Order = 1;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderBranchCode;
			element.Include = true;
			element = customisation.AddNew();
			element.Length = 8;
			element.Order = 2;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.SequenceNumber;
			element.Include = true;
			element = customisation.AddNew();
			element.Order = 50;
			element.ElementName = TransactionNumberSequenceCustomisation.ElementNames.TransactionHeaderDepartmentCode;
			element.Include = true;
			AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, customisation);

			Factory.Save();
			AssertEquals("Transaction number", "BNE00000001BRN", header.AH_TransactionNum);
		}

		public void TestSetCFXValues()
		{
			CreateChargeCollectionForTestSetCFXValuesTest(GlbBranch.CurrentBranch.PK, GlbBranch.CurrentBranch.PK, GlbDepartment.CurrentDepartment.PK);

			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = GlbCompany.CurrentCompany.PK;
			Charges.PostedInvoice.AH_GB = testBranch.PK;

			ZDateTime now = ZDateTime.Now;
			JCJournalHeader cFXHeader = Factory.NewWithValidTestData<JCJournalHeader>();
			cFXHeader.SetCFXValues(now, Charges);

			AssertEquals("AH_Ledger", ZArchitecture.Core.LedgerTypes.JobCosting, cFXHeader.AH_Ledger);
			AssertEquals("AH_TransationType", ZArchitecture.Core.TransactionTypes.Journal, cFXHeader.AH_TransactionType);
			AssertEquals("AH_Desc", "Job Costing Journal (CFX)".ToUpper(), cFXHeader.AH_Desc);
			AssertEquals("AH_InvoiceDate", now, cFXHeader.AH_InvoiceDate);
			AssertEquals("AH_RX_NKTransactionCurrency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, cFXHeader.AH_RX_NKTransactionCurrency);
			AssertEquals("AH_ExchangeRate", 1M, cFXHeader.AH_ExchangeRate);
			AssertEquals("AH_GB", testBranch.PK, cFXHeader.AH_GB);
			AssertEquals("AH_GE", GlbDepartment.CurrentDepartment.PK, cFXHeader.AH_GE);
			AssertEquals("AH_TransactionCount", (byte)1, cFXHeader.AH_TransactionCount);
		}

		public void TestPostJCJournalHeaderToLoginBranch()
		{
			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch testBranch = Factory.NewWithValidTestData<GlbBranch>();
			testBranch.GB_GC = testCompany.PK;
			GlbDepartment testDepartment = Factory.NewWithValidTestData<GlbDepartment>();
			Factory.Save();

			CreateChargeCollectionForTestSetCFXValuesTest(ZGuid.NewZGuid(), testBranch.PK, testDepartment.PK);

			bool originalValue = AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.Value;
			ZDateTime now = ZDateTime.Now;
			try
			{
				Charges.PostedInvoice.AH_GB = testBranch.PK;
				JCJournalHeader cFXHeader = Factory.NewWithValidTestData<JCJournalHeader>();
				cFXHeader.SetCFXValues(now, Charges);
				AssertEquals("Department", testDepartment.PK, cFXHeader.AH_GE);
				AssertEquals("Branch", testBranch.PK, cFXHeader.AH_GB);

				Charges.PostedInvoice.AH_GB = GlbBranch.CurrentBranch.PK;
				cFXHeader.SetCFXValues(now, Charges);
				AssertEquals("Department", testDepartment.PK, cFXHeader.AH_GE);
				AssertEquals("Branch", GlbBranch.CurrentBranch.PK, cFXHeader.AH_GB);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.PostJobInvoicingTransactionsToLoginBranch.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestGenerateReverseTransaction()
		{
			ZDateTime now = ZDateTime.Now;
			JCJournalHeader cFXHeader = Factory.New<JCJournalHeader>();

			cFXHeader.AH_Desc = "Job Costing Journal (CFX)";
			cFXHeader.AH_InvoiceDate = now.AddDays(-5);
			cFXHeader.AH_DueDate = now.AddDays(-5);
			cFXHeader.AH_PostDate = now.AddDays(-5);
			cFXHeader.AH_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			cFXHeader.AH_ExchangeRate = 1M;
			cFXHeader.AH_GB = GlbBranch.CurrentBranch.PK;
			cFXHeader.AH_GE = Creator.NonCurrentDepartment.PK;

			JCJournalLine line = cFXHeader.Lines.AddNew();
			line.AL_Sequence = (short)1;
			line.AL_Desc = "Description";
			line.AL_RX_NKTransactionCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			line.AL_ExchangeRate = 1M;
			line.AL_JH = Job != null ? Job.PK : ZGuid.Empty;
			line.AL_AC = ChargeCode.PK;
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = Creator.NonCurrentDepartment.PK;
			line.AL_AG = AccountingConfigurationRegistry.Instance.CFXAccount.Value;

			Assert("CFX Account should be filled in base data", AccountingConfigurationRegistry.Instance.CFXAccount.Value != ZGuid.Empty);

			decimal amount = -20.57m;
			line.AL_LineAmount = amount;
			line.AL_OSAmount = amount;

			IJobCosting originalCFXHeader = cFXHeader;
			originalCFXHeader.GenerateReverseTransaction(true);

			JCJournalHeader reverseTransaction = (JCJournalHeader)originalCFXHeader.ReverseTransaction;

			AssertNotNull("Reversing Transaction shouldn't be null", reverseTransaction);
			AssertEquals("Description should be blank", "JC AR/AP JOURNAL", reverseTransaction.AH_Desc);

			Assert("Invoice Date should be within 5 minutes of current time", now.AddMinutes(5) > reverseTransaction.AH_InvoiceDate);
			Assert("Invoice Date should be within 5 minutes of current time", now.AddMinutes(-5) < reverseTransaction.AH_InvoiceDate);
			Assert("Due Date should be within 5 minutes of current time", now.AddMinutes(5) > reverseTransaction.AH_DueDate);
			Assert("Due Date should be within 5 minutes of current time", now.AddMinutes(-5) < reverseTransaction.AH_DueDate);
			Assert("Post Date should be within 5 minutes of current time", now.AddMinutes(5) > reverseTransaction.AH_PostDate);
			Assert("Post Date should be within 5 minutes of current time", now.AddMinutes(-5) < reverseTransaction.AH_PostDate);

			AssertEquals("Currency should be the same as original", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, reverseTransaction.AH_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate should be one", 1m, reverseTransaction.AH_ExchangeRate);
			AssertEquals("Branch should be current branch", GlbBranch.CurrentBranch.PK, reverseTransaction.AH_GB);
			AssertEquals("Department should be non current department", Creator.NonCurrentDepartment.PK, reverseTransaction.AH_GE);

			JCJournalLine reversingLine = reverseTransaction.Lines[0];

			AssertEquals("Sequence should be same as original line", (short)1, reversingLine.AL_Sequence);
			AssertEquals("Description should be same as original", "Description", reversingLine.AL_Desc);
			AssertEquals("Currency should be same as on original line", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, reversingLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange rate should be 1", 1m, reversingLine.AL_ExchangeRate);
			AssertEquals("Job should be the same as original", Job.PK, reversingLine.AL_JH);
			AssertEquals("Branch should be same as original", GlbBranch.CurrentBranch.PK, reversingLine.AL_GB);
			AssertEquals("Department should be same as original", Creator.NonCurrentDepartment.PK, reversingLine.AL_GE);
			AssertEquals("GL Account should be CFX account", AccountingConfigurationRegistry.Instance.CFXAccount.Value, reversingLine.AL_AG);

			AssertEquals("Line Amount should be 20.57 and positive to effect reversal", 20.57m, reversingLine.AL_LineAmount);
			AssertEquals("OS Amount should be 20.57 and positive to effect reversal", 20.57m, reversingLine.AL_OSAmount);
		}

		[TestDate(2017, 2, 20)]
		public void TestCFXJournalHeaderReverseAgain()
		{
			RevenueRecognitionCollection registryCollection = new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = JobInvoicingConsumerTypes.Shipment.Code;
			registryValue.DirectionCode = Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.ActualArrivalDate;
			registryValue.RunPreSaveValidation();
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Creator.CreateTestPeriodsForEntireYear(2017);

			var shipment = Creator.CreateShipment("S0000001");
			shipment.JS_E_ARV = new ZDateTime(2017, 1, 20);
			Job job = new Job.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			job.LocalChargesPK = Creator.ZECTRA.PK;
			Creator.ZECTRA.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", 10M);
			Factory.Save();

			Creator.CreateExchangeRate(job, USD, .7M);
			var charge = Creator.CreateCharge(job, Creator.CC1, 100M, 100M);
			charge.JR_RX_NKSellCurrency = USD.RX_Code;
			charge.JR_OH_SellAccount = Creator.ZECTRA.PK;
			charge.JR_OSCostAmt = 100M;

			job.ApplyRevenueRecognitionDate(charge);
			Factory.Save();

			new ChargePoster(Factory).Post(charge);
			Factory.Save();

			var arInvoice = Factory.LoadTop1<ARInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable));
			AssertNotNull(arInvoice);
			var jcJournal = Factory.LoadTop1<JCJournalHeader>(new ZQuery(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.JobCosting));
			AssertNotNull(jcJournal);

			var periodManager = new PeriodManager(Factory);
			periodManager.CloseSubLedgerPeriod();
			Factory.Save();

			JobTransactionReverser reverser = new JobTransactionReverser(new InvoicingBase[] { arInvoice });
			reverser.ReverseAllInvoices("test", "IDE");
			Factory.Save();

			AssertEquals("Has no developer exception", 0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestCanApplyTaxBranch() => AssertEquals(false, Factory.NewWithValidTestData<JCJournalHeader>().CanApplyTaxBranch);

		#region Implementation

		TestObjectCreator Creator;
		Job Job;
		Charge Charge;
		AccChargeCode ChargeCode;
		IReceivablesPostingChargeCollection Charges;

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Job = Creator.CreateJob(null, 0m, null, 0m);
			ChargeCode = Creator.CreateChargeCode("CC", "Description", Core.Constants.ChargeType.Margin, 100M, null, null, "ALL");
		}

		void CreateChargeCollectionForTestSetCFXValuesTest(ZGuid chargesBranch, ZGuid jobBranch, ZGuid department)
		{
			Job = CreateJob("Z00001011", AALSHI, true, 5M, ABIGAS, true, 10M);
			Job.JH_GB = jobBranch;
			Job.JH_GE = department;

			AALSHI.CompanyData.SetARTaxApplicable(true);
			AALSHI.MiscServ.OM_ARWHTApplicable = true;
			Charge = CreateCharge(Job, MRG100, "Revenue Transaction Test", AUD, 250M, ZECTRA, AUD, 250M, AALSHI);
			Charge.JR_GE = department;
			Charge.JR_GB = chargesBranch;

			ARInvoice testInvoice = Factory.NewWithValidTestData<ARInvoice>();

			Charges = new IReceivablesPostingChargeCollection();
			Charges.PostedInvoice = testInvoice;
			Charges.Key = new PostingChargeKey(((IReceivablesPostingCharge)Charge).Debtor.PK, ((IReceivablesPostingCharge)Charge).InvoiceType, ((IReceivablesPostingCharge)Charge).Job.JobNumber, ZGuid.Empty, ZGuid.Empty, 0);
			Charges.Add(Charge);
		}

		Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			SetupJob(job, jobNumber, localClient, billLocalClientInLocalCurrency, localClientCFX, agent, billAgentInLocalCurrency, agentCFX);
			return job;
		}

		void SetupJob(Job job, ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;
		}

		Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor != null ? creditor.PK : ZGuid.Empty;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			chargeCode.AC_AT_GSTRate = gST.PK;
			chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			return chargeCode;
		}

		AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
		}

		AccWithholding CreateWithholdingTax(string code, string description, decimal rate)
		{
			AccWithholding taxRate = Factory.New<AccWithholding>();
			taxRate.AW_Code = code;
			taxRate.AW_Description = description;
			taxRate.AW_IsActive = true;
			taxRate.AW_Rate = rate;
			taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
			return taxRate;
		}

		#region AALSHI

		OrgHeader fAALSHI;
		OrgHeader AALSHI
		{
			get
			{
				if (fAALSHI == null)
				{
					fAALSHI = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "AALSHI");
				}
				return fAALSHI;
			}
		}

		#endregion

		#region ABIGAS

		OrgHeader fABIGAS;
		OrgHeader ABIGAS
		{
			get
			{
				if (fABIGAS == null)
				{
					fABIGAS = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ABIGAS");
				}
				return fABIGAS;
			}
		}

		#endregion

		#region ZECTRA

		OrgHeader fZECTRA;
		OrgHeader ZECTRA
		{
			get
			{
				if (fZECTRA == null)
				{
					fZECTRA = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "ZECTRA");
				}
				return fZECTRA;
			}
		}

		#endregion

		#region MRG100

		AccChargeCode fMRG100;
		AccChargeCode MRG100
		{
			get
			{
				if (fMRG100 == null)
				{
					fMRG100 = CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Core.Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fMRG100;
			}
		}

		#endregion

		#region AUD

		RefCurrency fAUD;
		RefCurrency AUD
		{
			get
			{
				if (fAUD == null)
				{
					fAUD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "AUD");
				}
				return fAUD;
			}
		}

		#endregion

		#region USD

		RefCurrency fUSD;
		RefCurrency USD
		{
			get
			{
				if (fUSD == null)
				{
					fUSD = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "USD");
				}
				return fUSD;
			}
		}

		#endregion

		#region GST1

		AccTaxRate fGST1;
		AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = CreateTaxRate("GST1", "GST Rate 1", 10);
				}
				return fGST1;
			}
		}

		#endregion

		#region WHT1

		AccWithholding fWHT1;
		AccWithholding WHT1
		{
			get
			{
				if (fWHT1 == null)
				{
					fWHT1 = CreateWithholdingTax("WHT1", "WHT Rate 1", 5);
				}
				return fWHT1;
			}
		}

		#endregion

		#endregion

	}
}
