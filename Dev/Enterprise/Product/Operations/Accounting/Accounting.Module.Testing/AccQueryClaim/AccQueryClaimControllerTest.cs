using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class AccQueryClaimControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var queryClaim = (AccQueryClaim)Factory.NewWithValidTestData(GetBusinessObjectType());
			Factory.Save();
			return queryClaim;
		}
	}
}
