namespace Enterprise.Customs.KR.GUI
{
	partial class FamilyRelationUserControl
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
            this.FamilyRelationGrid = new Enterprise.ZArchitecture.ZGrid();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FamilyRelationGrid)).BeginInit();
            this.FamilyRelationGrid.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.FamilyRelationCollection);
            // 
            // FamilyRelationGrid
            // 
            this.FamilyRelationGrid.AllowNavigation = false;
            this.FamilyRelationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.FamilyRelationGrid, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.KR.Business.FamilyRelationCollection)(null)))));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FamilyRelation)(null)).Code)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.FamilyRelation)(null)).Description)));
            this.FamilyRelationGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("41C14C12-1DEF-4EE5-B557-B6E23BD654D9", "Code");
            zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0230A5E1-FE2E-46DA-A6B9-DDACE323E0F8", "Description");
            zTextBoxColumnStyleInfo2.ColumnName = "Description";
			zTextBoxColumnStyleInfo2.TextAlign = System.Windows.Forms.HorizontalAlignment.Left;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
            this.FamilyRelationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.FamilyRelationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.FamilyRelationGrid.GridId = "a0ebfd6b-df1f-458f-ab72-33bdaabd7a81";
            this.FamilyRelationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.FamilyRelationGrid.LayoutKey = "FamilyRelationGrid";
            this.FamilyRelationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 4, true);
            this.FamilyRelationGrid.Name = "FamilyRelationGrid";
            this.FamilyRelationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 421, true);
            this.FamilyRelationGrid.TabIndex = 0;
            // 
            // FamilyRelationUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.FamilyRelationGrid);
            this.Name = "FamilyRelationUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 427, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FamilyRelationGrid)).EndInit();
            this.FamilyRelationGrid.ResumeLayout(false);
            this.FamilyRelationGrid.PerformLayout();
			this.CaptionRenderingEnabled = true;
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		public ZArchitecture.ZGrid FamilyRelationGrid;
	}
}
