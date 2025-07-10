using System;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class LVSSubHeadersUserControl : ZUserControl
	{
		public LVSSubHeadersUserControl()
		{
			InitializeComponent();
			Resize += LVSSubHeaderUserControl_Resize;
		}

		#region SplitContainer Layout Bug Fix

		//TODO: Replace this code with new ZSplitContainer when ready

		void LVSSubHeaderUserControl_Resize(object sender, EventArgs e)
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
			MainSplitContainer.Panel1.SuspendLayout();
			MainSplitContainer.Panel2.SuspendLayout();
			if (shouldMoveUp)
			{
				MainSplitContainer.SplitterDistance--;
				shouldMoveUp = false;
			}
			else
			{
				MainSplitContainer.SplitterDistance++;
				shouldMoveUp = true;
			}
			MainSplitContainer.Panel1.ResumeLayout();
			MainSplitContainer.Panel2.ResumeLayout();
		}

		IDisposable workItem;
		bool shouldMoveUp;

		#endregion
	}
}
