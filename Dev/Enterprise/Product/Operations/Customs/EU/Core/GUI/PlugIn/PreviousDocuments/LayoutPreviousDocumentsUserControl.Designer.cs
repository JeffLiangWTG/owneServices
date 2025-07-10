using System;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class LayoutPreviousDocumentsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailsLayoutControl = new Enterprise.Customs.EU.GUI.PlugIn.PreviousDocumentsDetailsLayoutControl();
			this.GridSplitter = new CargoWise.Windows.UI.KSplitter();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.PreviousDocumentsGrid);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 235, true);
			this.TopPanel.TabIndex = 0;
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, "FilteredInvoiceLines.PreviousDocuments");
			this.PreviousDocumentsGrid.CaptionVisible = false;
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "82ef5be5-70ef-487d-bcf7-d1af6bd48be2";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 235, true);
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.DetailsLayoutControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 238, true);
			this.BottomPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 75, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 75, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// DetailsLayoutControl
			// 
			this.DetailsLayoutControl.AllowDrop = true;
			this.DetailsLayoutControl.AutoScroll = true;
			this.DetailsLayoutControl.CaptionRenderingEnabled = true;
			this.BindingSource.SetBindingMember(this.DetailsLayoutControl, "FilteredInvoiceLines.PreviousDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).FilteredInvoiceLines)).SyncRoot)).PreviousDocuments)).SyncRoot)))));
			this.DetailsLayoutControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsLayoutControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 13, true);
			this.DetailsLayoutControl.Name = "DetailsLayoutControl";
			this.DetailsLayoutControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1206, 208, true);
			this.DetailsLayoutControl.TabIndex = 6;
			// 
			// GridSplitter
			// 
			this.GridSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.GridSplitter.DoNotSaveSplitterLayout = false;
			this.GridSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.GridSplitter.Name = "GridSplitter";
			this.GridSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 3, true);
			this.GridSplitter.TabIndex = 1;
			this.GridSplitter.TabStop = false;
			// 
			// LayoutPreviousDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TopPanel);
			this.Controls.Add(this.GridSplitter);
			this.Controls.Add(this.BottomPanel);
			this.Name = "LayoutPreviousDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 313, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		public PreviousDocumentsDetailsLayoutControl DetailsLayoutControl;
		public Enterprise.ZArchitecture.ZGrid PreviousDocumentsGrid;
		protected Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		protected Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		protected CargoWise.Windows.UI.KSplitter GridSplitter;
	}
}
