using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.ExportManifest.GUI
{
	public partial class ExportManifestDeclarationUserControl
	{
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.DepartureReportDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.manifestDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ManifestUserControl = new Enterprise.Customs.AU.ExportManifest.GUI.ExportManifestDetailsUserControl();
			this.messagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.exportMessageUserControl1 = new Enterprise.Customs.GUI.ExportMessageUserControl();
			this.notesTabPage = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.eventTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DepartureReportDetailsTabControl.SuspendLayout();
			this.manifestDetailsTabPage.SuspendLayout();
			this.messagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.ExportCustomsManifestHeader);
			// 
			// DepartureReportDetailsTabControl
			// 
			this.DepartureReportDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.DepartureReportDetailsTabControl.Controls.Add(this.manifestDetailsTabPage);
			this.DepartureReportDetailsTabControl.Controls.Add(this.messagesTabPage);
			this.DepartureReportDetailsTabControl.Controls.Add(this.notesTabPage);
			this.DepartureReportDetailsTabControl.Controls.Add(this.eventTabPage);
			this.DepartureReportDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DepartureReportDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DepartureReportDetailsTabControl.Name = "DepartureReportDetailsTabControl";
			this.DepartureReportDetailsTabControl.SelectedIndex = 0;
			this.DepartureReportDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 630, true);
			this.DepartureReportDetailsTabControl.TabIndex = 0;
			// 
			// ManifestDetailsTabPage
			// 
			this.manifestDetailsTabPage.Controls.Add(this.ManifestUserControl);
			this.manifestDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.manifestDetailsTabPage.Name = "ManifestDetailsTabPage";
			this.manifestDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 603, true);
			this.manifestDetailsTabPage.TabIndex = 0;
			this.manifestDetailsTabPage.Text = "Manifest Details";
			// 
			// ManifestUserControl
			// 
			this.ManifestUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ManifestUserControl, ".");
			this.ManifestUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestUserControl.Header = null;
			this.ManifestUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestUserControl.Name = "ManifestUserControl";
			this.ManifestUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 603, true);
			this.ManifestUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.messagesTabPage.Controls.Add(this.exportMessageUserControl1);
			this.messagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagesTabPage.Name = "MessagesTabPage";
			this.messagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.messagesTabPage.TabIndex = 1;
			this.messagesTabPage.Text = "Messages";
			// 
			// exportMessageUserControl1
			// 
			this.exportMessageUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.exportMessageUserControl1, ".");
			this.exportMessageUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.exportMessageUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.exportMessageUserControl1.Name = "exportMessageUserControl1";
			this.exportMessageUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.exportMessageUserControl1.TabIndex = 0;
			// 
			// NotesTabPage
			// 
			this.notesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.notesTabPage.Name = "NotesTabPage";
			this.notesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.notesTabPage.TabIndex = 2;
			// 
			// EventTabPage
			// 
			this.eventTabPage.ExcludeFromBindingOnSave = true;
			this.eventTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.eventTabPage.Name = "EventTabPage";
			this.eventTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 603, true);
			this.eventTabPage.TabIndex = 3;
			// 
			// ExportManifestDeclarationUserControl
			// 
			this.Controls.Add(this.DepartureReportDetailsTabControl);
			this.Name = "ExportManifestDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 730, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DepartureReportDetailsTabControl.ResumeLayout(false);
			this.manifestDetailsTabPage.ResumeLayout(false);
			this.messagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private ZTabPage manifestDetailsTabPage;
		private ZTabPage messagesTabPage;
		private ZStmNoteTabPage notesTabPage;
		private ZLogsTabPage eventTabPage;
		internal ExportManifestDetailsUserControl ManifestUserControl;
		private Customs.GUI.ExportMessageUserControl exportMessageUserControl1;
		public ZTemplateTabControl DepartureReportDetailsTabControl;
		private IContainer components;
	}
}
