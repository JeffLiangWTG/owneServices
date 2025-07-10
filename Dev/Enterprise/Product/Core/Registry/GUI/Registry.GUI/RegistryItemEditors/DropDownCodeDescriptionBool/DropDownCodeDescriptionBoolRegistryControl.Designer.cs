using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	partial class DropDownCodeDescriptionBoolRegistryControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DropDownCodeDescriptionBoolGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DropDownCodeDescriptionBoolGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.DropDownCodeDescriptionBool);
			// 
			// DropDownCodeDescriptionBoolGrid
			// 
			this.DropDownCodeDescriptionBoolGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DropDownCodeDescriptionBoolGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.DropDownCodeDescriptionBool)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DropDownCodeDescriptionBool)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.DropDownCodeDescriptionBool)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.DropDownCodeDescriptionBool)(null)).Bool)));
			this.DropDownCodeDescriptionBoolGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c0018ccb-2c0c-4357-a8c5-60fdc27539c4", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("585cbf2e-b1ba-4f32-b26b-92433b5e3f5d", "Description");
			zDropEditColumnStyleInfo2.ColumnName = "EnglishDescription";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DropDownCodeDescriptionBoolGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.DropDownCodeDescriptionBoolGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.DropDownCodeDescriptionBoolGrid.CopySelectedRowsAllowed = true;
			this.DropDownCodeDescriptionBoolGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DropDownCodeDescriptionBoolGrid.GridId = "0133e8bc-9b57-46aa-85f5-77c2f3bcf3a3";
			this.DropDownCodeDescriptionBoolGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DropDownCodeDescriptionBoolGrid.LayoutKey = "zGrid1";
			this.DropDownCodeDescriptionBoolGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DropDownCodeDescriptionBoolGrid.Name = "DropDownCodeDescriptionBoolGrid";
			this.DropDownCodeDescriptionBoolGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 152, true);
			this.DropDownCodeDescriptionBoolGrid.TabIndex = 0;
			// 
			// MultilingualDropDownCodeDescriptionBoolRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DropDownCodeDescriptionBoolGrid);
			this.Name = "MultilingualDropDownCodeDescriptionBoolRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DropDownCodeDescriptionBoolGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZGrid DropDownCodeDescriptionBoolGrid;

	}
}
