using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.DevTools;

namespace Enterprise.ZArchitecture.Tools
{
	public class SqlProfilerTool : IDevTool
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer Only Tool")]
		public string Name
		{
			get { return "SQL Profiler"; }
		}

		public bool AddAsButton
		{
			get { return false; }
		}

		public void Show(Form form)
		{
			var sqlCommandsManager = new SqlCommandsManager(TaskScheduler.FromCurrentSynchronizationContext());
			new ZSqlProfilerForm(sqlCommandsManager).Show();
		}
	}
}
