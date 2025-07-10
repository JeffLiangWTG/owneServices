using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Security;
using Enterprise.Security.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public class BusinessObjectActivatorTest : TestCaseWithFactory
	{
		public void TestCheckpointDisallowed()
		{
			var security = new SecurityForTest(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
			var activator = new BusinessObjectActivator();
			var dummy = Factory.New<DummyCancellable>();

			var checkpoint = new SecurityCheckpoint("XXX", (NoResString)"ZZZ", null, security);
			checkpoint.IsAllowed = false;
			activator.Activate(new BusinessObject[] { dummy }, checkpoint);

			AssertEquals("You are not allowed to Activate/Deactivate in this module. Please contact your system administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestActivateDeactivate()
		{
			var activator = new BusinessObjectActivator();

			var dummy1 = Factory.New<DummyCancellable>();
			var dummy2 = Factory.New<DummyCancellable>();
			var dummy3 = Factory.New<DummyCancellable>();
			var dummy4 = Factory.New<DummyCancellable>();
			Factory.Save();

			dummy1.CanCancelMessage = null;
			dummy2.CanCancelMessage = null;
			dummy3.CanCancelMessage = null;
			dummy4.CanCancelMessage = null;

			dummy1.IsCancelled = true;
			dummy2.IsCancelled = true;

			Assert(dummy1.IsCancelled);
			Assert(dummy2.IsCancelled);
			Assert(!dummy3.IsCancelled);
			Assert(!dummy4.IsCancelled);

			activator.Activate(new BusinessObject[] { dummy2, dummy3 }, EnvProxy.Instance.Security.None);

			Assert(dummy1.IsCancelled);
			Assert(!dummy2.IsCancelled);
			Assert(!dummy3.IsCancelled);
			Assert(!dummy4.IsCancelled);

			activator.Activate(new BusinessObject[] { dummy1, dummy2, dummy3, dummy4 }, EnvProxy.Instance.Security.None);

			Assert(!dummy1.IsCancelled);
			Assert(!dummy2.IsCancelled);
			Assert(!dummy3.IsCancelled);
			Assert(!dummy4.IsCancelled);

			activator.Deactivate(new BusinessObject[] { dummy3, dummy4 }, EnvProxy.Instance.Security.None);

			Assert(!dummy1.IsCancelled);
			Assert(!dummy2.IsCancelled);
			Assert(dummy3.IsCancelled);
			Assert(dummy4.IsCancelled);
		}

		delegate void ActivateDeactivate(BusinessObjectActivator activator);
		public void TestCannotActivate()
		{
			var dummy = Factory.New<DummyCancellable>();
			ActivateDeactivate activate = activator => { activator.Activate(new BusinessObject[] { dummy }, EnvProxy.Instance.Security.None); };
			ActivateDeactivate deactivate = activator => { activator.Deactivate(new BusinessObject[] { dummy }, EnvProxy.Instance.Security.None); };

			dummy.CanCancelMessage = "zzz";
			AssertActivateDeactivate(dummy, false, "zzz", deactivate);

			dummy.CanCancelMessage = null;
			AssertActivateDeactivate(dummy, false, "", deactivate);

			dummy.CanReactivateMessage = "123";
			AssertActivateDeactivate(dummy, true, "123", activate);

			dummy.CanReactivateMessage = null;
			AssertActivateDeactivate(dummy, true, "", activate);
		}

		void AssertActivateDeactivate(DummyCancellable dummy, bool isCancelled, string expectedMessage, ActivateDeactivate deleg)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			var activator = new BusinessObjectActivator();

			dummy.IsCancelled = isCancelled;
			deleg.Invoke(activator);
			if (!string.IsNullOrEmpty(expectedMessage))
			{
				AssertEquals("Should not change state", isCancelled, dummy.IsCancelled);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			else
			{
				AssertEquals("Should change state", !isCancelled, dummy.IsCancelled);
			}
		}
	}
}
