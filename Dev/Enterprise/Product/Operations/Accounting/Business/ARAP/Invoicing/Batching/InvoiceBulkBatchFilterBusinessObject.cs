using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	class InvoiceBulkBatchFilterBusinessObject : InvoiceBatchHeaderFilterBusinessObject
	{
		#region Filter

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.OrderBy = AccTransactionHeaderSchema.AH_OH.Name;
				return query;
			}
		}

		protected override ZQuery GetJobTypeFilter(ZString invoiceModuleType)
		{
			ZQuery query = base.GetJobTypeFilter(invoiceModuleType);

			if (!invoiceModuleType.IsEmpty)
			{
				ZDBOnlyQuery orgFilter = new ZDBOnlyQuery(typeof(AccTransactionHeader));

				var jobTypes = PeriodicInvoiceModuleDecider.GetJobTypesByInvoiceModule(invoiceModuleType);
				if (jobTypes.Count == 0)
				{
					jobTypes.Add("<NOJOB>");
				}

				string sQL = string.Format(@" 
											AH_OH in ( 
												SELECT OH_PK
												FROM dbo.OrgHeader 
												JOIN dbo.OrgCompanyData ON OH_PK = OB_OH 
												JOIN dbo.Orginvoicetype ON OB_PK = PI_OB 
												WHERE 
													PI_Module IN ('{0}')
													AND 
													OB_GC = '{1}'
											)", string.Join("', '", jobTypes.ToArray()), GlbCompany.CurrentCompany.PK);
				orgFilter.AddFilterAndZSQLParameterCollection(sQL, null);
				query.AddToFilter(orgFilter);
			}
			return query;
		}

		protected override bool ShouldAddOrganisationFilter
		{
			get { return false; }
		}

		#endregion
	}
}
