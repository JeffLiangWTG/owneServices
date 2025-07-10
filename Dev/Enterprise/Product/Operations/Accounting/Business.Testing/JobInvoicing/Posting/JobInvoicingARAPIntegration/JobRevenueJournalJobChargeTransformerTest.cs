using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class JobRevenueJournalJobChargeTransformerTest : BaseIntegrationTransformerTest
	{
		#region TestJobRevenueJournal

		public void TestJobRevenueJournal()
		{
			Job job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.GBP, 0.7M);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0M);

			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);

			var line1 = (JobRevenueJournalLine)journal.Lines[1];
			line1.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line1.AL_ExchangeRate = 2M;
			line1.OSUnsignedLineAmount = 200M;

			var line2 = (JobRevenueJournalLine)journal.Lines.AddNew();
			line2.AL_RX_NKTransactionCurrency = TestObjectCreator.GBP.RX_Code;
			line2.AL_ExchangeRate = 0.5M;
			line2.DebitCreditSign = DebitCreditDataEntry.DR;
			line2.OSUnsignedLineAmount = 10M;

			var line3 = (JobRevenueJournalLine)journal.Lines.AddNew();
			line3.AL_RX_NKTransactionCurrency = Constants.CurrencyCodes.Ukraine;
			line3.AL_ExchangeRate = 4M;
			line3.DebitCreditSign = DebitCreditDataEntry.CR;
			line3.OSUnsignedLineAmount = 80M;

			Factory.Save();

			AssertEquals("New exchange rate must not be added to the job rates.", 2, job.ExchangeRates.Count);
			AssertEquals(TestObjectCreator.GBP.RX_Code, job.ExchangeRates[0].JF_RX_NKRateCurrency);
			AssertEquals("Non zero exchange can't be overriden.", 0.7M, job.ExchangeRates[0].JF_BaseRate);
			AssertEquals(TestObjectCreator.USD.RX_Code, job.ExchangeRates[1].JF_RX_NKRateCurrency);
			AssertEquals("Zero exchange rate should be overriden.", 2M, job.ExchangeRates[1].JF_BaseRate);

			AssertEquals("Charges.Count", 4, job.Charges.Count);

			AssertCharge(job.Charges[0], journal.Lines[0]);
			AssertCharge(job.Charges[1], journal.Lines[1]);
			AssertCharge(job.Charges[2], journal.Lines[2]);
			AssertCharge(job.Charges[3], journal.Lines[3]);
		}

		public void TestJobRevenueJournalPopulatesChargesCorrectly()
		{
			using (AccountingConfigurationRegistry.Instance.JobRevenueJournalGLAccountDefaultingRules.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, AccountingConstants.JobRevenueJournalGLAccountDefaultingRuleTypes.Both.Code))
			{
				var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
				var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
				var dRRevJournalLine = (JobRevenueJournalLine)journal.Lines[0];
				dRRevJournalLine.CostRevenueType = TransactionLineTypes.Revenue;
				var cRRevJournalLine = (JobRevenueJournalLine)journal.Lines[1];
				cRRevJournalLine.CostRevenueType = TransactionLineTypes.Revenue;

				journal.Lines.AddNew();
				var dRCostJournalLine = ((JobRevenueJournalLine)journal.Lines[2]);
				dRCostJournalLine.OSUnsignedLineAmount = 15M;
				dRCostJournalLine.DebitCreditSign = nameof(DebitCredit.DR);
				dRCostJournalLine.CostRevenueType = TransactionLineTypes.Cost;

				journal.Lines.AddNew();
				var cRCostJournalLine = ((JobRevenueJournalLine)journal.Lines[3]);
				cRCostJournalLine.OSUnsignedLineAmount = 15M;
				cRCostJournalLine.DebitCreditSign = nameof(DebitCredit.CR);
				cRCostJournalLine.CostRevenueType = TransactionLineTypes.Cost;

				AssertEquals(4, journal.Lines.Count);
				AssertJournalLine(dRRevJournalLine, nameof(DebitCredit.DR), TransactionLineTypes.Revenue);
				AssertJournalLine(cRRevJournalLine, nameof(DebitCredit.CR), TransactionLineTypes.Revenue);
				AssertJournalLine(dRCostJournalLine, nameof(DebitCredit.DR), TransactionLineTypes.Cost);
				AssertJournalLine(cRCostJournalLine, nameof(DebitCredit.CR), TransactionLineTypes.Cost);

				Factory.Save();

				AssertEquals("Charges.Count", 4, job.Charges.Count);

				AssertCharge(job.Charges[0], journal.Lines[0]);
				AssertCharge(job.Charges[1], journal.Lines[1]);
				AssertCharge(job.Charges[2], journal.Lines[2], false);
				AssertCharge(job.Charges[3], journal.Lines[3], false);
			}
		}

		void AssertJournalLine(JobRevenueJournalLine line, ZString expectedDebitCreditSign, ZString expectedCostReveneuType)
		{
			AssertEquals(expectedDebitCreditSign, line.DebitCreditSign);
			AssertEquals(expectedCostReveneuType, line.CostRevenueType);
		}

		public void TestJobRevenueJournal_WithoutCriticalException()
		{
			Job job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			TestObjectCreator.CreateExchangeRate(job, TestObjectCreator.USD, 0.03M);

			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);

			var line0 = (JobRevenueJournalLine)journal.Lines[0];
			line0.AL_RX_NKTransactionCurrency = TestObjectCreator.USD.RX_Code;
			line0.AL_ExchangeRate = 0.03M;
			line0.OSUnsignedLineAmount = 7m;
			line0.LocalUnsignedLineAmount = 233.33m;

			var line1 = (JobRevenueJournalLine)journal.Lines[1];
			line1.OSUnsignedLineAmount = 233.33M;
			Factory.Save();

			AssertEquals("Charges.Count", 2, job.Charges.Count);
		}

		public void TestJobRevenueJournal_DisbursementCharge()
		{
			AccChargeCode disbursementCharge = TestObjectCreator.CC1;
			disbursementCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;

			Job job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);

			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(disbursementCharge, job, 100M);
			Factory.Save();

			AssertEquals("Charges.Count", 2, job.Charges.Count);

			AssertCharge(job.Charges[0], journal.Lines[0]);
			AssertCharge(job.Charges[1], journal.Lines[1]);
		}

		public void TestTransformCore_JR_SellTaxDate()
		{
			AccChargeCode disbursementCharge = TestObjectCreator.CC1;
			disbursementCharge.AC_ChargeType = Core.Constants.ChargeType.Disbursement;
			Job job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(disbursementCharge, job, 100M);
			journal.Lines[0].AL_AT = TestObjectCreator.VATSPV.PK;
			journal.Lines[0].AL_TaxDate = ZDate.Today.AddDays(5);
			journal.Lines[1].AL_AT = TestObjectCreator.VATSPV.PK;
			journal.Lines[1].AL_TaxDate = ZDate.Today.AddDays(8);
			Factory.Save();

			AssertEquals(TestObjectCreator.VATSPV.PK, job.Charges[0].JR_AT_SellGSTRate);
			AssertEquals(ZDate.Today.AddDays(5), job.Charges[0].JR_SellTaxDate);
			AssertEquals(TestObjectCreator.VATSPV.PK, job.Charges[1].JR_AT_SellGSTRate);
			AssertEquals(ZDate.Today.AddDays(8), job.Charges[1].JR_SellTaxDate);
		}

		public void TestTransformCore_JR_CostTaxDate()
		{
			var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			var dRRevJournalLine = (JobRevenueJournalLine)journal.Lines[0];
			dRRevJournalLine.CostRevenueType = TransactionLineTypes.Revenue;
			var cRRevJournalLine = (JobRevenueJournalLine)journal.Lines[1];
			cRRevJournalLine.CostRevenueType = TransactionLineTypes.Revenue;

			journal.Lines.AddNew();
			var crCostJournalLine = ((JobRevenueJournalLine)journal.Lines[2]);
			crCostJournalLine.OSUnsignedLineAmount = 15M;
			crCostJournalLine.DebitCreditSign = nameof(DebitCredit.CR);
			crCostJournalLine.CostRevenueType = TransactionLineTypes.Cost;
			crCostJournalLine.AL_AT = TestObjectCreator.VATSPV.PK;
			crCostJournalLine.AL_TaxDate = ZDate.Today.AddDays(3);

			dRRevJournalLine.OSUnsignedLineAmount = 115M;
			Factory.Save();

			AssertEquals(TestObjectCreator.VATSPV.PK, job.Charges[2].JR_AT_CostGSTRate);
			AssertEquals(ZDate.Today.AddDays(3), job.Charges[2].JR_CostTaxDate);
		}

		void AssertCharge(Charge charge, TransactionLine line, bool isSellJRJLine = true)
		{
			var linePKToCheck = isSellJRJLine ? charge.JR_AL_ARLine : charge.JR_AL_APLine;
			AssertEquals("charge must be linked to JR journal.", linePKToCheck, line.PK);

			AssertEquals("JR_AC", TestObjectCreator.CC1.PK, charge.JR_AC);
			AssertEquals("JR_Desc", line.AL_Desc, charge.JR_Desc);
			AssertEquals("JR_GB", line.AL_GB, charge.JR_GB);
			AssertEquals("JR_GE", line.AL_GE, charge.JR_GE);

			if (isSellJRJLine)
			{
				AssertEquals("JR_OH_SellAccount", ZGuid.Empty, charge.JR_OH_SellAccount);
				AssertEquals("JR_RX_NKSellCurrency", line.AL_RX_NKTransactionCurrency, charge.JR_RX_NKSellCurrency);
				AssertEquals("JR_AT_SellGSTRate", ZGuid.Empty, charge.JR_AT_SellGSTRate);
				AssertEquals("JR_AW_SellWHTRate", ZGuid.Empty, charge.JR_AW_SellWHTRate);
				AssertEquals("JR_OSSellExRate", line.AL_ExchangeRate, charge.JR_OSSellExRate);
				AssertEquals("JR_OSSellAmt", line.AL_OSExTaxAmount, charge.JR_OSSellAmt);
				AssertEquals("JR_LocalSellAmt", line.AL_LocalExTaxAmount, charge.JR_LocalSellAmt);
				AssertEquals("JR_OSSellGSTAmt", 0M, charge.JR_OSSellGSTAmt_Calc);
				AssertEquals("JR_OSCostAmt", 0M, charge.JR_OSCostAmt);
				AssertEquals("JR_EstimatedCost", 0M, charge.JR_EstimatedCost);
				AssertEquals("JR_AL_APLine", ZGuid.Empty, charge.JR_AL_APLine);
			}
			else
			{
				AssertEquals("JR_OH_CostAccount", ZGuid.Empty, charge.JR_OH_CostAccount);
				AssertEquals("JR_RX_NKCostCurrency", line.AL_RX_NKTransactionCurrency, charge.JR_RX_NKCostCurrency);
				AssertEquals("JR_AT_CostGSTRate", ZGuid.Empty, charge.JR_AT_CostGSTRate);
				AssertEquals("JR_AW_CostWHTRate", ZGuid.Empty, charge.JR_AW_CostWHTRate);
				AssertEquals("JR_OSCostExRate", line.AL_ExchangeRate, charge.JR_OSCostExRate);
				AssertEquals("JR_OSCostAmt", line.AL_OSExTaxAmount, -charge.JR_OSCostAmt);
				AssertEquals("JR_LocalCostAmt", line.AL_LocalExTaxAmount, -charge.JR_LocalCostAmt);
				AssertEquals("JR_OSCostGSTAmt_Calc", 0M, -charge.JR_OSCostGSTAmt_Calc);
				AssertEquals("JR_OSSellAmt", 0M, -charge.JR_OSSellAmt);
				AssertEquals("JR_EstimatedSell", 0M, -charge.JR_EstimatedRevenue);
				AssertEquals("JR_AL_ARLine", ZGuid.Empty, charge.JR_AL_ARLine);
			}
		}

		#endregion

		#region Implementation

		protected override BaseIntegrationTransformer GetTransformer(BusinessObjectFactory factory)
		{
			return new JobRevenueJournalJobChargeTransformer(factory);
		}

		protected override void SetUp()
		{
			base.SetUp();

			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper();
			periodTestHelper.SetupPeriods();
		}

		#endregion
	}
}