using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ServiceManager.GUI
{
	partial class LogViewerControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.ServiceManager.Business.ServiceTaskLogViewer);

			//
			// LogViewerControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "LogViewerControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		private void InitializeViewerControl(Enterprise.ServiceManager.Business.ServiceTaskLogViewer serviceTaskLogViewer)
		{
			if (serviceTaskLogViewer.IsUsingSearchBasedLogViewerControl)
			{
				this.viewerControl = new Enterprise.ServiceManager.GUI.SearchBasedLogViewerControl();
				this.BindingSource.SetBindingMember(this.viewerControl, "SearchBasedLogViewer");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ServiceManager.Business.SearchBasedLogViewer)(((Enterprise.ServiceManager.Business.ServiceTaskLogViewer)(null)).SearchBasedLogViewer)));
			}
			else
			{
				this.viewerControl = new Enterprise.ServiceManager.GUI.FileBasedLogViewerControl();
				this.BindingSource.SetBindingMember(this.viewerControl, "FileBasedLogViewer");
				CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ServiceManager.Business.FileBasedLogViewer)(((Enterprise.ServiceManager.Business.ServiceTaskLogViewer)(null)).FileBasedLogViewer)));
			}
			this.viewerControl.SuspendLayout();
			this.Controls.Add(this.viewerControl);

			this.viewerControl.Name = "StrategyLogViewerControl";
			this.viewerControl.AllowDrop = true;
			this.viewerControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.viewerControl.ResumeLayout(true);
			this.viewerControl.PerformLayout();
		}

		ZUserControl viewerControl;
	}
}
