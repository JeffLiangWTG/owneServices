using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.RSACryptography;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ExternalInterface
{
	class Accounting : IAccounting
	{
		#region IAccounting Members

		public IRegistry Registry
		{
			get { return new RegistryWrapper(); }
		}

		public bool IsENettOrganisation(ZGuid organisationPk)
		{
			return AccountingConfigurationRegistry.Instance.ENettRegistration.Value.OrganisationPK == organisationPk;
		}

		public bool ShouldCollectionCallCreateFollowUpAppointments
		{
			get { return AccountingConfigurationRegistry.Instance.CollectionCallCreateFollowUpAppointments.Value; }
		}

		public bool EnableLocalChargeCodeDescriptionDefault
		{
			get { return AccountingConfigurationRegistry.Instance.EnableLocalChargeCodeDescriptionDefault.Value; }
		}

		public bool ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors
		{
			get { return AccountingConfigurationRegistry.Instance.ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors.Value; }
		}

		public ZString SelfBillingInvoiceTransactionNumberPrefix(Guid companyPk)
		{
			return AccountingConfigurationRegistry.Instance.SelfBillingInvoiceTransactionNumberPrefix.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
		}

		public ZString InvoiceTransactionNumberPrefix(Guid companyPk)
		{
			return AccountingConfigurationRegistry.Instance.InvoiceTransactionNumberPrefix.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
		}

		public Guid ARAccountGroup
		{
			get { return AccountingConfigurationRegistry.Instance.ARAccountGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public Guid APAccountGroup
		{
			get { return AccountingConfigurationRegistry.Instance.APAccountGroup.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public int CollectionCallFollowUpDays
		{
			get { return AccountingConfigurationRegistry.Instance.CollectionCallFollowUpDays.Value; }
		}

		public ZString GLAccountFormat
		{
			get { return AccountingConfigurationRegistry.Instance.GLAccountFormat.Value; }
		}

		public ZString GetGLAccountFormat(string accountNum)
		{
			return AccountingConfigurationRegistry.GetGLAccountFormat(accountNum);
		}

		public bool OverrideInterOfficeBillingTaxIDToNOTREPORT
		{
			get { return AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORT.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public bool OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy
		{
			get { return AccountingConfigurationRegistry.Instance.OverrideInterOfficeBillingTaxIDToNOTREPORTForBranchOrgProxy.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public ZGuid GroupMemberBillingDefaultInvoiceTaxMessage
		{
			get { return AccountingConfigurationRegistry.Instance.GroupMemberBillingDefaultInvoiceTaxMessage.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid PLAppropriationAccount
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.PLAppropriationAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

		public IRegistryItem PlAppropriationAccountRegistryItem
		{
			get { return AccountingConfigurationRegistry.Instance.PLAppropriationAccount; }
		}

		public ICodeDescriptionPairList QueryClaimTypeCodeDescriptionPairList
		{
			get { return AccountingConfigurationRegistry.Instance.QueryClaimType.Value.GetCodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList ClaimReasonCodeDescriptionPairList
		{
			get { return AccountingConfigurationRegistry.Instance.ClaimReason.Value.GetCodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList ClaimStatusCodeDescriptionPairList
		{
			get { return AccountingConfigurationRegistry.Instance.ClaimStatus.Value.GetCodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList JobProfitLossReasonCodeDescriptionPairList
		{
			get { return AccountingConfigurationRegistry.Instance.JobProfitLossReasonCode.Value.GetCodeDescriptionPairList(); }
		}

		public ICodeDescriptionPairList GLPresentationJournalCategoriesList(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
		}

		public string GetCategorisWithChildren(ZString parentCode)
		{
			if(string.IsNullOrEmpty(parentCode))
			{
				return string.Empty;
			}

			var glPresentationJournalCategoryCollection = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value;
			var childCategories = glPresentationJournalCategoryCollection.Cast<GLPresentationJournalCategory>().Where(x => x.Bool && x.ParentCode == parentCode).OrderBy(x => x.Code).Select(x => x.Code).ToList();
			childCategories.Insert(0, parentCode);
			return string.Join(",", childCategories);
		}

		public ICodeDescriptionPairList GLPresentationJournalCategoriesGroupList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var glPresentationJournalCategoryCollection = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value;
				var parentCodeAlreadyAdd = new List<ZString>();
				foreach (var item in glPresentationJournalCategoryCollection.Cast<GLPresentationJournalCategory>().Where(x => x.Bool))
				{
					if (!item.ParentCode.IsEmpty && !parentCodeAlreadyAdd.Contains(item.ParentCode))
					{
						parentCodeAlreadyAdd.Add(item.ParentCode);
						var groupCode = new ZStringBuilder(item.ParentCode);
						groupCode.Append(",");
						foreach (var element in glPresentationJournalCategoryCollection.Cast<GLPresentationJournalCategory>().Where(x => x.ParentCode == item.ParentCode))
						{
							groupCode.Append(element.Code);
							groupCode.Append(",");
						}
						var groupCodeToAdd = groupCode.ToString().Trim(',');
						result.AddPair(groupCodeToAdd, ResString.GetMultilingualString("F62A7297-D0C2-4066-82DA-32AA505BFCFB", "{0} categories", groupCodeToAdd));
					}
					result.AddPair(item.Code, item.Description);
				}
				return result;
			}
		}

		public ICodeDescriptionPairList CashFlowCategoryCodeDescriptionList
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				foreach (CashFlowActivityConfiguration cashFlowActivity in AccountingMasterFilesRegistry.Instance.CashFlowActivityConfiguration.Value)
				{
					if (cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.Cash &&
						cashFlowActivity.ActivityType != CashFlowActivityConfiguratonLookups.ActivityTypeCodes.NonCash)
					{
						result.AddPair(cashFlowActivity.Code, cashFlowActivity.Description);
					}
				}
				result.AddPair("ZZZ", Res.GetString("6d4ecd70-219b-4242-acba-953e48d2076e", "Invalid classifications"));
				return result;
			}
		}

		public ICodeDescriptionPairList ReversalReasonCodesList
		{
			get { return AccountingMasterFilesRegistry.Instance.ReversalReasonCodesList.Value; }
		}

		public ICodeDescriptionPairList GoodsReceivedStatusCodesList
		{
			get { return AccountingConfigurationRegistry.Instance.GoodsReceivedStatusCodesList.Value; }
		}

		public ZGuid ProfitShareChargeCode
		{
			get { return AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value; }
		}

		public Guid BSAccountStartAccount
		{
			get { return (Guid)AccountingConfigurationRegistry.Instance.BSAccountStartAccount.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty); }
		}

#if DEBUG

		public IDisposable SetupEnableEPaymentFunctionalityRegistry(Guid companyPK, bool isOFXEPaymentsEnabled)
		{
			return TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(companyPK, isOFXEPaymentsEnabled);
		}

		public void AddReportOrder_AccountsOrderValue(ZString language, ZString country, ZString accountsOrderBeginsWith, ZGuid glAccountSecondReportStartsFrom)
		{
			var reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			var rOrder = reportOrders.FindByLanguageAndCountryCode(language, country);
			if (rOrder == null)
			{
				rOrder = reportOrders.AddNew();
				rOrder.Language = language;
				rOrder.CountryCode = country;
			}
			rOrder.GLAccountSecondReportStartsFrom = glAccountSecondReportStartsFrom;
			rOrder.AccountsOrderBeginsWith = accountsOrderBeginsWith;
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
		}

		public IDisposable SetupIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation(Guid companyPK, bool shouldIncludeCashAdvanceRequests)
		{
			return AccountingConfigurationRegistry.Instance.IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, shouldIncludeCashAdvanceRequests);
		}

		public IDisposable SetupGLPresentationJournalCategoriesListRegistry(Guid companyPK, ZString categoryCode, ZString categoryDescription)
		{
			var list = new GLPresentationJournalCategoryCollection();
			var category = list.AddNew();
			category.Code = categoryCode;
			category.Description = (NoResString)categoryDescription;
			category.Bool = true;
			return AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetTemporaryValue(companyPK, Guid.Empty, Guid.Empty, list);
		}

#endif

		public ZGuid ReportOrder_GLAccountSecondReportStartsFrom(ZString language, ZString country)
		{
			ReportOrder rOrder = AccountingConfigurationRegistry.Instance.ReportOrder.Value.FindByLanguageAndCountryCode(language, country);
			return rOrder != null ? rOrder.GLAccountSecondReportStartsFrom : ZGuid.Empty;
		}

		public string ReportOrder_AccountsOrderBeginsWith(ZString language, ZString country)
		{
			ReportOrder rOrder = AccountingConfigurationRegistry.Instance.ReportOrder.Value.FindByLanguageAndCountryCode(language, country);
			return rOrder != null ? (string)rOrder.AccountsOrderBeginsWith : string.Empty;
		}

		public bool PrintLogoOnCheque
		{
			get { return AccountingConfigurationRegistry.Instance.PrintLogoOnCheque.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public decimal CurrentPrimeRate
		{
			get { return RatingDataRegistry.Instance.CurrentPrimeRate.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public bool ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(BusinessObject plugin)
		{
			return AccountingConfigurationRegistry.Instance.ShouldAddJobInvoicingRecordAtSavingOrEditingOfOperationsJob(plugin as IJobInvoicingPlugIn);
		}

		public string AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation
		{
			get { return AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJobRegistryItemLocation; }
		}

		public bool UseJobNumberBasedInvoiceNumbers
		{
			get { return AccountingConfigurationRegistry.Instance.UseJobNumberBasedInvoiceNumbers.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK); }
		}

		public bool WIPMustHaveDebtorCode(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public bool AccrualMustHaveCreditorCode(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public Guid CustomsOther
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportOther
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportOther.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExWarehouse
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExWarehouse.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportAirUld
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportAirUld.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportSeaFcl
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportSeaLcl
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportRail
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportRoad
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsImportPost
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsImportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportSeaFcl
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportSeaFcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportSeaLcl
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportSeaLcl.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportAirUld
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportAirUld.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportRail
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportRail.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportRoad
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportRoad.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public Guid CustomsExportPost
		{
			get { return AccountingConfigurationRegistry.Instance.CustomsExportPost.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty); }
		}

		public string[] GetCaptionsOfRegistryItemsUsingGLHeader(Guid gLHeaderPK)
		{
			List<string> result = new List<string>();

			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.BankTransactionGLAccount,
				AccountingConfigurationRegistry.Instance.CASSGLAccount,
				AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount,
				AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
				AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
				AccountingConfigurationRegistry.Instance.APControlAccount,
				AccountingConfigurationRegistry.Instance.APJournalAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount,
				AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount,
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
				AccountingConfigurationRegistry.Instance.ARControlAccount,
				AccountingConfigurationRegistry.Instance.ARJournalAccount,
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.CFXAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
				AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
				AccountingConfigurationRegistry.Instance.GSTInputControlAccount,
				AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
				AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
				AccountingConfigurationRegistry.Instance.PLAppropriationAccount,
				AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
				AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
				AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
			};

			foreach (IRegistryItem item in registryItemsToCheck)
			{
				if (item.Value is Guid && ((Guid)item.Value) == gLHeaderPK)
				{
					result.Add(item.Caption);
				}
			}

			return result.ToArray();
		}

		public bool CollectConstructorCallStackDetails
		{
			get { return AccountingConfigurationRegistry.Instance.CollectConstructorCallStackDetailsToReportInCriticalValidationErrors.Value; }
		}

		public IRegistryItem IncludeUnpostedRevenueInCreditLimitCalculation
		{
			get { return AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInCreditLimitCalculation; }
		}

		public IRegistryItem IncludeUnpostedRevenueInGlobalCreditLimitCalculation => AccountingConfigurationRegistry.Instance.IncludeUnpostedRevenueInGlobalCreditLimitCalculation;

		public void SetStampDutyConfiguration(Guid companyPK, string taxIDs, decimal stampDutyFixedAmount, decimal stampDutyThreshold)
		{
			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(companyPK, Guid.Empty, Guid.Empty, taxIDs);
			AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.SetValue(companyPK, Guid.Empty, Guid.Empty, stampDutyFixedAmount);
			AccountingConfigurationRegistry.Instance.StampDutyThreshold.SetValue(companyPK, Guid.Empty, Guid.Empty, stampDutyThreshold);
		}

		public void SetMainGSTTaxIDConfiguration(Guid companyPK, Guid taxID)
		{
			AccountingConfigurationRegistry.Instance.MainGSTTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, taxID);
		}

		public void SetMainFreeGSTTaxIDConfiguration(Guid companyPK, Guid taxID)
		{
			AccountingConfigurationRegistry.Instance.MainFreeGSTTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, taxID);
		}

		public void SetMainGSTReverseTaxIDConfiguration(Guid companyPK, Guid taxID)
		{
			AccountingConfigurationRegistry.Instance.MainGSTReverseTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, taxID);
		}

		public void SetMainFreeGSTReverseTaxIDConfiguration(Guid companyPK, Guid taxID)
		{
			AccountingConfigurationRegistry.Instance.MainFreeGSTReverseTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, taxID);
		}

		public void SetMainNotReportableTaxIDConfiguration(Guid companyPK, Guid taxID)
		{
			AccountingConfigurationRegistry.Instance.MainNotReportableTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, taxID);
		}

		public void SetCASSFileImportDefaultTaxIDConfiguration(Guid companyPK, Guid standardRatedTaxID, Guid zeroRatedTaxID)
		{
			CASSFileImportDefaultTaxID defaultTaxID = new CASSFileImportDefaultTaxID();
			defaultTaxID.StandardRatedTaxID = standardRatedTaxID;
			defaultTaxID.ZeroRatedTaxID = zeroRatedTaxID;
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(companyPK, Guid.Empty, Guid.Empty, defaultTaxID);
		}

		public void SetTaxIDsAttractingStampDutyForTransformation()
		{
			var factory = new BusinessObjectFactory();

			var query = new ZQuery(GlbCompanySchema.GC_RN_NKCountryCode, Constants.CountryCodes.Italy);
			query.AddToFilter(GlbCompanySchema.GC_IsActive, true);
			var italyCompanies = factory.Load<GlbCompany>(query);

			foreach (var company in italyCompanies)
			{
				string taxIdsAttractingStampDuty = TaxIDsAttractingStampDuty(company.PK.ToGuid());
				if (!string.IsNullOrEmpty(taxIdsAttractingStampDuty) && taxIdsAttractingStampDuty != Guid.Empty.ToString())
				{
					var eSCLUSEB = AccTaxRate.FindExistingTaxRate(factory, "ESCLUSEB", "EXL", Constants.CountryCodes.Italy);
					if (eSCLUSEB != null && !taxIdsAttractingStampDuty.Contains(eSCLUSEB.PK.ToString()))
					{
						var newTaxIdsAttractingStampDuty = taxIdsAttractingStampDuty + ',' + eSCLUSEB.PK.ToString();
						AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, newTaxIdsAttractingStampDuty);
					}
				}
			}
		}

		public void SetTaxIDsAttractingStampDuty(Guid companyPK, string taxIds)
		{
			AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.SetValue(companyPK, Guid.Empty, Guid.Empty, taxIds);
		}

		public string TaxIDsAttractingStampDuty(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.TaxIDsAttractingStampDuty.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public decimal StampDutyFixedAmount(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.StampDutyFixedAmount.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public decimal StampDutyThreshold(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.StampDutyThreshold.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public void SetWIPMustHaveDebtorCode(Guid companyPK, bool value)
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public void SetAccrualMustHaveCreditorCode(Guid companyPK, bool value)
		{
			AccountingConfigurationRegistry.Instance.AccrualMustHaveCreditorCode.SetValue(companyPK, Guid.Empty, Guid.Empty, value);
		}

		public bool IsInvoicePaymentWebServiceEnabled(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.EnableInvoicePaymentWebService.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public bool UseWebServiceForCreditLimit // Should be removed after refactoring MasterFiles.Business.CreditChecker and Accounting.DataTransfer.Integration.OrgCreditLimitAndBalanceDetails
		{
			get { return AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit.Value; }
		}

		public bool IsMiscInvoiceInPeriodicInvoiceEnabled(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.EnableMiscInvoiceInPeriodicInvoice.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
		}

		public bool TaxRecognitionDefaultingRules_IsOrganisationOverridePermitted(string ledger)
		{
			return AccountingConfigurationRegistry.Instance.TaxRecognitionDefaultingRules.Value.IsOrganisationOverridePermitted(ledger);
		}

		public ICreditControlledDocumentsCheckConfiguration[] GetCreditControlledDocumentsCheckConfiguration()
		{
			return AccountingConfigurationRegistry.Instance.CreditLimitCheckOverdueInvoicesStatusCheck.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray();
		}

		public ICreditControlledDocumentsCheckConfiguration[] GetGlobalCreditControlledDocumentsCheckConfiguration()
		{
			return AccountingConfigurationRegistry.Instance.GlobalCreditLimitCheckOverdueInvoicesStatusCheck.Value.Cast<CreditControlledDocumentsCheckConfiguration>().ToArray();
		}

		public bool JobInvoicingCFXEnabled(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		public bool IsGLAccountUsedInCompanyLevelRegistry(ZGuid glHeaderPk, ZGuid[] companyPks)
		{
			return checkCompanyLevelRegistryForGLHeader(glHeaderPk, companyPks);
		}

		bool checkCompanyLevelRegistryForGLHeader(ZGuid glHeaderPK, ZGuid[] companyFilterPKs)
		{
			var factory = new BusinessObjectFactory();
			var departments = new GlbDepartmentCollection(factory);

			foreach (var company in companyFilterPKs)
			{
				if (AccountingConfigurationRegistry.Instance.CASSGLAccount.GetValueWithoutFallback(company.ToGuid(), Guid.Empty, Guid.Empty) == glHeaderPK
					|| AccountingConfigurationRegistry.Instance.CFXAccount.GetValueWithoutFallback(company.ToGuid(), Guid.Empty, Guid.Empty) == glHeaderPK)
				{
					return true;
				}

				foreach (var department in departments)
				{
					if (AccountingConfigurationRegistry.Instance.CFXAccount.GetValueWithoutFallback(company.ToGuid(), Guid.Empty, department.PK.ToGuid()) == glHeaderPK)
					{
						return true;
					}
				}

				IntercompanyClearingConfigurationCollection values = AccountingConfigurationRegistry.Instance.IntercompanyClearingConfiguration.GetValueWithoutFallback(company.ToGuid(), Guid.Empty, Guid.Empty);
				if (values.Cast<IntercompanyClearingConfiguration>().Any(x => x.ClearingGLAccount == glHeaderPK))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsGLAccountUsedInSystemLevelRegistry(ZGuid glHeaderPK)
		{
			IRegistryItem[] registryItemsToCheck =
			{
				AccountingConfigurationRegistry.Instance.DiscrepancyGLAccount,
				AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
				AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
				AccountingConfigurationRegistry.Instance.APControlAccount,
				AccountingConfigurationRegistry.Instance.ARControlAccount,
				AccountingConfigurationRegistry.Instance.CFXAccount,
				AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
				AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount,
				AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount,
				AccountingConfigurationRegistry.Instance.GSTInputControlAccount,
				AccountingConfigurationRegistry.Instance.GSTOutputControlAccount,
				AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
				AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
				AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
				AccountingConfigurationRegistry.Instance.APJournalAccount,
				AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.ARJournalAccount,
				AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount,
				AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount,
				AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.ARDiscountAccount,
				AccountingConfigurationRegistry.Instance.APDiscountAccount,
				AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
				AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
				AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
				AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
				AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
				AccountingConfigurationRegistry.Instance.BankTransactionGLAccount,
			};

			foreach (IRegistryItem item in registryItemsToCheck)
			{
				if (item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) is Guid && ((Guid)item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == glHeaderPK))
				{
					return true;
				}
			}

			return false;
		}

		#region Dissection Configuration

		IRegistryItem[] NotAllowedForSeparateNumberingRegistryItems => new IRegistryItem[]
		{
			AccountingConfigurationRegistry.Instance.AccruedCostControlAccount,
			AccountingConfigurationRegistry.Instance.AccruedRevenueControlAccount,
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount,
			AccountingConfigurationRegistry.Instance.ARDiscountAccount,
			AccountingConfigurationRegistry.Instance.APDiscountAccount,
			AccountingConfigurationRegistry.Instance.ARJournalAccount,
			AccountingConfigurationRegistry.Instance.APJournalAccount,
			AccountingConfigurationRegistry.Instance.APMatchingSessionControlAccount,
			AccountingConfigurationRegistry.Instance.ARMatchingSessionControlAccount,
			AccountingConfigurationRegistry.Instance.BadDebtWriteOffAccount,
			AccountingConfigurationRegistry.Instance.ClearingJournalClearingAccount,
			AccountingConfigurationRegistry.Instance.PeriodApportionmentARClearingAccount,
			AccountingConfigurationRegistry.Instance.PeriodApportionmentAPClearingAccount,
			AccountingConfigurationRegistry.Instance.OverpaymentsAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.RealizedExchangeLossAccount,
		};

		public bool IsNotAllowedForSeparateNumbering(ZGuid glHeaderPK)
		{
			return CheckControlAccounts(glHeaderPK, NotAllowedForSeparateNumberingRegistryItems);
		}

		IRegistryItem[] NotAllowedForDissectionAttributesRegistryItems => new IRegistryItem[]
		{
			AccountingConfigurationRegistry.Instance.CFXAccount,
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount,
			AccountingConfigurationRegistry.Instance.APControlAccountAdjustment,
			AccountingConfigurationRegistry.Instance.ARControlAccountAdjustment,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount,
			AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount,
			AccountingConfigurationRegistry.Instance.FinanceChargesAccount,
			AccountingConfigurationRegistry.Instance.ForeignCurrencyGLBalanceAdjustmentAccount,
			AccountingConfigurationRegistry.Instance.GLJournalClearingAccount,
			AccountingConfigurationRegistry.Instance.PLAppropriationAccount,
			AccountingConfigurationRegistry.Instance.AdvancedTurnoverTaxReturnAccount,
			AccountingConfigurationRegistry.Instance.SpecialVATPrepaymentAccount,
			AccountingConfigurationRegistry.Instance.WHTInputControlAccount,
			AccountingConfigurationRegistry.Instance.WHTOutputControlAccount,
		};

		public bool IsNotAllowedForDissectionAttributes(ZGuid glHeaderPK)
		{
			return CheckControlAccounts(glHeaderPK, NotAllowedForDissectionAttributesRegistryItems);
		}

		bool CheckControlAccounts(ZGuid glHeaderPK, IRegistryItem[] registryItems)
		{
			if (glHeaderPK.IsEmpty || registryItems == null || registryItems.Length == 0)
			{
				return false;
			}

			foreach (var item in registryItems)
			{
				if (item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) is Guid && ((Guid)item.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty) == glHeaderPK))
				{
					return true;
				}
			}

			return false;
		}

		public bool IsNotAllowedForSeparateNumberingRegistry(IRegistryItem registryItem)
		{
			return NotAllowedForSeparateNumberingRegistryItems.Select(x => x.Name).Contains(registryItem.Name);
		}

		public bool IsNotAllowedForDissectionAttributesRegistry(IRegistryItem registryItem)
		{
			return NotAllowedForDissectionAttributesRegistryItems.Select(x => x.Name).Contains(registryItem.Name);
		}

		#endregion

		public int InvoiceNumberLength(Guid companyPk)
		{
			var customisationCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
			return customisationCollection != null ? (int)customisationCollection.TotalLength : 8;
		}

		public int InvoiceNumberInNumericLength(Guid companyPk)
		{
			var customisationCollection = AccountingConfigurationRegistry.Instance.TransactionsNumberSequenceCustomisation.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);
			return customisationCollection != null ? (int)customisationCollection.TotalLengthInNumeric : 8;
		}

		public bool HasSubAccounts(BusinessObject headerOrLine)
		{
			var supportMultipleSubAccounts = headerOrLine as ISupportMultiSubAccounts;
			return supportMultipleSubAccounts?.SubAccounts?.SubAccountElements?.Any(x => x.SubAccountParentId != ZGuid.Empty) ?? false;
		}

		public string GetSubAccountsInfo(BusinessObject headerOrLine)
		{
			var supportMultipleSubAccounts = headerOrLine as ISupportMultiSubAccounts;
			var stringBuilder = new StringBuilder();
			supportMultipleSubAccounts?.SubAccounts?.SubAccountElements?.ForEach(x => stringBuilder.Append(FormattableString.Invariant($"{x.SubAccountTypeDisplayCode}:{x.SubAccountParentId} ")));
			return stringBuilder.ToString().Trim(' ');
		}

		public Guid APControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.APControlAccount.Value;
			}
		}

		public Guid ARControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ARControlAccount.Value;
			}
		}

		public Guid GSTInputControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.GSTInputControlAccount.Value;
			}
		}

		public Guid GSTOutputControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.GSTOutputControlAccount.Value;
			}
		}

		public Guid PendingGSTInputControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.PendingGSTInputControlAccount.Value;
			}
		}

		public Guid PendingGSTOutputControlAccount
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.PendingGSTOutputControlAccount.Value;
			}
		}

		public bool EnableBulkDisbursementJobsClosure => AccountingConfigurationRegistry.Instance.EnableBulkDisbursementJobsClosure.Value;

		public bool GetIsExistDsbBatchByCharge(ZGuid chargePK)
		{
			return DSBBatchHelper.GetIsExistDsbBatchByCharge(chargePK);
		}

		public DateTime GetGenerateJournalEntriesStartDate(Guid companyPK) => AccountingConfigurationRegistry.Instance.GenerateJournalEntriesStartDate.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);

		public bool IsEPaymentFunctionalityEnabledForAnyProvider(Guid companyPK)
		{
			if (companyPK != Guid.Empty)
			{
				return AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).IsEPaymentEnabledForAnyProvider;
			}
			else
			{
				throw new ArgumentException("companyPK should not be empty. Enable E-Payment Functionality registry can be configured at company level only.");
			}
		}

		public bool IsEPaymentFunctionalityEnabledForOFX(Guid companyPK)
		{
			if (companyPK != Guid.Empty)
			{
				return AccountingMasterFilesRegistry.Instance.EnableEPaymentFunctionality.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty).IsOFXEPaymentEnabled;
			}
			else
			{
				throw new ArgumentException("companyPK should not be empty. Enable E-Payment Functionality registry can be configured at company level only.");
			}
		}

		public bool IsIncludeCashAdvanceRequestsInCreditControlledDocumentEvaluationEnabled(Guid companyPK)
		{
			return AccountingConfigurationRegistry.Instance.IncludeCashAdvanceRequestsInCreditControlledDocumentEvaluation.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty);
		}

		public bool IsIncludedInElectronicProcessingChargeConfiguration(ZDateTime jobOpenDate, string jobTypeCode)
		{
			return AccountingConfigurationRegistry.Instance.ElectronicProcessingChargeConfiguration.Value.Cast<ElectronicProcessingChargeConfiguration>().Any(x => x.JobType == jobTypeCode && jobOpenDate >= x.StartDate && (x.EndDate.IsEmpty || jobOpenDate < x.EndDate.AddDays(1)));
		}

		public string RSADecrypt(string cipherText)
		{
			return new RSASecurityProvider().Decryptor.Decrypt(cipherText);
		}

		public decimal GetGSTVATConversionExchangeRate(ZGuid transactionHeaderPK, ZGuid companyPK)
		{
			var factory = new BusinessObjectFactory();
			var transactionHeader = factory.Load<TransactionHeader>(transactionHeaderPK);

			if (Env.CurrentCompany.Country.Currency.Code == transactionHeader.AH_RX_NKTransactionCurrency)
			{
				return 1;
			}

			var loginCompanyCountryCurrencyExchangeRate = ZDecimal.Zero;

			if (transactionHeader.AH_Ledger == LedgerTypes.CashBook)
			{
				loginCompanyCountryCurrencyExchangeRate = ExchangeRateCalculator.GetRate(
					Env.CurrentCompany.Country.Currency.Code,
					ExchangeRateType.Sell,
					transactionHeader.AH_SystemCreateTimeUtc.ToDateTime()
					);
			}
			else
			{
				var invoiceBase = (InvoicingBase)transactionHeader;
				loginCompanyCountryCurrencyExchangeRate = ExchangeRateCalculator.GetOverrideExchangeRate(
					Env.CurrentCompany.Country.Currency.Code,
					Env.CurrentCompany.LocalCurrency.PK == Env.CurrentCompany.Country.Currency.PK,
					companyPK,
					ExchangeRateType.Sell,
					ExchangeRateValidLedgerEnum.AR,
					transactionHeader.AH_InvoiceDate,
					transactionHeader.AH_PostDate,
					invoiceBase.InvoiceTaxDate);
			}

			return loginCompanyCountryCurrencyExchangeRate != 0
				? Utilities.Round(transactionHeader.AH_ExchangeRate / loginCompanyCountryCurrencyExchangeRate, 6)
				: 0;
		}

		public string DefaultPaymentType
		{
			get { return AccountingConfigurationRegistry.Instance.DefaultPaymentType.Value; }
		}

		#endregion

		class RegistryWrapper : IRegistry
		{
			#region IRegistry Members

			public IRegistryItem CreditorCreditLimitNotifyGroup
			{
				get { return AccountingConfigurationRegistry.Instance.CreditorCreditLimitNotifyGroup; }
			}

			public IRegistryItem ARCreditControlledDocumentsApprovalNotifyGroup
			{
				get { return AccountingConfigurationRegistry.Instance.ARCreditControlledDocumentsApprovalNotifyGroup; }
			}

			public IRegistryItem IsNettingSystem
			{
				get { return AccountingConfigurationRegistry.Instance.IsNettingSystem; }
			}

			public IRegistryItem JobDeactivationConfiguration => AccountingConfigurationRegistry.Instance.JobDeactivationConfiguration;

			public IRegistryItem EnablePayablesInvoiceProcessingPortal
			{
				get { return AccountingConfigurationRegistry.Instance.EnablePayablesInvoiceProcessingPortal; }
			}

			public IRegistryItem EnableImportingUniversalTransactionIntoPayableDraftInvoices
			{
				get { return AccountingConfigurationRegistry.Instance.EnableImportingUniversalTransactionIntoPayableDraftInvoices; }
			}
#if DEBUG

			public IRegistryItem JobBranchDefaultOrderRule
			{
				get { return AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule; }
			}

			public IRegistryItem UseWebServiceForCreditLimit
			{
				get { return AccountingConfigurationRegistry.Instance.UseWebServiceForCreditLimit; }
			}

			public IRegistryItem UseWebServiceForOutstandingBalance
			{
				get { return AccountingConfigurationRegistry.Instance.UseWebServiceForOutstandingBalance; }
			}

			public IRegistryItem UseWebServiceForUnpostedRevenue
			{
				get { return AccountingConfigurationRegistry.Instance.UseWebServiceForUnpostedRevenue; }
			}

			public IRegistryItem CreditLimitCheckWebServiceUrl
			{
				get { return AccountingConfigurationRegistry.Instance.CreditLimitCheckWebServiceUrl; }
			}

			public IRegistryItem CreateWIPOrAccrualWhenNoInvoicesPosted_ForTestOnly
			{
				get { return AccountingConfigurationRegistry.Instance.CreateWIPOrAccrualWhenNoInvoicesPosted; }
			}

			public IRegistryItem AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob_ForTestOnly
			{
				get { return AccountingConfigurationRegistry.Instance.AddJobInvoicingRecordAtSavingOrEditingOfOperationsJob; }
			}

			public IRegistryItem JobInvoicingCFXEnabled
			{
				get { return AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled; }
			}

			public IRegistryItem ARSuspenseControlAccount_ForTestOnly => AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount;

			public IRegistryItem APSuspenseControlAccount_ForTestOnly => AccountingConfigurationRegistry.Instance.APSuspenseControlAccount;

			public IRegistryItem JobRevenueJournalControlAccount_ForTestOnly => AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount;

			public void SetInvoicePostingExchangeRateOptionAP(ZGuid companyPK, string option)
				=> AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAP.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, option);

			public void SetInvoicePostingExchangeRateOptionAR(ZGuid companyPK, string option)
				=> AccountingConfigurationRegistry.Instance.InvoicePostingExchangeRateOptionAR.SetValue(companyPK.ToGuid(), Guid.Empty, Guid.Empty, option);

#endif

			public IRegistryItem CreditLimitCheckTemporaryCreditLimitIncreaseThreshold
			{
				get { return AccountingConfigurationRegistry.Instance.CreditLimitCheckTemporaryCreditLimitIncreaseThreshold; }
			}

			public IRegistryItem NettingControlAccount => AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount;

			public IRegistryItem NettingParticipationStartDate => AccountingConfigurationRegistry.Instance.NettingStartDate;

			public IRegistryItem NettingSystemOrganisation => AccountingConfigurationRegistry.Instance.NettingSystemOrg;

			public IRegistryItem ProfitShareChargeCode
			{
				get { return AccountingConfigurationRegistry.Instance.ProfitShareChargeCode; }
			}

			public string GetInvoicePostingExchangeRateOptionAR(ZGuid companyPK, bool isLocalInvoiceCurrencyType) => AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AR, isLocalInvoiceCurrencyType, companyPK);

			public string GetInvoicePostingExchangeRateOptionAP(ZGuid companyPK, bool isLocalInvoiceCurrencyType) => AccountingConfigurationRegistry.Instance.GetInvoicePostingExchangeRateOption(ExchangeRateValidLedgerEnum.AP, isLocalInvoiceCurrencyType, companyPK);

			public IRegistryItem ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation => AccountingConfigurationRegistry.Instance.ExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation;

			public IRegistryItem GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation => AccountingConfigurationRegistry.Instance.GlobalExcludeOpenClaimsAmountsFromOverdueCreditCheckingCalculation;

			public ICodeDescriptionPairList NoteGLAccountsStatisticalUnitsofMeasurement(Guid companyPk)
			{
				return AccountingConfigurationRegistry.Instance.NoteGLAccountsStatisticalUnitsofMeasurement.GetFallBackValueAtAllLevels(companyPk, Guid.Empty, Guid.Empty);
			}

			public ICodeDescriptionPairList TaxMessageGroupsManagement(Guid companyPK)
			{
				return AccountingMasterFilesRegistry.Instance.TaxMessageGroupsManagement.GetFallBackValueAtAllLevels(companyPK, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList();
			}

			public IRegistryItemInternals CurrencyAdjustmentExchangeGainAccount
				=> AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeGainAccount;

			public IRegistryItemInternals CurrencyAdjustmentExchangeLossAccount
				=> AccountingConfigurationRegistry.Instance.CurrencyAdjustmentExchangeLossAccount;

			public IRegistryItem EnableElectronicProcessingChargeFunctionality => AccountingConfigurationRegistry.Instance.EnableElectronicProcessingChargeFunctionality;

			#endregion
		}
	}
}

