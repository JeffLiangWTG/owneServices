using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.DbRestoreKey.Testing
{
	[TestedType(typeof(DbRestoreKeyForm))]
	public class DbRestoreKeyFormTest : ZFormBasherTest
	{
		public void TestDoesNotAskForSaveWhenCloseButtonIsClicked()
		{
			var generator = new DbRestoreKeyGenerator(Factory);
			using (var form = new DbRestoreKeyFormForTest(generator))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.GetCloseButtonForTesting().PerformClick();
				AssertEquals("Should not ask for save", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			using (var form = new DbRestoreKeyFormForTest(generator))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				generator.HasChanges = true;
				form.GetCloseButtonForTesting().PerformClick();
				AssertEquals("Should not ask for save", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation
		class DbRestoreKeyFormForTest : DbRestoreKeyForm
		{
			public DbRestoreKeyFormForTest(DbRestoreKeyGenerator dbRestoreKeyGenerator) : base(dbRestoreKeyGenerator)
			{
			}

			internal ZArchitecture.GUI.ZButton GetCloseButtonForTesting()
			{
				return CloseButton;
			}
		}

		protected override Form GetFormToBashCore()
		{
			DbRestoreKeyGenerator keyGenerator = new DbRestoreKeyGenerator(Factory);
			return new DbRestoreKeyForm(keyGenerator);
		}
		#endregion
	}
}
