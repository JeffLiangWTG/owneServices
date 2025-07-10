using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	partial class ConsignmentItemPackingDetailsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.PackingDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PackingDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).BeginInit();
			this.PackingDetailsGrid.SuspendLayout();
			this.PackingDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ExitControlBase.Business.ICusExitConsignmentPivotCollection<CusExitConsignmentPivot>);
			// 
			// PackingDetailsGrid
			// 
			this.PackingDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackingDetailsGrid, ".");
			this.PackingDetailsGrid.CaptionVisible = false;
			this.PackingDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGrid.GridId = "B28630A4-E6C7-4123-A66E-5A0302298184";
			this.PackingDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackingDetailsGrid.LayoutKey = "PackingDetailsGrid";
			this.PackingDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 22, true);
			this.PackingDetailsGrid.Name = "PackingDetailsGrid";
			this.PackingDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 136, true);
			this.PackingDetailsGrid.TabIndex = 0;
			// 
			// PackingDetailsGroupBox
			// 
			this.PackingDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.ExitControl.GUI.Res.GetData("46FF2AC2-7F19-4B02-B281-0AD0208F9675", "Packing Details");
			this.PackingDetailsGroupBox.Controls.Add(this.PackingDetailsGrid);
			this.PackingDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingDetailsGroupBox.Name = "PackingDetailsGroupBox";
			this.PackingDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 160, true);
			this.PackingDetailsGroupBox.TabIndex = 0;
			this.PackingDetailsGroupBox.TabStop = false;
			// 
			// ConsignmentItemPackingDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackingDetailsGroupBox);
			this.Name = "ConsignmentItemPackingDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PackingDetailsGrid)).EndInit();
			this.PackingDetailsGrid.ResumeLayout(false);
			this.PackingDetailsGrid.PerformLayout();
			this.PackingDetailsGroupBox.ResumeLayout(false);
			this.PackingDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PackingDetailsGrid;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox PackingDetailsGroupBox;
	}
}

