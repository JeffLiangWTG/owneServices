namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionBoolControl : RegistryZUserControl
	{
		internal const string BoolColumnName = "Bool";// SuppressCodeSmell Reason = Programmatic constant
		internal const string CodeColumnName = "Code";// SuppressCodeSmell Reason = Programmatic constant
		internal const string DescriptionColumnName = "EnglishDescription";// SuppressCodeSmell Reason = Programmatic constant

		protected internal Enterprise.ZArchitecture.ZGrid CodeDescriptionBoolGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionColumnStyleInfo;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			descriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.CodeDescriptionBoolGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionBoolCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			this.CodeDescriptionBoolGrid.AllowNavigation = false;
			this.CodeDescriptionBoolGrid.AllowSorting = false;
			this.CodeDescriptionBoolGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeDescriptionBoolGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionBool)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBool)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionBool)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CodeDescriptionBool)(null)).Bool)));
			this.CodeDescriptionBoolGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionBoolControl|d3dde190-4473-4e43-9863-d70599823abf", "Code");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = CodeColumnName;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CodeColumnType";
			zMultiControlColumnStyleInfo1.BindToList = "CodeList";
			descriptionColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionBoolControl|e1a390b5-5765-49df-a3fa-585d56401425", "Description");
			descriptionColumnStyleInfo.ColumnName = DescriptionColumnName;
			descriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionBoolControl|cf684bdb-059f-4ea4-b5c5-c66b673ecfe3", "Bool.");
			zCheckBoxColumnStyleInfo1.ColumnName = BoolColumnName;
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(descriptionColumnStyleInfo);
			this.CodeDescriptionBoolGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CodeDescriptionBoolGrid.CopySelectedRowsAllowed = true;
			this.CodeDescriptionBoolGrid.GridId = "9e6855b3-0f62-4285-a54b-cbf4c5ef232c";
			this.CodeDescriptionBoolGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionBoolGrid.LayoutKey = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionBoolGrid.Name = "CodeDescriptionBoolGrid";
			this.CodeDescriptionBoolGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			this.CodeDescriptionBoolGrid.TabIndex = 0;
			// 
			// CodeDescriptionBoolControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionBoolGrid);
			this.Name = "CodeDescriptionBoolControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionBoolGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
