using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public abstract class UpdateJobUserControlBaseTest : TestCaseWithFactory
	{
		public void TestUpdateJobUserControlShown()
		{
			using (var form = new ZForm())
			{
				var control = GetUpdateJobUserControl();
				form.Controls.Add(control);

				AssertNoExceptionThrown(() => form.Show());

				var childControl = control.Controls.Find(ChildControlName, true);
				AssertNotNull(childControl);
			}
		}

		protected abstract ZUserControl GetUpdateJobUserControl();

		protected abstract string ChildControlName { get; }
	}
}
