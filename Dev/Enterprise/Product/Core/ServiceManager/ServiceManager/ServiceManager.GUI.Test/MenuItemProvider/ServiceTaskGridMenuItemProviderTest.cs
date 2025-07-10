using System.IO;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ServiceManager.GUI.Testing
{
	public class ServiceTaskGridMenuItemProviderTest : TestCase
	{
		public void TestGetMenuItems_WithServiceTaskModuleAndSupportUser_ReturnsMenuItem()
		{
			// Arrange
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = ModuleIDs.StmServiceTask;

				// Act
				var result = menuItemProvider.GetMenuItems(moduleMock);

				// Assert
				AssertEquals(1, result.Count());
			}
		}

		public void TestGetMenuItems_WithStmServiceTaskModuleAndSupportUser_ReturnsMenuItem()
		{
			// Arrange
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = ModuleIDs.StmServiceTask;

				// Act
				var result = menuItemProvider.GetMenuItems(moduleMock);

				// Assert
				AssertEquals(1, result.Count());
			}
		}

		public void TestGetMenuItems_WithServiceTaskModuleAndNotSupportUser_ReturnsEmpty()
		{
			// Arrange
			using (EnvProxy.Instance.SetTemporaryUserContext(User.UnKnownUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = ModuleIDs.StmServiceTask;

				// Act
				var result = menuItemProvider.GetMenuItems(moduleMock);

				// Assert
				AssertEquals(0, result.Count());
			}
		}

		public void TestGetMenuItems_WithNonServiceTaskModuleAndSupportUser_ReturnsEmpty()
		{
			// Arrange
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = DummyModuleIDs.Dummy;

				// Act
				var result = menuItemProvider.GetMenuItems(moduleMock);

				// Assert
				AssertEquals(0, result.Count());
			}
		}

		public void TestMenuItemClick_WithDialogOk_ExportsServiceTasks()
		{
			// Arrange
			fileDialogMock
				.Setup(o => o.ShowDialog(null))
				.Returns(DialogResult.OK);

			fileDialogMock.Setup(o => o.OpenFile())
				.Returns(Stream.Null);

			fileDialogMock.SetupGet(o => o.UnmappedFileName)
				.Returns("test.csv");

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = ModuleIDs.StmServiceTask;
				var menuItem = menuItemProvider.GetMenuItems(moduleMock).First();

				// Act
				menuItem.PerformClick();

				// Assert
				fileDialogMock.Verify(o => o.ShowDialog(null), Times.Once);
				fileDialogMock.Verify(o => o.OpenFile(), Times.Once);

				serviceTaskCsvExporterMock.Verify(o => o.WriteTo(It.IsAny<Stream>()), Times.AtLeastOnce);

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, "The list of service tasks was saved as test.csv");
			}
		}

		public void TestMenuItemClick_WithDialogCancel_DoesNotExportsServiceTasks()
		{
			// Arrange
			fileDialogMock
				.Setup(o => o.ShowDialog(null))
				.Returns(DialogResult.Cancel);

			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			using (var moduleMock = new DummyFilterGridModule())
			{
				moduleMock.IDOverride = ModuleIDs.StmServiceTask;
				var menuItem = menuItemProvider.GetMenuItems(moduleMock).First();

				// Act
				menuItem.PerformClick();

				// Assert
				fileDialogMock.Verify(o => o.ShowDialog(null), Times.Once);
				fileDialogMock.Verify(o => o.OpenFile(), Times.Never);

				serviceTaskCsvExporterMock.Verify(o => o.WriteTo(It.IsAny<Stream>()), Times.Never);

				AssertEquals(UnitTestUserNotification.Instance.LastMessage.Text, null);
			}
		}

		public void TestCreateSaveDialog_EnsurePropertiesAreSet()
		{
			// Arrange, Act
			using var dialog = (ZSaveFileDialog)ServiceTaskGridMenuItemProvider.CreateSaveDialog();

			// Assert
			AssertEquals(dialog.UnmappedFileName, "ServiceTasks");
			AssertEquals(dialog.DefaultExt, "csv");
			AssertEquals(dialog.Filter, "*.csv|*.csv");
			Assert(dialog.AddExtension);
			Assert(dialog.OverwritePrompt);
		}

		protected override void SetUp()
		{
			fileDialogMock = new Mock<IFileDialog>();
			serviceTaskCsvExporterMock = new Mock<IServiceTaskCsvExporter>();
			menuItemProvider = new ServiceTaskGridMenuItemProvider(serviceTaskCsvExporterMock.Object, () => fileDialogMock.Object);
		}

		Mock<IFileDialog> fileDialogMock;
		Mock<IServiceTaskCsvExporter> serviceTaskCsvExporterMock;
		ServiceTaskGridMenuItemProvider menuItemProvider;
	}
}
