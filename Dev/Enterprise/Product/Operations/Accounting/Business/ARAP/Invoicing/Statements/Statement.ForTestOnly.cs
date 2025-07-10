#if DEBUG

using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class Statement
	{
		public AccountingPeriodCalculator PeriodCalculator_ForTestOnly => PeriodCalculator;

		public ZString SpecificCountryCode_ForTestOnly => SpecificCountryCode;

		public ZBool IsClientSpecific_ForTestOnly => IsClientSpecific;

		public ZString StatementMenuName_ForTestOnly => StatementMenuName;

		public void AddStandardSelectClause_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddStandardSelectClause(stringBuilder, sqlParams);
		}

		public DynamicBusinessObjectCollection GetBusinessObjCollection_ForTestOnly()
		{
			return GetBusinessObjCollection();
		}

		public void AddFilterForOneOrganisation_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForOneOrganisation(stringBuilder, sqlParams);
		}

		public void AddFilterForBatchOfOrganisations_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForBatchOfOrganisations(stringBuilder, sqlParams);
		}

		public void AddFilterForOneDebtorGroup_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForOneDebtorGroup(stringBuilder, sqlParams);
		}

		public void AddFilterForOrganisationBranch_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForOrganisationBranch(stringBuilder, sqlParams);
		}

		public void AddFilterForAccountsRelationship_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForAccountsRelationship(stringBuilder, sqlParams);
		}

		public void AddFilterForConsolidationCategory_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForConsolidationCategory(stringBuilder, sqlParams);
		}

		public void AddFilterForCreditRating_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForCreditRating(stringBuilder, sqlParams);
		}

		public void AddFilterForSalesRep_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForSalesRep(stringBuilder, sqlParams);
		}

		public void AddFilterForCustomerServiceRep_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForCustomerServiceRep(stringBuilder, sqlParams);
		}

		public void AddFilterForCreditController_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForCreditController(stringBuilder, sqlParams);
		}

		public void AddFilterForCurrency_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForCurrency(stringBuilder, sqlParams);
		}

		public void AddFilterForTransactionBranch_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForTransactionBranch(stringBuilder, sqlParams);
		}

		public void AddFilterForTransactionDepartment_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForTransactionDepartment(stringBuilder, sqlParams);
		}

		public void AddFilterForInvoiceDateOnOrBefore_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForInvoiceDateOnOrBefore(stringBuilder, sqlParams);
		}

		public void AddFilterForPostDateOnOrBefore_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForPostDateOnOrBefore(stringBuilder, sqlParams);
		}

		public void AddFilterForDueDatesOnOrBefore_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForDueDatesOnOrBefore(stringBuilder, sqlParams);
		}

		public void AddFilterForOutStandingAmount_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForOutStandingAmount(stringBuilder, sqlParams);
		}

		public void AddFilterForDisbursementInvoicesOnly_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddFilterForDisbursementInvoicesOnly(stringBuilder, sqlParams);
		}

		public void AddGroupByStatement_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddGroupByStatement(stringBuilder, sqlParams);
		}

		public void AddHavingFilter_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddHavingFilter(stringBuilder, sqlParams);
		}

		public void AddOrderByStatement_ForTestOnly(ZStringBuilder stringBuilder, ZSqlParameterCollection sqlParams)
		{
			AddOrderByStatement(stringBuilder, sqlParams);
		}

		public PrintTask GetPrintTaskForTest_ForTestOnly()
		{
			return GetPrintTaskForTest();
		}

		public ZDateTime EndOfPeriod_ForTestOnly => EndOfPeriod;

		public string OrganisationHeader_ForTestOnly => OrganisationHeader;

		public string OrganisationCompanyData_ForTestOnly => OrganisationCompanyData;

		public ZString SalesRep_Code_ForTestOnly => SalesRep_Code;

		public ZString CustomerServiceRep_Code_ForTestOnly => CustomerServiceRep_Code;

		public ZString CreditController_Code_ForTestOnly => CreditController_Code;

		public string StandardStatementAndCollectionLetterTemplateName_ForTestOnly => StandardStatementAndCollectionLetterTemplateName;

		public bool UseNewPrintStreaming_ForTestOnly => UseNewPrintStreaming;

		public IEnumerable<DocumentPack> GetPacksIEnum_ForTestOnly()
		{
			return GetPacksIEnum();
		}

		public void SetStatementDisplayDateForStatement_ForTestOnly(PrintStatement statementBizObj)
		{
			SetStatementDisplayDateForStatement(statementBizObj);
		}

		public ZGuid MenuPKForStatementOfAccount_ForTestOnly => MenuPKForStatementOfAccount;

		public bool ShouldAttachInvoices_ForTestOnly(PrintStatement statementBizObj)
		{
			return ShouldAttachInvoices(statementBizObj);
		}

		public AccountFeeInvoiceCreator accountFeeInvoiceCreator_ForTestOnly
		{
			get { return accountFeeInvoiceCreator; }
			set { accountFeeInvoiceCreator = value; }
		}
	}
}

#endif
