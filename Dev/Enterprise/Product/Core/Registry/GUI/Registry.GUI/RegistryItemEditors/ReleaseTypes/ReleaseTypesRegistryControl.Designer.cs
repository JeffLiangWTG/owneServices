using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	partial class ReleaseTypesRegistryControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.ReleaseTypesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zOriginalsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zCopiesCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseTypesGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.ReleaseTypes);
			// 
			// ReleaseTypesGrid
			// 
			this.ReleaseTypesGrid.AllowNavigation = false;
			this.ReleaseTypesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseTypesGrid, "Types");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.ReleaseTypes)(null)).Types)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ReleaseType)(((System.Collections.IList)(((Enterprise.Registry.Business.ReleaseTypes)(null)).Types)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.ReleaseType)(((System.Collections.IList)(((Enterprise.Registry.Business.ReleaseTypes)(null)).Types)).SyncRoot)).EnglishDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ReleaseType)(((System.Collections.IList)(((Enterprise.Registry.Business.ReleaseTypes)(null)).Types)).SyncRoot)).OriginalsNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ReleaseType)(((System.Collections.IList)(((Enterprise.Registry.Business.ReleaseTypes)(null)).Types)).SyncRoot)).CopiesNumber)));
			this.ReleaseTypesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|c0db4112-afb7-4fc6-b0f9-9a5fe61217b2", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|e4b6b3b5-31a2-479e-8a75-3398ac6c5f66", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EnglishDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|1c55f922-92ac-4183-a347-ca8efa96c80b", "Originals");
			zCalcEditColumnStyleInfo1.ColumnName = "OriginalsNumber";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|bedb7874-4654-4aa7-81c0-99da66869458", "Copies");
			zCalcEditColumnStyleInfo2.ColumnName = "CopiesNumber";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(56);
			this.ReleaseTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ReleaseTypesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ReleaseTypesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ReleaseTypesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ReleaseTypesGrid.GridId = "d8873736-f87a-49a5-b0f2-ea10563c8568";
			this.ReleaseTypesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ReleaseTypesGrid.LayoutKey = "ReleaseTypesGrid";
			this.ReleaseTypesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 55, true);
			this.ReleaseTypesGrid.Name = "ReleaseTypesGrid";
			this.ReleaseTypesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 384, true);
			this.ReleaseTypesGrid.TabIndex = 2;
			// 
			// zOriginalsCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zOriginalsCalcEdit, "OriginalsNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ReleaseTypes)(null)).OriginalsNumber)));
			this.zOriginalsCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|ddb5326f-1ac6-4861-9281-2cb740572726", "Default Number of Originals");
			this.zOriginalsCalcEdit.Decimals = 0;
			this.zOriginalsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 3, true);
			this.zOriginalsCalcEdit.Name = "zOriginalsCalcEdit";
			this.zOriginalsCalcEdit.ShowGroupSeparators = false;
			this.zOriginalsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.zOriginalsCalcEdit.TabIndex = 0;
			this.zOriginalsCalcEdit.Text = "0";
			this.zOriginalsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zCopiesCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.zCopiesCalcEdit, "CopiesNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Registry.Business.ReleaseTypes)(null)).CopiesNumber)));
			this.zCopiesCalcEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ReleaseTypesRegistryControl|04c4c962-4fc5-4395-b18d-411c3d6474b0", "Default Number of Copies");
			this.zCopiesCalcEdit.Decimals = 0;
			this.zCopiesCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 29, true);
			this.zCopiesCalcEdit.Name = "zCopiesCalcEdit";
			this.zCopiesCalcEdit.ShowGroupSeparators = false;
			this.zCopiesCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 20, true);
			this.zCopiesCalcEdit.TabIndex = 1;
			this.zCopiesCalcEdit.Text = "0";
			this.zCopiesCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ReleaseTypesRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zCopiesCalcEdit);
			this.Controls.Add(this.zOriginalsCalcEdit);
			this.Controls.Add(this.ReleaseTypesGrid);
			this.Name = "ReleaseTypesRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(465, 439, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ReleaseTypesGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid ReleaseTypesGrid;
		private Enterprise.ZArchitecture.ZCalcEdit zOriginalsCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit zCopiesCalcEdit;
	}
}
