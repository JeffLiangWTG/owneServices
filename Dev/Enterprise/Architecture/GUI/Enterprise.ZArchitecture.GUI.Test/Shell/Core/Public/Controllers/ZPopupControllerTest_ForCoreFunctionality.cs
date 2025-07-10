using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Environment;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class ZPopupControllerTest_ForCoreFunctionality : TestCase
	{
		public void TestShowsOnlyOneEditForm()
		{
			IZForm shown1 = null;
			IZForm shown2 = null;

			try
			{
				Controller.ShowSingletonForm();
				shown1 = Controller.LastShownForm;
				Controller.ShowSingletonForm();
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

		public void TestShowsDifferentNewForms()
		{
			IZForm shown1 = null;
			IZForm shown2 = null;

			try
			{
				Controller.ShowNewForm();
				shown1 = Controller.LastShownForm;
				Controller.ShowNewForm();
				shown2 = Controller.LastShownForm;
				AssertNotEquals("Should be different forms", shown1, shown2);
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
			AssertNull("No form should be shown when the security check point doesnt allow it", Controller.ShowNewForm());

			Controller.SecurityCheckPoint.SetIsAllowed(true);
			using (var form = Controller.ShowNewForm())
			{
				AssertNotNull("Form should be shown when the security check point allows it", form);
			}
		}

		#region Implementation

		TestPopupController Controller
		{
			get { return controller ?? (controller = new TestPopupController()); }
		}
		TestPopupController controller;

		#endregion

		#region Test classes

		class TestPopupController : ZPopupController
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

			public TestSecurityCheckpoint SecurityCheckPoint = new TestSecurityCheckpoint();

			protected override SecurityCheckpoint CheckPointForNew
			{
				get { return SecurityCheckPoint; }
			}

			public override ModuleIdentifier ModuleID
			{
				get { return null; }
			}
		}

		static IZSecurity GetSecurity()
		{
			return new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		internal class TestSecurityCheckpoint : SecurityCheckpoint
		{
			public TestSecurityCheckpoint() : base("Code", (NoResString)"Display", null, GetSecurity()) { }

			public void SetIsAllowed(bool value)
			{
				fIsAllowed = value;
			}

			public override bool IsAllowed
			{
				get { return fIsAllowed; }
			}
			bool fIsAllowed = true;
		}

		#endregion
	}
}
