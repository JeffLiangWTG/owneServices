using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.ZArchitecture.GUI.UserControls.Res;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.ZArchitecture.GUI
{
	internal partial class ZActivityLoggingUserControl : ZUserControl
	{
		public ZActivityLoggingUserControl()
		{
			InitializeComponent();
		}

		protected override void OnVisibleChanged(EventArgs e)
		{
			base.OnVisibleChanged(e);
			ControlDpiScalingHelper.SetHeight(ref ActivityLogGrid, zGroupBox1.Top - ActivityLogGrid.Top - ControlDpiScalingHelper.ScaleToCurrentDpiY(5), false);
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#endregion

		#region Find / Clear

		void FindButton_Click(object sender, EventArgs e)
		{
			if (ActivityLogFilterProvider.HasErrors)
			{
				Globals.Message.Show(Res.GetString("08F54941-8451-456A-93E7-76944E7B7875", "There are errors. Please correct these before searching."), Res.GetString("A3EB0BB3-7BB1-488E-A0F1-53CE85D81E7F", "Errors..."), MessageBoxButtons.OK, MessageBoxIcon.Error, DialogResult.OK);
			}
			else
			{
				ActivityLogs.Factory.ClearQueryCache();
				ActivityLogs.Load();
			}
		}

		void ClearButton_Click(object sender, EventArgs e)
		{
			ActivityLogFilterProvider.ClearActivityLogFilters();
			ActivityLogs.Load();
		}

		#endregion

		#region Implementation

		StmActivityLogFilterProvider ActivityLogFilterProvider
		{
			get { return (StmActivityLogFilterProvider)CurrentDataItem; }
		}

		StmActivityLogCollection ActivityLogs
		{
			get { return ActivityLogFilterProvider == null ? null : ActivityLogFilterProvider.Collection; }
		}

		#endregion
	}
}
