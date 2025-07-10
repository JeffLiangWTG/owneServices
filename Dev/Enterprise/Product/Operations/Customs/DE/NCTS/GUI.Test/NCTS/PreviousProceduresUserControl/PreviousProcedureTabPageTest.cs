using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	sealed class PreviousProcedureTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Previous Procedures", previousProcedureTabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(".", previousProcedureTabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = previousProcedureTabPage.CreateUserControl())
			{
				AssertType<NctsPreviousProceduresUserControl>(userControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			previousProcedureTabPage = new PreviousProcedureTabPage();
		}
		PreviousProcedureTabPage previousProcedureTabPage;
	}
}
