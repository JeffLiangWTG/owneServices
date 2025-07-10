using System.Windows.Forms;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	sealed class DbMaintenanceControlTest : TestCase
	{
		public void TestMessageBoxShownWhenDropButtonClicked()
		{
			var mock = new Mock<IDialogService>();
			var dbMaintenanceControl = new DBMaintenanceControl(mock.Object);
			AssertNoExceptionThrown(dbMaintenanceControl.DbDropButton_ClickForTest);
			mock.Verify(m => m.ShowMessageBox("Are you sure you want to delete this database?", "Confirm Delete", MessageBoxButtons.YesNo));
		}
	}
}
