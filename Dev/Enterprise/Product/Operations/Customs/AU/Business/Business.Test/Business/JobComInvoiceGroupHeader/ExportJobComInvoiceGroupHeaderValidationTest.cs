using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class ExportJobComInvoiceGroupHeaderValidationTest : BaseJobComInvoiceGroupHeaderValidationTest
	{
		#region Implementation

		protected override Customs.Business.JobComInvoiceGroupHeaderValidation GetNewValidationProvider(JobComInvoiceGroupHeader groupHeader)
		{
			return new ExportJobComInvoiceGroupHeaderValidation(groupHeader);
		}

		protected RefCurrency invalidCurrency;
		protected string invalidCurrencyCode = "XOF"; //in terms of AUCustoms

		protected override void SetUp()
		{
			base.SetUp();
			invalidCurrency = RefCurrency.LoadFromCurrencyCode(Factory, invalidCurrencyCode);
			if (invalidCurrency == null)
			{
				invalidCurrency = Factory.New<RefCurrency>();
				invalidCurrency.RX_Code = invalidCurrencyCode;
			}
		}

		#endregion
	}
}
