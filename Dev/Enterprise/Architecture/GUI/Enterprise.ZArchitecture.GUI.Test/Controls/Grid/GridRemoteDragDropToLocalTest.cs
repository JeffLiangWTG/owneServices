#if !WINZOR
using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.ZArchitecture.Testing
{
	class GridRemoteDragDropToLocalTest : TestCaseWithFactory
	{
		public void TestGrid_Uses_ObjectFactory_Not_New()
		{
			// Arrange : configuing the mocks 
			var terminalService = new Mock<RemoteDesktopServices.TerminalService>();
			terminalService.Setup(x => x.IsRemoteAppSession).Returns(true);

			var remoteChannelMock = new Mock<Enterprise.Integration.RemoteDesktopServices.IRemoteChannel>();
			remoteChannelMock.Setup(mock => mock.RegisteredRemoteMessageTypes).Returns(new string[] { RemoteDesktopServices.EnterpriseChannelMessageTypes.ListDirectory });
			remoteChannelMock.Setup(x => x.SendMessage<RemoteDesktopServices.MessageElements.ListDirectoryRequest[], RemoteDesktopServices.MessageElements.ListDirectoryResult[]>(
					It.IsAny<string>(),
					It.IsAny<RemoteDesktopServices.MessageElements.ListDirectoryRequest[]>()))
				.Returns(Array.Empty<RemoteDesktopServices.MessageElements.ListDirectoryResult>());

			// Arrange : substituting the ObjectFactory with the mocks
			using (ObjectFactory.Substitute(terminalService.Object))
			using (ObjectFactory.Substitute(remoteChannelMock.Object))
			{
				var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();
				dummy.AddChildDummy();
				using (var grid = new ZGridForTest(dummy))
				{
					var log = new StringBuilder();

					// Act 
					bool result = grid.InvokeShouldDragDropToLocal(log);

					// Assert
					terminalService.VerifyGet(x => x.IsRemoteAppSession, Times.Once, "Expected the grid to call IsRemoteAppSession on the container‐provided instance.");
					Assert("should be false here", !result);
				}
			}
		}

		public void TestDragDropToLocalTrackingInfo()
		{
			var partialExpectedText = "##################################\r\nDrag-Drop to local starts. 1 rows selected.\r\n1 dataobjects found.\r\nIs remote session and drag to local is supported.\r\nCreating DRTL message.\r\nDRTL message created.\r\nException thrown: Object reference not set to an instance of an object..\r\n\tStackTrace:    at Enterprise.ZArchitecture.ZGrid.DoDragDrop()";

			var dummy = Factory.NewWithValidTestData<DummyWithActiveCollection>();
			dummy.AddChildDummy();

			using (var form = new TestFormForRemoteDragDropToLocal(dummy))
			{
				form.Show();
				Application.DoEvents();

				form.grid1.SetSelectedRows(new int[] { 0 });
				form.grid1.DoDragDrop_Exposed();

				Assert(form.grid1.DragDropTrackingFormForTest.Visible);
				AssertContains(partialExpectedText, form.grid1.DragDropTrackingFormForTest.GetTextBoxContent());
			}

			ErrorReporter.Clear();
		}

		class ZGridForRemoteDragDropToLocalTest : ZGrid
		{
			protected override bool ShouldDragDropToLocal(StringBuilder builder)
			{
				return true;
			}

			protected override DragDropTrackingForm GetDragDropTrackingInfoForm()
			{
				DragDropTrackingFormForTest = new DragDropTrackingForm();
				return DragDropTrackingFormForTest;
			}

			public DragDropTrackingForm DragDropTrackingFormForTest;

			public void DoDragDrop_Exposed()
			{
				DoDragDrop();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && DragDropTrackingFormForTest != null)
				{
					DragDropTrackingFormForTest.Dispose();
					DragDropTrackingFormForTest = null;
				}
				base.Dispose(disposing);
			}
		}

		class TestFormForRemoteDragDropToLocal : ZForm
		{
			public TestFormForRemoteDragDropToLocal(DummyWithActiveCollection bizo)
				: base(bizo)
			{
				InitializeComponent();
			}

			new void InitializeComponent()
			{
				SuspendLayout();

				grid1 = new ZGridForRemoteDragDropToLocalTest
				{
					Location = ControlDpiScalingHelper.NewScaledPoint(4, 4),
					Size = ControlDpiScalingHelper.NewScaledSize(200, 100),
					BindTo = "ActiveCollection"
				};
				grid1.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 100));

				Controls.Add(grid1);

				ResumeLayout(true);
			}

			public ZGridForRemoteDragDropToLocalTest grid1;
		}

		public class ZGridForTest : ZGrid
		{
			public ZGridForTest(object bizo) : base()
			{
				this.BindTo = "ActiveCollection";
				this.ColumnStyles.Add(new ZTextBoxColumnStyleInfo(DummyBizoSchema.Constants.Z0_Code, 100));
			}

			public bool InvokeShouldDragDropToLocal(StringBuilder builder) => base.ShouldDragDropToLocal(builder);
		}
	}
}
#endif
