using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(JobProfitLossConsolController))]
	public class JobProfitLossConsolControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.JobProfitLossConsol;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			ForwardingConsol result = Factory.New<ForwardingConsol>();
			Factory.Save();
			return result;
		}
	}
}
