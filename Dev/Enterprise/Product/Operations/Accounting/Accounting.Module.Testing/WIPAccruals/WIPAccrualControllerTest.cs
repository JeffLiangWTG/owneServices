using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.WIPAccrual;
using Enterprise.Accounting.GUI.WipAccrual;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Accounting.Module.Testing
{
	public abstract class WIPAccrualControllerTest : ZControllerBasherTest
	{
		public void TestReverseSingleWipAndAccrual()
		{
			using (ZForm parentForm = new ZForm())
			{
				WIPAccrualForm form = null;
				bool oldReverseMultiple = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;
				bool oldReverseSingle = Env.Security.ReverseSingleWipOrAccrual.IsAllowed;

				try
				{
					BaseWIPAccrual wIPAccrual = (BaseWIPAccrual)GetBusinessObjectThatIsInTheDatabase();
					Controller.SetFormsModalTo(parentForm);

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;
					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = false;
					Controller.ShowDeleteForm(wIPAccrual);
					form = ZFormModaliser.ActiveForm as WIPAccrualForm;
					AssertNotNull("Delete form should be shown", form);
					Assert("Should not be any error popups", UnitTestUserNotification.Instance.LastMessage.WasNone);

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;
					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = true;
					Controller.ShowDeleteForm(wIPAccrual);
					form = ZFormModaliser.ActiveForm as WIPAccrualForm;
					AssertNotNull("Delete form should be shown", form);
					Assert("Should not be any error popups", UnitTestUserNotification.Instance.LastMessage.WasNone);

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = false;
					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = false;
					Controller.ShowDeleteForm(wIPAccrual);
					Assert("Error popup should be shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert("Message should relate to security", UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;
					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = true;
					Controller.ShowDeleteForm(wIPAccrual);
					form = ZFormModaliser.ActiveForm as WIPAccrualForm;
					AssertNotNull("Delete form should be shown", form);
				}
				finally
				{
					if (form != null)
					{
						form.Dispose();
					}
					Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = oldReverseMultiple;
					Env.Security.ReverseSingleWipOrAccrual.IsAllowed = oldReverseSingle;
				}
			}
		}

		public void TestReverseMultipleWipAndAccrual()
		{
			var oldReverseMultiple = Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed;
			var oldReverseSingle = Env.Security.ReverseSingleWipOrAccrual.IsAllowed;
			var wIPAccrual1 = (BaseWIPAccrual)GetBusinessObjectThatIsInTheDatabase();
			var wIPAccrual2 = (BaseWIPAccrual)GetBusinessObjectThatIsInTheDatabase();

			var multipleReversingProvider = new MultipleReversingProviderForLine();
			multipleReversingProvider.BizObjectsForReversing.Add(wIPAccrual1);
			multipleReversingProvider.BizObjectsForReversing.Add(wIPAccrual2);

			try
			{
				Controller.DeleteMultiple([multipleReversingProvider] );
				Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = true;
				Env.Security.ReverseSingleWipOrAccrual.IsAllowed = false;
				Assert("Should not be any error popups", UnitTestUserNotification.Instance.LastMessage.WasNone);
				Assert("Factory has context SkipJobHeaderRefreshParentDuringWIPAccrualReversing", multipleReversingProvider.Factory.HasContext(BusinessContext.SkipJobHeaderRefreshParentDuringWIPAccrualReversing));
			}
			finally
			{
				Env.Security.ReverseMultipleWipsAndAccruals.IsAllowed = oldReverseMultiple;
				Env.Security.ReverseSingleWipOrAccrual.IsAllowed = oldReverseSingle;
			}
		}
	}
}
