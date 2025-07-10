#if DEBUG
using System.ComponentModel;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KDesignerActionMethodItemTests : TestCase
	{
		#region Invoke

		public void TestInvoke_WithMethodName()
		{
			KDesignerActionMethodItem methodItem = new KDesignerActionMethodItem(ActionList, "RunVerbOnActionList", "");
			methodItem.Invoke();
			AssertEquals(true, ActionList.RunVerbOnActionListCalled);
		}

		public void TestInvoke_WithDelegate()
		{
			bool methodInvoked = false;
			KDesignerActionMethodItem verb = new KDesignerActionMethodItem(ActionList, delegate
			{ methodInvoked = true; }, "");
			verb.Invoke();
			AssertEquals(true, methodInvoked);
		}

		#endregion

		#region Test Classes

		class TestDesignerActionList : KDesignerActionList
		{
			public TestDesignerActionList(IComponent component) : base(component)
			{
			}

			public bool RunVerbOnActionListCalled;
			protected void RunVerbOnActionList()
			{ RunVerbOnActionListCalled = true; }
		}

		#endregion

		#region Implementation

		TestDesignerActionList ActionList
		{ get { return actionList ?? (actionList = new TestDesignerActionList(new Component())); } }
		TestDesignerActionList actionList;

		#endregion
	}
}
#endif
