using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLConsolidations
{
	public class ConsolidationBatchExportRow : AutoConsolidationBatchExportRow
	{
		public ConsolidationBatchExportRow(BusinessObjectFactory factory, ConsolidationBatchDetailsRow detailsRow)
			: base(factory)
		{
			Period = detailsRow.Period;
			GLAccount = detailsRow.GLAccount;
			BranchCode = detailsRow.BranchCode;
			DepartmentCode = detailsRow.DepartmentCode;
			ConsolidationGroup = detailsRow.ConsolidationGroupCode;
			PostingCompany = detailsRow.PostingCompanyCode;
			TransactionOrganization = detailsRow.TransactionOrganisationCode;
			PostingCompanyCurrency = detailsRow.PostingCompanyCurrency;
			AmountInPostingCompanyCurrency = detailsRow.AmountInPostingCompanyCurrency;
			TransactionCurrency = detailsRow.TransactionCurrency;
			AmountInTransactionCurrency = detailsRow.AmountInTransactionCurrency;
		}
	}
}