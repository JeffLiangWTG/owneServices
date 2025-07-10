using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class AHECCFindBoxTest : TestCaseWithFactory
	{
		public void TestShowEditOrViewForm()
		{
			using (var findBox = new AHECCFindBox())
			{
				((IFindBoxUserControl)findBox).ShowEditOrViewForm();
				AssertEquals("Editing of AHECC information is not supported", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
	}
}
