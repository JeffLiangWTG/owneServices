using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	public class JobInvoiceDescriptionLookupsTest : JobConfigurationSelectorLookupsTest
	{
		protected override IJobConfigurationSelector GetNewBizObj
		{
			get
			{
				return new JobInvoiceDescription();
			}
		}
	}
}