using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBulkOperationFilters
	{
		public ZString DefaultSupplierCostReferenceFilterValue_ForTestOnly
		{
			set { DefaultSupplierCostReferenceFilterValue = value; }
		}

		public ZGuid DefaultCreditorFilterValue_ForTestOnly
		{
			set { DefaultCreditorFilterValue = value; }
		}

		public ZGuid DefaultTaxBranchFilterValue_ForTestOnly
		{
			set { DefaultTaxBranchFilterValue = value; }
		}

		public virtual void ApplyDefaults_ForTestOnly(IEnumerable<ZString> skippedOrCategory = null)
		{
			ApplyDefaults();
		}
	}
}
