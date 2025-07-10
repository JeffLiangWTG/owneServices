using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DataTransfer.SystemMerge.Business.Organisation.Helpers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.SystemMerge.XmlDefinition;

namespace Enterprise.DataTransfer.SystemMerge.DataAdapters
{
	class SysMergeCompanyDataValueObjectHelper
	{
		public SysMergeCompanyDataValueObjectHelper(string errorContext)
		{
			this.ErrorContext = errorContext;
		}

		public readonly string ErrorContext;

		#region Import

		public void ImportFromValueObjectCollection(Xsd.SysMergeOrgCompanyDataCollection xsdCompanyDataCollection, OrgHeaderForDataTransfer org, IValueObjectImportContext context, SysMergeCompanyMapper importCompanyMapper)
		{
			if (xsdCompanyDataCollection.IsSpecified)
			{
				for (int i = 0; i < xsdCompanyDataCollection.Count; i++)
				{
					ImportFromValueObject(xsdCompanyDataCollection[i], org, context, importCompanyMapper);
				}
			}
		}

		void ImportFromValueObject(Xsd.SysMergeOrgCompanyData xsdOrgCompanyData, OrgHeaderForDataTransfer org, IValueObjectImportContext context, SysMergeCompanyMapper importCompanyMapper)
		{
			ZString mappedCompanyCode = importCompanyMapper.GetMappedCode(xsdOrgCompanyData.GC_Code);

			if (mappedCompanyCode.IsEmpty && !xsdOrgCompanyData.GC_Code.IsEmpty)
			{
				string skipMessage = Res.GetString("841e61ab-2d6b-49af-8384-57572ae75b29", "{0}: company related info skipped for [{1}].",
					ErrorContext, xsdOrgCompanyData.GC_Code);
				context.Notify(new InfoNotification(skipMessage));
			}
			else
			{
				GlbCompany company = importCompanyMapper.GetCompanyByCodeThrowingErrorIfNotFound(org.Factory, mappedCompanyCode);
				CreateImportedOrgCompanyData(xsdOrgCompanyData, company.PK, org, context);
			}
		}

		void CreateImportedOrgCompanyData(Xsd.SysMergeOrgCompanyData xsdOrgCompanyData, ZGuid companyPk, OrgHeaderForDataTransfer org, IValueObjectImportContext context)
		{
			OrgCompanyData newCompanyData = GetCompanyDataEnsuringIsUniqueForOrgAndCompany(companyPk, org);

			if (!xsdOrgCompanyData.GB_ControllingBranchCode.IsEmpty)
			{
				GlbBranch branch = newCompanyData.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, xsdOrgCompanyData.GB_ControllingBranchCode);

				if (branch != null)
				{
					newCompanyData.OB_GB_ControllingBranch = branch.PK;
				}
			}

			#region old fields
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_IsDebtorInfo, xsdOrgCompanyData.IsDebtor.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_IsCreditorInfo, xsdOrgCompanyData.IsCreditor.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APCategoryInfo, xsdOrgCompanyData.APCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APCreditLimitInfo, xsdOrgCompanyData.APCreditLimit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APPayInvoiceAfterPostingDefaultInfo, xsdOrgCompanyData.APPayInvoiceAfterPostingDefault.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APPaymentTermDaysInfo, xsdOrgCompanyData.APPaymentTermDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APPaymentTermsInfo, xsdOrgCompanyData.APPaymentTerms);
			newCompanyData.SetAPTaxApplicable(xsdOrgCompanyData.APTaxApplicable);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APWHTApplicableInfo, xsdOrgCompanyData.APWHTApplicable.ToString());

			if (!xsdOrgCompanyData.RX_APDefaultCurrencyCode.IsEmpty)
			{
				RefCurrency refCurrency = newCompanyData.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, xsdOrgCompanyData.RX_APDefaultCurrencyCode);
				if (refCurrency != null)
				{
					newCompanyData.OB_RX_NKAPDefltCurrency = refCurrency.RX_Code;
				}
			}

			if (!xsdOrgCompanyData.OG_APCreditorGroupCode.IsEmpty)
			{
				OrgCreditorGroup creaditorGroup = newCompanyData.Factory.LoadFromNaturalKey<OrgCreditorGroup>(OrgCreditorGroupSchema.OG_Code, xsdOrgCompanyData.OG_APCreditorGroupCode);
				if (creaditorGroup != null)
				{
					newCompanyData.OB_OG_APCreditorGroup = creaditorGroup.PK;
				}
			}

			if (!xsdOrgCompanyData.AB_APDefaultBankAccountCode.IsEmpty)
			{
				AccBankAccount bankAccount = newCompanyData.Factory.LoadFromNaturalKey<AccBankAccount>(AccBankAccountSchema.AB_Code, xsdOrgCompanyData.AB_APDefaultBankAccountCode);

				if (bankAccount != null)
				{
					newCompanyData.OB_AB_APDefaultBankAccount = bankAccount.PK;
				}
			}

			if (!xsdOrgCompanyData.AC_APDefaultChargeCode.IsEmpty)
			{
				ZQuery chargeCodeQuery = new ZQuery(AccChargeCodeSchema.AC_GC, newCompanyData.OB_GC);
				chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_Code, xsdOrgCompanyData.AC_APDefaultChargeCode);
				AccChargeCode[] chargeCodes = newCompanyData.Factory.Load<AccChargeCode>(chargeCodeQuery);

				if (chargeCodes.Length > 0)
				{
					newCompanyData.OB_AC_APDefaultChargeCode = chargeCodes[0].PK;
				}
			}

			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APAirlineAccountNumberInfo, xsdOrgCompanyData.APAirlineAccountNumber);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARAutoUpdateRatesInfo, xsdOrgCompanyData.ARAutoUpdateRates.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCategoryInfo, xsdOrgCompanyData.ARCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCombinedStatementInvoiceInfo, xsdOrgCompanyData.ARCombinedStatementInvoice.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARConsolidatedAccountingCategoryInfo, xsdOrgCompanyData.ARConsolidatedAccountingCategory);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditLimitInfo, xsdOrgCompanyData.ARCreditLimit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditRatingInfo, xsdOrgCompanyData.ARCreditRating);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARTreatDisbursementsAsStandardValueInfo, xsdOrgCompanyData.ARTreatDisbursementsAsStandardValue.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARDontShowTaxOnDocsInfo, xsdOrgCompanyData.ARDontShowTaxOnDocs.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARBuyersConsolInvoicingStyleInfo, xsdOrgCompanyData.ARBuyersConsolInvoicingStyle);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARUseSettlementGroupCreditLimitInfo, xsdOrgCompanyData.ARUseSettlementGroupCreditLimit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditApprovedInfo, xsdOrgCompanyData.ARCreditApproved.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_AROnCreditHoldInfo, xsdOrgCompanyData.AROnCreditHold.ToString());

			if (xsdOrgCompanyData.ARAccountAndCreditReviewDue != null)
			{
				newCompanyData.OB_ARAccountAndCreditReviewDue = (ZDateTime)xsdOrgCompanyData.ARAccountAndCreditReviewDue;
			}

			if (!xsdOrgCompanyData.AB_ARPayToAccountCode.IsEmpty)
			{
				AccBankAccount bankAccount = newCompanyData.Factory.LoadFromNaturalKey<AccBankAccount>(AccBankAccountSchema.AB_Code, xsdOrgCompanyData.AB_ARPayToAccountCode);

				if (bankAccount != null)
				{
					newCompanyData.OB_AB_ARPayToAccount = bankAccount.PK;
				}
			}

			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARPreviousChequeDrawerInfo, xsdOrgCompanyData.ARPreviousChequeDrawer);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARPreviousChequeDrawerBankInfo, xsdOrgCompanyData.ARPreviousChequeDrawerBank);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARPreviousChequeDrawerBankBranchInfo, xsdOrgCompanyData.ARPreviousChequeDrawerBankBranch);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARReceiptInvoiceAfterPostingDefaultInfo, xsdOrgCompanyData.ARReceiptInvoiceAfterPostingDefault.ToString());
			newCompanyData.SetARTaxApplicable(xsdOrgCompanyData.ARTaxApplicable);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_AREftCustomsPaymentMethodInfo, xsdOrgCompanyData.AREftCustomsPaymentMethod);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARWHTApplicableInfo, xsdOrgCompanyData.ARWHTApplicable.ToString());

			if (!xsdOrgCompanyData.RX_ARDefaultCurrencyCode.IsEmpty)
			{
				RefCurrency refCurrency = newCompanyData.Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, xsdOrgCompanyData.RX_ARDefaultCurrencyCode);
				if (refCurrency != null)
				{
					newCompanyData.OB_RX_NKARDDefltCurrency = refCurrency.RX_Code;
				}
			}

			if (!xsdOrgCompanyData.OJ_ARDebtorGroupCode.IsEmpty)
			{
				OrgDebtorGroup debtorGroup = newCompanyData.Factory.LoadFromNaturalKey<OrgDebtorGroup>(OrgDebtorGroupSchema.OJ_Code, xsdOrgCompanyData.OJ_ARDebtorGroupCode);
				if (debtorGroup != null)
				{
					newCompanyData.OB_OJ_ARDebtorGroup = debtorGroup.PK;
				}
			}

			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARIncludeInwardsWhsConsolidatedInvoiceInfo, xsdOrgCompanyData.ARIncludeInwardsWhsConsolidatedInvoice.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARIncludeOutwardsWhsConsolidatedInvoiceInfo, xsdOrgCompanyData.ARIncludeOutwardsWhsConsolidatedInvoice.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARWhsStorageCalcMethodInfo, xsdOrgCompanyData.ARWhsStorageCalcMethod);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_CRIsShipsAgencyPrincipalInfo, xsdOrgCompanyData.CRIsShipsAgencyPrincipal.ToString());

			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_IMUsedBondedWhsInfo, xsdOrgCompanyData.IMUsedBondedWhs.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APQualityAssuredInfo, xsdOrgCompanyData.APQualityAssured.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_APQualityAssuredCheckedDateInfo, xsdOrgCompanyData.APQualityAssuredCheckedDate);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARQualityAssuredInfo, xsdOrgCompanyData.ARQualityAssured.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARQualityAssuredCheckedDateInfo, xsdOrgCompanyData.ARQualityAssuredCheckedDate);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARWarehouseRatingPeriodInfo, xsdOrgCompanyData.ARWarehouseRatingPeriod);
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_WhsClientFreeStorageDaysInfo, xsdOrgCompanyData.WhsClientFreeStorageDays.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_WhsOverrideFreeStorageInfo, xsdOrgCompanyData.WhsOverrideFreeStorage.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARExternalDebtorCodeInfo, xsdOrgCompanyData.ARExternalDebtorCode.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARClientNumberInfo, xsdOrgCompanyData.ARClientNumber.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARAllowMultiCurrencyPaymentInfo, xsdOrgCompanyData.ARAllowMultiCurrencyPayment.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditAgreedPaymentMethodInfo, xsdOrgCompanyData.ARCreditAgreedPaymentMethod.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardNumInfo, xsdOrgCompanyData.ARCreditCardNum.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardTypeInfo, xsdOrgCompanyData.ARCreditCardType.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardHolderInfo, xsdOrgCompanyData.ARCreditCardHolder.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardExpire_MonthInfo, xsdOrgCompanyData.ARCreditCardExpire_Month.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardExpire_YearInfo, xsdOrgCompanyData.ARCreditCardExpire_Year.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCreditCardAdditionalInfoInfo, xsdOrgCompanyData.ARCreditCardAdditionalInfo.ToString());
			#endregion
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_IMBillAgentChargesDirectInfo, xsdOrgCompanyData.IMBillAgentChargesDirect.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_EXBillAgentChargesDirectInfo, xsdOrgCompanyData.EXBillAgentChargesDirect.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARTemporaryCreditLimitIncreaseInfo, xsdOrgCompanyData.ARTemporaryCreditLimitIncrease.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARTemporaryCreditLimitIncreaseExpiryInfo, xsdOrgCompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime);
			newCompanyData.OB_ARTemporaryCreditLimitIncreaseExpiry = xsdOrgCompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime;
			context.SetPropertyInfoValueIfValueNotEmpty(newCompanyData.OB_ARCustomerSelfBillsRevenueInfo, xsdOrgCompanyData.ARCustomerSelfBillsRevenue.ToString());

			ImportOrgInvoiceType(newCompanyData, xsdOrgCompanyData);
			ImportOrgInvoiceRollupOrGroup(newCompanyData, xsdOrgCompanyData);
			ImportAccAPAccountDetails(newCompanyData, xsdOrgCompanyData);

			SysMergeARTermsValueObjectHelper arTermHelper = new SysMergeARTermsValueObjectHelper(ErrorContext);
			arTermHelper.ImportFromValueObjectCollection(xsdOrgCompanyData.OrgARTerms, newCompanyData, context);
			arTermHelper.ImportFromValueObjectCollection(xsdOrgCompanyData.AccCFXUpliftConfigurations, newCompanyData, context);
		}

		OrgCompanyData GetCompanyDataEnsuringIsUniqueForOrgAndCompany(ZGuid companyPk, OrgHeaderForDataTransfer org)
		{
			ZQuery existingCompanyDataQuery = new ZQuery();
			existingCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_OH, org.PK);
			existingCompanyDataQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, companyPk);
			OrgCompanyData[] existingCompanyDatas = org.Factory.Load<OrgCompanyData>(existingCompanyDataQuery);

			OrgCompanyData result = null;

			if (existingCompanyDatas.Length > 0)
			{
				result = existingCompanyDatas[0];

				for (int i = 1; i < existingCompanyDatas.Length; i++)
				{
					existingCompanyDatas[i].Delete();
				}
			}
			else
			{
				result = org.Factory.New<OrgCompanyData>();
				result.OB_GC = companyPk;
				result.OB_OH = org.PK;
			}

			return result;
		}

		void ImportOrgInvoiceType(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			if (xsdOrgCompanyData.OrgInvoiceTypes.IsSpecified)
			{
				foreach (Xsd.SysMergeOrgInvoiceType xsdOrgInvoiceType in xsdOrgCompanyData.OrgInvoiceTypes)
				{
					OrgInvoiceType invoiceType = companyData.Factory.New<OrgInvoiceType>();
					invoiceType.PI_OB = companyData.PK;
					invoiceType.PI_Module = xsdOrgInvoiceType.Module;
					invoiceType.PI_Type = xsdOrgInvoiceType.Type;
					invoiceType.PI_Interval = xsdOrgInvoiceType.Interval;
					invoiceType.PI_StartDay = xsdOrgInvoiceType.StartDay;
					invoiceType.PI_SecondaryType = xsdOrgInvoiceType.SecondaryType;
					invoiceType.PI_ServiceDirection = xsdOrgInvoiceType.ServiceDirection;
					invoiceType.PI_TransportMode = xsdOrgInvoiceType.TransportMode;
					invoiceType.PI_IsInclude = xsdOrgInvoiceType.IsInclude;
				}
			}
		}

		void ImportOrgInvoiceRollupOrGroup(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			if (xsdOrgCompanyData.OrgInvoiceRollupOrGroups.IsSpecified)
			{
				foreach (Xsd.SysMergeOrgInvoiceRollupOrGroup xsdOrgInvoiceRollupOrGroup in xsdOrgCompanyData.OrgInvoiceRollupOrGroups)
				{
					OrgInvoiceRollupOrGroup invoiceGroup = companyData.Factory.New<OrgInvoiceRollupOrGroup>();
					invoiceGroup.PG_OB = companyData.PK;
					invoiceGroup.PG_JobType = xsdOrgInvoiceRollupOrGroup.JobType;
					invoiceGroup.PG_TransportMode = xsdOrgInvoiceRollupOrGroup.TransportMode;
					invoiceGroup.PG_ServiceDirection = xsdOrgInvoiceRollupOrGroup.ServiceDirection;
					invoiceGroup.PG_GroupOrSubTotal = xsdOrgInvoiceRollupOrGroup.GroupOrSubTotal;
					invoiceGroup.PG_GroupOrSubtotalStyle = xsdOrgInvoiceRollupOrGroup.GroupOrSubTotalStyle;
					invoiceGroup.PG_InvoiceLineDisplayOption = xsdOrgInvoiceRollupOrGroup.InvoiceLineDisplayOption;
					invoiceGroup.PG_InvoicePostingStyle = xsdOrgInvoiceRollupOrGroup.InvoicePostingStyle;
				}
			}
		}

		void ImportAccAPAccountDetails(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			if (xsdOrgCompanyData.AccAPAccountDetails.IsSpecified)
			{
				foreach (Xsd.SysMergeAccAPAccountDetails xsdAccAPAccountDetail in xsdOrgCompanyData.AccAPAccountDetails)
				{
					AccAPAccountDetails accountDetails = companyData.Factory.New<AccAPAccountDetails>();
					accountDetails.A1_OB = companyData.PK;
					accountDetails.A1_AccountName = xsdAccAPAccountDetail.AccountName;
					accountDetails.A1_BankAccount = xsdAccAPAccountDetail.BankAccount;
					accountDetails.A1_BankAddress1 = xsdAccAPAccountDetail.BankAddress1;
					accountDetails.A1_BankAddress2 = xsdAccAPAccountDetail.BankAddress2;
					accountDetails.A1_BankAddress3 = xsdAccAPAccountDetail.BankAddress3;
					accountDetails.A1_BankBranchName = xsdAccAPAccountDetail.BankBranchName;
					accountDetails.A1_BankBsb = xsdAccAPAccountDetail.BankBsb;
					accountDetails.A1_BankName = xsdAccAPAccountDetail.BankName;
					accountDetails.A1_BankSwift = xsdAccAPAccountDetail.BankSwift;
					accountDetails.A1_IsDefaultAccount = xsdAccAPAccountDetail.IsDefaultAccount;
					accountDetails.A1_PaymentMethod = xsdAccAPAccountDetail.PaymentMethod;
					accountDetails.A1_RX_NKAccountCurrency = xsdAccAPAccountDetail.RX_AccountCurrency_NK;
					accountDetails.A1_IBANNumber = xsdAccAPAccountDetail.BankIBAN;
					accountDetails.A1_RN_NKCountryCode = xsdAccAPAccountDetail.CountryCode;
				}
			}
		}

		#endregion

		#region Export

		public void ExportToValueObjectCollection(OrgHeaderForDataTransfer org, Xsd.SysMergeOrgCompanyDataCollection xsdOrgCompanyDataCollection)
		{
			ZQuery query = new ZQuery(OrgCompanyDataSchema.OB_OH, org.PK);
			OrgCompanyData[] companyDataArray = org.Factory.Load<OrgCompanyData>(query);

			for (int i = 0; i < companyDataArray.Length; i++)
			{
				OrgCompanyData companyData = companyDataArray[i];
				Xsd.SysMergeOrgCompanyData xsdCompanyData = new Xsd.SysMergeOrgCompanyData();

				if (!companyData.OB_OH.IsEmpty)
				{
					ExportToValueObject(companyData, xsdCompanyData);
					xsdOrgCompanyDataCollection.Add(xsdCompanyData);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1809:AvoidExcessiveLocals")]
		public void ExportToValueObject(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			#region old fields
			if (companyData.Company != null)
			{
				xsdOrgCompanyData.GC_Code = companyData.Company.GC_Code;
				xsdOrgCompanyData.GC_CodeSpecified = true;
			}
			if (companyData.ControllingBranch != null)
			{
				xsdOrgCompanyData.GB_ControllingBranchCode = companyData.ControllingBranch.GB_Code;
				xsdOrgCompanyData.GB_ControllingBranchCodeSpecified = true;
			}

			if (!companyData.OB_IsDebtor.IsEmpty)
			{
				xsdOrgCompanyData.IsDebtor = companyData.OB_IsDebtor;
				xsdOrgCompanyData.IsDebtorSpecified = true;
			}
			if (!companyData.OB_IsCreditor.IsEmpty)
			{
				xsdOrgCompanyData.IsCreditor = companyData.OB_IsCreditor;
				xsdOrgCompanyData.IsCreditorSpecified = true;
			}
			if (!companyData.OB_APCategory.IsEmpty)
			{
				xsdOrgCompanyData.APCategory = companyData.OB_APCategory;
				xsdOrgCompanyData.APCategorySpecified = true;
			}
			if (!companyData.OB_APCreditLimit.IsEmpty)
			{
				xsdOrgCompanyData.APCreditLimit = companyData.OB_APCreditLimit;
				xsdOrgCompanyData.APCreditLimitSpecified = true;
			}
			if (!companyData.OB_APPayInvoiceAfterPostingDefault.IsEmpty)
			{
				xsdOrgCompanyData.APPayInvoiceAfterPostingDefault = companyData.OB_APPayInvoiceAfterPostingDefault;
				xsdOrgCompanyData.APPayInvoiceAfterPostingDefaultSpecified = true;
			}
			InvoiceTerm apTerm = companyData.GetAPTerm();
			if (!apTerm.Days.IsEmpty)
			{
				xsdOrgCompanyData.APPaymentTermDays = apTerm.Days;
				xsdOrgCompanyData.APPaymentTermDaysSpecified = true;
			}
			if (!apTerm.Term.IsEmpty)
			{
				xsdOrgCompanyData.APPaymentTerms = apTerm.Term;
				xsdOrgCompanyData.APPaymentTermsSpecified = true;
			}
			if (!companyData.OB_APVATConfig.IsEmpty)
			{
				xsdOrgCompanyData.APTaxApplicable = companyData.IsAPTaxApplicable;
				xsdOrgCompanyData.APTaxApplicableSpecified = true;
			}
			if (!companyData.OB_APWHTApplicable.IsEmpty)
			{
				xsdOrgCompanyData.APWHTApplicable = companyData.OB_APWHTApplicable;
				xsdOrgCompanyData.APWHTApplicableSpecified = true;
			}
			if (!companyData.OB_RX_NKAPDefltCurrency.IsEmpty)
			{
				xsdOrgCompanyData.RX_APDefaultCurrencyCode = companyData.OB_RX_NKAPDefltCurrency;
				xsdOrgCompanyData.RX_APDefaultCurrencyCodeSpecified = true;
			}

			if (!companyData.OB_OG_APCreditorGroup.IsEmpty)
			{
				xsdOrgCompanyData.OG_APCreditorGroupCode = companyData.Factory.Load<OrgCreditorGroup>(companyData.OB_OG_APCreditorGroup).OG_Code;
				xsdOrgCompanyData.OG_APCreditorGroupCodeSpecified = true;
			}

			if (!companyData.OB_AB_APDefaultBankAccount.IsEmpty)
			{
				AccBankAccount bankAccount = companyData.Factory.Load<AccBankAccount>(companyData.OB_AB_APDefaultBankAccount);

				if (bankAccount != null)
				{
					xsdOrgCompanyData.AB_APDefaultBankAccountCode = bankAccount.AB_Code;
					xsdOrgCompanyData.AB_APDefaultBankAccountCodeSpecified = true;
				}
			}

			if (!companyData.OB_AC_APDefaultChargeCode.IsEmpty)
			{
				AccChargeCode chargeCode = companyData.Factory.Load<AccChargeCode>(companyData.OB_AC_APDefaultChargeCode);

				if (chargeCode != null)
				{
					xsdOrgCompanyData.AC_APDefaultChargeCode = chargeCode.AC_Code;
					xsdOrgCompanyData.AC_APDefaultChargeCodeSpecified = true;
				}
			}

			if (!companyData.OB_AB_ARPayToAccount.IsEmpty)
			{
				AccBankAccount bankAccount = companyData.Factory.Load<AccBankAccount>(companyData.OB_AB_ARPayToAccount);

				if (bankAccount != null)
				{
					xsdOrgCompanyData.AB_ARPayToAccountCode = bankAccount.AB_Code;
					xsdOrgCompanyData.AB_ARPayToAccountCodeSpecified = true;
				}
			}

			if (!companyData.OB_APAirlineAccountNumber.IsEmpty)
			{
				xsdOrgCompanyData.APAirlineAccountNumber = companyData.OB_APAirlineAccountNumber;
				xsdOrgCompanyData.APAirlineAccountNumberSpecified = true;
			}
			if (!companyData.OB_ARAutoUpdateRates.IsEmpty)
			{
				xsdOrgCompanyData.ARAutoUpdateRates = companyData.OB_ARAutoUpdateRates;
				xsdOrgCompanyData.ARAutoUpdateRatesSpecified = true;
			}
			if (!companyData.OB_ARCategory.IsEmpty)
			{
				xsdOrgCompanyData.ARCategory = companyData.OB_ARCategory;
				xsdOrgCompanyData.ARCategorySpecified = true;
			}
			if (!companyData.OB_ARCombinedStatementInvoice.IsEmpty)
			{
				xsdOrgCompanyData.ARCombinedStatementInvoice = companyData.OB_ARCombinedStatementInvoice;
				xsdOrgCompanyData.ARCombinedStatementInvoiceSpecified = true;
			}
			if (!companyData.OB_ARConsolidatedAccountingCategory.IsEmpty)
			{
				xsdOrgCompanyData.ARConsolidatedAccountingCategory = companyData.OB_ARConsolidatedAccountingCategory;
				xsdOrgCompanyData.ARConsolidatedAccountingCategorySpecified = true;
			}
			if (!companyData.OB_ARCreditLimit.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditLimit = companyData.OB_ARCreditLimit;
				xsdOrgCompanyData.ARCreditLimitSpecified = true;
			}
			if (!companyData.OB_ARCreditRating.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditRating = companyData.OB_ARCreditRating;
				xsdOrgCompanyData.ARCreditRatingSpecified = true;
			}
			if (!companyData.OB_ARDontShowTaxOnDocs.IsEmpty)
			{
				xsdOrgCompanyData.ARDontShowTaxOnDocs = companyData.OB_ARDontShowTaxOnDocs;
				xsdOrgCompanyData.ARDontShowTaxOnDocsSpecified = true;
			}
			if (!companyData.OB_ARBuyersConsolInvoicingStyle.IsEmpty)
			{
				xsdOrgCompanyData.ARBuyersConsolInvoicingStyle = companyData.OB_ARBuyersConsolInvoicingStyle;
				xsdOrgCompanyData.ARBuyersConsolInvoicingStyleSpecified = true;
			}
			if (!companyData.OB_ARUseSettlementGroupCreditLimit.IsEmpty)
			{
				xsdOrgCompanyData.ARUseSettlementGroupCreditLimit = companyData.OB_ARUseSettlementGroupCreditLimit;
				xsdOrgCompanyData.ARUseSettlementGroupCreditLimitSpecified = true;
			}
			if (!companyData.OB_ARCreditApproved.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditApproved = companyData.OB_ARCreditApproved;
				xsdOrgCompanyData.ARCreditApprovedSpecified = true;
			}
			if (!companyData.OB_AROnCreditHold.IsEmpty)
			{
				xsdOrgCompanyData.AROnCreditHold = companyData.OB_AROnCreditHold;
				xsdOrgCompanyData.AROnCreditHoldSpecified = true;
			}
			if (!companyData.OB_ARAccountAndCreditReviewDue.IsEmpty)
			{
				xsdOrgCompanyData.ARAccountAndCreditReviewDue = companyData.OB_ARAccountAndCreditReviewDue.ToDateTime();
				xsdOrgCompanyData.ARAccountAndCreditReviewDueSpecified = true;
			}
			if (!companyData.OB_ARPreviousChequeDrawer.IsEmpty)
			{
				xsdOrgCompanyData.ARPreviousChequeDrawer = companyData.OB_ARPreviousChequeDrawer;
				xsdOrgCompanyData.ARPreviousChequeDrawerSpecified = true;
			}
			if (!companyData.OB_ARPreviousChequeDrawerBank.IsEmpty)
			{
				xsdOrgCompanyData.ARPreviousChequeDrawerBank = companyData.OB_ARPreviousChequeDrawerBank;
				xsdOrgCompanyData.ARPreviousChequeDrawerBankSpecified = true;
			}
			if (!companyData.OB_ARPreviousChequeDrawerBankBranch.IsEmpty)
			{
				xsdOrgCompanyData.ARPreviousChequeDrawerBankBranch = companyData.OB_ARPreviousChequeDrawerBankBranch;
				xsdOrgCompanyData.ARPreviousChequeDrawerBankBranchSpecified = true;
			}
			if (!companyData.OB_ARReceiptInvoiceAfterPostingDefault.IsEmpty)
			{
				xsdOrgCompanyData.ARReceiptInvoiceAfterPostingDefault = companyData.OB_ARReceiptInvoiceAfterPostingDefault;
				xsdOrgCompanyData.ARReceiptInvoiceAfterPostingDefaultSpecified = true;
			}
			if (!companyData.OB_ARVATConfig.IsEmpty)
			{
				xsdOrgCompanyData.ARTaxApplicable = companyData.IsARTaxApplicable;
				xsdOrgCompanyData.ARTaxApplicableSpecified = true;
			}
			if (!companyData.OB_AREftCustomsPaymentMethod.IsEmpty)
			{
				xsdOrgCompanyData.AREftCustomsPaymentMethod = companyData.OB_AREftCustomsPaymentMethod;
				xsdOrgCompanyData.AREftCustomsPaymentMethodSpecified = true;
			}
			if (!companyData.OB_ARWHTApplicable.IsEmpty)
			{
				xsdOrgCompanyData.ARWHTApplicable = companyData.OB_ARWHTApplicable;
				xsdOrgCompanyData.ARWHTApplicableSpecified = true;
			}
			if (!companyData.OB_RX_NKARDDefltCurrency.IsEmpty)
			{
				xsdOrgCompanyData.RX_ARDefaultCurrencyCode = companyData.OB_RX_NKARDDefltCurrency;
				xsdOrgCompanyData.RX_ARDefaultCurrencyCodeSpecified = true;
			}
			if (!companyData.OB_OJ_ARDebtorGroup.IsEmpty)
			{
				xsdOrgCompanyData.OJ_ARDebtorGroupCode = companyData.Factory.Load<OrgDebtorGroup>(companyData.OB_OJ_ARDebtorGroup).OJ_Code;
				xsdOrgCompanyData.OJ_ARDebtorGroupCodeSpecified = true;
			}
			if (!companyData.OB_ARIncludeInwardsWhsConsolidatedInvoice.IsEmpty)
			{
				xsdOrgCompanyData.ARIncludeInwardsWhsConsolidatedInvoice = companyData.OB_ARIncludeInwardsWhsConsolidatedInvoice;
				xsdOrgCompanyData.ARIncludeInwardsWhsConsolidatedInvoiceSpecified = true;
			}
			if (!companyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice.IsEmpty)
			{
				xsdOrgCompanyData.ARIncludeOutwardsWhsConsolidatedInvoice = companyData.OB_ARIncludeOutwardsWhsConsolidatedInvoice;
				xsdOrgCompanyData.ARIncludeOutwardsWhsConsolidatedInvoiceSpecified = true;
			}
			if (!companyData.OB_ARWhsStorageCalcMethod.IsEmpty)
			{
				xsdOrgCompanyData.ARWhsStorageCalcMethod = companyData.OB_ARWhsStorageCalcMethod;
				xsdOrgCompanyData.ARWhsStorageCalcMethodSpecified = true;
			}
			if (!companyData.OB_CRIsShipsAgencyPrincipal.IsEmpty)
			{
				xsdOrgCompanyData.CRIsShipsAgencyPrincipal = companyData.OB_CRIsShipsAgencyPrincipal;
				xsdOrgCompanyData.CRIsShipsAgencyPrincipalSpecified = true;
			}

			if (!companyData.OB_IMUsedBondedWhs.IsEmpty)
			{
				xsdOrgCompanyData.IMUsedBondedWhs = companyData.OB_IMUsedBondedWhs;
				xsdOrgCompanyData.IMUsedBondedWhsSpecified = true;
			}
			if (!companyData.OB_APQualityAssured.IsEmpty)
			{
				xsdOrgCompanyData.APQualityAssured = companyData.OB_APQualityAssured;
				xsdOrgCompanyData.APQualityAssuredSpecified = true;
			}
			if (!companyData.OB_APQualityAssuredCheckedDate.IsEmpty)
			{
				xsdOrgCompanyData.APQualityAssuredCheckedDate = companyData.OB_APQualityAssuredCheckedDate;
				xsdOrgCompanyData.APQualityAssuredCheckedDateSpecified = true;
			}
			if (!companyData.OB_ARQualityAssured.IsEmpty)
			{
				xsdOrgCompanyData.ARQualityAssured = companyData.OB_ARQualityAssured;
				xsdOrgCompanyData.ARQualityAssuredSpecified = true;
			}
			if (!companyData.OB_ARQualityAssuredCheckedDate.IsEmpty)
			{
				xsdOrgCompanyData.ARQualityAssuredCheckedDate = companyData.OB_ARQualityAssuredCheckedDate;
				xsdOrgCompanyData.ARQualityAssuredCheckedDateSpecified = true;
			}
			if (!companyData.OB_ARWarehouseRatingPeriod.IsEmpty)
			{
				xsdOrgCompanyData.ARWarehouseRatingPeriod = companyData.OB_ARWarehouseRatingPeriod;
				xsdOrgCompanyData.ARWarehouseRatingPeriodSpecified = true;
			}
			if (!companyData.OB_WhsClientFreeStorageDays.IsEmpty)
			{
				xsdOrgCompanyData.WhsClientFreeStorageDays = companyData.OB_WhsClientFreeStorageDays;
				xsdOrgCompanyData.WhsClientFreeStorageDaysSpecified = true;
			}
			if (!companyData.OB_WhsOverrideFreeStorage.IsEmpty)
			{
				xsdOrgCompanyData.WhsOverrideFreeStorage = companyData.OB_WhsOverrideFreeStorage;
				xsdOrgCompanyData.WhsOverrideFreeStorageSpecified = true;
			}
			if (!companyData.OB_ARExternalDebtorCode.IsEmpty)
			{
				xsdOrgCompanyData.ARExternalDebtorCode = companyData.OB_ARExternalDebtorCode;
				xsdOrgCompanyData.ARExternalDebtorCodeSpecified = true;
			}
			if (!companyData.OB_ARClientNumber.IsEmpty)
			{
				xsdOrgCompanyData.ARClientNumber = companyData.OB_ARClientNumber;
				xsdOrgCompanyData.ARClientNumberSpecified = true;
			}
			if (!companyData.OB_ARAllowMultiCurrencyPayment.IsEmpty)
			{
				xsdOrgCompanyData.ARAllowMultiCurrencyPayment = companyData.OB_ARAllowMultiCurrencyPayment;
				xsdOrgCompanyData.ARAllowMultiCurrencyPaymentSpecified = true;
			}
			if (!companyData.OB_ARCreditAgreedPaymentMethod.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditAgreedPaymentMethod = companyData.OB_ARCreditAgreedPaymentMethod;
				xsdOrgCompanyData.ARCreditAgreedPaymentMethodSpecified = true;
			}
			if (!companyData.OB_ARCreditCardNum.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardNum = companyData.OB_ARCreditCardNum;
				xsdOrgCompanyData.ARCreditCardNumSpecified = true;
			}
			if (!companyData.OB_ARCreditCardType.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardType = companyData.OB_ARCreditCardType;
				xsdOrgCompanyData.ARCreditCardTypeSpecified = true;
			}
			if (!companyData.OB_ARCreditCardHolder.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardHolder = companyData.OB_ARCreditCardHolder;
				xsdOrgCompanyData.ARCreditCardHolderSpecified = true;
			}
			if (!companyData.OB_ARCreditCardExpire_Month.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardExpire_Month = companyData.OB_ARCreditCardExpire_Month;
				xsdOrgCompanyData.ARCreditCardExpire_MonthSpecified = true;
			}
			if (!companyData.OB_ARCreditCardExpire_Year.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardExpire_Year = companyData.OB_ARCreditCardExpire_Year;
				xsdOrgCompanyData.ARCreditCardExpire_YearSpecified = true;
			}
			if (!companyData.OB_ARCreditCardAdditionalInfo.IsEmpty)
			{
				xsdOrgCompanyData.ARCreditCardAdditionalInfo = companyData.OB_ARCreditCardAdditionalInfo;
				xsdOrgCompanyData.ARCreditCardAdditionalInfoSpecified = true;
			}
			#endregion
			if (!companyData.OB_IMBillAgentChargesDirect.IsEmpty)
			{
				xsdOrgCompanyData.IMBillAgentChargesDirect = companyData.OB_IMBillAgentChargesDirect;
				xsdOrgCompanyData.IMBillAgentChargesDirectSpecified = true;
			}
			if (!companyData.OB_EXBillAgentChargesDirect.IsEmpty)
			{
				xsdOrgCompanyData.EXBillAgentChargesDirect = companyData.OB_EXBillAgentChargesDirect;
				xsdOrgCompanyData.EXBillAgentChargesDirectSpecified = true;
			}
			if (!companyData.OB_ARTemporaryCreditLimitIncrease.IsEmpty)
			{
				xsdOrgCompanyData.ARTemporaryCreditLimitIncrease = companyData.OB_ARTemporaryCreditLimitIncrease;
				xsdOrgCompanyData.ARTemporaryCreditLimitIncreaseSpecified = true;
			}
			if (!companyData.OB_ARTemporaryCreditLimitIncreaseExpiry.IsEmpty)
			{
				xsdOrgCompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTime = companyData.OB_ARTemporaryCreditLimitIncreaseExpiry;
				xsdOrgCompanyData.ARTemporaryCreditLimitIncreaseExpiryLocalBranchTimeSpecified = true;
			}
			if (!companyData.OB_ARCustomerSelfBillsRevenue.IsEmpty)
			{
				xsdOrgCompanyData.ARCustomerSelfBillsRevenue = companyData.OB_ARCustomerSelfBillsRevenue;
				xsdOrgCompanyData.ARCustomerSelfBillsRevenueSpecified = true;
			}

			ExportOrgInvoiceType(companyData, xsdOrgCompanyData);
			ExportOrgInvoiceRollupOrGroup(companyData, xsdOrgCompanyData);
			ExportAccAPAccountDetails(companyData, xsdOrgCompanyData);

			SysMergeARTermsValueObjectHelper arTermHelper = new SysMergeARTermsValueObjectHelper(ErrorContext);
			arTermHelper.ExportToValueObjectCollection(companyData, xsdOrgCompanyData.OrgARTerms);
			arTermHelper.ExportToValueObjectCollection(companyData, xsdOrgCompanyData.AccCFXUpliftConfigurations);
		}

		void ExportOrgInvoiceType(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			ZQuery query = new ZQuery(OrgInvoiceTypeSchema.PI_OB, companyData.PK);
			OrgInvoiceType[] orgInvoiceTypes = companyData.Factory.Load<OrgInvoiceType>(query);

			foreach (OrgInvoiceType orgInvoiceType in orgInvoiceTypes)
			{
				Xsd.SysMergeOrgInvoiceType xsdOrgInvoiceType = xsdOrgCompanyData.OrgInvoiceTypes.AddNew();

				xsdOrgInvoiceType.Module = orgInvoiceType.PI_Module;
				xsdOrgInvoiceType.Type = orgInvoiceType.PI_Type;
				xsdOrgInvoiceType.Interval = orgInvoiceType.PI_Interval;
				xsdOrgInvoiceType.StartDay = orgInvoiceType.PI_StartDay;
				xsdOrgInvoiceType.SecondaryType = orgInvoiceType.PI_SecondaryType;
				xsdOrgInvoiceType.ServiceDirection = orgInvoiceType.PI_ServiceDirection;
				xsdOrgInvoiceType.TransportMode = orgInvoiceType.PI_TransportMode;
				xsdOrgInvoiceType.IsInclude = orgInvoiceType.PI_IsInclude;
			}
		}

		void ExportOrgInvoiceRollupOrGroup(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			ZQuery query = new ZQuery(OrgInvoiceRollupOrGroupSchema.PG_OB, companyData.PK);
			OrgInvoiceRollupOrGroup[] invoiceGroups = companyData.Factory.Load<OrgInvoiceRollupOrGroup>(query);

			foreach (OrgInvoiceRollupOrGroup invoiceGroup in invoiceGroups)
			{
				Xsd.SysMergeOrgInvoiceRollupOrGroup xsdOrgInvoiceRollupOrGroup = xsdOrgCompanyData.OrgInvoiceRollupOrGroups.AddNew();

				xsdOrgInvoiceRollupOrGroup.JobType = invoiceGroup.PG_JobType;
				xsdOrgInvoiceRollupOrGroup.TransportMode = invoiceGroup.PG_TransportMode;
				xsdOrgInvoiceRollupOrGroup.ServiceDirection = invoiceGroup.PG_ServiceDirection;
				xsdOrgInvoiceRollupOrGroup.GroupOrSubTotal = invoiceGroup.PG_GroupOrSubTotal;
				xsdOrgInvoiceRollupOrGroup.GroupOrSubTotalStyle = invoiceGroup.PG_GroupOrSubtotalStyle;
				xsdOrgInvoiceRollupOrGroup.InvoiceLineDisplayOption = invoiceGroup.PG_InvoiceLineDisplayOption;
				xsdOrgInvoiceRollupOrGroup.InvoicePostingStyle = invoiceGroup.PG_InvoicePostingStyle;
			}
		}

		void ExportAccAPAccountDetails(OrgCompanyData companyData, Xsd.SysMergeOrgCompanyData xsdOrgCompanyData)
		{
			AccAPAccountDetails[] accountDetails = companyData.Factory.Load<AccAPAccountDetails>(new ZQuery(AccAPAccountDetailsSchema.A1_OB, companyData.PK));

			foreach (AccAPAccountDetails accountDetail in accountDetails)
			{
				Xsd.SysMergeAccAPAccountDetails xsdAccAPAccountDetail = xsdOrgCompanyData.AccAPAccountDetails.AddNew();

				xsdAccAPAccountDetail.AccountName = accountDetail.A1_AccountName;
				xsdAccAPAccountDetail.BankAccount = accountDetail.A1_BankAccount;
				xsdAccAPAccountDetail.BankAddress1 = accountDetail.A1_BankAddress1;
				xsdAccAPAccountDetail.BankAddress2 = accountDetail.A1_BankAddress2;
				xsdAccAPAccountDetail.BankAddress3 = accountDetail.A1_BankAddress3;
				xsdAccAPAccountDetail.BankBranchName = accountDetail.A1_BankBranchName;
				xsdAccAPAccountDetail.BankBsb = accountDetail.A1_BankBsb;
				xsdAccAPAccountDetail.BankName = accountDetail.A1_BankName;
				xsdAccAPAccountDetail.BankSwift = accountDetail.A1_BankSwift;
				xsdAccAPAccountDetail.IsDefaultAccount = accountDetail.A1_IsDefaultAccount;
				xsdAccAPAccountDetail.PaymentMethod = accountDetail.A1_PaymentMethod;
				xsdAccAPAccountDetail.RX_AccountCurrency_NK = accountDetail.A1_RX_NKAccountCurrency;
				xsdAccAPAccountDetail.BankIBAN = accountDetail.A1_IBANNumber;
				xsdAccAPAccountDetail.CountryCode = accountDetail.A1_RN_NKCountryCode;

				// Set Specified flag for boolean/numeric fields so they get serialised to XML
				// Boolean
				xsdAccAPAccountDetail.IsDefaultAccountSpecified = true;
			}
		}

		#endregion
	}
}
