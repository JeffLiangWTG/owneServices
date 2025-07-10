using System;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class SystemDefinableRegistryImageCollectionControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid RegistryImageGrid;
		internal protected ImageSelectionControl RegistryImageSelectionControl;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SystemDefinableRegistryImageCollection);
			// 
			// RegistryImageGrid
			// 
			this.RegistryImageGrid.AllowNavigation = false;
			this.RegistryImageGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RegistryImageGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.SystemDefinableRegistryImage)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SystemDefinableRegistryImage)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.SystemDefinableRegistryImage)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.SystemDefinableRegistryImage)(null)).SystemDefined)));
			this.RegistryImageGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SystemDefinableRegistryImageCollectionControl|d35e9533-ae93-419c-843b-1bf633ba7f60", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SystemDefinableRegistryImageCollectionControl|10154e01-894d-4a4d-af5c-4d708fb8284b", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(280);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("SystemDefinableRegistryImageCollectionControl|805898c2-e85e-4e32-8870-db696940f2fe", "System");
			zCheckBoxColumnStyleInfo1.ColumnName = "SystemDefined";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			this.RegistryImageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RegistryImageGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RegistryImageGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RegistryImageGrid.GridId = "3daf2557-55bd-40a4-b61f-ab2cc7de53a3";
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Registry.Business.SystemDefinableRegistryImage)(null)).Image)));
			this.RegistryImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 302, true);
			this.RegistryImageSelectionControl.Name = "RegistryImageSelectionControl";
			this.RegistryImageSelectionControl.ReadOnly = true;
			this.RegistryImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 136, true);
			this.RegistryImageSelectionControl.TabIndex = 1;
			// 
			// SystemDefinableRegistryImageCollectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RegistryImageSelectionControl);
			this.Controls.Add(this.RegistryImageGrid);
			this.Name = "SystemDefinableRegistryImageCollectionControl";
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
