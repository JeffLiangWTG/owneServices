using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JCJournalLine))]
	public class JCJournalLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAL_CFXControlAccount()
		{
			JCJournalLine line = Factory.New<JCJournalLine>();
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, testObjectCreator.GLHeader1.PK.ToGuid());
			AssertEquals("Enterprise level", testObjectCreator.GLHeader1.PK, line.AL_CFXControlAccount);
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testObjectCreator.GLHeader2.PK.ToGuid());
			AssertEquals("Company level", testObjectCreator.GLHeader2.PK, line.AL_CFXControlAccount);
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("JCJournalLine should not be deleted", true);
		}

		public void TestSetCFXValues()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			Job job = CreateJob("Z00001000", ZECTRA, true, 10M, ABIGAS, true, 10M);
			CreateExchangeRate(job, USD, .7M);
			Charge charge = CreateCharge(job, MRG100, "Set CFX Values", USD, 200M, AALSHI, USD, 400M, ZECTRA);

			JCJournalHeader cFXHeader = Factory.New<JCJournalHeader>();
			JCJournalLine cFXLine = cFXHeader.Lines.AddNew();

			cFXLine.SetCFXValues(job, charge);

			AssertEquals("Line Type", ZArchitecture.Core.TransactionLineTypes.Revenue, cFXLine.AL_LineType);
			AssertEquals("Sequence", (byte)1, cFXLine.AL_Sequence);
			AssertEquals("Description", "Set CFX Values", cFXLine.AL_Desc);
			AssertEquals("Line Amount", -63.49M, cFXLine.AL_LineAmount);
			AssertEquals("OS Amount", -63.49M, cFXLine.AL_OSAmount);
			AssertEquals("Currency", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, cFXLine.AL_RX_NKTransactionCurrency);
			AssertEquals("Exchange Rate", 1M, cFXLine.AL_ExchangeRate);
			AssertEquals("Job Header", charge.JR_JH, cFXLine.AL_JH);
			AssertEquals("Charge Code", charge.JR_AC, cFXLine.AL_AC);
			AssertEquals("Branch", charge.JR_GB, cFXLine.AL_GB);
			AssertEquals("Department", charge.JR_GE, cFXLine.AL_GE);
			AssertEquals("GL Header", charge.ChargeCode.AC_AG_RevenueAccount, cFXLine.AL_AG);
		}

		public void TestSetCFXValuesWithOverrides()
		{
			ZQuery query = new ZQuery(GlbDepartmentSchema.GE_IsActive, true);
			GlbDepartment department1 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department1.PK);
			GlbDepartment department2 = Factory.LoadTop1<GlbDepartment>(query);
			query.AddToFilter(GlbDepartmentSchema.PK, SQLComparisonOperator.NotEqual, department2.PK);
			GlbDepartment department3 = Factory.LoadTop1<GlbDepartment>(query);
			AssertNotEquals("department1 should not equal department2", department1.PK, department2.PK);
			AssertNotEquals("department2 should not equal department3", department2.PK, department3.PK);
			AssertNotEquals("department1 should not equal department3", department1.PK, department3.PK);

			Guid oldCFXAccount1 = AccountingConfigurationRegistry.Instance.CFXAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, department1.PK.ToGuid());
			Guid oldCFXAccount2 = AccountingConfigurationRegistry.Instance.CFXAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, department2.PK.ToGuid());
			Guid oldEnterpriseCFXAccount = AccountingConfigurationRegistry.Instance.CFXAccount.Value;

			try
			{
				query = new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.BalanceSheetAccount);
				query.AddToFilter(new ZQuery(AccGLHeaderSchema.AG_AccountType, Core.Constants.AccountType.ProfitAndLossAccount), JoinCondition.Or);
				query.AddToFilter(AccGLHeaderSchema.AG_IsActive, true);
				AccGLHeader header1 = Factory.LoadTop1<AccGLHeader>(query);
				query.AddToFilter(AccGLHeaderSchema.PK, SQLComparisonOperator.NotEqual, header1.PK);
				AccGLHeader header2 = Factory.LoadTop1<AccGLHeader>(query);
				query.AddToFilter(AccGLHeaderSchema.PK, SQLComparisonOperator.NotEqual, header2.PK);
				AccGLHeader header3 = Factory.LoadTop1<AccGLHeader>(query);
				AssertNotEquals("header1 should not equal header2", header1.PK, header2.PK);
				AssertNotEquals("header1 should not equal header3", header1.PK, header3.PK);
				AssertNotEquals("header2 should not equal header3", header2.PK, header3.PK);

				AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				//enterprise level configuration
				AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, header3.PK.ToGuid());
				AssertEquals("Env.Registry.CFXAccount should equal", header3.PK.ToGuid(), AccountingConfigurationRegistry.Instance.CFXAccount.Value);
				AssertCFXLine(department1.PK);
				AssertCFXLine(department2.PK);
				AssertCFXLine(department3.PK);

				//department level configuration
				AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department1.PK.ToGuid(), header1.PK.ToGuid());
				AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department2.PK.ToGuid(), header2.PK.ToGuid());
				AssertCFXLine(department1.PK);
				AssertCFXLine(department2.PK);
				AssertCFXLine(department3.PK);
			}
			finally
			{
				using (AccountingConfigurationRegistry.Instance.CFXAccount.DataType.SuspendValidation())
				{
					AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldEnterpriseCFXAccount);
					AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department1.PK.ToGuid(), oldCFXAccount1);
					AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department2.PK.ToGuid(), oldCFXAccount2);
				}
			}
		}

		#region Implementation

		void AssertCFXLine(ZGuid department)
		{
			Job job = CreateJob("Z00001000", ZECTRA, true, 10M, ABIGAS, true, 10M, department);
			CreateExchangeRate(job, USD, .7M);
			Charge charge = CreateCharge(job, MRG100, "Set CFX Values", USD, 200M, AALSHI, USD, 400M, ZECTRA, department);
			JCJournalHeader cFXHeader = Factory.New<JCJournalHeader>();
			JCJournalLine cFXLine = cFXHeader.Lines.AddNew();
			cFXLine.SetCFXValues(job, charge);
			AssertEquals("GL Header", charge.ChargeCode.AC_AG_RevenueAccount, cFXLine.AL_AG);
		}

		protected TestObjectCreator testObjectCreator;

		protected override void SetUp()
		{
			base.SetUp();
			testObjectCreator = new TestObjectCreator(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return factory.New<JCJournalHeader>().Lines.AddNew();
		}

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX)
		{
			return CreateJob(jobNumber, localClient, billLocalClientInLocalCurrency, localClientCFX, agent, billAgentInLocalCurrency, agentCFX, GlbDepartment.CurrentDepartment.PK);
		}

		protected Job CreateJob(ZString jobNumber, OrgHeader localClient, bool billLocalClientInLocalCurrency, decimal localClientCFX,
			OrgHeader agent, bool billAgentInLocalCurrency, decimal agentCFX, ZGuid department)
		{
			Job job = Factory.NewJobForTesting<Job>();
			job.JH_JobNum = jobNumber;
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = department;
			job.LocalChargesPK = localClient.PK;
			job.AgentCollectPK = agent.PK;
			localClient?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", localClientCFX);
			agent?.CompanyData.AccCFXConfigurations.SetUplifts("ALL", "ALL", "ALL", agentCFX);
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
			return CreateCharge(parentJob, chargeCode, desc, costCurrency, oSCostAmt, creditor, sellCurrency, oSSellAmt, debtor, GlbDepartment.CurrentDepartment.PK);
		}

		protected Charge CreateCharge(Job parentJob, AccChargeCode chargeCode, ZString desc, RefCurrency costCurrency, ZDecimal oSCostAmt, OrgHeader creditor,
			RefCurrency sellCurrency, ZDecimal oSSellAmt, OrgHeader debtor, ZGuid department)
		{
			Charge charge = parentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_Desc = desc;

			charge.JR_OH_CostAccount = creditor.PK;
			charge.JR_RX_NKCostCurrency = costCurrency.RX_Code;
			charge.JR_OSCostAmt = oSCostAmt;

			charge.JR_OH_SellAccount = debtor.PK;
			charge.JR_RX_NKSellCurrency = sellCurrency.RX_Code;
			charge.JR_OSSellAmt = oSSellAmt;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = department;
			return charge;
		}

		protected AccChargeCode CreateChargeCode(string code, string description, string chargeType, decimal marginPercentage, AccTaxRate gST, AccWithholding wHT)
		{
			AccChargeCode chargeCode = Factory.New<AccChargeCode>();
			chargeCode.AC_Code = code;
			chargeCode.AC_Desc = description;
			chargeCode.AC_ChargeType = chargeType;
			chargeCode.AC_MarginPercentage = marginPercentage;
			if (gST != null)
			{
				chargeCode.AC_AT_GSTRate = gST.PK;
			}

			if (wHT != null)
			{
				chargeCode.AC_AW_WithholdingTaxRate = wHT.PK;
			}
			return chargeCode;
		}

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
					fAALSHI.CompanyData.SetAPTaxApplicable(true);
					fAALSHI.MiscServ.OM_APWHTApplicable = true;
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
					fMRG100 = CreateChargeCode("MRG100", "Margin 100 With GST & WHT", Core.Constants.ChargeType.Margin, 100, null, null);
					fMRG100.AC_AG_AccrualAccount = fMRG100.AC_AG_CostAccount = fMRG100.AC_AG_RevenueAccount = fMRG100.AC_AG_WIPAccount = testObjectCreator.GLHeader1.PK;
				}

				return fMRG100;
			}
		}

		#endregion

		#endregion
	}
}
