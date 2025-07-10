namespace Enterprise.Registry.GUI
{
	partial class NewsSectionControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		Enterprise.ZArchitecture.ZGrid Grid;
		System.ComponentModel.IContainer components = null;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.Grid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.NewsSection);
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.NewsSection)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.NewsSection)(null)).LayoutPanelID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.NewsSection)(null)).SectionID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.NewsSection)(null)).SectionName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.NewsSection)(null)).HideReadItems)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.NewsSection)(null)).MandatoryToRead)));
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "LayoutPanelID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.ColumnName = "SectionID";
			zTextBoxColumnStyleInfo2.ColumnName = "SectionName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCheckBoxColumnStyleInfo1.ColumnName = "HideReadItems";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.ColumnName = "MandatoryToRead";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.Grid.CopySelectedRowsAllowed = true;
			this.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Grid.GridId = "81d418b9-a109-4b10-b7a9-067c82fb835e";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "EntryChargeTypesGrid";
			this.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Grid.Name = "Grid";
			this.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			this.Grid.TabIndex = 0;
			// 
			// NewsSectionControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Grid);
			this.Name = "NewsSectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(669, 288, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
