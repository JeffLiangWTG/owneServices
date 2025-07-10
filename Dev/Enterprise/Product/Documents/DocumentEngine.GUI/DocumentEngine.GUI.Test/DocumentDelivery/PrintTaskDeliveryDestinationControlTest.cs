using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.DocumentDelivery.Testing
{
	sealed class PrintTaskDeliveryDestinationControlTest : TestCaseWithFactory
	{
		public void TestTaskSettings()
		{
			PrintTask task = new PrintTask();
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				AssertNotNull(form.Control.TaskSettings);
				AssertEquals(task.TaskSettings, form.Control.TaskSettings);
			}
		}
		public void TestPrintAsDraftCheckbox()
		{
			PrintTask task = new PrintTask();

			task.TaskSettings.IsDraft = true;
			using (TestForm form = new TestForm(task.TaskSettings))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				task.TaskSettings.IsDraft = ZBool.True;
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertEquals(true, task.TaskSettings.IsDraft);
			}
		}
		#region Implementation

		internal class TestForm : ZForm
		{
			public TestForm(PrintTaskSettings entity)
				: base(entity)
			{
				Control = new PrintTaskDeliveryDestinationControl();
				this.Control.DataSourceTypeName = "Enterprise.DocumentEngine.PrintTaskSettings";
				this.DataSourceTypeName = "Enterprise.DocumentEngine.PrintTaskSettings";
				this.Controls.Add(Control);
				this.Control.SetDataBinding(entity, "");
			}

			public PrintTaskDeliveryDestinationControl Control;
		}

		#endregion
	}
}
