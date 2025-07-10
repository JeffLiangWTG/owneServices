using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	public class AccountFeeInvoiceCreator : NonPersistentBusinessObject, IProgressFormSupportable, IObsoleteValidation
	{
		public AccountFeeInvoiceCreator(IAccountFeeInvoiceSupportable accountFeeInvoiceSupporter)
		{
			this.supporter = accountFeeInvoiceSupporter;
			this.factoryForInvoiceCreation = new BusinessObjectFactory();
			accFeeTaxID = GetDefaultTaxID();
		}

		readonly BusinessObjectFactory factoryForInvoiceCreation;
		readonly IAccountFeeInvoiceSupportable supporter;

		#region Properties

		#region AccFeeFromDate
		public ZDateTime AccFeeFromDate
		{
			get { return accFeeFromDate; }
			set
			{
				SetNonPersistentPropertyValue(AccFeeFromDateInfo, ref accFeeFromDate, value);
				if (!IsValidationSuspended)
				{
					ValidateAccFeeFromDate();
				}
			}
		}
		ZDateTime accFeeFromDate;

		public ZPropertyInfo AccFeeFromDateInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeeFromDate)); }
		}
		#endregion

		#region AccFeeToDate
		public ZDateTime AccFeeToDate
		{
			get { return accFeeToDate; }
			set
			{
				SetNonPersistentPropertyValue(AccFeeToDateInfo, ref accFeeToDate, value);
				if (!IsValidationSuspended)
				{
					ValidateAccFeeToDate();
				}
				if (AccFeeInvoiceDate.IsEmpty)
				{
					AccFeeInvoiceDate = AccFeeToDate;
				}
				if (AccFeePostDate.IsEmpty)
				{
					AccFeePostDate = AccFeeToDate;
				}
			}
		}
		ZDateTime accFeeToDate;

		public ZPropertyInfo AccFeeToDateInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeeToDate)); }
		}
		#endregion

		#region AccFeeInvoiceDate
		public ZDateTime AccFeeInvoiceDate
		{
			get { return accFeeInvoiceDate; }
			set
			{
				var calculatedDate = ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate() ? ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(value) : value;
				SetNonPersistentPropertyValue(AccFeeInvoiceDateInfo, ref accFeeInvoiceDate, calculatedDate);
				if (!IsValidationSuspended)
				{
					ValidateAccFeeInvoiceDate();
				}
			}
		}
		ZDateTime accFeeInvoiceDate;

		public ZPropertyInfo AccFeeInvoiceDateInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeeInvoiceDate)); }
		}
		#endregion

		#region AccFeePostDate
		public ZDateTime AccFeePostDate
		{
			get { return accFeePostDate; }
			set
			{
				var calculatedDate = ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate() ? ARDefaultInvoiceAndPostDateCalculator.GetDefaultDate(value) : value;
				SetNonPersistentPropertyValue(AccFeePostDateInfo, ref accFeePostDate, calculatedDate);

				if (!IsValidationSuspended)
				{
					ValidateAccFeeFromDate();
					ValidateAccFeeToDate();
					ValidateAccFeePostDate();
				}
			}
		}
		ZDateTime accFeePostDate;

		public ZPropertyInfo AccFeePostDateInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeePostDate)); }
		}
		#endregion

		#region AccFeeInvoiceDescription
		[MaxLength(128)]
		public ZString AccFeeInvoiceDescription
		{
			get { return accFeeInvoiceDescription; }
			set
			{
				SetNonPersistentPropertyValue(AccFeeInvoiceDescriptionInfo, ref accFeeInvoiceDescription, value);
				if (!IsValidationSuspended)
				{
					ValidateAccFeeInvoiceDescription();
				}
			}
		}
		ZString accFeeInvoiceDescription;

		public ZPropertyInfo AccFeeInvoiceDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeeInvoiceDescription)); }
		}
		#endregion

		#region AccFeeTaxID

		[List("TaxRate_List")]
		public ZGuid AccFeeTaxID
		{
			get { return accFeeTaxID; }
			set
			{
				SetNonPersistentPropertyValue(AccFeeTaxIDInfo, ref accFeeTaxID, value);
				if (!IsValidationSuspended)
				{
					ValidateAccFeeTaxID();
				}
			}
		}
		ZGuid accFeeTaxID;

		public ZPropertyInfo AccFeeTaxIDInfo
		{
			get { return GetZPropertyInfo(nameof(AccFeeTaxID)); }
		}

		public bool AccFeeTaxID_ReadOnly
		{
			get
			{
				return !supporter.CreateAccountFee
						|| !supporter.Company.GC_IsGSTRegistered
						|| !AccountingConfigurationRegistry.Instance.ReceivableAllowUserToModifyGSTId.GetFallBackValueAtAllLevels(supporter.Company.PK.ToGuid(), supporter.Branch.PK.ToGuid(), Guid.Empty);
			}
		}
		#endregion

		#endregion

		#region Property Validation

		public void ValidateAccFeeFromDate()
		{
			AccFeeFromDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccFeeFromDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(AccFeeFromDateInfo);
		}

		public void ValidateAccFeeToDate()
		{
			AccFeeToDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccFeeToDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(AccFeeToDateInfo);
		}

		public void ValidateAccFeeInvoiceDate()
		{
			AccFeeInvoiceDateInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccFeeInvoiceDateInfo);
			TypeValidation.CheckValidZDateTimeAndRange(AccFeeInvoiceDateInfo);

			if (!AccFeeInvoiceDateInfo.HasErrors() && ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate())
			{
				AccFeeInvoiceDateInfo.AddWarning(Res.GetString("198b8e2b-a483-4133-a0d5-abe6e4681693", @"Invoice Date is recalculated as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'."));
			}
		}

		public void ValidateAccFeePostDate()
		{
			AccFeePostDateInfo.ClearAllNotifications();

			if (!AccFeeFromDateInfo.HasErrors() || !AccFeeToDateInfo.HasErrors())
			{
				AccFeePostDateInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(AccFeePostDateInfo);
				TypeValidation.CheckValidZDateTimeAndRange(AccFeePostDateInfo);

				if (!AccFeePostDateInfo.HasErrors())
				{
					if (AccFeePostDate.Date < AccFeeFromDate.Date || AccFeePostDate.Date > AccFeeToDate.Date)
					{
						AccFeePostDateInfo.AddError(Res.GetString("d90a977b-956d-4825-8500-4c4f913e1e24", "The Post date must be within the selected Applicability Period"));
					}
				}

				if (!AccFeePostDateInfo.HasErrors())
				{
					var periodValidation = new PeriodValidationProvider(factoryForInvoiceCreation);
					periodValidation.CheckDateFallsIntoValidPeriod(AccFeePostDateInfo);
				}

				if (!AccFeePostDateInfo.HasErrors())
				{
					if (AccFeePostDate.Date > ZDateTime.Today)
					{
						AccFeePostDateInfo.AddError(TransactionHeaderValidation.FuturePostDateError);
					}
				}

				if (!AccFeePostDateInfo.HasErrors())
				{
					if (AccFeePostDate.Date < ZDateTime.Today)
					{
						if (!AccountingConfigurationRegistry.Instance.AllowBackPostingSubLedgerTransaction.Value || !Env.Security.ReceivablesPostToPreviousOrFutureOpenPeriod.IsAllowed)
						{
							AccFeePostDateInfo.AddError(TransactionHeaderValidation.PreviousPostDateError);
						}
						else
						{
							AccFeePostDateInfo.AddWarning(TransactionHeaderValidation.PreviousPostDateWarning);
						}
					}
				}

				if (!AccFeePostDateInfo.HasErrors() && ARDefaultInvoiceAndPostDateCalculator.ShouldUseDefaultDate())
				{
					AccFeeInvoiceDateInfo.AddWarning(Res.GetString("79ae588a-c8dc-4d65-84e0-64c0d199f858", @"Post Date is recalculated as the Registry 'Invoice and Post Dates Defaulting Behavior' is set to 'MTH - Month End Suspension'."));
				}
			}
		}

		public void ValidateAccFeeInvoiceDescription()
		{
			AccFeeInvoiceDescriptionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AccFeeInvoiceDescriptionInfo);
		}

		public void ValidateAccFeeTaxID()
		{
			AccFeeTaxIDInfo.ClearAllNotifications();
			if (supporter.Company.GC_IsGSTRegistered)
			{
				MandatoryValidation.CheckEntered(AccFeeTaxIDInfo);
			}
			TypeValidation.CheckValidGuid(AccFeeTaxIDInfo);
			ListValidation.ErrorIfInvalidPK(AccFeeTaxIDInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			if (!IsValidationSuspended)
			{
				AccFeeFromDateInfo.ClearAllNotifications();
				AccFeeToDateInfo.ClearAllNotifications();
				AccFeeInvoiceDateInfo.ClearAllNotifications();
				AccFeePostDateInfo.ClearAllNotifications();
				AccFeeInvoiceDescriptionInfo.ClearAllNotifications();
				AccFeeTaxIDInfo.ClearAllNotifications();

				if (supporter.CreateAccountFee)
				{
					ValidateAccFeeFromDate();
					ValidateAccFeeToDate();
					ValidateAccFeeInvoiceDate();
					ValidateAccFeePostDate();
					ValidateAccFeeInvoiceDescription();
					ValidateAccFeeTaxID();

					base.RunPreSaveValidationCore();
				}
			}
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Internal key")]
		public AccTaxRateCollection TaxRate_List
		{
			get { return factoryForInvoiceCreation.GetCachedValue("AccountFee" + GlbCompany.CurrentCompany.PK.ToStringKey() + nameof(AccTaxRateCollection) + "Active", () => new VATAccTaxRateCollection(factoryForInvoiceCreation, new ZQuery(AccTaxRateSchema.AT_IsActive, true))); }
		}

		public void SetDefaultTaxID()
		{
			AccFeeTaxID = GetDefaultTaxID();
		}

		Guid GetDefaultTaxID()
		{
			var result = Guid.Empty;

			if (supporter.Company.GC_IsGSTRegistered)
			{
				result = AccountingConfigurationRegistry.Instance.AccountFeeDefaultTaxID.GetFallBackValueAtAllLevels(supporter.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			}

			return result;
		}

		#region Account Fee Invoice Function

		public void CreateAndSaveAccountFeeInvoices()
		{
			if (supporter.CreateAccountFee)
			{
				var status = Res.GetString("47eeef98-baf4-45e5-a293-193b349d087d", "Loading Account Fee Settings");
				UpdateProgressStatus(status, 0, 100, false);
				var businessObjCollection = GetAccountFeeSettingsFromDatabase();

				if (businessObjCollection.Count > 0)
				{
					var currentProgress = 0;
					var totalProgress = businessObjCollection.Count;

					status = Res.GetString("223fc9b2-868f-415b-8baf-bc6033998b86", "Preparing Account Fee Invoice");
					UpdateProgressStatus(status, currentProgress, totalProgress, false);
					var accountFeeInvoiceInfos = GenerateInfosFromDatabaseRows(businessObjCollection);

					var messages = new ZStringBuilder();
					int invoiceCount = 0;
					foreach (var invoiceInfo in accountFeeInvoiceInfos)
					{
						var invoice = CreateInvoice(invoiceInfo, messages);
						invoiceCount = invoiceCount + (invoice != null ? 1 : 0);
						status = Res.GetString("6f8eb6d6-c6ab-4222-a527-02d4ebb5fbe0", "Creating Account Fee Invoice : {0} of {1}", ++currentProgress, totalProgress);
						UpdateProgressStatus(status, currentProgress, totalProgress, false);
					}

					status = Res.GetString("aa297407-e97d-4102-8cc9-8f18136bcf88", "Saving Invoices to database.");
					UpdateProgressStatus(status, totalProgress, totalProgress, false);

					factoryForInvoiceCreation.Save();

					status = Res.GetString("c06c1006-ba12-4d94-b668-5099adfd9d03", "Account Fee Invoice creation process completed successfully.");
					messages.Append(Res.GetString("5cd8ba2f-ae42-4c44-b120-6d999eaa930d", "[{0}] {1} Invoice Posted.", Env.Time.CurrentLocalDateTime, invoiceCount));
					log = messages.ToStringWithNewLineBetweenAppends();

					UpdateProgressStatus(status, totalProgress, totalProgress, true);
				}
				else
				{
					log = status = Res.GetString("82696325-24f7-40f3-abd5-cc2d5800692e", "No Account Fee Invoice to generate");
					UpdateProgressStatus(status, 100, 100, true);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "SQL query")]
		public DynamicBusinessObjectCollection GetAccountFeeSettingsFromDatabase()
		{
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@PostDateFrom", AccFeeFromDate, AccTransactionHeaderSchema.AH_PostDate));
			sqlParams.Add(ZSqlParameter.New("@PostDateTo", AccFeeToDate.EndOfDay(), AccTransactionHeaderSchema.AH_PostDate));
			sqlParams.Add(ZSqlParameter.New("@Company", supporter.Company.PK, GlbCompanySchema.PK));

			var debtorQuery = supporter.GetOrgPKQueryForAccountFee(sqlParams);
			if (!String.IsNullOrEmpty(debtorQuery))
			{
				debtorQuery = string.Format(" INNER JOIN ({0}) Debtors ON OH_PK = OrgPK ", debtorQuery);
			}
			var sqlQuery = ZString.Format(@"SELECT	CompanyPK, OrgPK, AAF_AG_GLAccount, AAF_RX_NKFeeCurrency, AAF_FeeAmount, AAF_Rule, GenerateAccountFee
											FROM	fnGetAccountFee(@Company, NULL, @PostDateFrom, @PostDateTo)
													{0}
											WHERE	GenerateAccountFee = 1", debtorQuery);

			var settings = new DynamicBusinessObjectCollection(factoryForInvoiceCreation);
			settings.Load(sqlQuery, sqlParams);
			return settings;
		}

		List<AccountFeeInvoiceInfo> GenerateInfosFromDatabaseRows(DynamicBusinessObjectCollection businessObjCollection)
		{
			var result = new List<AccountFeeInvoiceInfo>();

			foreach (DynamicBusinessObject bizO in businessObjCollection)
			{
				var generateAccountFee = new ZInt(bizO[GenerateAccountFee]) == 1;

				if (generateAccountFee)
				{
					var feeInfo = new AccountFeeInvoiceInfo
					{
						OrganisationPK = new ZGuid(bizO[OrgPK]),
						CompanyPK = supporter.Company.PK,
						BranchPK = supporter.Branch.PK,
						AccFeeGLAccountPK = new ZGuid(bizO[AAF_AG_GLAccount]),
						AccFeeCurrencyNK = new ZString(bizO[AAF_RX_NKFeeCurrency]),
						AccFeeAmount = new ZDecimal(bizO[AAF_FeeAmount]),
						AccFeeInvoiceDate = AccFeeInvoiceDate,
						AccFeePostDate = AccFeePostDate,
						AccFeeInvoiceDescription = AccFeeInvoiceDescription
					};

					result.Add(feeInfo);
				}
			}

			return result;
		}

		ARInvoice CreateInvoice(AccountFeeInvoiceInfo invoiceInfo, ZStringBuilder msgBuilder)
		{
			ARInvoice arInvoice = null;

			var orgHeader = factoryForInvoiceCreation.Load<OrgHeader>(invoiceInfo.OrganisationPK);
			var currency = RefCurrency.LoadFromCurrencyCode(factoryForInvoiceCreation, invoiceInfo.AccFeeCurrencyNK);
			var glHeader = factoryForInvoiceCreation.Load<AccGLHeader>(invoiceInfo.AccFeeGLAccountPK);
			var taxRate = factoryForInvoiceCreation.Load<AccTaxRate>(AccFeeTaxID);
			var isGSTApplicable = GlbCompany.CurrentCompany.GC_IsGSTRegistered && orgHeader.CompanyData.IsARTaxApplicable;

			var errorMessage = ValidateAccountFeeInvoiceInfo(orgHeader, currency, glHeader, invoiceInfo.AccFeeAmount, taxRate, isGSTApplicable);
			if (!errorMessage.IsEmpty)
			{
				msgBuilder.Append(Res.GetString("54b2d144-3536-4e3a-9a08-75ef93c7e2a1", "[{0}] {1}", Env.Time.CurrentLocalDateTime, errorMessage));
			}
			else
			{
				bool isLocalCurrency = currency != null && currency.RX_Code == supporter.Company.GC_RX_NKLocalCurrency;

				arInvoice = factoryForInvoiceCreation.New<ARInvoice>();
				arInvoice.AH_Desc = invoiceInfo.AccFeeInvoiceDescription;
				arInvoice.AH_OH = invoiceInfo.OrganisationPK;
				arInvoice.AH_GC = invoiceInfo.CompanyPK;
				arInvoice.AH_GB = invoiceInfo.BranchPK;
				arInvoice.AH_RX_NKTransactionCurrency = invoiceInfo.AccFeeCurrencyNK;
				arInvoice.AH_ExchangeRate = isLocalCurrency ? 1 : currency.CurrentSellRate;
				arInvoice.AH_InvoiceDate = invoiceInfo.AccFeeInvoiceDate;
				arInvoice.AH_PostDate = invoiceInfo.AccFeePostDate;

				var line = arInvoice.Lines.AddNew() as ARInvoiceLine;
				line.GenericCharge = invoiceInfo.AccFeeGLAccountPK;
				line.AL_Desc = glHeader.AG_DescriptionMultilingual;
				line.AL_GB = invoiceInfo.BranchPK;
				line.AL_GC = invoiceInfo.CompanyPK;
				if (isGSTApplicable)
				{
					line.AL_AT = taxRate.PK;
				}
				line.AL_RX_NKTransactionCurrency = invoiceInfo.AccFeeCurrencyNK;
				line.AL_ExchangeRate = isLocalCurrency ? 1 : currency.CurrentSellRate;
				line.AL_OSExTaxAmount = invoiceInfo.AccFeeAmount;

				msgBuilder.Append(Res.GetString("bbc2ff5c-3ca2-4997-bc27-7d87c108c978", "[{0}] Created Account Fee Invoice for {1}", Env.Time.CurrentLocalDateTime, orgHeader.OH_Code));
			}
			return arInvoice;
		}

		#endregion

		#region Account Fee Invoice Validation

		ZString ValidateAccountFeeInvoiceInfo(OrgHeader orgHeader, RefCurrency currency, AccGLHeader glHeader, ZDecimal feeAmount, AccTaxRate taxRate, bool isGSTApplicable)
		{
			var errorMessages = new ZStringBuilder();

			if (orgHeader == null)
			{
				errorMessages.Append(Res.GetString("b1291934-5ecb-49b3-bd3d-94921db48962", "Invalid Organization. Account Fee Invoice cannot be created"));
			}
			else
			{
				if (!orgHeader.OH_IsActive)
				{
					errorMessages.Append(Res.GetString("82adf31c-1395-45b2-bca6-c4257bd08b96", "Organization: {0} is Inactive. Account Fee Invoice cannot be created", orgHeader.OH_Code));
				}
				if (!orgHeader.OH_IsDebtor)
				{
					errorMessages.Append(Res.GetString("29e0d304-6da0-4edb-9195-084536d0e237", "Organization: {0} is not a Debtor. Account Fee Invoice cannot be created", orgHeader.OH_Code));
				}

				ValidateCurrency(errorMessages, orgHeader, currency);
				ValidateGLAcc(errorMessages, orgHeader, glHeader);

				if (isGSTApplicable && taxRate == null)
				{
					errorMessages.Append(Res.GetString("04c79ae1-4fcb-4321-b717-37ec08a47ff6", "Tax Rate is missing. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}

				if (feeAmount <= 0)
				{
					errorMessages.Append(Res.GetString("742a9251-f45d-45dc-a198-1fb60f238038", "Account Fee Amount must be a positive number. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}

				ValidateDates(errorMessages);
			}

			return errorMessages.ToStringWithNewLineBetweenAppends();
		}

		void ValidateCurrency(ZStringBuilder errorMessages, OrgHeader orgHeader, RefCurrency currency)
		{
			if (currency == null)
			{
				errorMessages.Append(Res.GetString("b8614228-a791-41f8-b081-70c716c59bec", "Invalid Currency. Account Fee Invoice cannot be created for {0}", orgHeader.OH_Code));
			}
			else
			{
				if (!currency.RX_IsActive)
				{
					errorMessages.Append(Res.GetString("d5a802c3-4fba-4482-ae11-643ea7876673", "Currency: {0} is Inactive. Account Fee Invoice cannot be created for {1}", currency.RX_Code, orgHeader.OH_Code));
				}

				if (currency.CurrentSellRate <= 0 && currency.RX_Code != supporter.Company.GC_RX_NKLocalCurrency)
				{
					errorMessages.Append(Res.GetString("5c5d6411-b161-4418-8da0-3e9b78c1139a", "Currency: {0} does not have a Sell Exchange Rate. Account Fee Invoice cannot be created for {1}", currency.RX_Code, orgHeader.OH_Code));
				}
			}
		}

		void ValidateGLAcc(ZStringBuilder errorMessages, OrgHeader orgHeader, AccGLHeader glHeader)
		{
			if (glHeader == null)
			{
				errorMessages.Append(Res.GetString("c1275562-7692-4c7d-aae4-1947ad52f7ca", "Invalid GL Account. Account Fee Invoice cannot be created for {0}", orgHeader.OH_Code));
			}
			else
			{
				var accountTypes = new string[] { Core.Constants.AccountType.ProfitAndLossAccount, Core.Constants.AccountType.BalanceSheetAccount };

				if (!glHeader.AG_IsActive)
				{
					errorMessages.Append(Res.GetString("13b8319e-92d2-4e63-a44d-86c636cebece", "GL Account: {0} is Inactive. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}

				if (!accountTypes.Contains<string>(glHeader.AG_AccountType))
				{
					errorMessages.Append(Res.GetString("f75ff8ca-cb60-4c8f-9ca2-6dede46d163b", "GL Account: {0} has a inappropriate Account Type. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}

				if (glHeader.AG_ControlAccount)
				{
					errorMessages.Append(Res.GetString("42133cf9-568e-40c0-9e3f-829be6c6b49e", "GL Account: {0} is a Control Account. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}

				if (glHeader.AG_DisallowDirectPosting)
				{
					errorMessages.Append(Res.GetString("6af8f0ca-4619-464d-b697-61827f623043", "Direct Posting to GL Account: {0} is not allowed. Account Fee Invoice cannot be created for {1}", glHeader.AG_AccountNum, orgHeader.OH_Code));
				}
			}
		}

		void ValidateDates(ZStringBuilder errorMessages)
		{
			//Validate PostDate
			ValidateAccFeePostDate();
			if (AccFeePostDateInfo.GetMessageErrors().Any())
			{
				AccFeePostDateInfo.GetMessageErrors().ToList().ForEach(x => errorMessages.Append(x.Message));
			}

			//validate Invoice Date
			ValidateAccFeeInvoiceDate();
			if (AccFeeInvoiceDateInfo.GetMessageErrors().Any())
			{
				AccFeeInvoiceDateInfo.GetMessageErrors().ToList().ForEach(x => errorMessages.Append(x.Message));
			}
		}

		#endregion

		#region IProgressFormSupportable

		public string CurrentStatusText
		{
			get { return currentStatusText; }
		}

		public string Log { get { return log; } }

		public int CompletedItems
		{
			get { return completedItems; }
		}

		public int TotaItemsToComplete
		{
			get { return totaItemToComplete; }
		}

		public event Action<IProgressFormSupportable, bool> RaiseProgressUpdateEvent;

		void UpdateProgressStatus(string statusText, int processItem, int totalItemToProcess, bool isProcessCompleted)
		{
			currentStatusText = statusText;
			completedItems = processItem;
			totaItemToComplete = totalItemToProcess;

			if (RaiseProgressUpdateEvent != null)
			{
				RaiseProgressUpdateEvent(this, isProcessCompleted);
			}
		}

		string currentStatusText;
		string log;
		int completedItems;
		int totaItemToComplete;

		#endregion

		const string GenerateAccountFee = "GenerateAccountFee";
		const string AAF_AG_GLAccount = "AAF_AG_GLAccount";
		const string AAF_RX_NKFeeCurrency = "AAF_RX_NKFeeCurrency";
		const string AAF_FeeAmount = "AAF_FeeAmount";
		const string OrgPK = "OrgPK";

		public class AccountFeeInvoiceInfo
		{
			public ZGuid OrganisationPK
			{
				get;
				set;
			}

			public ZGuid CompanyPK
			{
				get;
				set;
			}

			public ZGuid BranchPK
			{
				get;
				set;
			}

			public ZDateTime AccFeeInvoiceDate
			{
				get;
				set;
			}

			public ZDateTime AccFeePostDate
			{
				get;
				set;
			}

			public ZString AccFeeInvoiceDescription
			{
				get;
				set;
			}

			public ZString AccFeeCurrencyNK
			{
				get;
				set;
			}

			public ZDecimal AccFeeAmount
			{
				get;
				set;
			}

			public ZGuid AccFeeGLAccountPK
			{
				get;
				set;
			}
		}
	}
}
