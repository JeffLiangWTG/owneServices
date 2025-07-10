using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IE.ExitControl.Business;

namespace Enterprise.Customs.IE.ExitControl.GUI.Testing
{
	sealed class ConsignmentAuthorisationsTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Authorizations", consignmentAuthorisationsTabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(nameof(CusExitConsignment.CusAuthorizationUsages), consignmentAuthorisationsTabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = consignmentAuthorisationsTabPage.CreateUserControl())
			{
				AssertType<ConsignmentAuthorisationsTabUserControl>(userControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			consignmentAuthorisationsTabPage = new ConsignmentAuthorisationsTabPage();
		}

		ConsignmentAuthorisationsTabPage consignmentAuthorisationsTabPage;
	}
}
