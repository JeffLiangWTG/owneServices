namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class EXDJobComInvoiceGroupHeaderValidationTest : ExportJobComInvoiceGroupHeaderValidationTest
	{
		#region Implementation

		protected override Customs.Business.JobComInvoiceGroupHeaderValidation GetNewValidationProvider(JobComInvoiceGroupHeader groupHeader)
		{
			return new EXDJobComInvoiceGroupHeaderValidation(groupHeader);
		}

		#endregion

	}
}
