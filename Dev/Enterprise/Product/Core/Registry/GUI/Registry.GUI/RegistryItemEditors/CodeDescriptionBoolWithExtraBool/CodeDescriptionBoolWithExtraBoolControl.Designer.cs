namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionBoolWithExtraBoolControl : RegistryZUserControl
	{
		internal const string BoolColumnName = "Bool";// SuppressCodeSmell Reason = Programmatic constant
		internal const string Bool2ColumnName = "Bool2";// SuppressCodeSmell Reason = Programmatic constant
		internal const string CodeColumnName = "Code";// SuppressCodeSmell Reason = Programmatic constant
		internal const string DescriptionColumnName = "EnglishDescription";// SuppressCodeSmell Reason = Programmatic constant

		internal Enterprise.ZArchitecture.ZGrid CodeDescriptionBoolGrid;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CodeDescriptionBoolGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).BeginInit();
			this.CodeDescriptionBoolGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBoolCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			this.CodeDescriptionBoolGrid.AllowNavigation = false;
			this.CodeDescriptionBoolGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.CodeDescriptionBoolGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBool)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBool)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBool)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBool)(null)).Bool)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionBoolWithExtraBool)(null)).Bool2)));
			this.CodeDescriptionBoolGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionBoolWithExtraBoolControl|d3dde190-4473-4e43-9863-d70599823abf", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = CodeColumnName;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionBoolWithExtraBoolControl|e1a390b5-5765-49df-a3fa-585d56401425", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = DescriptionColumnName;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.ColumnName = BoolColumnName;
			zCheckBoxColumnStyleInfo2.ColumnName = Bool2ColumnName;
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CodeDescriptionBoolGrid.GridId = "9e6855b3-0f62-4285-a54b-cbf4c5ef232b";
			this.CodeDescriptionBoolGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeDescriptionBoolGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionBoolGrid.LayoutKey = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionBoolGrid.Name = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			this.CodeDescriptionBoolGrid.TabIndex = 0;
			// 
			// CodeDescriptionBoolWithExtraBoolControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionBoolGrid);
			this.Name = "CodeDescriptionBoolWithExtraBoolControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).EndInit();
			this.CodeDescriptionBoolGrid.ResumeLayout(false);
			this.CodeDescriptionBoolGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
