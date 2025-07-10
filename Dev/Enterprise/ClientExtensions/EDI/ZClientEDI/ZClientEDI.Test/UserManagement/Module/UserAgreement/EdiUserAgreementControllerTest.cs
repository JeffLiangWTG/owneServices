using CargoWise.EntityFramework;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	[TestedType(typeof(EdiUserAgreementController))]
	public class EdiUserAgreementControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.UserAgreements;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var result = Factory.NewWithValidTestData<EdiUserAgreement>();
			Factory.Save();
			return result;
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			var controller = new EdiUserAgreementController();
			Assert("MakeUrlsOnlyOpenableForCurrentCompany should be false - enable open agreement links without needing to log into a specific branch", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
