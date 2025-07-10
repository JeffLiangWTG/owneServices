using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class UpdateReciprocalExchangeRatesTestCase : TestCaseWithFactory
	{
		protected ICompany company;
		protected IBranch branch;

		protected override void SetUp()
		{
			base.SetUp();

			company = Env.CurrentCompany;
			branch = Env.CurrentBranch;
		}

		void updateReciprocalExchangeRates(char isReciprocal)
		{
			string cmd = string.Format("UPDATE dbo.GlbCompany " +
										"SET GC_IsReciprocal = {0}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetDate() " +
										"WHERE GC_PK = '{1}' " +
										"EXEC updateReciprocalExchangeRates '{2}', 'TST'", isReciprocal == 'Y' ? "1" : "0", company.PK.ToString(), company.PK.ToString());
			TestConnection.ExecuteNonQuery(cmd);
			Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).GC_IsReciprocal = isReciprocal == 'Y';
		}

		[TestDate(2022, 01, 11)]
		public void TestExchangeRatesAreInvertedWhenIsReciprocalFlagIsChanged_SystemLevelRateTypes()
		{
			TestExchangeRatesAreInvertedWhenIsReciprocalFlagIsChangedCore(AccountingMasterFilesConstants.GetExchangeRateTypesList_SystemLevel().GetAllCodes());
		}

		[TestDate(2022, 01, 11)]
		public void TestExchangeRatesAreInvertedWhenIsReciprocalFlagIsChanged_CompanyLevelRateTypes()
		{
			TestExchangeRatesAreInvertedWhenIsReciprocalFlagIsChangedCore(AccountingMasterFilesConstants.GetExchangeRateTypesList_CompanyLevel().GetAllCodes());
		}

		void TestExchangeRatesAreInvertedWhenIsReciprocalFlagIsChangedCore(string[] rateTypes)
		{
			var objectCreator = new TestObjectCreator(Factory);
			objectCreator.CreateTestPeriodsForEntireYear(2022);
			Factory.Save();

			var currency = Factory.NewWithValidTestData<RefCurrency>();
			var rates = new List<RefExchangeRate>();
			foreach (var rateType in rateTypes)
			{
				var exchangeRate = currency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = rateType;
				exchangeRate.RE_SellRate = 9.0m;
				exchangeRate.RE_GC = company.PK;
				exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-5);
				exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(10);
				rates.Add(exchangeRate);
			}
			Factory.Save();

			updateReciprocalExchangeRates('N');
			AssertExchangeRateInversion();
			rates.ForEach(exchangeRate => exchangeRate.RE_SellRate = 9.0m);
			Factory.Save();

			updateReciprocalExchangeRates('Y');
			AssertExchangeRateInversion();

			void AssertExchangeRateInversion()
			{
				foreach (var rate in rates)
				{
					rate.Reload();
					if (rate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRate ||
						rate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsRateSecondary ||
						rate.RE_ExRateType == Core.Constants.ExchangeRateTypes.Code.CustomsMeasureEURExRate)
					{
						AssertEquals($"Exchange rate with '{rate.RE_ExRateType}' rate type must NOT be inverted.", 9.0m, rate.RE_SellRate);
					}
					else
					{
						AssertEquals($"Exchange rate with '{rate.RE_ExRateType}' rate type must be inverted.", 0.111111m, rate.RE_SellRate);
					}
				}
			}
		}

		public void TestTransactionLines()
		{
			ARInvoice invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = branch.PK;
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_GB = branch.PK;
			ZString currencyCode = getCurrencyCode();
			invoice.AH_RX_NKTransactionCurrency = currencyCode;
			invoice.AH_ExchangeRate = 7.0M;
			line.AL_RX_NKTransactionCurrency = currencyCode;
			line.AL_LineAmount = 1000.00M;
			line.AL_GSTVAT = 100.00M;
			line.AL_OSAmount = 9900.00M;
			line.AL_AG = new TestObjectCreator(Factory).GLHeader1.PK;
			Factory.Save();

			AssertEquals("AL_ExchangeRate", 7.0M, line.AL_ExchangeRate);

			updateReciprocalExchangeRates('N');

			line.Reload();

			AssertEquals("AL_ExchangeRate", 9.000000000M, line.AL_ExchangeRate);

			updateReciprocalExchangeRates('Y');

			line.Reload();

			AssertEquals("AL_ExchangeRate", 0.111111111M, line.AL_ExchangeRate);
		}

		public void TestJobConsolCost()
		{
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			ApportionmentListing appList = new ApportionmentListing(Factory, consol);
			JobConsolCost cost = appList.CostsCollection.TryAddNew();
			cost.E6_GC = company.PK;
			string currencyCode = getCurrencyCode();
			cost.E6_AC_ChargeCode = chargeCode.PK;
			cost.E6_RX_NKCurrency = currencyCode;
			cost.E6_ExchangeRate = 0.111111111M;
			cost.E6_LocalCostAmount = 900.00M;
			cost.E6_OSCostAmount = 100.00M;
			Factory.Save();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.111111111M, cost.E6_ExchangeRate);

			updateReciprocalExchangeRates('N');
			cost.Reload();
			foreach (ApportionSplitCharge splitCharge in cost.ApportionmentCharges)
			{
				splitCharge.InvoicingJob.ExchangeRates.Reload(true);
				splitCharge.Reload();
			}
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.111111M, cost.E6_ExchangeRate);

			cost.E6_ExchangeRate = 18.0M;
			cost.E6_OSCostAmount = 1800.00M;
			cost.E6_LocalCostAmount = 100.00M;
			Factory.Save();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 18.0M, cost.E6_ExchangeRate);

			updateReciprocalExchangeRates('Y');
			cost.Reload();
			foreach (ApportionSplitCharge splitCharge in cost.ApportionmentCharges)
			{
				splitCharge.InvoicingJob.ExchangeRates.Reload(true);
				splitCharge.Reload();
			}
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.055556M, cost.E6_ExchangeRate);

			cost.E6_ExchangeRate = 9.0M;
			cost.E6_OSCostAmount = 0.0M;
			Factory.Save();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 9.0M, cost.E6_ExchangeRate);

			updateReciprocalExchangeRates('N');
			cost.Reload();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.111111M, cost.E6_ExchangeRate);

			cost.E6_ExchangeRate = 9.0M;
			cost.E6_OSCostAmount = 0.0M;
			Factory.Save();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 9.0M, cost.E6_ExchangeRate);

			updateReciprocalExchangeRates('Y');
			cost.Reload();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.111111M, cost.E6_ExchangeRate);

			cost.E6_ExchangeRate = 0.0M;
			cost.E6_OSCostAmount = 0.0M;
			Factory.Save();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.0M, cost.E6_ExchangeRate);

			updateReciprocalExchangeRates('Y');
			cost.Reload();
			AssertEquals(JobConsolCostSchema.Constants.E6_ExchangeRate, 0.0M, cost.E6_ExchangeRate);
		}

		public void TestTransactionHeader()
		{
			APJournal journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_GB = branch.PK;
			journal.DebitCreditSign = DebitCreditDataEntry.CR;
			journal.AH_RX_NKTransactionCurrency = getCurrencyCode();
			journal.AH_Ledger = LedgerTypes.IncompleteTransactions;
			journal.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			//initialExchangeRate can be an arbitrary number not equal to the expectedExchangeRate or the reciprocal of the expectedExchangeRate
			AssertTransactionHeaderExchangeRate(journal, 9000.00M, 900.00M, 0M, 1100.00M, 7.0M, 'N', 0.111111M);

			AssertTransactionHeaderExchangeRate(journal, 9000.00M, 900.00M, 90M, 1100.00M, 7.0M, 'N', 0.110110M);

			AssertTransactionHeaderExchangeRate(journal, 0M, 0M, 90M, 100.00M, 7.0M, 'N', 1.111111M);

			//initialExchangeRate can be an arbitrary number not equal to the expectedExchangeRate or the reciprocal of the expectedExchangeRate
			AssertTransactionHeaderExchangeRate(journal, 1000.00M, 100.00M, 0M, 9900.00M, 7.0M, 'Y', 0.111111M);

			AssertTransactionHeaderExchangeRate(journal, 1000.00M, 100.00M, 10M, 9900.00M, 7.0M, 'Y', 0.112121M);

			AssertTransactionHeaderExchangeRate(journal, 0M, 0M, 10M, 9M, 7.0M, 'Y', 1.111111M);

			//the expectedExchangeRate will be the reciprocal of the initialExpectedRate
			AssertTransactionHeaderExchangeRate(journal, 0.0M, 0.0M, 0.0M, 0M, 9.0M, 'N', 0.111111M);

			//the expectedExchangeRate will be the reciprocal of the initialExpectedRate
			AssertTransactionHeaderExchangeRate(journal, 0.0M, 0.0M, 0.0M, 0M, 9.0M, 'Y', 0.111111M);

			//the expectedExchangeRate will equal the initialExchangeRate
			AssertTransactionHeaderExchangeRate(journal, 0.0M, 0.0M, 0.0M, 0M, 0.0M, 'N', 0.0M);
		}

		void AssertTransactionHeaderExchangeRate(APJournal journal, decimal invoiceAmount, decimal gSTAmount, decimal otherTaxAmount, decimal oSTotal, decimal initialExchangeRate, char isReciprocal, decimal expectedExchangeRate)
		{
			journal.AH_ExchangeRate = initialExchangeRate;
			journal.AH_InvoiceAmount = invoiceAmount;
			journal.AH_GSTAmount = gSTAmount;
			journal.AH_LocalTaxAmountOtherTaxes = otherTaxAmount;
			journal.AH_OSTotal = oSTotal;
			journal.AH_OutstandingAmount = invoiceAmount + gSTAmount + otherTaxAmount;
			Factory.Save();
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_ExchangeRate, initialExchangeRate, journal.AH_ExchangeRate);
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, invoiceAmount, journal.AH_InvoiceAmount);
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_GSTAmount, gSTAmount, journal.AH_GSTAmount);
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_LocalTaxAmountOtherTaxes, otherTaxAmount, journal.AH_LocalTaxAmountOtherTaxes);
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_OSTotal, oSTotal, journal.AH_OSTotal);
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_OutstandingAmount, invoiceAmount + gSTAmount + otherTaxAmount, journal.AH_OutstandingAmount);

			updateReciprocalExchangeRates(isReciprocal);
			journal.Reload();
			AssertEquals(AccTransactionHeaderSchema.Constants.AH_ExchangeRate, expectedExchangeRate, journal.AH_ExchangeRate);
		}

		string getCurrencyCode()
		{
			return Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, Env.CurrentCompany.LocalCurrency.Code)).RX_Code;
		}

		string getCurrencyCodeWithSubUnitRatio100OrPlus()
		{
			var query = new ZQuery(RefCurrencySchema.RX_Code, SQLComparisonOperator.NotEqual, Env.CurrentCompany.LocalCurrency.Code);
			query.AddToFilter(new ZQuery(RefCurrencySchema.RX_SubUnitRatio, SQLComparisonOperator.GreaterThanOrEqualTo, 100));
			var result = Factory.LoadTop1<RefCurrency>(query);
			return (result == null) ? string.Empty : result.RX_Code.ToString();
		}

		public void TestJobCharge()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			Charge charge = job.Charges.AddNew();
			charge.JR_GB = branch.PK;
			AccChargeCode chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_GC = company.PK;
			charge.JR_AC = chargeCode.PK;

			var code = getCurrencyCodeWithSubUnitRatio100OrPlus();
			Assert("There should be at least one RefCurrency with 'RX_SubUnitRatio >= 100' and 'RX_Code <> local currency code'.", !string.IsNullOrEmpty(code));

			//note in the next line that the initialOSCostExRate has 8 dp and the expectedOSCostExRate has 9 dp
			AssertJobChargeOSCostExRate(charge, code, 'N', 99.99M, 900.0M, 0.1111M, 0.111100M);
			AssertJobChargeOSCostExRate(charge, code, 'Y', 900.0M, 100.0M, 9.0M, 0.111111M);
			AssertJobChargeOSCostExRate(charge, code, 'N', 0.0M, 0.0M, 9.0M, 0.111111M);

			//note in the next line that the initialOSSellExRate has 8 dp and the expectedOSSellExRate has 9 dp
			AssertJobChargeOSSellExRate(charge, code, 'Y', 900.0M, 100.0M, 9.0M, 0.111111M);
			AssertJobChargeOSSellExRate(charge, code, 'N', 99.99M, 900.0M, 9.0009M, 0.111100M);
			AssertJobChargeOSSellExRate(charge, code, 'N', 0.0M, 0.0M, 9.0M, 0.111111M);
		}

		void AssertJobChargeOSCostExRate(Charge charge, string currencyCode, char isReciprocal, decimal oSCostAmt, decimal localCostAmt, decimal initialOSCostExRate, decimal expectedOSCostExRate)
		{
			charge.JR_RX_NKCostCurrency = currencyCode;
			AssertNotNull(charge.CostExchangeRate);
			charge.CostExchangeRate.SetBuyRate_ForTestOnly(initialOSCostExRate);
			charge.JR_OSCostAmt = oSCostAmt;
			charge.JR_LocalCostAmt = localCostAmt;
			if (charge.RevenueExchangeRate != null)
			{
				charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(initialOSCostExRate);
			}
			Factory.Save();
			AssertEquals(JobChargeSchema.Constants.JR_OSCostExRate, initialOSCostExRate, charge.JR_OSCostExRate);
			AssertEquals(JobChargeSchema.Constants.JR_OSCostAmt, oSCostAmt, charge.JR_OSCostAmt);
			AssertEquals(JobChargeSchema.Constants.JR_LocalCostAmt, localCostAmt, charge.JR_LocalCostAmt);

			updateReciprocalExchangeRates(isReciprocal);
			((BusinessObject)company).Reload();
			charge.InvoicingJob.ExchangeRates.Reload(true);
			charge.Reload();
			AssertEquals(JobChargeSchema.Constants.JR_OSCostExRate, expectedOSCostExRate, charge.JR_OSCostExRate);
		}

		void AssertJobChargeOSSellExRate(Charge charge, string currencyCode, char isReciprocal, decimal oSSellAmt, decimal localSellAmt, decimal initialOSSellExRate, decimal expectedOSSellExRate)
		{
			charge.JR_RX_NKSellCurrency = currencyCode;
			charge.JR_OSSellAmt = oSSellAmt;
			AssertNotNull(charge.RevenueExchangeRate);
			charge.RevenueExchangeRate.SetBuyRate_ForTestOnly(initialOSSellExRate);
			charge.JR_OSSellExRate = initialOSSellExRate;
			charge.JR_LocalSellAmt = localSellAmt;
			Factory.Save();
			AssertEquals(JobChargeSchema.Constants.JR_OSSellExRate, initialOSSellExRate, charge.JR_OSSellExRate);
			AssertEquals(JobChargeSchema.Constants.JR_OSSellAmt, oSSellAmt, charge.JR_OSSellAmt);
			AssertEquals(JobChargeSchema.Constants.JR_LocalSellAmt, localSellAmt, charge.JR_LocalSellAmt);

			updateReciprocalExchangeRates(isReciprocal);
			((BusinessObject)company).Reload();
			charge.InvoicingJob.ExchangeRates.Reload(true);
			charge.Reload();
			AssertEquals(JobChargeSchema.Constants.JR_OSSellExRate, expectedOSSellExRate, charge.JR_OSSellExRate);
		}

		public void TestPaymentApproval()
		{
			APPaymentApprovalWithoutAuthorisation pa = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			pa.AV_Status = "AWA";
			pa.AV_GB = branch.PK;
			pa.AV_RX_NKPaymentCurrency = getCurrencyCode();

			AssertPaymentApprovalPayExRate(pa, 'N', 9.0M, 0.111111M, null, 0, 0, 0);
			AssertPaymentApprovalPayExRate(pa, 'Y', 9.0M, 0.111111M, null, 0, 0, 0);
			AssertPaymentApprovalPayExRate(pa, 'N', 0.0M, 0.0M, null, 0, 0, 0);

			APJournal journal = Factory.NewWithValidTestData<APJournal>();
			journal.AH_GB = branch.PK;
			journal.DebitCreditSign = DebitCreditDataEntry.CR;
			journal.AH_RX_NKTransactionCurrency = getCurrencyCode();
			journal.AH_Ledger = LedgerTypes.IncompleteTransactions;
			journal.AH_TransactionType = TransactionTypes.IncompleteInvoice;

			AssertPaymentApprovalPayExRate(pa, 'N', 9.0M, 0.111111M, journal, 0.0M, 0.0M, 0.0M);
			AssertPaymentApprovalPayExRate(pa, 'Y', 9.0M, 0.111111M, journal, 0.0M, 0.0M, 0.0M);
			AssertPaymentApprovalPayExRate(pa, 'N', 0.0M, 0.0M, journal, 0.0M, 0.0M, 0.0M);
			AssertPaymentApprovalPayExRate(pa, 'N', 9.0M, 0.111111M, journal, 9000.00M, 900.00M, 1100.00M);
			AssertPaymentApprovalPayExRate(pa, 'Y', 9.0M, 0.111111M, journal, 1000.00M, 100.00M, 9900.00M);
		}

		void AssertPaymentApprovalPayExRate(APPaymentApprovalWithoutAuthorisation pa, char isReciprocal, decimal initialPayExRate, decimal expectedPayExRate, APJournal journal, decimal invoiceAmount, decimal gSTAmount, decimal oSTotal)
		{
			if (journal == null)
			{
				pa.AV_AH = Guid.Empty;
				AssertEquals("AV_AH", Guid.Empty, pa.AV_AH);
			}
			else
			{
				journal.AH_ExchangeRate = initialPayExRate;
				journal.AH_InvoiceAmount = invoiceAmount;
				journal.AH_GSTAmount = gSTAmount;
				journal.AH_OSTotal = oSTotal;
				journal.AH_OutstandingAmount = invoiceAmount + gSTAmount;
				pa.AV_AH = journal.PK;
				Factory.Save();
				AssertEquals(AccTransactionHeaderSchema.Constants.AH_ExchangeRate, initialPayExRate, journal.AH_ExchangeRate);
				AssertEquals(AccTransactionHeaderSchema.Constants.AH_InvoiceAmount, invoiceAmount, journal.AH_InvoiceAmount);
				AssertEquals(AccTransactionHeaderSchema.Constants.AH_GSTAmount, gSTAmount, journal.AH_GSTAmount);
				AssertEquals(AccTransactionHeaderSchema.Constants.AH_OSTotal, oSTotal, journal.AH_OSTotal);
				AssertEquals(AccTransactionHeaderSchema.Constants.AH_OutstandingAmount, invoiceAmount + gSTAmount, journal.AH_OutstandingAmount);
			}
			pa.AV_PayExRate = initialPayExRate;
			Factory.Save();
			pa.Reload();
			AssertEquals("AV_PayExRate", initialPayExRate, pa.AV_PayExRate);

			updateReciprocalExchangeRates(isReciprocal);
			if (journal != null)
			{
				journal.Reload();
			}

			pa.Reload();
			AssertEquals("AV_PayExRate", expectedPayExRate, pa.AV_PayExRate);
		}

		public void TestJobExRate()
		{
			Job job = Factory.NewJobWithValidTestDataForTesting<Job>();
			job.JH_GC = company.PK;
			job.JH_GB = branch.PK;

			Accounting.Business.JobInvoicing.ExchangeRate rate = job.ExchangeRates.AddNew();
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			rate.JF_RX_NKRateCurrency = currency.RX_Code;

			AssertJobExRate(rate, 'N', 9.0M, 0.111111M, 9.0M, 0.111111M);
			AssertJobExRate(rate, 'Y', 9.0M, 0.111111M, 9.0M, 0.111111M);
		}

		void AssertJobExRate(Accounting.Business.JobInvoicing.ExchangeRate rate, char isReciprocal, decimal initialBuyRate, decimal expectedBuyRate, decimal initialSellRate, decimal expectedSellRate)
		{
			rate.JF_BaseRate = initialBuyRate;
			Factory.Save();
			AssertEquals(nameof(rate.JF_BaseRate), initialBuyRate, rate.JF_BaseRate);
			AssertEquals(nameof(rate.JF_SellRate), initialSellRate, rate.JF_SellRate);

			updateReciprocalExchangeRates(isReciprocal);
			rate.Reload();
			AssertEquals(nameof(rate.JF_BaseRate), expectedBuyRate, rate.JF_BaseRate);
			AssertEquals(nameof(rate.JF_SellRate), expectedSellRate, rate.JF_SellRate);
		}

		public void TestJobVoyageExRate()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			consol.Transports[0].JW_Vessel = "QF";
			consol.Transports[0].JW_VoyageFlight = "1234";
			consol.Transports[0].JW_RL_NKLoadPort = "AUSYD";
			consol.Transports[0].JW_RL_NKDiscPort = "SGSIN";
			consol.Transports[0].JW_ETA = ZDateTime.Now;
			consol.Transports[0].JW_ETD = ZDateTime.Now.AddDays(1);
			consol.Transports[0].JW_IsLinked = ZBool.True;

			VoyageExRate rate = consol.Transports[0].Sailing.Voyage.ExRates.AddNew();
			rate.E8_GC = company.PK;
			//rate.E8_RX_NKExCurrency = "USD";
			RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
			rate.E8_RX_NKExCurrency = currency.RX_Code;
			Factory.Save();

			AssertVoyageExchangeRate(rate, 'N', 9.0M, 0.111111M);
			AssertVoyageExchangeRate(rate, 'Y', 9.0M, 0.111111M);
			AssertVoyageExchangeRate(rate, 'N', 0.0M, 0.0M);
		}

		void AssertVoyageExchangeRate(VoyageExRate rate, char isReciprocal, decimal initialExchangeRate, decimal expectedExchangeRate)
		{
			rate.E8_VoyageExchangeRate = initialExchangeRate;
			AssertEquals(JobVoyageExRateSchema.Constants.E8_VoyageExchangeRate, initialExchangeRate, rate.E8_VoyageExchangeRate);
			Factory.Save();

			updateReciprocalExchangeRates(isReciprocal);
			rate.Reload();
			AssertEquals(JobVoyageExRateSchema.Constants.E8_VoyageExchangeRate, expectedExchangeRate, rate.E8_VoyageExchangeRate);
		}
	}
}
