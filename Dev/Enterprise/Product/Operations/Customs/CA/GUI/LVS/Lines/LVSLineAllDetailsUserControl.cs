using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSLineAllDetailsUserControl : ZUserControl
	{
		public LVSLineAllDetailsUserControl()
		{
			InitializeComponent();
			Resize += LVSLineAllDetailsUserControl_Resize;
		}

		#region SplitContainer Layout Bug Fix

		//TODO: Replace this code with new ZSplitContainer when ready

		void LVSLineAllDetailsUserControl_Resize(object sender, EventArgs e)
		{
			if (!DesignModeFinder.IsDesigning)
			{
				if (workItem != null)
				{
					workItem.Dispose();
				}

				workItem = UserIdleWorker.QueueWorkItem(this, 0, new Action(RefreshSplitter));
			}
		}

		void RefreshSplitter()
		{
			DetailsSplitContainer.Panel1.SuspendLayout();
			DetailsSplitContainer.Panel2.SuspendLayout();
			if (shouldMoveLeft)
			{
				DetailsSplitContainer.SplitterDistance -= ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				shouldMoveLeft = false;
			}
			else
			{
				DetailsSplitContainer.SplitterDistance += ControlDpiScalingHelper.ScaleToCurrentDpiX(5);
				shouldMoveLeft = true;
			}
			DetailsSplitContainer.Panel1.ResumeLayout();
			DetailsSplitContainer.Panel2.ResumeLayout();
		}

		IDisposable workItem;
		bool shouldMoveLeft;

		#endregion
	}
}
