namespace Enterprise.Customs.DE.GUI.Registry
{
	partial class MessageVersionRegistryItemControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.MessageVersionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageVersionGrid)).BeginInit();
			this.MessageVersionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Registry.MessageVersionRegistry);
			// 
			// MessageVersionGrid
			// 
			this.MessageVersionGrid.AllowNavigation = false;
			this.MessageVersionGrid.AllowReadOnlyToModifyTabStop = true;
			this.MessageVersionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MessageVersionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Registry.MessageVersionRegistry)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Registry.MessageVersionRegistry)(null)).SystemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Registry.MessageVersionRegistry)(null)).VersionNumber)));
			this.MessageVersionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "SystemCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "VersionNumber";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			this.MessageVersionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessageVersionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MessageVersionGrid.GridId = "ed22097b-d719-4617-92f5-7f392f7b997a";
			this.MessageVersionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessageVersionGrid.LayoutKey = "MessageVersionGrid";
			this.MessageVersionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageVersionGrid.Name = "MessageVersionGrid";
			this.MessageVersionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			this.MessageVersionGrid.TabIndex = 0;
			// 
			// MessageVersionRegistryItemControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessageVersionGrid);
			this.Name = "MessageVersionRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageVersionGrid)).EndInit();
			this.MessageVersionGrid.ResumeLayout(false);
			this.MessageVersionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid MessageVersionGrid;
	}
}
