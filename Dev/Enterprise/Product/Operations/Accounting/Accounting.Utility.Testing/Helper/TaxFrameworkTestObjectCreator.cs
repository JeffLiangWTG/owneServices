using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Moq;
using static Enterprise.Core.Constants;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Utility.Testing
{
	public class TaxFrameworkTestObjectCreator : AccountingTestObjectCreator
	{
		public TaxFrameworkTestObjectCreator(BusinessObjectFactory factory) : base(factory) { }

		#region CreateTaxTransaction

		public class CreateTaxTransactionParameters
		{
			public AccTaxConfiguration TaxConfiguration
			{
				get => taxConfiguration;
				set
				{
					taxConfiguration = value;
					TaxSystemCode = taxConfiguration.ETC_TaxSystemCode;
					Ledger = taxConfiguration.ETC_Ledger;

					TaxBasis =
						 taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.PostDate.Code ? TaxBasisList.Posting.Code :
						(taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.MatchDate.Code ? TaxBasisList.Matching.Code :
						(taxConfiguration.ETC_TaxRealisationMethod == TaxRealisationMethods.PostDateOfMatchTransaction.Code ? TaxBasisList.PostingOnMatching.Code : ""));

					LedgerControlAccountPK = taxConfiguration.ETC_AG_LedgerControlAccount;
					TaxControlAccountPK = taxConfiguration.ETC_AG_TaxControlAccount;
					TaxExpenseAccountPK = taxConfiguration.ETC_AG_TaxExpenseAccount;
					TaxPendingControlAccountPK = taxConfiguration.ETC_AG_TaxPendingControlAccount;
				}
			}
			AccTaxConfiguration taxConfiguration;
			public AccTransactionHeader TransactionHeader
			{
				get => transactionHeader;
				set
				{
					transactionHeader = value;
					TransactionHeaderPK = TransactionHeader.PK;
					PostDate = transactionHeader.AH_PostDate.Date;
				}
			}
			AccTransactionHeader transactionHeader;
			public ZGuid? TransactionHeaderPK { get; set; }

			public GlbCompany Company
			{
				get => company;
				set
				{
					company = value;
					CompanyPK = company.PK;
				}
			}
			GlbCompany company;
			public ZGuid CompanyPK { get; set; } = GlbCompany.CurrentCompany.PK;

			public GlbBranch Branch
			{
				get => branch;
				set
				{
					branch = value;
					BranchPK = branch.PK;
				}
			}
			GlbBranch branch;
			public ZGuid BranchPK { get; set; } = GlbBranch.CurrentBranch.PK;

			public GlbDepartment Department
			{
				get => department;
				set
				{
					department = value;
					DepartmentPK = department.PK;
				}
			}
			GlbDepartment department;
			public ZGuid DepartmentPK { get; set; } = GlbDepartment.CurrentDepartment.PK;

			public ZDate PostDate { get; set; } = ZDate.Today;

			public AccTaxRate TaxId { get; set; }

			public (ZInt Numerator, ZInt Denominator)? TaxRate { get; set; }

			public ZDecimal LocalTaxAmount { get; set; }

			public ZDecimal OsTaxAmount { get; set; }

			public ZDecimal LocalTaxBaseAmount { get; set; }

			public ZDecimal OsTaxBaseAmount { get; set; }

			public TaxSystemsConfiguration TaxSystem
			{
				get => taxSystem;
				set
				{
					taxSystem = value;
					TaxSystemCode = taxSystem.Code;
					TaxSuperType = TaxSystem.TaxSuperType;
					AffectsSourceTransactionTotal = TaxSystem.IncludeInInvoceTotal;
				}
			}
			TaxSystemsConfiguration taxSystem;

			public ZString TaxSystemCode { get; set; } = "TS";

			public RefCurrency Currency
			{
				get => currency;
				set
				{
					currency = value;
					CurrencyCode = currency.RX_Code;
				}
			}
			RefCurrency currency;

			public ZString CurrencyCode { get; set; } = CurrencyCodes.Australia;

			public ZString ServiceCode { get; set; }

			public ZString ServiceCodeDescription { get; set; }

			public ZString TaxBasis { get; set; } = TaxBasisList.Posting.Code;

			public ZString TaxSuperType { get; set; } = TaxSuperTypeList.Perceptions.Code;

			public ZDate RealisationDate { get; set; }

			public ZDate TaxDate { get; set; } = ZDate.Today;

			public ZBool IsCancelled { get; set; }

			public AccGLHeader LedgerControlAccount
			{
				get => ledgerControlAccount;
				set
				{
					ledgerControlAccount = value;
					LedgerControlAccountPK = ledgerControlAccount.PK;
				}
			}
			AccGLHeader ledgerControlAccount;
			public ZGuid LedgerControlAccountPK { get; set; }

			public AccGLHeader TaxControlAccount
			{
				get => taxControlAccount;
				set
				{
					taxControlAccount = value;
					TaxControlAccountPK = taxControlAccount.PK;
				}
			}
			AccGLHeader taxControlAccount;
			public ZGuid TaxControlAccountPK { get; set; }

			public AccGLHeader TaxExpenseAccount
			{
				get => taxExpenseAccount;
				set
				{
					taxExpenseAccount = value;
					TaxExpenseAccountPK = taxExpenseAccount.PK;
				}
			}
			AccGLHeader taxExpenseAccount;
			public ZGuid TaxExpenseAccountPK { get; set; }

			public AccGLHeader TaxPendingControlAccount
			{
				get => taxPendingControlAccount;
				set
				{
					taxPendingControlAccount = value;
					TaxPendingControlAccountPK = taxPendingControlAccount.PK;
				}
			}
			AccGLHeader taxPendingControlAccount;
			public ZGuid TaxPendingControlAccountPK { get; set; }

			public ZString Ledger { get; set; } = TaxConfigurationLedgers.AccountsPayable.Code;

			public ZDecimal? EffectiveRate { get; set; }

			public ZBool AffectsSourceTransactionTotal { get; set; } = true;

			public AccTransactionHeader MatchTransaction { get; set; }

			public ZBool DoesNotCreateGLMovemetsOnSaving { get; set; }
		}

		public AccTaxTransaction CreateTaxTransaction(CreateTaxTransactionParameters parameters)
		{
			var taxTransaction = Factory.New<AccTaxTransaction>();

			taxTransaction.ATT_TaxDate = parameters.TaxDate;
			taxTransaction.ATT_IsCancelled = parameters.IsCancelled;
			taxTransaction.ATT_AH = parameters.TransactionHeaderPK ?? Factory.NewWithValidTestData<APInvoice>().PK;
			taxTransaction.ATT_Basis = parameters.TaxBasis;

			taxTransaction.ATT_AG_LedgerControlAccount = parameters.LedgerControlAccountPK;
			taxTransaction.ATT_AG_TaxControlAccount = parameters.TaxControlAccountPK;
			taxTransaction.ATT_AG_TaxExpenseAccount = parameters.TaxExpenseAccountPK;
			taxTransaction.ATT_AG_TaxPendingControlAccount = parameters.TaxPendingControlAccountPK;
			taxTransaction.ATT_Ledger = parameters.Ledger;
			taxTransaction.ATT_ETC = parameters.TaxConfiguration?.PK ?? Factory.NewWithValidTestData<AccTaxConfiguration>().PK;
			taxTransaction.ATT_TaxSystemCode = parameters.TaxSystemCode;

			taxTransaction.ATT_Basis = parameters.TaxBasis;

			taxTransaction.ATT_GC = parameters.CompanyPK;
			taxTransaction.ATT_GB = parameters.BranchPK;
			taxTransaction.ATT_GE_Department = parameters.DepartmentPK;

			taxTransaction.ATT_RX_NKOSTaxCurrency = parameters.CurrencyCode;

			taxTransaction.ATT_AffectsSourceTransactionTotal = parameters.AffectsSourceTransactionTotal;
			taxTransaction.ATT_OSTaxBaseAmount = parameters.OsTaxBaseAmount;
			taxTransaction.ATT_LocalTaxBaseAmount = parameters.LocalTaxBaseAmount;
			taxTransaction.ATT_OSTaxAmount = parameters.OsTaxAmount;
			taxTransaction.ATT_LocalTaxAmount = parameters.LocalTaxAmount;

			taxTransaction.ATT_PostDate = parameters.PostDate;

			if (parameters.TaxRate != default)
			{
				taxTransaction.ATT_RateNumerator = parameters.TaxRate.Value.Numerator;
				taxTransaction.ATT_RateDenominator = parameters.TaxRate.Value.Denominator;
			}

			taxTransaction.ATT_AT_TaxID = (parameters.TaxId ?? Factory.NewWithValidTestData<AccTaxRate>()).PK;

			taxTransaction.ATT_TaxAuthorityServiceCode = parameters.ServiceCode;

			taxTransaction.ATT_TaxAuthorityServiceCodeDescription = parameters.ServiceCodeDescription;

			taxTransaction.ATT_RealisationDate = parameters.RealisationDate;

			taxTransaction.ATT_TaxSuperType = parameters.TaxSuperType;

			if (parameters.EffectiveRate.HasValue)
			{
				taxTransaction.SetEffectiveRateOnTaxRecordCreation(parameters.EffectiveRate.Value);
			}

			taxTransaction.ATT_AH_MatchTransaction = parameters.MatchTransaction != null ? parameters.MatchTransaction.PK : ZGuid.Empty;

			if (parameters.DoesNotCreateGLMovemetsOnSaving)
			{
				var glMovementProcessorMock = new Mock<IGLMovementProcessor>();
				taxTransaction.SubstituteGLMovementProcessor_ForTestOnly(glMovementProcessorMock.Object);
			}

			return taxTransaction;
		}

		#endregion

		public AccTaxRecordTransactionLinePivot CreateTaxTransactionLinePivot(ZGuid taxTranactionPK, ITaxableTransactionLine line, bool isTaxExpense = false)
		{
			var pivot = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot.ATP_ATT = taxTranactionPK;
			pivot.LinkLine(line);
			pivot.ATP_IsTaxExpense = isTaxExpense;

			return pivot;
		}

		public AccTaxGLMovement CreateAccTaxGLMovement(ZGuid taxTransactionPK = default, ZGuid? debitAccountPK = default, ZGuid? creditAccountPK = default, ZDecimal? amount = default, ZInt? period = default, ZDate? date = default, string type = default)
		{
			var glMovement = Factory.New<AccTaxGLMovement>();
			if (taxTransactionPK != default)
			{
				glMovement.ATM_ATT_TaxTransaction = taxTransactionPK;
			}

			glMovement.ATM_AG_DebitAccount = debitAccountPK ?? Factory.NewWithValidTestData<AccGLHeader>().PK;
			glMovement.ATM_AG_CreditAccount = creditAccountPK ?? Factory.NewWithValidTestData<AccGLHeader>().PK;

			glMovement.ATM_Amount = amount ?? 1;
			glMovement.ATM_Date = date ?? ZDate.Today;

			if (period.HasValue)
			{
				glMovement.ATM_Period = period.Value;
			}
			else if (glMovement.ATM_Period == 0)
			{
				glMovement.ATM_Period = 1;
			}

			glMovement.ATM_Type = type ?? TaxGLMovementTypeList.Normal.Code;

			return glMovement;
		}

		public void CalculateTaxes(InvoicingBase invoice)
		{
			var taxRecordParent = TaxFrameworkObjectFactory.GetInvoicingBaseTaxRecordParent(invoice);
			((ITaxProcessor)new TaxProcessor()).ProcessTaxesOnPosting(taxRecordParent);
		}

		public Mock<ITaxFrameworkConfigurationHelper> SetupMockTaxFrameworkConfigurationHelper()
		{
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			var mockITaxFrameworkConfigurationHelper = new Mock<ITaxFrameworkConfigurationHelper>();
			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetTaxFrameworkConfigurationHelper()).Returns(mockITaxFrameworkConfigurationHelper.Object);
			ObjectFactory.Substitute(mockIAccountingMasterFilesDependencyFactory.Object);
			return mockITaxFrameworkConfigurationHelper;
		}
	}
}
