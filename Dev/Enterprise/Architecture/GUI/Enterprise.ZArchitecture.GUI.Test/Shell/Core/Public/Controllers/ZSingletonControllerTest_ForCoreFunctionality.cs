using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZSingletonControllerTest_ForCoreFunctionality : TestCase
	{
		public void TestOnlyShowsOneNewForm()
		{
			IZForm shown1 = null;
			IZForm shown2 = null;

			try
			{
				Controller.ShowNewForm();
				shown1 = Controller.LastShownForm;
				Controller.ShowNewForm();
				shown2 = Controller.LastShownForm;
				AssertEquals("Should be same form", shown1, shown2);
			}
			finally
			{
				if (shown1 != null)
				{
					shown1.Dispose();
				}

				if (shown2 != null)
				{
					shown2.Dispose();
				}
			}
		}

		public void TestSecurityCheckPoint()
		{
			Controller.SecurityCheckPoint.SetIsAllowed(false);
			AssertContains("You do not have the appropriate", Controller.SecurityCheckPoint.ErrorMessageForNotAllowed);
			AssertInnermostException("No form should be shown when the security check point doesnt allow it", typeof(SecurityAccessDeniedException), () => Controller.ShowNewForm());

			Controller.SecurityCheckPoint.SetIsAllowed(true);
			using (var form = Controller.ShowNewForm())
			{
				AssertNotNull("Form should be shown when the security check point allows it", form);
			}
		}

		#region Test Classes

		class TestSingletonController : ZSingletonController
		{
			protected override IZForm GetForm(IBusiness businessEntity)
			{
				return new ZForm();
			}

			public override ControllerID ID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override Type TypeOfTopLevelBusinessObject
			{
				get { return typeof(DummyBusinessObject); }
			}

			public ZPopupControllerTest_ForCoreFunctionality.TestSecurityCheckpoint SecurityCheckPoint =
				new ZPopupControllerTest_ForCoreFunctionality.TestSecurityCheckpoint();

			protected override SecurityCheckpoint CheckPointForNew
			{
				get { return SecurityCheckPoint; }
			}

			public override ModuleIdentifier ModuleID
			{
				get { return null; }
			}
		}

		#endregion

		#region Implementation

		TestSingletonController Controller
		{
			get
			{
				if (fController == null)
				{
					fController = new TestSingletonController();
				}
				return fController;
			}
		}
		TestSingletonController fController;

		#endregion
	}
}
