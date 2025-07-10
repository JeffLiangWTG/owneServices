using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	public partial class ClientAndAgentBrandingControl : RegistryZUserControl
	{
		protected ZGroupBox ImageGroupBox;
		protected ImageSelectionControl ImageControl;
		protected ZGrid CodeAndDescriptionGrid;
		protected ZGroupBox BrandMappingGroupBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ImageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImageControl = new ImageSelectionControl();
			this.BrandMappingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CodeAndDescriptionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ImageGroupBox.SuspendLayout();
			this.BrandMappingGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CodeAndDescriptionGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingCollection);
			// 
			// ImageGroupBox
			// 
			this.ImageGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ClientAndAgentBrandingControl|93146b17-c841-4ea8-8960-513e8da99b7f", "Brand Letterhead");
			this.ImageGroupBox.Controls.Add(this.ImageControl);
			this.ImageGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ImageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 227, true);
			this.ImageGroupBox.Name = "ImageGroupBox";
			this.ImageGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ImageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 200, true);
			this.ImageGroupBox.TabIndex = 4;
			this.ImageGroupBox.TabStop = false;
			// 
			// ImageControl
			// 
			this.BindingSource.SetBindingMember(this.ImageControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingBusinessObject)(null)).Image)));
			this.ImageControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ImageControl.Name = "ImageControl";
			this.ImageControl.ReadOnly = false;
			this.ImageControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 175, true);
			this.ImageControl.TabIndex = 3;
			// 
			// BrandMappingGroupBox
			// 
			this.BrandMappingGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ClientAndAgentBrandingControl|823f4268-2aed-4bc8-a69c-78ad4c207d56", "Brand Mapping");
			this.BrandMappingGroupBox.Controls.Add(this.CodeAndDescriptionGrid);
			this.BrandMappingGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandMappingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BrandMappingGroupBox.Name = "BrandMappingGroupBox";
			this.BrandMappingGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.BrandMappingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 224, true);
			this.BrandMappingGroupBox.TabIndex = 5;
			this.BrandMappingGroupBox.TabStop = false;
			// 
			// CodeAndDescriptionGrid
			// 
			this.CodeAndDescriptionGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CodeAndDescriptionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingBusinessObject)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingBusinessObject)(null)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingBusinessObject)(null)).CodeList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.ClientAndAgentBrandingBusinessObject)(null)).Description)));
			this.CodeAndDescriptionGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "CodeList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ClientAndAgentBrandingControl|908846c0-66d1-4903-9c1f-1650ab6cf46c", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("ClientAndAgentBrandingControl|173f0d81-b40b-4d57-9d8c-281bd5762b29", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.CodeAndDescriptionGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CodeAndDescriptionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CodeAndDescriptionGrid.GridId = "bb5f9f44-2542-4b69-a285-89f174b32493";
			this.CodeAndDescriptionGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CodeAndDescriptionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CodeAndDescriptionGrid.LayoutKey = "zGrid1";
			this.CodeAndDescriptionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.CodeAndDescriptionGrid.Name = "CodeAndDescriptionGrid";
			this.CodeAndDescriptionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 199, true);
			this.CodeAndDescriptionGrid.TabIndex = 1;
			// 
			// ClientAndAgentBrandingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrandMappingGroupBox);
			this.Controls.Add(this.ImageGroupBox);
			this.Name = "ClientAndAgentBrandingControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 430, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ImageGroupBox.ResumeLayout(false);
			this.BrandMappingGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.CodeAndDescriptionGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
