using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	class ZGuidFindBoxWithSelectedEventTest : ZGuidFindBoxTest
	{
		public void TestZGuidFindBoxWithSelectedEventWithOutCollection()
		{
			DeleteAllDummies();
			CreateDummies();
			Dummy = Factory.New<FindBoxDummyBusinessObject>();
			Dummy.Dummies.Add(Dummy1);
			Dummy.Dummies.Add(Dummy2);
			using (var testForm = new ZChildForm())
			{
				IsBindingToDetail = true;
				CreateControls(testForm, "");
				testForm.Show();
				ZGuidFindBox.CodeBox.Focus();
				ZGuidFindBox.SelectFromPopupFormWithoutDisplaying();
				AssertEquals("Should contain 2 Dummies", 2, ZGuidFindBox.List.Count);
				AssertSame("Lists should be same", ZGuidFindBox.List, ((ZGuidFindBoxWithSelectedEvent)ZGuidFindBox).ModuleForTest.ModuleDecisionProvider.List);
			}
		}

		protected override ZFindBoxUserControl NewFindBoxTester
		{
			get
			{
				return new ZGuidFindBoxWithSelectedEvent() { ModuleID = DummyModuleIDs.Dummy };
			}
		}
	}
}
