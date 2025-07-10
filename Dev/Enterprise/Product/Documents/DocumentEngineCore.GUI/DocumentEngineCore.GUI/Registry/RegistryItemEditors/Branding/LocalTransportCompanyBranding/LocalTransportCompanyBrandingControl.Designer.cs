using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class LocalTransportCompanyBrandingControl
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
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo  findBoxColumnStyleInfo = new ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo dropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			this.BrandingGrid = new ZGrid();
			this.ImageSelectionControl = new ImageSelectionControl();
			this.ImageBox = new ZGroupBox();
			this.BrandDetailsBox = new ZGroupBox();
						((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BrandingGrid)).BeginInit();
			this.ImageBox.SuspendLayout();
			this.BrandDetailsBox.SuspendLayout();
			this.SuspendLayout();
			//
			// Binding Source
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBrandingCollection);
			//
			// Branding Grid
			//
			this.BrandingGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.BrandingGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)).LocalTransportCompanyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)).LocalTransportCompanyCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)).LabelName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)).LabelNameList)));
			this.BrandingGrid.CaptionVisible = false;
			//
			//findBoxColumnStyleInfo
			//
			findBoxColumnStyleInfo.BindToList = "LocalTransportCompanyCollection";
			findBoxColumnStyleInfo.CaptionResourceString = Res.GetData("LocalTransportCompanyBrandingControl|da7618da-3632-4591-89a9-1102ca3f1910", "Local Transport Company");
			findBoxColumnStyleInfo.ColumnName = "LocalTransportCompanyPK";
			findBoxColumnStyleInfo.IsMandatory = true;
			findBoxColumnStyleInfo.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			//
			//dropEditColumnStyleInfo
			//
			dropEditColumnStyleInfo.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("LocalTransportCompanyBrandingControl|c53eecf2-58b8-41d7-9bd5-59ae9e17d477", "Label Name");
			dropEditColumnStyleInfo.ColumnName = "LabelName";
			//
			//BrandingGrid
			//
			this.BrandingGrid.ColumnStyles.Add(findBoxColumnStyleInfo);
			this.BrandingGrid.ColumnStyles.Add(dropEditColumnStyleInfo);
			this.BrandingGrid.GridId = "1650c7d6-8cfd-45cb-a9d9-70b87a2c242c";
			this.BrandingGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandingGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.BrandingGrid.LayoutKey = "BrandingGrid";
			this.BrandingGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.BrandingGrid.Name = "BrandingGrid";
			this.BrandingGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 74, true);
			this.BrandingGrid.TabIndex = 2;
			// 
			// ImageBoundImageSelectionControl
			// 
			this.BindingSource.SetBindingMember(this.ImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.DocumentEngineCore.Registry.LocalTransportCompanyBranding)(null)).Image)));
			this.ImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.ImageSelectionControl.Name = "ImageSelectionControl";
			this.ImageSelectionControl.ReadOnly = false;
			this.ImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 115, true);
			this.ImageSelectionControl.TabIndex = 3;
			// 
			// ImageGroupBox
			// 
			this.ImageBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("LocalTransportCompanyBrandingControl|af2cd766-01f8-4500-8123-75adbf0f5948", "Brand Label Logo");
			this.ImageBox.Controls.Add(this.ImageSelectionControl);
			this.ImageBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ImageBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 229, true);
			this.ImageBox.Name = "ImageGroupBox";
			this.ImageBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ImageBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 140, true);
			this.ImageBox.TabStop = false;
			// 
			// BrandMappingGroupBox
			// 
			this.BrandDetailsBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("LocalTransportCompanyBrandingControl|aa6c8e41-ff62-469c-81da-e73423c04ce2", "Brand Mapping");
			this.BrandDetailsBox.Controls.Add(this.BrandingGrid);
			this.BrandDetailsBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BrandDetailsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BrandDetailsBox.Name = "BrandDetailsBox";
			this.BrandDetailsBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.BrandDetailsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 99, true);
			this.BrandDetailsBox.TabIndex = 4;
			this.BrandDetailsBox.TabStop = false;
			// 
			// LocalTransportCompanyBrandingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BrandDetailsBox);
			this.Controls.Add(this.ImageBox);
			this.Name = "LocalTransportCompanyBrandingControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 372, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BrandingGrid)).EndInit();
			this.ImageBox.ResumeLayout(false);
			this.BrandDetailsBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion

		Enterprise.ZArchitecture.ZGrid BrandingGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox ImageBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox BrandDetailsBox;
		ImageSelectionControl ImageSelectionControl;
	}
}
