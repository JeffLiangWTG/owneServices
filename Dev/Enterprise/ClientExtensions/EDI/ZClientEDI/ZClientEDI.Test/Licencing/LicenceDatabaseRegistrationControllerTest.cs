using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Module;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.Module.Testing
{
	[TestedType(typeof(LicenceDatabaseRegistrationController))]
	public class LicenceDatabaseRegistrationControllerTest : ZSingletonControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ClientControllerRegistration.LicenceDatabaseRegistration;

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase() => Factory.NewWithValidTestData<LicenceDatabase>();
	}
}
