namespace Enterprise.Customs.DE.GUI.Registry
{
	partial class SendAcknowledgementsRegistryItemControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.AcknowledgementsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AcknowledgementsGrid)).BeginInit();
			this.AcknowledgementsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Registry.SendAcknowledgementsRegistry);
			// 
			// AcknowledgementsGrid
			// 
			this.AcknowledgementsGrid.AllowNavigation = false;
			this.AcknowledgementsGrid.AllowReadOnlyToModifyTabStop = true;
			this.AcknowledgementsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcknowledgementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Registry.SendAcknowledgementsRegistry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Registry.SendAcknowledgementsRegistry)(null)).EBSCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Registry.SendAcknowledgementsRegistry)(null)).SendGroupPK)));
			this.AcknowledgementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "";
			zDropEditColumnStyleInfo1.ColumnName = "EBSCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zGuidFindBoxColumnStyleInfo1.Caption = "";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SendGroupPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AcknowledgementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AcknowledgementsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AcknowledgementsGrid.GridId = "D47D69B4-94D8-460F-B894-9194E1B1714F";
			this.AcknowledgementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AcknowledgementsGrid.LayoutKey = "AcknowledgementsGrid";
			this.AcknowledgementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AcknowledgementsGrid.Name = "AcknowledgementsGrid";
			this.AcknowledgementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 218, true);
			this.AcknowledgementsGrid.TabIndex = 0;
			// 
			// SendAcknowledgementsRegistryItemControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AcknowledgementsGrid);
			this.Name = "SendAcknowledgementsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 221, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AcknowledgementsGrid)).EndInit();
			this.AcknowledgementsGrid.ResumeLayout(false);
			this.AcknowledgementsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid AcknowledgementsGrid;
	}
}
