using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class IntercompanyCostsApportionment : NonPersistentBusinessObject, IObsoleteValidation
	{
		#region Schema

		public abstract class Schema
		{
			public const string TaxBranch = "TaxBranch";
			public const string TaxBranchName = "TaxBranchName";
		}

		#endregion

		public readonly IntercompanyCostsApportionmentInvoiceLine InvoiceLine;

		public IntercompanyCostsApportionment(BusinessObjectFactory factory, IntercompanyCostsApportionmentInvoiceLine invoiceLine)
			: base(factory)
		{
			InvoiceLine = invoiceLine;
			ExchangeRate = InvoiceLine.Invoice.ExchangeRate.Rate;
			IsUpdatingForeignApportionedAmountSuspended = false;
			IsUpdatingApportionmentFactorSuspended = false;
			IsAdjustingForeignGSTSuspended = false;
			IsAdjustingLocalGSTSuspended = false;
			IsAdjustingLocalAmountSuspended = false;
		}

		#region Properties

		#region Company

		[ReadOnly(true)]
		[List("Companies")]
		public ZGuid Company
		{
			get { return fCompany; }
			set
			{
				if (fCompany != value)
				{
					SetNonPersistentPropertyValue(CompanyInfo, ref fCompany, value);
					updateCompanyLocalCurrency();
					updateIntercompanyGLAccount();
					updateCompanyPostToGLAccount();
					updateAccountingPeriod();
				}
				if (!IsValidationSuspended)
				{
					ValidateCompany();
				}
			}
		}
		ZGuid fCompany;

		public ZPropertyInfo CompanyInfo
		{
			get { return GetZPropertyInfo(nameof(Company)); }
		}

		public void ValidateCompany()
		{
			CompanyInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(CompanyInfo, Companies, ResString.GetMultilingualString("2a647386-4a96-4bb8-b493-a83fe5cd7c20", "Please enter a valid Company."));
			if (Company == ZGuid.Empty)
			{
				CompanyInfo.AddError(Res.GetString("60862a0f-45d7-40bb-94be-8e64627e721a", "Please enter a valid Company."));
			}
		}

		public GlbCompanyCollection Companies
		{
			get
			{
				if (fCompanies == null)
				{
					ZQuery companiesQuery = new ZQuery(GlbCompanySchema.GC_IsActive, SQLComparisonOperator.Equal, ZBool.True);
					fCompanies = new GlbCompanyCollection(Factory, companiesQuery);
				}
				return fCompanies;
			}
		}
		GlbCompanyCollection fCompanies;

		#endregion

		#region CompanyLocalCurrency

		[MaxLength(3)]
		[List("Currencies")]
		public ZString CompanyLocalCurrency
		{
			get { return fCompanyLocalCurrency; }
			set
			{
				if (fCompanyLocalCurrency != value)
				{
					SetNonPersistentPropertyValue(CompanyLocalCurrencyInfo, ref fCompanyLocalCurrency, value);
					updateCompanyLocalExchangeRate();
				}
				if (!IsValidationSuspended)
				{
					ValidateCompanyLocalCurrency();
				}
			}
		}
		ZString fCompanyLocalCurrency;

		public ZPropertyInfo CompanyLocalCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyLocalCurrency)); }
		}

		public void ValidateCompanyLocalCurrency()
		{
			CompanyLocalCurrencyInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(CompanyLocalCurrencyInfo, Currencies, ResString.GetMultilingualString("024b1e12-6592-4782-bd55-e31afbcce1c2", "Please enter a valid Company Local Currency."));
		}

		#endregion

		#region Currencies

		public RefCurrencyCollection Currencies
		{
			get
			{
				if (fCurrencies == null)
				{
					fCurrencies = new RefCurrencyCollection(Factory);
				}
				return fCurrencies;
			}
		}
		RefCurrencyCollection fCurrencies;

		#endregion

		#region Branch

		[ReadOnly(true)]
		[List("Branches")]
		public ZGuid Branch
		{
			get
			{
				return fBranch;
			}
			set
			{
				if (fBranch != value)
				{
					SetNonPersistentPropertyValue(BranchInfo, ref fBranch, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateBranch();

					var branch = Factory.Load<GlbBranch>(Branch);
					if (GlbBranchCombinationValidation.ShouldValidateCombination(branch))
					{
						ValidateDepartment();
					}
				}
			}
		}
		ZGuid fBranch;

		public ZPropertyInfo BranchInfo
		{
			get { return GetZPropertyInfo(nameof(Branch)); }
		}

		public void ValidateBranch()
		{
			BranchInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(BranchInfo, Branches, ResString.GetMultilingualString("2a3adf82-0af7-47b2-9b68-a25c2ce0c0f8", "Please enter a valid Branch."));
			if (Branch == ZGuid.Empty)
			{
				BranchInfo.AddError(Res.GetString("1cf5d3b2-e699-47d8-b905-ef4ca92305cc", "Please enter a valid Branch."));
			}
		}

		public GlbBranchCollection Branches
		{
			get
			{
				if (fBranches == null)
				{
					fBranches = new GlbBranchCollection(Factory);
				}
				return fBranches;
			}
		}
		GlbBranchCollection fBranches;

		#endregion

		#region Department

		[ReadOnly(true)]
		[List("Departments")]
		public ZGuid Department
		{
			get { return fDepartment; }
			set
			{
				if (fDepartment != value)
				{
					SetNonPersistentPropertyValue(DepartmentInfo, ref fDepartment, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateDepartment();
				}
			}
		}
		ZGuid fDepartment;

		public ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(Department)); }
		}

		public void ValidateDepartment()
		{
			DepartmentInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidPK(DepartmentInfo, Departments, ResString.GetMultilingualString("857a2250-320d-42a6-bd4d-d88420a01ece", "Please enter a valid Department."));
			if (Department == ZGuid.Empty)
			{
				DepartmentInfo.AddError(Res.GetString("9f56c421-2c1a-441b-8701-c3d3d5b46d2b", "Please enter a valid Department."));
			}

			if (!IsInDatabase || BranchInfo.HasChanges || DepartmentInfo.HasChanges)
			{
				var branch = Factory.Load<GlbBranch>(Branch);
				var department = Factory.Load<GlbDepartment>(Department);
				if (branch != null && department != null)
				{
					GlbBranchCombinationValidation.CheckBranchDepartmentCombination(DepartmentInfo, branch, department);
				}
			}
		}

		public GlbDepartmentCollection Departments
		{
			get { return new GlbDepartmentCollection(Factory); }
		}

		#endregion

		#region IntercompanyGLAccount

		[List("GLAccounts")]
		public ZGuid IntercompanyGLAccount
		{
			get
			{
				return fIntercompanyGLAccount;
			}
			set
			{
				if (fIntercompanyGLAccount != value)
				{
					SetNonPersistentPropertyValue(IntercompanyGLAccountInfo, ref fIntercompanyGLAccount, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateIntercompanyGLAccount();
				}
			}
		}
		ZGuid fIntercompanyGLAccount;

		public ZPropertyInfo IntercompanyGLAccountInfo
		{
			get { return GetZPropertyInfo(nameof(IntercompanyGLAccount)); }
		}

		public void ValidateIntercompanyGLAccount()
		{
			IntercompanyGLAccountInfo.ClearAllNotifications();
			if (Company != GlbCompany.CurrentCompany.PK)
			{
				if (IntercompanyGLAccount == ZGuid.Empty)
				{
					IntercompanyGLAccountInfo.AddError(Res.GetString("508fdd28-7902-4811-b5a2-44cb958449f2", "Please setup the Intercompany Clearing Configuration in the registry."));
				}
			}
		}

		#endregion

		#region CompanyPostToGLAccount

		[List("GLAccounts")]
		public ZGuid CompanyPostToGLAccount
		{
			get
			{
				return fCompanyPostToGLAccount;
			}
			set
			{
				if (fCompanyPostToGLAccount != value)
				{
					SetNonPersistentPropertyValue(CompanyPostToGLAccountInfo, ref fCompanyPostToGLAccount, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateCompanyPostToGLAccount();
				}
			}
		}
		ZGuid fCompanyPostToGLAccount;

		public ZPropertyInfo CompanyPostToGLAccountInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyPostToGLAccount)); }
		}

		AccGLHeader CompanyPostToGLHeader
		{
			get { return Factory.Load<AccGLHeader>(CompanyPostToGLAccount); }
		}

		public void ValidateCompanyPostToGLAccount()
		{
			CompanyPostToGLAccountInfo.ClearAllNotifications();
			if (Company != GlbCompany.CurrentCompany.PK)
			{
				if (CompanyPostToGLAccount == ZGuid.Empty)
				{
					CompanyPostToGLAccountInfo.AddError(Res.GetString("bf584ae0-2ad5-4c1a-97bb-bd2c17d95ad0", "Please setup the Intercompany Clearing Configuration in the registry."));
				}
			}

			if (CompanyPostToGLHeader != null && !CompanyPostToGLHeader.AG_IsGlobal && !CompanyPostToGLHeader.CompanyFilters.Cast<AccGLHeaderCompanyFilter>().Any(x => x.ACF_GC_Company == Company))
			{
				CompanyPostToGLAccountInfo.AddError(Res.GetString("0b4029a5-8efc-4325-8061-3b1c115e1e4e", "This GL Account cannot be used for the company"));
			}
		}

		#endregion

		#region GLAccounts

		public AccGLHeaderCollection GLAccounts
		{
			get { return new AccGLHeaderCollection(Factory); }
		}

		#endregion

		#region LocalAmount (in login company currency)

		public ZDecimal LocalAmount
		{
			get { return fLocalAmount; }
			set
			{
				if (fLocalAmount != value)
				{
					SetNonPersistentPropertyValue(LocalAmountInfo, ref fLocalAmount, value);
					if (!IsAdjustingLocalAmountSuspended)
					{
						AdjustLocalAmounts();
					}
					updateCompanyLocalAmount();
					updateLocalGST();
				}
				if (!IsValidationSuspended)
				{
					ValidateAllLocalAmount();
				}
			}
		}
		ZDecimal fLocalAmount;

		public ZPropertyInfo LocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(LocalAmount)); }
		}

		protected void ValidateAllLocalAmount()
		{
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				apportionment.ValidateLocalAmount();
			}
		}

		public void ValidateLocalAmount()
		{
			LocalAmountInfo.ClearAllNotifications();
			ZDecimal total = 0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.LocalAmount;
			}
			if (total != InvoiceLine.LocalAmount)
			{
				LocalAmountInfo.AddError(Res.GetString("030bf496-389c-4b88-a392-0c6ab3bd0b72", "Local Amount column must sum to Invoice Line Local Amount."));
			}
		}

		#endregion

		#region ForeignApportionedAmount (in invoice currency)

		[ReadOnly(true)]
		public ZDecimal ForeignApportionedAmount
		{
			get { return fForeignApportionedAmount; }
			set
			{
				if (fForeignApportionedAmount != value)
				{
					ForeignApportionedAmountWithoutRounding = value;
					value = decimal.Round(value, InvoiceLine.Invoice.CurrencyObject.Decimals);
					SetNonPersistentPropertyValue(ForeignApportionedAmountInfo, ref fForeignApportionedAmount, value);
					if (InvoiceLine.Amount != 0.0M)
					{
						AdjustForeignApportionedAmounts();
					}
					updateForeignGST();
					updateLocalAmount();
					if ((!IsUpdatingApportionmentFactorSuspended) && (InvoiceLine.ApportionmentMethod == ZGuid.Empty))
					{
						updateApportionmentFactors();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateAllForeignApportionedAmounts();
					InvoiceLine.ValidateAmount();
				}
			}
		}
		ZDecimal fForeignApportionedAmount;

		ZDecimal ForeignApportionedAmountWithoutRounding;

		public ZPropertyInfo ForeignApportionedAmountInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignApportionedAmount)); }
		}

		void ValidateAllForeignApportionedAmounts()
		{
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				apportionment.ValidateForeignApportionedAmount();
			}
		}

		public void ValidateForeignApportionedAmount()
		{
			ForeignApportionedAmountInfo.ClearAllNotifications();
			if (ForeignApportionedAmount == 0.0M)
			{
				ForeignApportionedAmountInfo.AddError(Res.GetString("f53f5e6d-3efb-4804-9b6a-706cbb7f4a79", "Please enter an Amount."));
			}
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.ForeignApportionedAmount;
			}
			if ((total != InvoiceLine.Amount) && (!ForeignApportionedAmountInfo.HasErrors()))
			{
				ForeignApportionedAmountInfo.AddError(Res.GetString("50b394cf-98a0-4e9e-98fe-7d6f4fd652bf", "Foreign Amount column in Apportionment Details must sum to the Invoice Line Amount."));
			}
		}

		public bool IsUpdatingApportionmentFactorSuspended { get; set; }

		public bool IsAdjustingForeignGSTSuspended { get; set; }
		public bool IsAdjustingLocalGSTSuspended { get; set; }
		public bool IsAdjustingLocalAmountSuspended { get; set; }

		#endregion

		#region ForeignGST

		[ReadOnly(true)]
		public ZDecimal ForeignGST
		{
			get { return fForeignGST; }
			set
			{
				if (fForeignGST != value)
				{
					value = decimal.Round(value, InvoiceLine.Invoice.CurrencyObject.Decimals);
					SetNonPersistentPropertyValue(ForeignGSTInfo, ref fForeignGST, value);
					if (!IsAdjustingForeignGSTSuspended)
					{
						AdjustForeignGST();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateAllForeignGST();
				}
			}
		}
		ZDecimal fForeignGST;

		public ZPropertyInfo ForeignGSTInfo
		{
			get { return GetZPropertyInfo(nameof(ForeignGST)); }
		}

		public void AdjustForeignGST()
		{
			if (!InvoiceLine.IsApportionmentMethodEmpty)
			{
				ZDecimal total = 0.0M;
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					total += apportionment.ForeignGST;
				}
				ZDecimal difference = InvoiceLine.Tax - total;
				if (total != InvoiceLine.Tax)
				{
					IntercompanyCostsApportionment apportionment = getApportionmentWithLargestForeignGST();
					if (apportionment != null)
					{
						apportionment.fForeignGST += difference;
						apportionment.ForeignGSTInfo.RefreshBinding();
						ValidateAllForeignGST();
					}
				}
			}
		}

		IntercompanyCostsApportionment getApportionmentWithLargestForeignGST()
		{
			IntercompanyCostsApportionment result = null;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				if ((result == null) || (apportionment.ForeignGST > result.ForeignGST))
				{
					result = apportionment;
				}
			}
			return result;
		}

		public void ValidateAllForeignGST()
		{
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				apportionment.ValidateForeignGST();
			}
		}

		public void ValidateForeignGST()
		{
			ForeignGSTInfo.ClearAllNotifications();
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.ForeignGST;
			}
			if (total != InvoiceLine.Tax)
			{
				ForeignGSTInfo.AddError(Res.GetString("e3ce01bd-32a8-4f47-966b-3fbb499c9d21", "Foreign GST column must sum to Invoice Line Tax."));
			}
		}

		#endregion

		#region Company Local Amount (in company currency)

		public ZDecimal CompanyLocalAmount
		{
			get { return fCompanyLocalAmount; }
			set
			{
				if (fCompanyLocalAmount != value)
				{
					SetNonPersistentPropertyValue(CompanyLocalAmountInfo, ref fCompanyLocalAmount, value);
				}
			}
		}
		ZDecimal fCompanyLocalAmount;

		public ZPropertyInfo CompanyLocalAmountInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyLocalAmount)); }
		}

		#endregion

		#region ExchangeRate (invoice exchange rate)

		public ZDecimal ExchangeRate
		{
			get { return fExchangeRate; }
			set
			{
				if (fExchangeRate != value)
				{
					SetNonPersistentPropertyValue(ExchangeRateInfo, ref fExchangeRate, value);
					updateLocalAmount();
				}
				if (!IsValidationSuspended)
				{
					ValidateExchangeRate();
				}
			}
		}
		ZDecimal fExchangeRate;

		public ZPropertyInfo ExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRate)); }
		}

		public void ValidateExchangeRate()
		{
			ExchangeRateInfo.ClearAllNotifications();
			if (ExchangeRate == 0.0M || ExchangeRateInfo.Value.IsEmpty)
			{
				ExchangeRateInfo.AddError(Res.GetString("9bd33ef1-6d0d-4b87-8ed8-a6f317657e37", "Please make sure there is a buy exchange rate for {0} in the current company.", InvoiceLine.Invoice.CurrencyObject.RX_Code));
			}
		}

		#endregion

		#region TaxBranch

		[List("Branches")]
		public ZGuid TaxBranch
		{
			get
			{
				return fTaxBranch;
			}
			set
			{
				if (fTaxBranch != value)
				{
					SetNonPersistentPropertyValue(TaxBranchInfo, ref fTaxBranch, value);
				}
			}
		}
		ZGuid fTaxBranch;

		public ZPropertyInfo TaxBranchInfo
		{
			get { return GetZPropertyInfo(Schema.TaxBranch); }
		}

		GlbBranch TaxBranchObject
		{
			get { return Factory.Load<GlbBranch>(TaxBranch); }
		}

		public bool TaxBranch_ReadOnly => true;

		#endregion

		#region TaxBranchName

		public ZString TaxBranchName
		{
			get { return TaxBranchObject != null ? TaxBranchObject.GB_BranchName : ZString.Empty; }
		}

		public ZPropertyInfo TaxBranchNameInfo
		{
			get { return GetZPropertyInfo(Schema.TaxBranchName); }
		}

		#endregion

		#region CompanyLocalExchangeRate

		public ZDecimal CompanyLocalExchangeRate
		{
			get { return fCompanyLocalExchangeRate; }
			set
			{
				if (fCompanyLocalExchangeRate != value)
				{
					SetNonPersistentPropertyValue(CompanyLocalExchangeRateInfo, ref fCompanyLocalExchangeRate, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateCompanyLocalExchangeRate();
				}
			}
		}
		ZDecimal fCompanyLocalExchangeRate;

		public ZPropertyInfo CompanyLocalExchangeRateInfo
		{
			get { return GetZPropertyInfo(nameof(CompanyLocalExchangeRate)); }
		}

		public void ValidateCompanyLocalExchangeRate()
		{
			CompanyLocalExchangeRateInfo.ClearAllNotifications();
			if (CompanyLocalExchangeRate == 0.0M || CompanyLocalExchangeRateInfo.Value.IsEmpty)
			{
				GlbCompany company = Factory.Load<GlbCompany>(Company);
				string name = string.Empty;
				if (company != null)
				{
					name = company.GC_Name;
				}
				CompanyLocalExchangeRateInfo.AddError(Res.GetString("c0a4b6bc-771a-48fa-ba42-b21d208bd0d2", "Please make sure there is a buy exchange rate for {0} in this company ({1}).", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, name));
			}
		}

		#endregion

		#region AccountingPeriod

		public ZInt AccountingPeriod
		{
			get { return fAccountingPeriod; }
			set
			{
				if (fAccountingPeriod != value)
				{
					SetNonPersistentPropertyValue(AccountingPeriodInfo, ref fAccountingPeriod, value);
				}
				if (!IsValidationSuspended)
				{
					ValidateAccountingPeriod();
				}
			}
		}
		ZInt fAccountingPeriod;

		public ZPropertyInfo AccountingPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(AccountingPeriod)); }
		}

		public void ValidateAccountingPeriod()
		{
			AccountingPeriodInfo.ClearAllNotifications();
			if (AccountingPeriod == 0)
			{
				AccountingPeriodInfo.AddError(Res.GetString("6f0ff5d1-6ed7-4ea1-a232-bb67cc2ea099", "Please make sure Accounting Periods are setup in Period Management for this company, for the Posted Date."));
			}
			ZQuery query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, Company);
			query.AddToFilter(AccPeriodManagementSchema.AM_Period, SQLComparisonOperator.Equal, AccountingPeriod);
			AccPeriodManagement periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);
			if (periodManagement != null && periodManagement.AM_IsGeneralLedgerClosed)
			{
				AccountingPeriodInfo.AddError(Res.GetString("1a4c7db6-19c1-4503-bc8b-8427ff25e016", "The General Ledger Accounting period is closed in this company."));
			}
		}

		#endregion

		#region ApportionmentFactor

		[ReadOnly(true)]
		public ZDecimal ApportionmentFactor
		{
			get { return fApportionmentFactor; }
			set
			{
				if (fApportionmentFactor != value)
				{
					SetNonPersistentPropertyValue(ApportionmentFactorInfo, ref fApportionmentFactor, value);
					if ((!IsUpdatingForeignApportionedAmountSuspended) && (InvoiceLine.ApportionmentMethod == ZGuid.Empty))
					{
						updateForeignApportionedAmounts();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateAllApportionmentFactors();
				}
			}
		}
		ZDecimal fApportionmentFactor;

		public ZPropertyInfo ApportionmentFactorInfo
		{
			get { return GetZPropertyInfo(nameof(ApportionmentFactor)); }
		}

		public void ValidateApportionmentFactor()
		{
			ApportionmentFactorInfo.ClearAllNotifications();
			if ((ApportionmentFactor <= 0.0M) || (ApportionmentFactor > 100M))
			{
				ApportionmentFactorInfo.AddError(Res.GetString("b4e520f4-8401-4c2b-84f8-53106eaef7ae", "Must be greater than 0 and less than or equal to 100."));
			}
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.ApportionmentFactor;
			}
			if ((total != 100M) && (!ApportionmentFactorInfo.HasErrors()))
			{
				ApportionmentFactorInfo.AddError(Res.GetString("8d6d19f0-9eda-4d9f-8413-9f4c17e28b8e", "Apportionment Factor column must sum to 100."));
			}
		}

		void ValidateAllApportionmentFactors()
		{
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				apportionment.ValidateApportionmentFactor();
			}
		}

		public bool IsUpdatingForeignApportionedAmountSuspended { get; set; }

		#endregion

		#region ExRate

		ExchangeRate ExRate
		{
			get
			{
				if (fExRate == null)
				{
					GlbCompany company = Factory.Load<GlbCompany>(Company);
					Currency currency = !CompanyLocalCurrency.IsEmpty ? new Currency(CompanyLocalCurrency) : null;
					if (currency != null && Company.IsValid)
					{
						fExRate = new ExchangeRate(company.GC_IsReciprocal, currency.Decimals, Company.ToGuid());
					}
				}
				return fExRate;
			}
		}
		ExchangeRate fExRate;

		#endregion

		#region LocalAmountWithoutRounding

		ZDecimal LocalAmountWithoutRounding { get; set; }

		#endregion

		#region LocalGST

		public ZDecimal LocalGST
		{
			get { return fLocalGST; }
			set
			{
				if (fLocalGST != value)
				{
					SetNonPersistentPropertyValue(LocalGSTInfo, ref fLocalGST, value);
					if (!IsAdjustingLocalGSTSuspended)
					{
						AdjustLocalGST();
					}
				}
				if (!IsValidationSuspended)
				{
					ValidateAllLocalGST();
				}
			}
		}
		ZDecimal fLocalGST;

		public ZPropertyInfo LocalGSTInfo
		{
			get { return GetZPropertyInfo(nameof(LocalGST)); }
		}

		protected void ValidateAllLocalGST()
		{
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				apportionment.ValidateLocalGST();
			}
		}

		public void ValidateLocalGST()
		{
			LocalGSTInfo.ClearAllNotifications();
			ZDecimal total = 0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.LocalGST;
			}
			if (total != InvoiceLine.LocalTax)
			{
				LocalGSTInfo.AddError(Res.GetString("7f034ce3-4e54-4a54-a6b3-2e3ea35fb5cd", "Local GST column must sum to Invoice Line Local Tax."));
			}
		}

		public void AdjustLocalGST()
		{
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.LocalGST;
			}
			ZDecimal difference = InvoiceLine.LocalTax - total;
			if (total != InvoiceLine.LocalTax)
			{
				IntercompanyCostsApportionment apportionment = getApportionmentWithLargestLocalGST();
				if (apportionment != null)
				{
					apportionment.fLocalGST += difference;
					apportionment.LocalGSTInfo.RefreshBinding();
					ValidateAllLocalGST();
				}
			}
		}

		IntercompanyCostsApportionment getApportionmentWithLargestLocalGST()
		{
			IntercompanyCostsApportionment result = null;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				if ((result == null) || (apportionment.LocalGST > result.LocalGST))
				{
					result = apportionment;
				}
			}
			return result;
		}

		#endregion

		#region Description

		[MaxLength(1024)]
		public ZString Description
		{
			get { return fDescription; }
			set
			{
				if (fDescription != value)
				{
					SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value);
				}
			}
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		#endregion

		#region TemplateLineDescription

		public ZString TemplateLineDescription { get; set; }

		#endregion

		#endregion

		#region Implementation

		public class AmountsSuspender : IDisposable
		{
			public AmountsSuspender(IntercompanyCostsApportionment apportionment)
			{
				this.apportionment = apportionment;
				apportionment.IsAdjustingForeignGSTSuspended = true;
				apportionment.IsAdjustingLocalAmountSuspended = true;
				apportionment.IsAdjustingLocalGSTSuspended = true;
			}

			readonly IntercompanyCostsApportionment apportionment;

			void IDisposable.Dispose()
			{
				apportionment.IsAdjustingForeignGSTSuspended = false;
				apportionment.IsAdjustingLocalAmountSuspended = false;
				apportionment.IsAdjustingLocalGSTSuspended = false;
			}
		}

		public AmountsSuspender GetAmountsSuspender()
		{
			return new AmountsSuspender(this);
		}

		public ZGuid GetCostGLAccount()
		{
			ZGuid result = ZGuid.Empty;
			if (InvoiceLine.GenericChargeBizO != null)
			{
				if (InvoiceLine.GenericChargeBizO.VC_IsGLAccount)
				{
					result = InvoiceLine.GenericCharge;
				}
				else
				{
					result = InvoiceLine.ChargeCode.AC_AG_CostAccount;
				}
			}
			return result;
		}

		void updateCompanyLocalCurrency()
		{
			if (Company.IsValid)
			{
				GlbCompany company = Factory.Load<GlbCompany>(Company);
				if (company != null)
				{
					CompanyLocalCurrency = company.GC_RX_NKLocalCurrency;
				}
			}
		}

		void updateCompanyLocalExchangeRate()
		{
			if (CompanyLocalCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				CompanyLocalExchangeRate = 1.0M;
			}
			else
			{
				if (InvoiceLine.Invoice.PostedDate.IsValid && ExRate != null)
				{
					CompanyLocalExchangeRate = ExRate.GetRate(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, InvoiceLine.Invoice.RateType, InvoiceLine.Invoice.PostedDate.ToDateTime());
				}
				else
				{
					CompanyLocalExchangeRate = 0.0M;
				}
			}
		}

		void updateCompanyLocalAmount()
		{
			if (!Company.Equals(ZGuid.Empty) && !CompanyLocalCurrency.Equals(ZGuid.Empty))
			{
				if (CompanyLocalExchangeRate == 1.0m)
				{
					CompanyLocalAmount = LocalAmount;
				}
				else
				{
					CompanyLocalAmount = ExRate.ForeignToLocal(LocalAmountWithoutRounding, CompanyLocalExchangeRate);
				}
			}
		}

		void updateLocalAmount()
		{
			LocalAmountWithoutRounding = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(ForeignApportionedAmountWithoutRounding, ExchangeRate);
			LocalAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocal(ForeignApportionedAmount, ExchangeRate);
		}

		public void updateLocalGST()
		{
			if (InvoiceLine.AL_AT.IsValid)
			{
				LocalGST = TaxAmountCalculator.GetLocalTaxAmount(Factory, GlbCompany.CurrentCompany.PK, LocalAmount, InvoiceLine.TaxRate, InvoiceLine.TaxRate?.GetRate(InvoiceLine.AL_TaxDate), InvoiceLine.TaxRate?.GetEffectiveExtraRate(InvoiceLine.AL_TaxDate), ForeignGST, ExchangeRate);
			}
			else
			{
				LocalGST = 0.0M;
			}
		}

		public void updateForeignGST()
		{
			if (InvoiceLine.AL_AT.IsValid)
			{
				//ForeignGST = ForeignApportionedAmountWithoutRounding * (InvoiceLine.TaxRate.GetRate() / 100M);
				ForeignGST = InvoiceLine.Tax * ApportionmentFactor / 100M;
			}
			else
			{
				ForeignGST = 0.0M;
			}
		}

		void updateIntercompanyGLAccount()
		{
			if ((Company.IsValid) && (Company != GlbCompany.CurrentCompany.PK))
			{
				GlbCompany company = Factory.Load<GlbCompany>(Company);
				if (company != null)
				{
					foreach (IntercompanyClearingConfiguration config in AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.Value)
					{
						if (config.Company.Equals(company.GC_Code))
						{
							IntercompanyGLAccount = config.ClearingGLAccount;
							break;
						}
					}
				}
			}
			else
			{
				IntercompanyGLAccount = ZGuid.Empty;
			}
		}

		void updateCompanyPostToGLAccount()
		{
			if ((Company.IsValid) && (Company != GlbCompany.CurrentCompany.PK))
			{
				foreach (IntercompanyClearingConfiguration config in AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.GetFallBackValueAtAllLevels(Company.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if (config.Company.Equals(GlbCompany.CurrentCompany.GC_Code))
					{
						CompanyPostToGLAccount = config.ClearingGLAccount;
						break;
					}
				}
			}
			else
			{
				CompanyPostToGLAccount = ZGuid.Empty;
			}
		}

		void updateAccountingPeriod()
		{
			Period period;
			if ((Company.IsValid) && (InvoiceLine.Invoice.PostedDate.IsValid))
			{
				period = GetPeriodByCompanyAndDate(Factory, Company, InvoiceLine.Invoice.PostedDate);
			}
			else
			{
				period = null;
			}
			AccountingPeriod = (period == null) ? (ZInt)0 : period.AM_Period;
		}

		Period GetPeriodByCompanyAndDate(BusinessObjectFactory factory, ZGuid company, ZDateTime date)
		{
			Period period;
			ZQuery filter = new ZQuery();
			filter.AddToFilter(AccPeriodManagementSchema.AM_GC_Company, SQLComparisonOperator.Equal, company);
			filter.AddToFilter(AccPeriodManagementSchema.AM_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			filter.AddToFilter(AccPeriodManagementSchema.AM_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			period = factory.LoadTop1<Period>(filter);
			return period;
		}

		public void updateReadOnly()
		{
			CompanyInfo.RefreshBinding();
			BranchInfo.RefreshBinding();
			DepartmentInfo.RefreshBinding();
			ForeignApportionedAmountInfo.RefreshBinding();
			ApportionmentFactorInfo.RefreshBinding();
		}

		void updateApportionmentFactors()
		{
			//ForeignApportionedAmount has manually been updated
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.ForeignApportionedAmount;
			}
			if (total > 0.0M)
			{
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					try
					{
						apportionment.IsUpdatingForeignApportionedAmountSuspended = true;
						apportionment.ApportionmentFactor = decimal.Round((apportionment.ForeignApportionedAmount / total * 100.0M), 3);
					}
					finally
					{
						apportionment.IsUpdatingForeignApportionedAmountSuspended = false;
					}
				}
				total = 0.0M;
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					total += apportionment.ApportionmentFactor;
				}

				ZDecimal difference = 100.0M - total;
				if (total != 100.0M)
				{
					IntercompanyCostsApportionment apportionment = getApportionmentWithLargestApportionmentFactor();
					if (apportionment != null)
					{
						try
						{
							apportionment.IsUpdatingForeignApportionedAmountSuspended = true;
							apportionment.ApportionmentFactor += difference;
						}
						finally
						{
							apportionment.IsUpdatingForeignApportionedAmountSuspended = false;
						}
					}
				}
			}
		}

		IntercompanyCostsApportionment getApportionmentWithLargestApportionmentFactor()
		{
			IntercompanyCostsApportionment result = null;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				if ((result == null) || (apportionment.ApportionmentFactor > result.ApportionmentFactor))
				{
					result = apportionment;
				}
			}
			return result;
		}

		IntercompanyCostsApportionment getApportionmentWithLargestForeignApportionedAmount()
		{
			IntercompanyCostsApportionment result = null;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				if ((result == null) || (apportionment.ForeignApportionedAmount > result.ForeignApportionedAmount))
				{
					result = apportionment;
				}
			}
			return result;
		}

		void updateForeignApportionedAmounts()
		{
			//ApportionmentFactor has manually been updated
			ZDecimal total = 0.0M;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				total += apportionment.ApportionmentFactor;
			}
			if (total == 100.0M)
			{
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					try
					{
						apportionment.IsUpdatingApportionmentFactorSuspended = true;
						apportionment.ForeignApportionedAmount = decimal.Round((apportionment.ApportionmentFactor / total * InvoiceLine.Amount), 2);
					}
					finally
					{
						apportionment.IsUpdatingApportionmentFactorSuspended = false;
					}
				}
				adjustForeignApportionedAmounts(true);
			}
		}

		public void AdjustForeignApportionedAmounts()
		{
			adjustForeignApportionedAmounts(false);
		}

		void adjustForeignApportionedAmounts(bool update)
		{
			if (((!InvoiceLine.IsApportionmentMethodEmpty) || update) && !InvoiceLine.AreApportionmentsBeingUpdated)
			{
				ZDecimal total = 0.0M;
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					total += apportionment.ForeignApportionedAmount;
				}
				ZDecimal difference = InvoiceLine.Amount - total;
				if (difference != 0.0M)
				{
					IntercompanyCostsApportionment apportionment = getApportionmentWithLargestForeignApportionedAmount();
					if (apportionment != null)
					{
						apportionment.fForeignApportionedAmount += difference;
						apportionment.ForeignApportionedAmountInfo.RefreshBinding();
						apportionment.updateForeignGST();
						apportionment.updateLocalAmount();
						ValidateAllForeignApportionedAmounts();
					}
				}
			}
		}

		IntercompanyCostsApportionment getApportionmentWithLargestLocalAmount()
		{
			IntercompanyCostsApportionment result = null;
			foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
			{
				if ((result == null) || (apportionment.LocalAmount > result.LocalAmount))
				{
					result = apportionment;
				}
			}
			return result;
		}

		public void AdjustLocalAmounts()
		{
			if (!InvoiceLine.IsApportionmentMethodEmpty && !InvoiceLine.AreApportionmentsBeingUpdated)
			{
				ZDecimal total = 0.0M;
				foreach (IntercompanyCostsApportionment apportionment in InvoiceLine.Apportionments)
				{
					total += apportionment.LocalAmount;
				}
				ZDecimal difference = InvoiceLine.LocalAmount - total;
				if (difference != 0.0M)
				{
					IntercompanyCostsApportionment apportionment = getApportionmentWithLargestLocalAmount();
					if (apportionment != null)
					{
						apportionment.fLocalAmount += difference;
						apportionment.LocalAmountInfo.RefreshBinding();
					}
				}
			}
		}

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCompany();
			ValidateCompanyLocalCurrency();
			ValidateBranch();
			ValidateDepartment();
			ValidateForeignApportionedAmount();
			ValidateApportionmentFactor();
			ValidateAccountingPeriod();
			ValidateExchangeRate();
			ValidateCompanyLocalExchangeRate();
			ValidateIntercompanyGLAccount();
			ValidateCompanyPostToGLAccount();
			ValidateAllForeignGST();
			ValidateAllLocalGST();
			ValidateAllLocalAmount();
		}

		#endregion

	}
}
