namespace Enterprise.Customs.BE.GUI.Registry
{
	partial class CustomsRegistryItemControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CustomsRegistryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsRegistryGrid)).BeginInit();
			this.CustomsRegistryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.CustomsRegistry);
			// 
			// CustomsRegistryGrid
			// 
			this.CustomsRegistryGrid.AllowNavigation = false;
			this.CustomsRegistryGrid.AllowReadOnlyToModifyTabStop = true;
			this.CustomsRegistryGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomsRegistryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BE.Business.CustomsRegistry)(null)).Organization)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.CustomsRegistry)(null)).DeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Customs.BE.Business.CustomsRegistry)(null)).StartingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.BE.Business.CustomsRegistry)(null)).StartingNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.BE.Business.CustomsRegistry)(null)).CurrentNo)));
			this.CustomsRegistryGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Organization";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "DeclarationType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.ColumnName = "StartingDate";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			zCalcEditColumnStyleInfo4.ColumnName = "StartingNo";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			zCalcEditColumnStyleInfo5.ColumnName = "CurrentNo";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(73);
			this.CustomsRegistryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.CustomsRegistryGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.CustomsRegistryGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.CustomsRegistryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.CustomsRegistryGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.CustomsRegistryGrid.GridId = "19470225-D876-4C9D-9E79-E23A28BF7CE7";
			this.CustomsRegistryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CustomsRegistryGrid.LayoutKey = "CustomsRegistryGrid";
			this.CustomsRegistryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsRegistryGrid.Name = "CustomsRegistryGrid";
			this.CustomsRegistryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			this.CustomsRegistryGrid.TabIndex = 0;
			// 
			// CustomsRegistryItemControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsRegistryGrid);
			this.Name = "CustomsRegistryItemControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 100, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CustomsRegistryGrid)).EndInit();
			this.CustomsRegistryGrid.ResumeLayout(false);
			this.CustomsRegistryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid CustomsRegistryGrid;
	}
}
