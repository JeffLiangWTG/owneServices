using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.Testing
{
	public static class HelperMethodsForTests
	{
		public static void AssertWindowsAndMessagesShown(string[] expectedMessagesInShownOrder = null, string typeNameOfLastFormShownDialogForTest = null, string typeNameOfLastFormShownForTest = null)
		{
			AssertionWithHtml.CombineAssertions("AssertWindowsAndMessagesShown", () =>
			{
				if (expectedMessagesInShownOrder == null)
				{
					expectedMessagesInShownOrder = Array.Empty<string>();
				}

				Assertion.AssertArrayEqualsByElements("Messages", expectedMessagesInShownOrder.Reverse().ToArray(), UnitTestUserNotification.Instance.PreviousMessages.Where(x => x.Text != null).Select(x => x.Text).ToArray());
				UnitTestUserNotification.Instance.ClearMessages();

				Assertion.AssertEquals("LastFormShownDialogForTest", typeNameOfLastFormShownDialogForTest, ZFormModaliser.LastFormShownDialogForTest?.GetType().Name);
				Assertion.AssertEquals("LastFormShownForTest", typeNameOfLastFormShownForTest, ZFormModaliser.LastFormShownForTest?.GetType().Name);
			});
		}

		#region SetZFormModaliserToCatchShownDialogByType

		public static void SetZFormModaliserToCatchShownDialogByType<T>(out Func<T> getShownDialog)
		{
			Func<IBusiness> getShownDialogBusinessEntity;
			SetZFormModaliserToCatchShownDialogByType(out getShownDialog, out getShownDialogBusinessEntity);
		}

		public static void SetZFormModaliserToCatchShownDialogByType<T>(out Func<T> getShownDialog, out Func<IBusiness> getShownDialogBusinessEntity)
		{
			Func<object> getShownDialogAsObject;
			SetZFormModaliserToCatchShownDialogByType(typeof(T), out getShownDialogAsObject, out getShownDialogBusinessEntity);

			getShownDialog = () => (T)getShownDialogAsObject();
		}

		public static void SetZFormModaliserToCatchShownDialogByType(Type dialogType, out Func<object> getShownDialog)
		{
			Func<IBusiness> getShownDialogBusinessEntity;
			SetZFormModaliserToCatchShownDialogByType(dialogType, out getShownDialog, out getShownDialogBusinessEntity);
		}

		public static void SetZFormModaliserToCatchShownDialogByType(Type dialogType, out Func<object> getShownDialog, out Func<IBusiness> getShownDialogBusinessEntity)
		{
			object dialog = null;
			IBusiness dialogBusinessEntity = null;
			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs((formOrDialog) =>
			{
				if (formOrDialog.GetType() == dialogType || formOrDialog.GetType().IsSubclassOf(dialogType))
				{
					dialog = formOrDialog;
					ZForm dialogAsZForm = dialog as ZForm;
					if (dialogAsZForm != null)
					{
						dialogBusinessEntity = dialogAsZForm.BusinessEntity;
					}
				}
			});

			getShownDialog = () => dialog;
			getShownDialogBusinessEntity = () => dialogBusinessEntity;
		}

		#endregion
	}
}
