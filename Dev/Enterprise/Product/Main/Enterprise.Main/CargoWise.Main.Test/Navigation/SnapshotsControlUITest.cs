#if !WINZOR
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CargoWise.Main.Data;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test
{
	class SnapshotsControlUITest : WpfControlTestWithWrapperWindow<SnapshotsControl>
	{
		SnapshotsViewModel viewModel;
		readonly Mock<ICommand> runSnapshotCommand = new Mock<ICommand>();
		protected override Control GetControlToBashCore()
		{
			var snapshotsControl = new SnapshotsControl();
			runSnapshotCommand.Setup(x => x.CanExecute(It.IsAny<object>())).Returns(true);
			viewModel = new SnapshotsViewModel(new SnapshotsRepository(), Guid.NewGuid()) { RunSnapshotCommand = runSnapshotCommand.Object };
			snapshotsControl.DataContext = viewModel;
			return snapshotsControl;
		}

		[RequiresSTA]
		[GuiTest]
		public void TestInvokeUpdateCommand()
		{
			viewModel.Snapshots.Add(new Snapshot());
			viewModel.UpdateSnapshots();
			runSnapshotCommand.Verify(x => x.Execute(It.IsAny<object>()), Times.Once());
			Assert(true);
		}

		[RequiresSTA]
		[GuiTest]
		public void TestSnapShotsIsEmpty()
		{
			Assert(viewModel.Snapshots.Count == 0);
			Assert(ControlToTest.ShowEmptyContainer.Visibility == Visibility.Visible);
			Assert(ControlToTest.EmptyContainerRectangle.Visibility == Visibility.Visible);
			Assert(ControlToTest.SnapshotsPanel.Visibility == Visibility.Collapsed);
		}

		[RequiresSTA]
		[GuiTest]
		public void TestEditButton()
		{
			Assert(viewModel.Snapshots.Count == 0);
			Assert(!ControlToTest.IsEditing);
			Assert(ControlToTest.CancelButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.SaveChangesButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.EditButton.Visibility == Visibility.Visible);

			SimulateButtonClick(ControlToTest.EditButton);

			Assert(ControlToTest.IsEditing);
			Assert(ControlToTest.CancelButton.Visibility == Visibility.Visible);
			Assert(ControlToTest.SaveChangesButton.Visibility == Visibility.Visible);
			Assert(ControlToTest.EditButton.Visibility == Visibility.Collapsed);
		}

		[RequiresSTA]
		[GuiTest]
		public void TestCancelButton()
		{
			SimulateButtonClick(ControlToTest.EditButton);

			Assert(ControlToTest.CancelButton.Visibility == Visibility.Visible);
			Assert(ControlToTest.IsEditing);

			SimulateButtonClick(ControlToTest.CancelButton);

			Assert(!ControlToTest.IsEditing);
			Assert(ControlToTest.CancelButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.SaveChangesButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.EditButton.Visibility == Visibility.Visible);
		}

		[RequiresSTA]
		[GuiTest]
		public void TestSaveButton()
		{
			SimulateButtonClick(ControlToTest.EditButton);

			Assert(ControlToTest.SaveChangesButton.Visibility == Visibility.Visible);
			Assert(ControlToTest.IsEditing);

			SimulateButtonClick(ControlToTest.SaveChangesButton);

			Assert(!ControlToTest.IsEditing);
			Assert(ControlToTest.CancelButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.SaveChangesButton.Visibility == Visibility.Collapsed);
			Assert(ControlToTest.EditButton.Visibility == Visibility.Visible);
		}
	}
}
#endif
