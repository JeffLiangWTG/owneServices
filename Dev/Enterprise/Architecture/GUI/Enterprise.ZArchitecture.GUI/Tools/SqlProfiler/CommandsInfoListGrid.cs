using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ZArchitecture.Tools
{
	[ToolboxItem(false)]
	public partial class CommandsInfoListGrid : ZUserControl
	{
		public CommandsInfoListGrid()
		{
			InitializeComponent();

			SetFieldValue(typeof(DataGrid), "positionChangedHandler", zGrid1, new EventHandler((sender, e) => { }));
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource == null && SqlProfilerManager != null)
			{
				SqlProfilerManager.Dispose();
			}

			base.SetDataBinding(dataSource, dataMember);

			if (!this.IsDesignMode())
			{
				EnableToolStripButtons();
			}
		}

		SqlCommandsManager SqlProfilerManager
		{
			get { return (SqlCommandsManager)DataSource; }
		}

		void tsButtonStart_Click(object sender, EventArgs e)
		{
			if (SqlProfilerManager != null)
			{
				SqlProfilerManager.Start();
				EnableToolStripButtons();
			}
		}

		void tsButtonPause_Click(object sender, EventArgs e)
		{
			if (SqlProfilerManager != null)
			{
				SqlProfilerManager.Pause();
				EnableToolStripButtons();
			}
		}

		void tsButtonStop_Click(object sender, EventArgs e)
		{
			if (SqlProfilerManager != null)
			{
				SqlProfilerManager.Stop();
				EnableToolStripButtons();
			}
		}

		void tsButtonClear_Click(object sender, EventArgs e)
		{
			if (SqlProfilerManager != null)
			{
				SqlProfilerManager.Clear();
			}
		}

		void EnableToolStripButtons()
		{
			if (SqlProfilerManager != null)
			{
				tsButtonStart.Enabled = SqlProfilerManager.State != SqlProfilerState.Running;
				tsButtonPause.Enabled = SqlProfilerManager.State == SqlProfilerState.Running;
				tsButtonStop.Enabled = SqlProfilerManager.State != SqlProfilerState.Stopped;
			}
		}

		void SetFieldValue(Type type, string filedName, object instance, object value)
		{
			var fieldInfo = type.GetField(filedName, BindingFlags.Instance | BindingFlags.NonPublic);
			fieldInfo.SetValue(instance, value);
		}
	}
}
