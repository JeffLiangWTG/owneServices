using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.ExitControl.Business;

namespace Enterprise.Customs.DE.ExitControl.GUI.Testing
{
	sealed class ConsignmentAdditionalInformationTabPageTest : TestCaseWithFactory
	{
		public void TestCaption()
		{
			AssertEquals("Additional Information", consignmentAdditionalInformationTabPage.Caption.Caption);
		}

		public void TestUserControlBindingMember()
		{
			AssertEquals(nameof(CusExitConsignment.AdditionalInfos), consignmentAdditionalInformationTabPage.UserControlBindingMember);
		}

		public void TestCreateUserControl()
		{
			using (var userControl = consignmentAdditionalInformationTabPage.CreateUserControl())
			{
				AssertType<ConsignmentAdditionalInformationTabUserControl>(userControl);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			consignmentAdditionalInformationTabPage = new ConsignmentAdditionalInformationTabPage();
		}
		ConsignmentAdditionalInformationTabPage consignmentAdditionalInformationTabPage;
	}
}
