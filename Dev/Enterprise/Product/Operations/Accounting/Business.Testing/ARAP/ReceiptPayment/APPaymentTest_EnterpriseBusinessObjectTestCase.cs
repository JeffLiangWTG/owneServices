using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.ReceiptPayment.Testing
{
	[TestedType(typeof(APPayment))]
	public class APPaymentTest_EnterpriseBusinessObjectTestCase : EnterpriseBusinessObjectTestCase
	{
		protected override bool IsDeleteSupported()
		{
			return false;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}

		[TestedType(typeof(APPayment))]
		public class APPaymentMatchingTest : ReceiptPaymentBaseMatchingTest
		{
			protected override ReceiptPaymentBase GetNewReceiptPayment()
			{
				return Factory.New<APPayment>();
			}
		}

		#region Implementation

		#region Create Business Objects

		protected void SetAPInvoiceInfo(Charge charge, string invoiceNumber, ZDateTime invoiceDate, ZDateTime dueDate)
		{
			charge.JR_APInvoiceNum = invoiceNumber;
			charge.JR_APInvoiceDate = invoiceDate;
			charge.JR_PaymentDate = dueDate;
		}

		protected void SetAPPaymentInfo(Charge charge, string paymentType, AccBankAccount bankAccount, string chequeOrReference)
		{
			charge.JR_PaymentType = paymentType;
			charge.JR_AB = bankAccount.PK;
			charge.JR_ChequeNo = chequeOrReference;
		}

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

		protected AccBankAccount CreateBankAccount(string code, string desc, string name, string abbreviation, RefCurrency currency)
		{
			AccBankAccount bankAccount = Factory.New<AccBankAccount>();
			bankAccount.AB_Code = code;
			bankAccount.AB_GB = GlbBranch.CurrentBranch.PK;
			bankAccount.AB_Desc = desc;

			AccGLHeader gLAccount = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_ControlAccount, ZBool.True));
			bankAccount.AB_AG = gLAccount.PK;
			bankAccount.AB_BankName = name;
			bankAccount.AB_BankAbbreviation = abbreviation;
			bankAccount.AB_BSB = "123456";
			bankAccount.AB_AccountNum = "12345678";
			bankAccount.AB_RX_NKAccountCurrency = currency.RX_Code;

			return bankAccount;
		}

		protected AccTaxRate CreateTaxRate(string code, string description, int rate)
		{
			AccTaxRate taxRate = Factory.New<AccTaxRate>();
			taxRate.AT_Code = code;
			taxRate.AT_Description = description;
			taxRate.AT_IsActive = true;
			taxRate.AT_RN_NKCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			taxRate.SetRateNumerator_ForTestOnly(rate);
			return taxRate;
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

		#region GBP

		protected RefCurrency fGBP;
		protected RefCurrency GBP
		{
			get
			{
				if (fGBP == null)
				{
					fGBP = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, "GBP");
				}
				return fGBP;
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

		protected AccChargeCode MRG100
		{
			get { return TestObjectCreator.MRG100; }
		}

		#endregion

		#region AUDBankAccount

		protected AccBankAccount fAUDBankAccount;
		protected AccBankAccount AUDBankAccount
		{
			get
			{
				if (fAUDBankAccount == null)
				{
					fAUDBankAccount = CreateBankAccount("ZHSBC", "HSBC AUD ACCT", "HSBC", "AUD", AUD);
				}
				return fAUDBankAccount;
			}
		}

		#endregion

		#region USDBankAccount

		protected AccBankAccount fUSDBankAccount;
		protected AccBankAccount USDBankAccount
		{
			get
			{
				if (fUSDBankAccount == null)
				{
					fUSDBankAccount = CreateBankAccount("ZHSBC", "HSBC USD ACCT", "HSBC", "USD", USD);
				}
				return fUSDBankAccount;
			}
		}

		#endregion

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
