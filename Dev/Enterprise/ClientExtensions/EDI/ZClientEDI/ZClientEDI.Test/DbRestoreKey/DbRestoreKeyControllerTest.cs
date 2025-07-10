using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DbRestoreKey.Testing
{
	[TestedType(typeof(DbRestoreKeyController))]
	public class DbRestoreKeyControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.DbRestoreKey;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			return Factory.New<DbRestoreKeyGenerator>();
		}
	}
}
