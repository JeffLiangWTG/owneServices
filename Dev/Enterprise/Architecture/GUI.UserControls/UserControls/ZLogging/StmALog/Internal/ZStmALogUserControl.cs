using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZStmALogUserControl : ZUserControl
	{
		public ZStmALogUserControl()
		{
			InitializeComponent();
		}

		public ZStmALogUserControl(IStmALogParent master, GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject = null)
		{
			InitializeComponent();
			SetupStmLogsControl(master, getStmALogFilterStripBusinessObject);
		}

		public void SetupStmLogsControl(IStmALogParent master, GetStmALogFilterStripBusinessObject getStmALogFilterStripBusinessObject = null)
		{
			if (master != null)
			{
				SuspendLayout();
				Controls.Clear();
				if (LogsModule != null)
				{
					LogsModule.Dispose();
					LogsModule = null;
				}
				LogsModule = ZModuleFactory.Instance.Create((ModuleIDs.StmALog)) as ZStmALogModule;
				LogsModule.InitData(master, getStmALogFilterStripBusinessObject);
				LogsControl = (ZStmALogFilterControl)LogsModule.EmbeddedControl;
				Controls.Add(LogsControl);
				LogsControl.Name = nameof(LogsControl);
				LogsControl.Dock = DockStyle.Fill;
				LogsControl.DockPadding.All = 5;
				LogsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
				LogsControl.FilteredGrid.ReadOnly = true;
				ResumeLayout(true);
			}
		}

		public void CheckNewAndCancelSecurity(bool canCreateNewEvent, bool canCancelEvent)
		{
			LogsControl.AllowNew = canCreateNewEvent;
			LogsControl.AllowCancel = canCancelEvent;
			return;
		}

		public ZStmALogFilterControl LogsControl { get; private set; }

		public ZStmALogModule LogsModule { get; private set; }

		#region Logs To Show

		public LogsToShow LogsToShow
		{
			get { return logsToShow; }
			set
			{
				logsToShow = value;
				if (LogsControl?.FilterBusinessObject is ZStmALogFilterBusinessObject filterBizo)
				{
					filterBizo.LogsToShow = value;
				}
			}
		}
		LogsToShow logsToShow;

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing && LogsModule?.IsDisposed == false)
			{
				LogsModule.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion

		internal IBusinessObjectCollection GridCollection
		{
			get { return LogsControl.GridCollection; }
		}
	}
}
