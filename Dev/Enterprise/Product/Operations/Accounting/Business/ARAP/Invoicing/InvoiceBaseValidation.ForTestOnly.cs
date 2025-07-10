#if DEBUG

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class InvoiceBaseValidation
	{
		public void CheckAH_TransactionNum_ForTestOnly()
		{
			CheckAH_TransactionNum();
		}

		public void CheckGSTInclusiveAmounts_ForTestOnly()
		{
			CheckGSTInclusiveAmounts();
		}

		public void ValidateConsolCostsWithoutLines_ForTestOnly()
		{
			ValidateConsolCostsWithoutLines();
		}

		public void ValidateComplianceSequenceNotNull_ForTestOnly()
		{
			ValidateComplianceSequenceNotNull();
		}
	}
}

#endif
