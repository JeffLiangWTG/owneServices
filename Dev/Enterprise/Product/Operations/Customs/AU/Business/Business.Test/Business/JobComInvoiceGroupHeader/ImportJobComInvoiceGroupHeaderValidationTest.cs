namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ImportJobComInvoiceGroupHeaderValidationTest : BaseJobComInvoiceGroupHeaderValidationTest
	{
		#region Implementation

		protected override Customs.Business.JobComInvoiceGroupHeaderValidation GetNewValidationProvider(JobComInvoiceGroupHeader groupHeader)
		{
			return new ImportJobComInvoiceGroupHeaderValidation(groupHeader);
		}

		#endregion
	}
}
