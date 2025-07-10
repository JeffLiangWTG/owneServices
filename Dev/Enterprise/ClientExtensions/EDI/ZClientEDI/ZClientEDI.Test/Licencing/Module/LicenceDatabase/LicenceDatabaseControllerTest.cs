using CargoWise.EntityFramework;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Module.Testing
{
	[TestedType(typeof(LicenceDatabaseController))]
	public class LicenceDatabaseControllerTest : ZControllerBasherTest
	{
		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var org = factory.New<EDIOrgHeader>();
			org.OH_FullName = "Name";
			org.MainAddress.OA_Address1 = "Address";
			org.OH_RL_NKClosestPort = "USLAX";
			factory.Save();
			var licence = factory.NewWithValidTestData<LicenceEnterprise>();
			factory.Save();
			return licence;
		}

		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.LicenceEnterprise;
		}
	}
}
