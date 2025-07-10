using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class RegistryImageCollectionControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid RegistryImageGrid;
		internal protected ImageSelectionControl RegistryImageSelectionControl;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.RegistryImageGrid = new Enterprise.ZArchitecture.ZGrid();
			this.RegistryImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RegistryImageGrid)).BeginInit();
			this.RegistryImageGrid.SuspendLayout();
			this.RegistryImageSelectionControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.RegistryImageCollection);
			// 
			// RegistryImageGrid
			// 
			this.RegistryImageGrid.AllowNavigation = false;
			this.RegistryImageGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RegistryImageGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.RegistryImage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RegistryImage)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.RegistryImage)(null)).EnglishDescription)));
			this.RegistryImageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryImageCollectionControl|c377398d-2916-47f1-b2df-31518de77ba2", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("RegistryImageCollectionControl|4484f10e-c3a3-4cdc-9441-b8d4db569ba9", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(350);
			this.RegistryImageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RegistryImageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RegistryImageGrid.GridId = "afc793a2-9068-4eba-aa95-fd614e1a3ccc";
			this.RegistryImageGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RegistryImageGrid.LayoutKey = "Grid";
			this.RegistryImageGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RegistryImageGrid.Name = "RegistryImageGrid";
			this.RegistryImageGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(422, 298, true);
			this.RegistryImageGrid.TabIndex = 0;
			// 
			// RegistryImageSelectionControl
			// 
			this.RegistryImageSelectionControl.AllowDrop = true;
			this.RegistryImageSelectionControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.RegistryImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Registry.Business.RegistryImage)(null)).Image)));
			this.RegistryImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 302, true);
			this.RegistryImageSelectionControl.Name = "RegistryImageSelectionControl";
			this.RegistryImageSelectionControl.ReadOnly = true;
			this.RegistryImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 136, true);
			this.RegistryImageSelectionControl.TabIndex = 1;
			// 
			// RegistryImageCollectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RegistryImageSelectionControl);
			this.Controls.Add(this.RegistryImageGrid);
			this.Name = "RegistryImageCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 440, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RegistryImageGrid)).EndInit();
			this.RegistryImageGrid.ResumeLayout(false);
			this.RegistryImageGrid.PerformLayout();
			this.RegistryImageSelectionControl.ResumeLayout(true);
			this.RegistryImageSelectionControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
