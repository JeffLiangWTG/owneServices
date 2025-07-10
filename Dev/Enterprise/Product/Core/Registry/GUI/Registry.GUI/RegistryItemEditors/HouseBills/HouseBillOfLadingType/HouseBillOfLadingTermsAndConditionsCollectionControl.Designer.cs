using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class HouseBillOfLadingTermsAndConditionsCollectionControl : RegistryZUserControl
	{
		internal Enterprise.ZArchitecture.ZGrid TermsAndConditionsGrid;
		internal ImageSelectionControl TermsAndConditionsImageSelectionControl;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TermsAndConditionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.TermsAndConditionsImageSelectionControl = new Enterprise.ZArchitecture.GUI.ImageSelectionControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.TermsAndConditionsGrid)).BeginInit();
			this.TermsAndConditionsGrid.SuspendLayout();
			this.TermsAndConditionsImageSelectionControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditionsCollection);
			// 
			// TermsAndConditionsGrid
			// 
			this.TermsAndConditionsGrid.AllowNavigation = false;
			this.TermsAndConditionsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TermsAndConditionsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditions)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditions)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditions)(null)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditions)(null)).DeliveryMode)));
			this.TermsAndConditionsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("HouseBillOfLadingTermsAndConditionsCollectionControl|03d55be1-079d-4316-a35e-ea2c3a4eb316", "Code");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("HouseBillOfLadingTermsAndConditionsCollectionControl|4b7ff750-63d9-4f71-954c-54042b18a9a4", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("HouseBillOfLadingTermsAndConditionsCollectionControl|456351c0-1f6e-4803-b73d-53e7324cdf57", "Delivery Mode");
			zDropEditColumnStyleInfo1.ColumnName = "DeliveryMode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.TermsAndConditionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TermsAndConditionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TermsAndConditionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TermsAndConditionsGrid.GridId = "ff6f0a72-d9da-4323-a757-156f3137bccc";
			this.TermsAndConditionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TermsAndConditionsGrid.LayoutKey = "Grid";
			this.TermsAndConditionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TermsAndConditionsGrid.Name = "TermsAndConditionsGrid";
			this.TermsAndConditionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 286, true);
			this.TermsAndConditionsGrid.TabIndex = 0;
			// 
			// TermsAndConditionsImageSelectionControl
			// 
			this.TermsAndConditionsImageSelectionControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TermsAndConditionsImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.Registry.Business.HouseBillOfLadingTermsAndConditions)(null)).Image)));
			this.TermsAndConditionsImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.TermsAndConditionsImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 292, true);
			this.TermsAndConditionsImageSelectionControl.Name = "TermsAndConditionsImageSelectionControl";
			this.TermsAndConditionsImageSelectionControl.ReadOnly = true;
			this.TermsAndConditionsImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 156, true);
			this.TermsAndConditionsImageSelectionControl.TabIndex = 1;
			// 
			// HouseBillOfLadingTermsAndConditionsCollectionControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TermsAndConditionsImageSelectionControl);
			this.Controls.Add(this.TermsAndConditionsGrid);
			this.Name = "HouseBillOfLadingTermsAndConditionsCollectionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 448, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.TermsAndConditionsGrid)).EndInit();
			this.TermsAndConditionsGrid.ResumeLayout(false);
			this.TermsAndConditionsGrid.PerformLayout();
			this.TermsAndConditionsImageSelectionControl.ResumeLayout(true);
			this.TermsAndConditionsImageSelectionControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
