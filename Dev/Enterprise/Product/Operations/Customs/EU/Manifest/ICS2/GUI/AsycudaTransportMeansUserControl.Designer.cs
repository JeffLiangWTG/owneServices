namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class AsycudaTransportMeansUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AsycudaTransportMeansGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaTransportMeansGrid)).BeginInit();
			this.AsycudaTransportMeansGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider);
			// 
			// AsycudaTransportMeansGrid
			// 
			this.AsycudaTransportMeansGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AsycudaTransportMeansGrid, "AsycudaTransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaTransportMeans)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)).SyncRoot)).TPM_IdentificationNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaTransportMeans)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)).SyncRoot)).TPM_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaTransportMeans)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)).SyncRoot)).TPM_TypeOfIdentification)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaTransportMeans)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)).SyncRoot)).TPM_TypeOfTransportMeans)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaTransportMeans)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.IAsycudaTransportMeansProvider)(null)).AsycudaTransportMeans)).SyncRoot)).TPM_RN_NKTransportNationality)));
			this.AsycudaTransportMeansGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "TPM_IdentificationNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "TPM_ReferenceNumber";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "TPM_TypeOfIdentification";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "TPM_TypeOfTransportMeans";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "TPM_RN_NKTransportNationality";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.AsycudaTransportMeansGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AsycudaTransportMeansGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AsycudaTransportMeansGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AsycudaTransportMeansGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AsycudaTransportMeansGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AsycudaTransportMeansGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AsycudaTransportMeansGrid.GridId = "f32e8b5c-1385-479c-819c-45d8fe442ff6";
			this.AsycudaTransportMeansGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AsycudaTransportMeansGrid.LayoutKey = "AsycudaTransportMeansGrid";
			this.AsycudaTransportMeansGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AsycudaTransportMeansGrid.Name = "AsycudaTransportMeansGrid";
			this.AsycudaTransportMeansGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 100, true);
			this.AsycudaTransportMeansGrid.TabIndex = 0;
			// 
			// AsycudaTransportMeansUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AsycudaTransportMeansGrid);
			this.Name = "AsycudaTransportMeansUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AsycudaTransportMeansGrid)).EndInit();
			this.AsycudaTransportMeansGrid.ResumeLayout(false);
			this.AsycudaTransportMeansGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid AsycudaTransportMeansGrid;
	}
}
