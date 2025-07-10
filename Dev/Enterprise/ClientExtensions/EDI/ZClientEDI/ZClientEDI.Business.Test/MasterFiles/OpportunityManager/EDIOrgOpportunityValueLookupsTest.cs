using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	public class EDIOrgOpportunityValueLookupsTest : OrgOpportunityValueLookupsTest
	{
		protected override int ExpectedNumberOfValueItems
		{
			get { return 9; }
		}
	}
}