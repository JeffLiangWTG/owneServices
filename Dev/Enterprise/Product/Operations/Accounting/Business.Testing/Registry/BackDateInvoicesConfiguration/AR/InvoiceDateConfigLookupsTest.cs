namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class InvoiceDateConfigLookupsTest : InvoiceDateConfigurationLookupsTest
	{
		protected override IInvoiceDateConfiguration GetNewBizObj
		{
			get
			{
				return new InvoiceDateConfiguration();
			}
		}
	}
}