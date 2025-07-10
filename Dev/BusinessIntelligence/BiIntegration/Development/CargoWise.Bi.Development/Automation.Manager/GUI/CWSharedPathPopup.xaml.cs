using System;
using System.IO;
using System.Windows;
using CargoWise.Main.Navigation.WPF.Test;

namespace CargoWise.Bi.Development.Automation.Manager
{
	#region SuppressResourceStringsCheckRegion
#if DEBUG
	[TestExcludeWPFControlFromBasher]
#endif
	public partial class CWSharedPathPopup : Window, IDisposable
	{
		public CWSharedPathPopup()
		{
			InitializeComponent();
		}

		void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(CWSharedPath.Text))
			{
				MessageBox.Show("CargoWise/Shared path not set yet."); // Not using ZorKArchitecture
				return;
			}
			if (!Directory.Exists(Path.Combine(CWSharedPath.Text, "CargoWise.DbUpgrader")))
			{
				MessageBox.Show("The specified CargoWise/Shared path does not appear to be valid."); // Not using ZorKArchitecture
				return;
			}

			DialogResult = true;
			Close();
		}

		void CancelButtion_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		#region Dispose

		bool disposed;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
				}
				disposed = true;
			}
		}

		#endregion
	}
	#endregion
}
