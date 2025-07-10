using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Startup.Tools.Testing
{
	[TestedType(typeof(DialogService))]
	sealed class DialogServiceTest : TestCase
	{
		public void TestSelectFolder_WhenDriveMappingFails()
		{
			// Arrange
			var expectedSelectedPath = "C:\\TestUser";
			var dialogMock = new Mock<IZFolderBrowserDialog>();
			dialogMock.Setup(x => x.UnmappedSelectedPath).Returns(expectedSelectedPath);
			dialogMock.Setup(x => x.MappedSelectedPath).Returns(null as string);
			dialogMock.Setup(x => x.IsNeedingToUseEnterpriseChannel).Returns(true);
			dialogMock.Setup(x => x.ShowDialog(It.IsAny<IWin32Window>(), It.IsAny<bool>())).Returns(DialogResult.OK);

			// Partial Mock DialogService to use mocked IZFolderBrowserDialog
			var parentFormMock = new Mock<Form>();
			var dialogService = new Mock<DialogService>(parentFormMock.Object);
			dialogService.Setup(x => x.CreateSelectFolderDialog()).Returns(dialogMock.Object);
			dialogService.Setup(x => x.CheckFolderAccess(It.IsAny<string>())).Returns(true);

			// Act
			var userSelectedPath = dialogService.Object.SelectFolder("C:\\DefaultPath", "Please select a folder for snapshots");

			// Assert ShowDialog was called while canceledIfPathCannotAccess passed as false
			dialogMock.Verify(x => x.ShowDialog(It.IsAny<IWin32Window>(), false), Times.Once());

			// Assert SelecFolder returned the user selected path
			AssertEquals(expectedSelectedPath, userSelectedPath);
		}
	}
}
