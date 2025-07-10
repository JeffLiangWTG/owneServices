using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class InvoiceLineTest : InvoicingLineBaseTest
	{
		bool OldIsGSTRegistered;
		bool OldIsWHTRegistered;

		protected override void SetUp()
		{
			base.SetUp();
			OldIsGSTRegistered = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;

			OldIsWHTRegistered = GlbCompany.CurrentCompany.GC_IsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = true;
		}
		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_IsWHTRegistered = OldIsWHTRegistered;
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = OldIsGSTRegistered;
		}

		protected override bool LineCanHaveTaxComponent
		{
			get { return true; }
		}

		protected override bool LineCanHaveForeignCurrency
		{
			get { return true; }
		}

		protected void TestChargeCodeChangeDoesntChangeOtherFields(Func<TransactionPendingAllocation, InvoicingBase> converter)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var invoicePendingAllocation = Factory.New<TransactionPendingAllocation>();
			invoicePendingAllocation.AH_TransactionNum = "INV";
			invoicePendingAllocation.AH_OH = org.PK;
			invoicePendingAllocation.AH_OSExTaxAmount = 100m;
			invoicePendingAllocation.AH_OSTaxAmount = 10m;
			invoicePendingAllocation.AH_Desc = "Test Description";
			invoicePendingAllocation.AH_PostDate = ZDateTime.Now.AddDays(-5);

			var at1 = TestObjectCreator.CreateTaxRate("AT1", "Rate 1", 1);
			at1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
			var chargeCode = CreateChargeCode("CC1", "Charge Code 1", Core.Constants.ChargeType.Margin, 100, at1, TestObjectCreator.WHT1);

			Factory.Save();

			var result = converter(invoicePendingAllocation);

			// Setup for IsGSTMandatory check, otherwise updating charge code doesn't update other fields
			result.AH_OH = result.AH_Ledger == LedgerTypes.AccountsReceivable ? GSTRegisteredDebtor.PK : GSTRegisteredCreditor.PK;

			// When tax, tax message and description is empty
			var line = (InvoicingLineBase)result.Lines.AddNew();
			line.GenericCharge = chargeCode.PK;

			AssertEquals(at1.PK, line.AL_AT);
			AssertEquals(TestObjectCreator.TaxMsg1.PK, line.AL_A9_VATClass);
			AssertEquals(chargeCode.AC_Desc, line.AL_Desc);

			// When tax is not empty, tax message is empty
			var lineDesc = "Custom description";

			var line1 = (InvoicingLineBase)result.Lines.AddNew();
			line1.AL_AT = TestObjectCreator.KDV1.PK;

			line1.GenericCharge = chargeCode.PK;

			AssertEquals(TestObjectCreator.KDV1.PK, line1.AL_AT);
			Assert("Tax message should not be set when tac id is not empty.", line1.AL_A9_VATClass.IsEmpty);

			// When tax, tax message and description is not empty
			var line2 = (InvoicingLineBase)result.Lines.AddNew();
			line2.AL_AT = TestObjectCreator.KDV1.PK;
			line2.AL_A9_VATClass = TestObjectCreator.TaxMsg2.PK;
			line2.AL_Desc = lineDesc;

			line2.GenericCharge = chargeCode.PK;

			AssertEquals(TestObjectCreator.KDV1.PK, line2.AL_AT);
			AssertEquals(TestObjectCreator.TaxMsg2.PK, line2.AL_A9_VATClass);
			AssertEquals(lineDesc, line2.AL_Desc);
		}

		#region Create Business Objects

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			job.JH_LocalChargesCFX = localClientCFX;
			job.JH_AgentChargesCFX = agentCFX;
			return job;
		}

		protected ExchangeRate CreateExchangeRate(Job parentJob, RefCurrency currency, decimal buyRate)
		{
			ExchangeRate exchangeRate = parentJob.ExchangeRates.AddNew();
			exchangeRate.JF_RX_NKRateCurrency = currency.RX_Code;
			exchangeRate.JF_BaseRate = buyRate;
			return exchangeRate;
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, ZString costPlaceOfSupply = default)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_CostPlaceOfSupply = costPlaceOfSupply;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			return charge;
		}

		protected AccWithholding CreateWithholdingTax(string code, string description, int rate)
		{
			AccWithholding taxRate = Factory.New<AccWithholding>();
			taxRate.AW_Code = code;
			taxRate.AW_Description = description;
			taxRate.AW_IsActive = true;
			taxRate.AW_Rate = rate;
			taxRate.AW_GC = GlbCompany.CurrentCompany.PK;
			return taxRate;
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
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

		#endregion

		#region AUD

		protected RefCurrency fAUD;
		protected RefCurrency AUD
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

		protected RefCurrency fUSD;
		protected RefCurrency USD
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

		#region ABIGAS

		protected OrgHeader fABIGAS;
		protected OrgHeader ABIGAS
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

		#region AALSHI

		protected OrgHeader fAALSHI;
		protected OrgHeader AALSHI
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

		#region ZECTRA

		protected OrgHeader fZECTRA;
		protected OrgHeader ZECTRA
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

		protected AccChargeCode fMRG100;
		protected AccChargeCode MRG100
		{
			get
			{
				if (fMRG100 == null)
				{
					fMRG100 = CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Constants.ChargeType.Margin, 100, GST1, WHT1);
				}
				return fMRG100;
			}
		}

		#endregion

		#region MRG100_1

		protected AccChargeCode fMRG100_1;
		protected AccChargeCode MRG100_1
		{
			get
			{
				if (fMRG100_1 == null)
				{
					fMRG100_1 = CreateChargeCode("MRG100.1", "Margin 100 With GST & WHT", Constants.ChargeType.Margin, 100, GST2, WHT2);
				}
				return fMRG100_1;
			}
		}

		#endregion

		#region GST1

		protected AccTaxRate fGST1;
		protected AccTaxRate GST1
		{
			get
			{
				if (fGST1 == null)
				{
					fGST1 = TestObjectCreator.CreateTaxRate("GST1", "GST Rate 1", 10);
				}
				return fGST1;
			}
		}

		#endregion

		#region GST2

		protected AccTaxRate fGST2;
		protected AccTaxRate GST2
		{
			get
			{
				if (fGST2 == null)
				{
					fGST2 = TestObjectCreator.CreateTaxRate("GST2", "GST Rate 2", 5);
				}
				return fGST2;
			}
		}

		#endregion

		#region WHT1

		protected AccWithholding fWHT1;
		protected AccWithholding WHT1
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

		#region WHT2

		protected AccWithholding fWHT2;
		protected AccWithholding WHT2
		{
			get
			{
				if (fWHT2 == null)
				{
					fWHT2 = CreateWithholdingTax("WHT2", "WHT Rate 2", 10);
				}
				return fWHT2;
			}
		}

		#endregion
	}
}
