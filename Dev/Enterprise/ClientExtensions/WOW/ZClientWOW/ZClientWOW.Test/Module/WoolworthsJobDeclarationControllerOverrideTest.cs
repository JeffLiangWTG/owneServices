using CargoWise.EntityFramework;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.Wow
{
	[TestedType(typeof(WoolworthsJobDeclarationControllerOverride))]
	public class WoolworthsJobDeclarationControllerOverrideTest : ZControllerBasherTest
	{
		protected override Enterprise.ZArchitecture.Modules.ControllerID GetControllerID()
		{
			return ControllerIDs.Customs.JobDeclaration;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			return declaration;
		}
	}
}
