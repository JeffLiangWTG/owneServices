using System;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Visualisation.Testing
{
	sealed class VisualizerViewControllerTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestSaveButtonClicked()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var visualizerManager = mocks.Create<IVisualizerManager>();
			var view = mocks.Create<IVisualizerView>();

			view.Object.SaveButtonClicked += null;

			var controller = new VisualizerViewController(visualizerManager.Object, view.Object);
			view.Raise(m => m.SaveButtonClicked += null, EventArgs.Empty);

			visualizerManager.Verify(m => m.SaveData());
			visualizerManager.Verify(m => m.ClearData(), Times.Never());
			visualizerManager.Verify(m => m.RevertData(), Times.Never());
			view.Verify(m => m.Close());
		}

		[ExpectNoExceptions]
		public void TestDiscardButtonClicked()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var visualizerManager = mocks.Create<IVisualizerManager>();
			var view = mocks.Create<IVisualizerView>();

			view.Object.DiscardButtonClicked += null;

			var controller = new VisualizerViewController(visualizerManager.Object, view.Object);
			view.Raise(m => m.DiscardButtonClicked += null, EventArgs.Empty);

			visualizerManager.Verify(m => m.SaveData(), Times.Never());
			visualizerManager.Verify(m => m.ClearData(), Times.Never());
			view.Verify(m => m.Close());
		}

		[ExpectNoExceptions]
		public void TestResetButtonClickedWithConfirmation()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var visualizerManager = mocks.Create<IVisualizerManager>();
			var view = mocks.Create<IVisualizerView>();

			view.Setup(m => m.ShowConfirmation("You are about to clear the overriding data for this menu.", "Are you sure?")).Returns(true);
			visualizerManager.Setup(m => m.ClearData());
			view.Setup(m => m.Close());

			view.Object.ResetButtonClicked += null;

			var controller = new VisualizerViewController(visualizerManager.Object, view.Object);
			view.Raise(m => m.ResetButtonClicked += null, EventArgs.Empty);
		}

		[ExpectNoExceptions]
		public void TestResetButtonClickedButCancelled()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var visualizerManager = mocks.Create<IVisualizerManager>();
			var view = mocks.Create<IVisualizerView>();

			view.Setup(m => m.ShowConfirmation("You are about to clear the overriding data for this menu.", "Are you sure?")).Returns(false);

			view.Object.ResetButtonClicked += null;

			var controller = new VisualizerViewController(visualizerManager.Object, view.Object);
			view.Raise(m => m.ResetButtonClicked += null, EventArgs.Empty);

			visualizerManager.Verify(m => m.ClearData(), Times.Never());
			view.Verify(m => m.Close(), Times.Never());
		}

		[ExpectNoExceptions]
		public void TestCloseFormNotByClickTheButtons()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var visualizerManager = mocks.Create<IVisualizerManager>();
			var view = mocks.Create<IVisualizerView>();

			view.Object.ViewClosing += null;

			var controller = new VisualizerViewController(visualizerManager.Object, view.Object);
			view.Raise(m => m.ViewClosing += null, EventArgs.Empty);

			visualizerManager.Verify(m => m.SaveData(), Times.Never());
			visualizerManager.Verify(m => m.ClearData(), Times.Never());
			visualizerManager.Verify(m => m.RevertData());
		}
	}
}
