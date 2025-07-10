using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	[TestedType(typeof(IssueManagerController))]
	public class IssueManagerControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return Modules.ClientControllerRegistration.IssueManager;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			EdiHelpErrorLog result = Factory.New<EdiHelpErrorLog>();
			Factory.Save();
			return result;
		}

		public void TestMakeUrlsOnlyOpenableForCurrentCompany()
		{
			IssueManagerController controller = new IssueManagerController();
			Assert("MakeUrlsOnlyOpenableForCurrentCompany should be false - enable open issue links without needing to log into a specific branch", !controller.MakeUrlsOnlyOpenableForCurrentCompany);
		}
	}
}
