using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.ZArchitecture.GUI
{
	public class ZStmALogTabPage : ZBindingTabPage
	{
		[DefaultValue(true)]
		public override bool ExcludeFromBindingOnSave
		{
			get { return true; }
		}

		public bool FilterStripsModifiable
		{
			get
			{
				return filterStripsModifiable;
			}
			set
			{
				filterStripsModifiable = value;
				if (EventUserControl != null)
				{
					EventUserControl.SkipSettingChildControlReadOnly = value;
				}
			}
		}

		bool filterStripsModifiable;

		public LogsToShow LogsToShow
		{
			get { return EventUserControl == null ? preBoundLogsToShow : EventUserControl.LogsToShow; }
			set
			{
				if (EventUserControl != null)
				{
					EventUserControl.LogsToShow = value;
				}
				else
				{
					preBoundLogsToShow = value;
				}
			}
		}
		LogsToShow preBoundLogsToShow;

		protected override void SetDataBindingCore(object dataSource, string dataMember)
		{
			var stmALogParent = (IStmALogParent)dataSource;
			if (stmALogParent != null)
			{
				this.SuspendLayout();

				splitContainer = new KSplitContainer();
				splitContainer.Dock = DockStyle.Fill;
				splitContainer.Orientation = Orientation.Horizontal;
				splitContainer.Panel2Collapsed = true;
				splitContainer.Width = ControlDpiScalingHelper.MarkAsScaled(this.Width);
				splitContainer.Height = ControlDpiScalingHelper.MarkAsScaled(this.Height);

				EventUserControl = new ZStmALogUserControl(stmALogParent, GetStmALogFilterStripBusinessObject);
				EventUserControl.Dock = DockStyle.Fill;
				EventUserControl.LogsToShow = preBoundLogsToShow;
				EventUserControl.MinimumSize = ControlDpiScalingHelper.NewScaledSize(0, 100);
				EventUserControl.SkipSettingChildControlReadOnly = filterStripsModifiable;
				splitContainer.Panel1.Controls.Add(EventUserControl);

				sourceInfoUserControl = new EventSourceInfoUserControl();
				sourceInfoUserControl.Dock = DockStyle.Fill;
				splitContainer.Panel2.Controls.Add(sourceInfoUserControl);

				Controls.Add(splitContainer);

				this.ResumeLayout();

				try
				{
					splitContainer.Panel1MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(200);
					splitContainer.Panel2MinSize = ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
					splitContainer.SplitterDistance = ControlDpiScalingHelper.MarkAsScaled(Math.Max(0, splitContainer.Height - ControlDpiScalingHelper.ScaleToCurrentDpiY(100)));
				}
				catch (InvalidOperationException)
				{
					//control too large or too small - give up
				}

				this.ResumeLayout();
			}
			base.SetDataBindingCore(dataSource, "");

			if (sourceInfoUserControl != null)
			{
				sourceInfoUserControl.SetDataBinding(EventUserControl.GridCollection, "");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				EventUserControl?.Dispose();
				EventUserControl = null;
			}

			base.Dispose(disposing);
		}

		public GetStmALogFilterStripBusinessObject GetStmALogFilterStripBusinessObject { get; set; }
		public ZStmALogUserControl EventUserControl { get; private set; }
		KSplitContainer splitContainer;
		EventSourceInfoUserControl sourceInfoUserControl;
	}
}
