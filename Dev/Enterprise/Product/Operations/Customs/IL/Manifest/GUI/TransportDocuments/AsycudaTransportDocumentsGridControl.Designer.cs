namespace Enterprise.Customs.IL.Manifest.GUI
{
	public partial class AsycudaTransportDocumentsGridControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.gridTransportDocuments = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.gridTransportDocuments)).BeginInit();
			this.gridTransportDocuments.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Manifest.Business.AsycudaBill);
			// 
			// gridTransportDocuments
			// 
			this.gridTransportDocuments.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.gridTransportDocuments, "TransportDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).TransportDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaTransportDocumentInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).TransportDocuments)).SyncRoot)).CSI_CodeUserInterface)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Manifest.Business.AsycudaTransportDocumentInfo)(((System.Collections.IList)(((Enterprise.Customs.IL.Manifest.Business.AsycudaBill)(null)).TransportDocuments)).SyncRoot)).CSI_ReferenceNumberUserInterface)));
			this.gridTransportDocuments.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_CodeUserInterface";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumberUserInterface";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.gridTransportDocuments.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.gridTransportDocuments.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.gridTransportDocuments.Dock = System.Windows.Forms.DockStyle.Fill;
			this.gridTransportDocuments.GridId = "6F864AB6-2339-424B-866C-F1932FCAE769";
			this.gridTransportDocuments.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.gridTransportDocuments.LayoutKey = "gridTransportDocuments";
			this.gridTransportDocuments.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.gridTransportDocuments.Name = "gridTransportDocuments";
			this.gridTransportDocuments.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 166, true);
			this.gridTransportDocuments.TabIndex = 0;
			// 
			// AsycudaTransportDocumentsGridControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.gridTransportDocuments);
			this.Name = "AsycudaTransportDocumentsGridControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 166, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.gridTransportDocuments)).EndInit();
			this.gridTransportDocuments.ResumeLayout(false);
			this.gridTransportDocuments.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.ZGrid gridTransportDocuments;

		#endregion
	}
}
