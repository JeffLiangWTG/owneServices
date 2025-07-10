using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngineCore.GUI.Registry
{
	partial class DeliveryOrderUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.PrincipalGrid = new Enterprise.ZArchitecture.ZGrid();
			this.PrincipalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImageGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImageBoundImageSelectionControl = new ImageSelectionControl();
			this.TermsAndConditionsLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).BeginInit();
			this.PrincipalGroupBox.SuspendLayout();
			this.ImageGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DeliveryOrderCollection);
			// 
			// PrincipalGrid
			// 
			this.PrincipalGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PrincipalGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)).PrincipalPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)).Principals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)).PrintParameter)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)).PrintParameters)));
			this.PrincipalGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Principals";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DeliveryOrderUserControl|16ad2d07-8b81-41f3-b17e-1cd93869258f", "Principal");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "PrincipalPK";
			zGuidFindBoxColumnStyleInfo1.IsMandatory = true;
			zGuidFindBoxColumnStyleInfo1.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			zDropEditColumnStyleInfo1.BindToList = "PrintParameters";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DeliveryOrderUserControl|dcc2e99f-9e56-4ec9-8dbc-8d1676bf38db", "Print Parameter");
			zDropEditColumnStyleInfo1.ColumnName = "PrintParameter";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			this.PrincipalGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.PrincipalGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PrincipalGrid.GridId = "9fbc7256-827f-4036-8a49-6c625d4896a5";
			this.PrincipalGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrincipalGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PrincipalGrid.LayoutKey = "PrincipalGrid";
			this.PrincipalGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.PrincipalGrid.Name = "PrincipalGrid";
			this.PrincipalGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 52, true);
			this.PrincipalGrid.TabIndex = 2;
			// 
			// PrincipalGroupBox
			// 
			this.PrincipalGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DeliveryOrderUserControl|68caad33-cf82-4f63-8b67-14ea2dc5ee69", "Principal");
			this.PrincipalGroupBox.Controls.Add(this.PrincipalGrid);
			this.PrincipalGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PrincipalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PrincipalGroupBox.Name = "PrincipalGroupBox";
			this.PrincipalGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.PrincipalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 77, true);
			this.PrincipalGroupBox.TabIndex = 4;
			this.PrincipalGroupBox.TabStop = false;
			// 
			// ImageGroupBox
			// 
			this.ImageGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DeliveryOrderUserControl|41249963-902d-4875-a27a-feaaa21d5ca1", "Terms and Conditions");
			this.ImageGroupBox.Controls.Add(this.ImageBoundImageSelectionControl);
			this.ImageGroupBox.Controls.Add(this.TermsAndConditionsLabel);
			this.ImageGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ImageGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 80, true);
			this.ImageGroupBox.Name = "ImageGroupBox";
			this.ImageGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(6, true);
			this.ImageGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(424, 224, true);
			this.ImageGroupBox.TabIndex = 6;
			this.ImageGroupBox.TabStop = false;
			// 
			// ImageBoundImageSelectionControl
			// 
			this.BindingSource.SetBindingMember(this.ImageBoundImageSelectionControl, "Image");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Drawing.Image)(((Enterprise.DocumentEngineCore.Registry.DeliveryOrder)(null)).Image)));
			this.ImageBoundImageSelectionControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImageBoundImageSelectionControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 60, true);
			this.ImageBoundImageSelectionControl.Name = "ImageBoundImageSelectionControl";
			this.ImageBoundImageSelectionControl.ReadOnly = false;
			this.ImageBoundImageSelectionControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 158, true);
			this.ImageBoundImageSelectionControl.TabIndex = 3;
			// 
			// TermsAndConditionsLabel
			// 
			this.TermsAndConditionsLabel.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DeliveryOrderUserControl|b9179b4b-cbe5-4339-bac1-6d3b18029b6a", "Choose an image to be used as Terms and Conditions on Delivery Order");
			this.TermsAndConditionsLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TermsAndConditionsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.TermsAndConditionsLabel.Name = "TermsAndConditionsLabel";
			this.TermsAndConditionsLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0, true);
			this.TermsAndConditionsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(412, 41, true);
			this.TermsAndConditionsLabel.TabIndex = 6;
			// 
			// DeliveryOrderUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PrincipalGroupBox);
			this.Controls.Add(this.ImageGroupBox);
			this.Name = "DeliveryOrderUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 307, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PrincipalGrid)).EndInit();
			this.PrincipalGroupBox.ResumeLayout(false);
			this.ImageGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid PrincipalGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox PrincipalGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox ImageGroupBox;
		private ImageSelectionControl ImageBoundImageSelectionControl;
		private Enterprise.ZArchitecture.ZLabel TermsAndConditionsLabel;

	}
}
