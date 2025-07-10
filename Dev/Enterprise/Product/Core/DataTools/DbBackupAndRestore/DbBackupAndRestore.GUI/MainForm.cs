using System;
using System.ComponentModel.Design.Serialization;
using System.Windows.Forms;
using System.Xml.XPath;
using CargoWise.Common;

namespace Enterprise.DataTools.DbBackupAndRestore.GUI
{
	[DesignerSerializer(typeof(CargoWise.Windows.UI.Design.ControlDpiScalingCodeDomSerializer), typeof(CodeDomSerializer))]
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		public void ShowRestoreTabPage()
		{
			DbBackupAndRestoreTabControl.SelectedIndex = 1;
		}

		#region DPI scaling overrides

		protected override void OnLayout(LayoutEventArgs levent)
		{
			if (VisualStudioDetector.IsVisualStudio)
			{
				this.AutoScaleMode = AutoScaleMode.None;
			}
			else
			{
				this.AutoScaleMode = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleMode;
				this.AutoScaleDimensions = CargoWise.Windows.UI.ControlDpiScalingHelper.DpiScaleDimensions;
			}

			base.OnLayout(levent);
		}

		#endregion

		void MainForm_Load(object sender, EventArgs e)
		{
			this.Text = "Database Backup And Restore - " + GetVersionReleaseInfo();
		}

		string GetVersionReleaseInfo()
		{
			try
			{
				string versionNumber = Enterprise.ZArchitecture.Core.ReleaseInfo.Instance.VersionNumber.ToString();
				string releaseText = Enterprise.ZArchitecture.Core.ReleaseInfo.Instance.ReleaseDisplayText;
				return String.Format("{0} ({1})", versionNumber, releaseText);
			}
			catch (ArgumentException) { }
			catch (XPathException) { }
			catch (InvalidOperationException) { }
			catch (OverflowException) { }
			return "(release info not available - please run with loader)";
		}
	}
}
