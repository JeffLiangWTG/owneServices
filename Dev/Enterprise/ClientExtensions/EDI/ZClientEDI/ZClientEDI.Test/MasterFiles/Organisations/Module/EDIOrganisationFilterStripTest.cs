using System.Linq;
using Enterprise.Client.EDI.MarketingManager;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	public class EDIOrganisationFilterStripTest : TestCase
	{
		public void TestCustomFilterControlBuilder()
		{
			using (var filterStrip = new EDIOrganisationFilterStrip())
			{
				Assert(filterStrip.CustomFilterControlsBuilders.Any(x => x is LicenceUsageFilterControlBuilder));
			}
		}
	}
}