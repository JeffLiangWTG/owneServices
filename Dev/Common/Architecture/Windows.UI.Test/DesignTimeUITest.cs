using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Moq;
using NUnit.Framework;

namespace CargoWise.Windows.UI.Testing
{
	sealed class DesignTimeUITest : TestCase
	{
		[ExpectNoExceptions]
		public void TestShowMessageOnceUntilIdle()
		{
			var uiService = new Mock<IUIService>();

			uiService.Object.ShowMessage("Message1");
			uiService.Object.ShowMessage("Message3");

			ServiceContainer serviceProvider = new ServiceContainer();
			serviceProvider.AddService(typeof(IUIService), uiService.Object);

			DesignTimeUI.ShowMessageOnceUntilIdle(serviceProvider, "Message1");
			DesignTimeUI.ShowMessageOnceUntilIdle(serviceProvider, "Message1"); // ShowMessage() not called here
			FireApplicationIdle();
			DesignTimeUI.ShowMessageOnceUntilIdle(serviceProvider, "Message3");
		}

		void FireApplicationIdle()
		{
			Form form = new Form();

			Timer timer = new Timer();
			timer.Tick += delegate
			{ form.Dispose(); };
			timer.Interval = 200;
			timer.Start();

			form.ShowDialog();
			timer.Dispose();
		}
	}
}
