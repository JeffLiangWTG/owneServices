using System.Windows.Forms;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithGroupControl : RegistryZUserControl
	{
		internal const string GroupColumnName = "Group";// SuppressCodeSmell Reason = Programmatic constant
		internal const string CodeColumnName = "Code";// SuppressCodeSmell Reason = Programmatic constant
		internal const string DescriptionColumnName = "EnglishDescription";// SuppressCodeSmell Reason = Programmatic constant

		protected internal Enterprise.ZArchitecture.ZGrid CodeDescriptionWithGroupGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo descriptionColumnStyleInfo;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			descriptionColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CodeDescriptionWithGroupGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithGroupGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionBoolCollection);
			// 
			// CodeDescriptionBoolGrid
			// 
			this.CodeDescriptionWithGroupGrid.AllowNavigation = false;
			this.CodeDescriptionWithGroupGrid.AllowSorting = false;
			this.CodeDescriptionWithGroupGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeDescriptionWithGroupGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithGroup)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithGroup)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithGroup)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithGroup)(null)).Group)));
			this.CodeDescriptionWithGroupGrid.CaptionVisible = false;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionWithGroupControl|acafcfc5-7cb5-425b-995e-432f3a0bd83b", "Code");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = CodeColumnName;
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CodeColumnType";
			zMultiControlColumnStyleInfo1.BindToList = "CodeList";
			descriptionColumnStyleInfo.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionWithGroupControl|5fd5323d-576a-4e31-81fc-be0dc9768b29", "Description");
			descriptionColumnStyleInfo.ColumnName = DescriptionColumnName;
			descriptionColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionWithGroupControl|9e232c84-3d34-4ec2-91d5-ec0715ab82df", "Group.");
			zDropEditColumnStyleInfo1.CharacterCasing = CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = GroupColumnName;
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			zDropEditColumnStyleInfo1.BindToList = "GroupLookup";

			this.CodeDescriptionWithGroupGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.CodeDescriptionWithGroupGrid.ColumnStyles.Add(descriptionColumnStyleInfo);
			this.CodeDescriptionWithGroupGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CodeDescriptionWithGroupGrid.CopySelectedRowsAllowed = true;
			this.CodeDescriptionWithGroupGrid.GridId = "d7f613f8-4dc4-4127-9173-901fc8d522d3";
			this.CodeDescriptionWithGroupGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionWithGroupGrid.LayoutKey = "CodeDescriptionWithGroupGrid";
			this.CodeDescriptionWithGroupGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionWithGroupGrid.Name = "CodeDescriptionBoolGrid";
			this.CodeDescriptionWithGroupGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			this.CodeDescriptionWithGroupGrid.TabIndex = 0;
			// 
			// CodeDescriptionBoolControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionWithGroupGrid);
			this.Name = "CodeDescriptionWithGroupControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithGroupGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
