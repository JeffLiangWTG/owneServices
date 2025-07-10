using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class ZPopupModuleTest : TestCase
	{
		public void TestShow()
		{
			using (ZPopupModule module = new MockZPopupModule())
			{
				module.Show();
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;
				AssertNull("LastController.ParentModalForm", lastController.ParentModalForm);
				using (var form = lastController.LastShownForm)
				{
					AssertEquals("LastController.LastShownForm.GetType()", typeof(MockZForm), form.GetType());
				}
			}
		}

		public void TestShowModal()
		{
			using (ZPopupModule module = new MockZPopupModule())
			using (var parentForm = new ZForm())
			{
				module.ShowModal(parentForm);
				var lastController = ((IPopupModuleInternalsForTesting)module).LastController;
				AssertEquals("LastController.ParentModalForm", parentForm, lastController.ParentModalForm);
				using (var form = lastController.LastShownForm)
				{
					AssertEquals("LastController.LastShownForm.GetType()", typeof(MockZForm), form.GetType());
				}
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestShowModalWithNullArgument()
		{
			using (ZPopupModule module = new MockZPopupModule())
			{
				module.ShowModal(null);
			}
		}

		#region class MockZPopupModule

		class MockZPopupModule : ZPopupModule
		{
			protected override ZPopupController GetNewController()
			{
				return new MockZPopupController();
			}

			public override ModuleIdentifier ID
			{
				get { return DummyModuleIDs.Dummy; }
			}

			public override SecurityCheckpoint SecurityCheckpoint
			{
				get { return (SecurityCheckpoint)EnvProxy.Instance.Security.None; }
			}

			protected override LicenceCheckpoint LicenceCheckPointCore
			{
				get { return (LicenceCheckpoint)EnvProxy.Instance.Licence.AlwaysAllow; }
			}
		}

		#endregion

		#region class MockZSingletonController

		class MockZPopupController : ZPopupController
		{
			public override ControllerID ID
			{
				get { return DummyControllerIDs.Dummy; }
			}

			public override Type TypeOfTopLevelBusinessObject
			{
				get { return typeof(DummyBusinessObject); }
			}

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				return new MockZForm();
			}

			protected override SecurityCheckpoint CheckPointForNew
			{
				get { return (SecurityCheckpoint)EnvProxy.Instance.Security.None; }
			}

			public override ModuleIdentifier ModuleID
			{
				get { return null; }
			}
		}

		#endregion

		#region class MockZForm

		class MockZForm : ZForm
		{
		}

		#endregion
	}
}
