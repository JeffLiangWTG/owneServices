namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class BackDateInvoiceDateConfigReadOnlyTest : InvoiceDateConfigurationReadOnlyTest
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