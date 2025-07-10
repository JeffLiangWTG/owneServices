using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Invoices;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.Rohlig.Bellin
{
	public class BellinInvoiceBatchFilter : FinancialInvoiceTransactionExportFilter
	{
		public BellinInvoiceBatchFilter(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider) : base(factory, filterProvider)
		{
		}
		protected internal SchemaDateTimeColumn FromToDatesColumnToFilterOnForTest => FromToDatesColumnToFilterOn;
		protected override SchemaDateTimeColumn FromToDatesColumnToFilterOn
		{
			get { return AccTransactionHeaderSchema.AH_InvoiceDate; }
		}

		protected override void AddOrganisationsSubQuery(ZDBOnlyQuery mainQuery, SchemaGuidColumn foreignKeyToMainTable, SchemaGuidColumn foreignKeyToBranch)
		{
			if (FilterProvider.Organisations.Count > 0 || !FilterProvider.AccountGroup.IsEmpty)
			{
				ZDBOnlySubQuery organisationsSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), foreignKeyToMainTable);
				if (FilterProvider.Organisations.Count > 0)
				{
					organisationsSubQuery.AddToFilter(OrgHeaderSchema.PK, FilterProvider.Organisations.GetPKs());
				}

				if (!FilterProvider.AccountGroup.IsEmpty)
				{
					ZDBOnlySubQuery companyDataSubQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgHeaderSchema.PK);
					ZDBOnlySubQuery companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), OrgCompanyDataSchema.OB_GC);
					ZDBOnlySubQuery branchSubQuery = new ZDBOnlySubQuery(typeof(GlbBranch), foreignKeyToBranch);
					branchSubQuery.AddFilterAndZSQLParameterCollection(" GB_PK = AH_GB ", null);
					companySubQuery.AddSubQuery(GlbCompanySchema.PK, GlbBranchSchema.GB_GC, branchSubQuery, JoinCondition.And);
					companyDataSubQuery.AddSubQuery(companySubQuery, JoinCondition.And);
					companyDataSubQuery.AddToFilter(OrgCompanyDataSchema.OB_OG_APCreditorGroup, FilterProvider.AccountGroup);
					organisationsSubQuery.AddSubQuery(OrgHeaderSchema.PK, OrgCompanyDataSchema.OB_OH, companyDataSubQuery, JoinCondition.And);
				}

				mainQuery.AddSubQuery(organisationsSubQuery, JoinCondition.And);
			}
		}

		#region Implementation

		#endregion

	}
}
