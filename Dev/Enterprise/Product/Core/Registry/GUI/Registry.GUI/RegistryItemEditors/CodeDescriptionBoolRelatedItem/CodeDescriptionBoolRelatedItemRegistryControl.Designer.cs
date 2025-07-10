namespace Enterprise.Registry.GUI
{
	partial class CodeDescriptionBoolRelatedItemRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CodeDescriptionBoolRelatedItemGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolRelatedItemGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem);
			// 
			// CodeDescriptionBoolRelatedItemGrid
			// 
			this.CodeDescriptionBoolRelatedItemGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CodeDescriptionBoolRelatedItemGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)).RelatedItemCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolRelatedItem)(null)).RelatedItemDescription)));
			this.CodeDescriptionBoolRelatedItemGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c0018ccb-2c0c-4357-a8c5-60fdc27539c4", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("585cbf2e-b1ba-4f32-b26b-92433b5e3f5d", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("c43b43fe-1492-4f27-b82e-742f0d5c16a6", "Bool.");
			zCheckBoxColumnStyleInfo1.ColumnName = "Bool";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("C3FC89AD-CD6E-444C-9D1E-B0B53ECCAE14", "Government Code");
			zTextBoxColumnStyleInfo3.ColumnName = "RelatedItemCode";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("66127962-7559-4171-8aae-5f92837dcaea", "Secondary List");
			zDropEditColumnStyleInfo1.ColumnName = "RelatedItemDescription";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CodeDescriptionBoolRelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeDescriptionBoolRelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CodeDescriptionBoolRelatedItemGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CodeDescriptionBoolRelatedItemGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.CodeDescriptionBoolRelatedItemGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CodeDescriptionBoolRelatedItemGrid.CopySelectedRowsAllowed = true;
			this.CodeDescriptionBoolRelatedItemGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeDescriptionBoolRelatedItemGrid.GridId = "0133e8bc-9b57-46aa-85f5-77c2f3bcf3a3";
			this.CodeDescriptionBoolRelatedItemGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionBoolRelatedItemGrid.LayoutKey = "zGrid1";
			this.CodeDescriptionBoolRelatedItemGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CodeDescriptionBoolRelatedItemGrid.Name = "CodeDescriptionBoolRelatedItemGrid";
			this.CodeDescriptionBoolRelatedItemGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(297, 152, true);
			this.CodeDescriptionBoolRelatedItemGrid.TabIndex = 0;
			// 
			// MultilingualCodeDescriptionBoolRelatedItemRegistryControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionBoolRelatedItemGrid);
			this.Name = "MultilingualCodeDescriptionBoolRelatedItemRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolRelatedItemGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		protected ZArchitecture.ZGrid CodeDescriptionBoolRelatedItemGrid;

	}
}
