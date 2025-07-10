using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class ZFormModaliserTest : TestCase
	{
		public void TestDisabled()
		{
			try
			{
				using (var form1 = new Form())
				using (var form2 = new Form())
				using (var form3 = new Form())
				using (var colorDialog = new ColorDialog())
				using (var messageBox = new ZMessageBox("", "", MessageBoxButtons.OK, MessageBoxIcon.Hand))
				{
					ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(
						formOrDialog =>
						{
							DelegateCalls++;
							DelegateLastFormPassedIn = formOrDialog;
						});

					CheckPreShowDelegateCalled(form1, delegate
					{ ZFormModaliser.Show(form1, form2); });
					CheckPreShowDelegateCalled(form3, delegate
					{ ZFormModaliser.ShowDialogWithoutDispose(form3); });
					CheckPreShowDelegateCalled(messageBox, delegate
					{ ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox); });

					ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
					CheckPreShowDelegateNotCalled(delegate
					{ ZFormModaliser.Show(form1, form2); });
					CheckPreShowDelegateNotCalled(delegate
					{ ZFormModaliser.ShowDialogWithoutDispose(form3); });
					CheckPreShowDelegateNotCalled(delegate
					{ ZFormModaliser.ShowMessageBoxWithoutDispose(messageBox); });
				}
			}
			finally
			{
				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
			}
		}
		int DelegateCalls;
		object DelegateLastFormPassedIn;

		void CheckPreShowDelegateCalled(object formToExpectToBePassedToDelegate, MethodInvoker invokee)
		{
			var callCount = DelegateCalls;
			invokee.Invoke();
			AssertEquals("PreShow delegate should be called", callCount + 1, DelegateCalls);
			AssertEquals("The right form/dialog object was passed into the PreShow delegate", formToExpectToBePassedToDelegate, DelegateLastFormPassedIn);
		}

		void CheckPreShowDelegateNotCalled(MethodInvoker invokee)
		{
			var callCount = DelegateCalls;
			invokee.Invoke();
			AssertEquals("PreShow delegate should not be called", callCount, DelegateCalls);
		}
	}
}
