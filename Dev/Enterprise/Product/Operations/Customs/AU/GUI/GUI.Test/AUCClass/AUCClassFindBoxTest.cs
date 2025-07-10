using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AUCClassFindBoxTest : TestCaseWithFactory
	{
		public void TestShowEditOrViewForm()
		{
			using (var findBox = new AUCClassFindBox())
			{
				((IFindBoxUserControl)findBox).ShowEditOrViewForm();
				AssertEquals("Editing of tariff information is not supported", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
