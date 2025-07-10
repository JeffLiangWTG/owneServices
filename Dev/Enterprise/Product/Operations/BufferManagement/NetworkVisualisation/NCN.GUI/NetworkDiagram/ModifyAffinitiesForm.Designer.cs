namespace Enterprise.BufferManagement.NetworkVisualisation.GUI
{
	partial class ModifyAffinitiesForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		public new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZColorDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ShapeAffinitiesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeAffinitiesGrid)).BeginInit();
			this.ShapeAffinitiesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 187, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinityCollection);
			// 
			// ShapeAffinitiesGrid
			// 
			this.ShapeAffinitiesGrid.AllowNavigation = false;
			this.ShapeAffinitiesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ShapeAffinitiesGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinity)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinity)(null)).Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinity)(null)).Color)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinity)(null)).AllowedConcurrency)));
			this.ShapeAffinitiesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "Name";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ColumnName = "Color";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "AllowedConcurrency";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ShapeAffinitiesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ShapeAffinitiesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ShapeAffinitiesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ShapeAffinitiesGrid.CopySelectedRowsAllowed = true;
			this.ShapeAffinitiesGrid.GridId = "15f6257c-7569-4cfc-b2f5-565784c4d4a9";
			this.ShapeAffinitiesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ShapeAffinitiesGrid.LayoutKey = "ShapeAffinitiesGrid";
			this.ShapeAffinitiesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 29, true);
			this.ShapeAffinitiesGrid.Name = "ShapeAffinitiesGrid";
			this.ShapeAffinitiesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(508, 152, true);
			this.ShapeAffinitiesGrid.TabIndex = 1;
			// 
			// zLabel1
			// 
			this.zLabel1.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("333dd9fe-aa1d-4c31-a38e-adb86438bca9", "Affinities are used to indicate a relationship between an entity and resources.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 2, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(459, 26, true);
			this.zLabel1.TabIndex = 2;
			// 
			// ModifyAffinitiesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.BufferManagement.NetworkVisualisation.GUI.Res.GetData("6b00a3ea-af44-41d7-8005-a59ab117fe6a", "Diagram Affinities");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 211, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ShapeAffinitiesGrid);
			this.DataSourceType = typeof(Enterprise.BufferManagement.NetworkVisualisation.Business.ShapeAffinityCollection);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(530, 250, true);
			this.Name = "ModifyAffinitiesForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ShapeAffinitiesGrid, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ShapeAffinitiesGrid)).EndInit();
			this.ShapeAffinitiesGrid.ResumeLayout(false);
			this.ShapeAffinitiesGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid ShapeAffinitiesGrid;
		private ZArchitecture.ZLabel zLabel1;

	}
}
