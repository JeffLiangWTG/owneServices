#if !WINZOR
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test
{
	public abstract class WpfControlTestWithWrapperWindow<T> : TestCase where T : Control
	{
		protected Window WrapperWindow;
		protected Func<Window> CreateWindowFunc;
		protected T ControlToTest;
		protected override void SetUp()
		{
			base.SetUp();
			if (CreateWindowFunc == null)
			{
				CreateWindowFunc = () => new Window
				{
					Width = 800,
					Height = 600,
				};
			}
			WrapperWindow = CreateWindowFunc();
			ControlToTest = GetControlToBashCore() as T;
			WrapperWindow.Content = ControlToTest;
			WrapperWindow.Show();
		}
		protected abstract Control GetControlToBashCore();
		protected override void TearDown()
		{
			base.TearDown();
			if (WrapperWindow != null)
			{
				WrapperWindow.Close();
				WrapperWindow = null;
			}
		}

		protected void SimulateButtonClick(Button button)
		{
			var args = new RoutedEventArgs(ButtonBase.ClickEvent);
			button.RaiseEvent(args);
		}
	}
}

#endif
