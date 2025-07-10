namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithMandatoryDescriptionControl : RegistryZUserControl
	{
		internal const string CodeColumnName = "Code";// SuppressCodeSmell Reason = Programmatic constant
		internal const string DescriptionColumnName = "EnglishDescription";// SuppressCodeSmell Reason = Programmatic constant

		protected internal Enterprise.ZArchitecture.ZGrid CodeDescriptionWithMandatoryDescriptionGrid;
		protected Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1;
		protected internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2;

		void InitializeComponent()
		{
			this.CodeDescriptionWithMandatoryDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithMandatoryDescriptionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CodeDescriptionWithMandatoryDescriptionCollection);
			// 
			// CodeDescriptionWithMandatoryDescriptionGrid
			// 
			this.CodeDescriptionWithMandatoryDescriptionGrid.AllowNavigation = false;
			this.CodeDescriptionWithMandatoryDescriptionGrid.AllowSorting = false;
			this.CodeDescriptionWithMandatoryDescriptionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CodeDescriptionWithMandatoryDescriptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CodeDescriptionWithMandatoryDescription)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithMandatoryDescription)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CodeDescriptionWithMandatoryDescription)(null)).EnglishDescription)));
			this.CodeDescriptionWithMandatoryDescriptionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionWithMandatoryDescriptionControl|91464d32-0627-429d-8bb1-2c42c8bc4f04", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = CodeColumnName;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CodeDescriptionWithMandatoryDescriptionControl|127c469e-3943-4708-8229-54ecff7d404b", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = DescriptionColumnName;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);

			this.CodeDescriptionWithMandatoryDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeDescriptionWithMandatoryDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CodeDescriptionWithMandatoryDescriptionGrid.CopySelectedRowsAllowed = true;
			this.CodeDescriptionWithMandatoryDescriptionGrid.GridId = "f463f891-8fa1-4eaa-a3cf-a9f3d475d845";
			this.CodeDescriptionWithMandatoryDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeDescriptionWithMandatoryDescriptionGrid.LayoutKey = "CodeDescriptionWithMandatoryDescriptionGrid";
			this.CodeDescriptionWithMandatoryDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CodeDescriptionWithMandatoryDescriptionGrid.Name = "CodeDescriptionWithMandatoryDescriptionGrid";
			this.CodeDescriptionWithMandatoryDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			this.CodeDescriptionWithMandatoryDescriptionGrid.TabIndex = 0;
			// 
			// CodeDescriptionWithMandatoryDescriptionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeDescriptionWithMandatoryDescriptionGrid);
			this.Name = "CodeDescriptionWithMandatoryDescriptionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 152, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CodeDescriptionWithMandatoryDescriptionGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
