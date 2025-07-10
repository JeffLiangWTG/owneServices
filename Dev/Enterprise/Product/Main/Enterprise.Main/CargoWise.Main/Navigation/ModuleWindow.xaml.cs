#if !WINZOR

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CargoWise.BrandManager;
using CargoWise.Main.Navigation.ViewModels;
using CargoWise.Main.Screenshot;
using Enterprise.ZArchitecture.GUI;

namespace CargoWise.Main.Navigation
{
	/// <summary>
	/// Interaction logic for ModuleWindow.xaml
	/// </summary>
	public partial class ModuleWindow : Window
	{
		public ModuleWindow()
		{
			InitializeComponent();
			Icon = ConvertIconToImageSource(BrandingFactory.Instance.ProductIcon);
			NextAppBar.MainCategoryMenuListBox.Visibility = Visibility.Collapsed;
			NextAppBar.PopupMenu.Visibility = Visibility.Collapsed;
		}

		WindowState lastWindowState;

		protected override void OnStateChanged(EventArgs e)
		{
			if (WindowState != WindowState.Minimized && lastWindowState != WindowState)
			{
				lastWindowState = base.WindowState;
			}

			base.OnStateChanged(e);
		}

		public void RestoreLastWindowStateFromMinimized()
		{
			if (WindowState == WindowState.Minimized)
			{
				WindowState = lastWindowState;
			}
		}

		void ClearSelections()
		{
			if (DataContext is NavigationViewModel viewModel)
			{
				viewModel.ForceClearSearch();
				viewModel.HideAllMenus();
				NextAppBar.QuickSearch.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
			}
		}

		void CloseSearchWindow(object target, ExecutedRoutedEventArgs e)
		{
			if (e.Parameter is string parameter
				&& parameter == "Escape"
				&& DataContext is NavigationViewModel viewModel
				&& !string.IsNullOrEmpty(viewModel.SearchViewModel.SearchValue))
			{
				viewModel.SearchViewModel.SearchValue = string.Empty;
				NextAppBar.QuickSearch.Focus();
				e.Handled = true;
				return;
			}
			ClearSelections();
			e.Handled = false;
		}

		void HandleSearchShortcut(object sender, ExecutedRoutedEventArgs e)
		{
			if (DataContext is NavigationViewModel viewModel && !viewModel.IsInSearchMode)
			{
				GenerateOverlay();
				viewModel.IsInSearchMode = true;
			}
			if (!NextAppBar.QuickSearch.IsFocused)
			{
				NextAppBar.QuickSearch.Focus();
			}
		}

		static ImageSource ConvertIconToImageSource(Icon icon)
		{
			using (var bitmap = icon.ToBitmap())
			{
				var hBitmap = bitmap.GetHbitmap();
				try
				{
					return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
						hBitmap,
						IntPtr.Zero,
						Int32Rect.Empty,
						BitmapSizeOptions.FromWidthAndHeight(icon.Width, icon.Height)
					);
				}
				finally
				{
					DeleteObject(hBitmap);
				}
			}
		}

		[DllImport("gdi32.dll")]
		static extern bool DeleteObject(IntPtr hObject);

		Bitmap ComposeScreenshot(Control control, ElementHost elementHost, FrameworkElement wpfChild)
		{
			var formBmp = ScreenshotHelper.CaptureControlWithPrintWindow(control);
			var wpfBmp = ScreenshotHelper.CaptureWpfElement(wpfChild);

			using (var g = Graphics.FromImage(formBmp))
			{
				var hostLocation = control.PointToClient(elementHost.PointToScreen(System.Drawing.Point.Empty));
				g.DrawImage(wpfBmp, hostLocation.X, hostLocation.Y, elementHost.Width, elementHost.Height);
			}

			return formBmp;
		}

		void GenerateOverlay()
		{
			if (winFormsHost.Child is Control control)
			{
				var host = control.FindSingleOrDefault<ElementHost>("HostControl", maxLevelsDeep: 2);

				var bmp = host == null
				 ? ScreenshotHelper.CaptureControlWithPrintWindow(control)
				 : ComposeScreenshot(control, host, host.Child as FrameworkElement);

				WinFormsSnapshot.Source = ScreenshotHelper.ConvertBitmapToImageSource(bmp);
			}
		}
	}
}
#endif
