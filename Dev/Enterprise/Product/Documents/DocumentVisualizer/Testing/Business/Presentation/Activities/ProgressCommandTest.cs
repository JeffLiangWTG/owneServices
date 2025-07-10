using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class ProgressCommandTest : TestCase
	{
		public void TestCommand_ShouldProxyPropertiesFromNestedCommand()
		{
			var mock = new Mock<ICommand>();

			mock.SetupGet(m => m.Id).Returns("ID");
			mock.SetupGet(m => m.Caption).Returns("Caption");
			mock.SetupGet(m => m.Image).Returns("ProxyImageObject");
			mock.SetupGet(m => m.IsEnabled).Returns(true);
			mock.SetupGet(m => m.IsVisible).Returns(true);

			var progressCommand = new ProgressCommand(mock.Object);

			AssertEquals("ProgressCommand Id", progressCommand.Id, mock.Object.Id);
			AssertEquals("ProgressCommand Caption", progressCommand.Caption, mock.Object.Caption);
			AssertEquals("ProgressCommand Image", progressCommand.Image, mock.Object.Image);
			AssertEquals("ProgressCommand IsEnabled", progressCommand.IsEnabled, mock.Object.IsEnabled);
			AssertEquals("ProgressCommand IsVisible", progressCommand.IsVisible, mock.Object.IsVisible);
		}

		#region TestCommand_Invoke

		public void TestCommand_Invoke_ShouldStartProgressManagerAndInvokeNestedCommand()
		{
			using (TempSetIsUserInteractive(true))
			{
				var hasCommandBeenInvoked = false;
				var hasProgressManagerBeenStarted = false;

				var mockManager = new Mock<IProgressManager>();
				mockManager.Setup(m => m.Start())
					.Callback(() => { hasProgressManagerBeenStarted = true; });

				using (ObjectFactory.Substitute(mockManager.Object))
				{
					var mock = new Mock<ICommand>();
					mock.SetupGet(m => m.Id).Returns("ID");
					mock
						.Setup(m => m.Invoke())
						.Callback(() =>
						{
							hasCommandBeenInvoked = true;
						})
						.Returns(true);

					var progressCommand = new ProgressCommand(mock.Object);

					Assert("Invoke result", progressCommand.Invoke());
					Assert("Nested command has been invoked", hasCommandBeenInvoked);
					Assert("ProgressManager has been started", hasProgressManagerBeenStarted);
				}
			}
		}

		public void TestCommand_Invoke_WithParameters_ShouldStartProgressManagerAndInvokeNestedCommand()
		{
			using (TempSetIsUserInteractive(true))
			{
				var hasCommandBeenInvoked = false;
				var hasProgressManagerBeenStarted = false;

				var mockManager = new Mock<IProgressManager>();
				mockManager.Setup(m => m.Start())
					.Callback(() => { hasProgressManagerBeenStarted = true; });

				using (ObjectFactory.Substitute(mockManager.Object))
				{
					var mock = new Mock<ICommand>();
					mock.SetupGet(m => m.Id).Returns("ID");
					mock
						.Setup(m => m.Invoke(It.IsAny<MacroMap>()))
						.Callback(() =>
						{
							hasCommandBeenInvoked = true;
						})
						.Returns(true);

					var progressCommand = new ProgressCommand(mock.Object);
					var invokeParameters = new MacroMap(new Dictionary<string, object>());

					Assert("Invoke result", progressCommand.Invoke(invokeParameters));
					Assert("Nested command has been invoked", hasCommandBeenInvoked);
					Assert("ProgressManager has been started", hasProgressManagerBeenStarted);
				}
			}
		}

		public void TestCommand_Invoke_ShouldNotUseProgressManagerWhenNotUserInteractive()
		{
			using (TempSetIsUserInteractive(false))
			{
				var hasCommandBeenInvoked = false;
				var hasProgressManagerBeenStarted = false;

				var mockManager = new Mock<IProgressManager>();
				mockManager.Setup(m => m.Start())
					.Callback(() => { hasProgressManagerBeenStarted = true; });

				using (ObjectFactory.Substitute(mockManager.Object))
				{
					var mock = new Mock<ICommand>();
					mock.SetupGet(m => m.Id).Returns("ID");
					mock
						.Setup(m => m.Invoke())
						.Callback(() =>
						{
							hasCommandBeenInvoked = true;
						})
						.Returns(true);

					var progressCommand = new ProgressCommand(mock.Object);

					Assert("Invoke result", progressCommand.Invoke());
					Assert("Nested command has been invoked", hasCommandBeenInvoked);
					Assert("ProgressManager has not been started because it's not user interactive environment", !hasProgressManagerBeenStarted);
				}
			}
		}

		public void TestCommand_Invoke_WithParameters_ShouldNotUseProgressManagerWhenNotUserInteractive()
		{
			using (TempSetIsUserInteractive(false))
			{
				var hasCommandBeenInvoked = false;
				var hasProgressManagerBeenStarted = false;

				var mockManager = new Mock<IProgressManager>();
				mockManager.Setup(m => m.Start())
					.Callback(() => { hasProgressManagerBeenStarted = true; });

				using (ObjectFactory.Substitute(mockManager.Object))
				{
					var mock = new Mock<ICommand>();
					mock.SetupGet(m => m.Id).Returns("ID");
					mock
						.Setup(m => m.Invoke(It.IsAny<MacroMap>()))
						.Callback(() =>
						{
							hasCommandBeenInvoked = true;
						})
						.Returns(true);

					var progressCommand = new ProgressCommand(mock.Object);
					var invokeParameters = new MacroMap(new Dictionary<string, object>());

					Assert("Invoke result", progressCommand.Invoke(invokeParameters));
					Assert("Nested command has been invoked", hasCommandBeenInvoked);
					Assert("ProgressManager has not been started because it's not user interactive environment", !hasProgressManagerBeenStarted);
				}
			}
		}

		IDisposable TempSetIsUserInteractive(bool isUserInteractive)
		{
			var originalIsUserInteractive = Globals.IsUserInteractive;
			Globals.IsUserInteractive = isUserInteractive;

			return new DisposableAction(() => Globals.IsUserInteractive = originalIsUserInteractive);
		}

		#endregion
	}
}
