#if !WINZOR
using System;
using System.Windows;
using CargoWise.Main.Startup.Login;

namespace Enterprise.Startup.Login.Testing;

public partial class NextLoginUserControlTest
{
	class WindowForTest : IDisposable
	{
		readonly Window window;

		public WindowForTest(NextLoginUserControl uut)
		{
			window = new Window { Content = uut };
			window.Show();
		}

		public void Dispose()
		{
			window.Close();
		}
	}
}
#endif
