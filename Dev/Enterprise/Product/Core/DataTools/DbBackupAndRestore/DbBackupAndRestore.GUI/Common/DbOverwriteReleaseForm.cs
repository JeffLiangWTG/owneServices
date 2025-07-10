using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.DataTools.DbBackupAndRestore.Business;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	 partial class DbOverwriteReleaseForm : Form
	{
		public DbOverwriteReleaseForm()
		{
			InitializeComponent();
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			AutoScaleMode = ControlDpiScalingHelper.DpiScaleMode;
			AutoScaleDimensions = ControlDpiScalingHelper.DpiScaleDimensions;
			base.OnLayout(levent);
		}

		public DbOverwriteReleaseForm(SessionInfo info)
		{
			InitializeComponent();
			ReleaseKeyTextBox.DataBindings.Add("Text", info, "UserEnteredReleaseKey");
			InitializeSessionInfoTextBox(info.DbServer, info.DatabaseName, info.SessionDisplay);
		}

		void InitializeSessionInfoTextBox(string dbServer, string databaseName, string sessionDisplay)
		{
			ClientInfoTextBox.Text =
				"Server     = " + dbServer + "\r\n" +
				"Database   = " + databaseName + "\r\n" +
				"Session ID = " + sessionDisplay;
		}
	}
}