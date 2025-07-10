namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class PreviousDocumentUserControl
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
			this.TypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ItemNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ComplementTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NumOfPackagesDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.QuantityDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.StatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeCodeFindBox.SuspendLayout();
			this.NumOfPackagesDropEdit.SuspendLayout();
			this.QuantityDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument);
			// 
			// TypeCodeFindBox
			// 
			this.TypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Code)));
			this.TypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 20, true);
			this.TypeCodeFindBox.Name = "TypeCodeFindBox";
			this.TypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TypeCodeFindBox.ParentType = null;
			this.TypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.TypeCodeFindBox.TabIndex = 0;
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = null;
			this.ReferenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 48, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// ItemNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ItemNumberCalcEdit, "CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ItemNumber)));
			this.ItemNumberCalcEdit.CaptionResourceString = null;
			this.ItemNumberCalcEdit.DecimalPlaces = 0;
			this.ItemNumberCalcEdit.Decimals = 0;
			this.ItemNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 85, true);
			this.ItemNumberCalcEdit.Name = "ItemNumberCalcEdit";
			this.ItemNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.ItemNumberCalcEdit.TabIndex = 4;
			this.ItemNumberCalcEdit.Text = "0";
			this.ItemNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ComplementTextBox
			// 
			this.BindingSource.SetBindingMember(this.ComplementTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber2)));
			this.ComplementTextBox.CaptionResourceString = null;
			this.ComplementTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ComplementTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 176, true);
			this.ComplementTextBox.Name = "ComplementTextBox";
			this.ComplementTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.ComplementTextBox.TabIndex = 7;
			// 
			// NumOfPackagesDropEdit
			// 
			this.NumOfPackagesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NumOfPackagesDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity2)));
			this.NumOfPackagesDropEdit.BindToAmount = "CSI_Quantity2";
			this.NumOfPackagesDropEdit.BindToUnit = "CSI_UnitOfQuantity2";
			this.NumOfPackagesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 113, true);
			this.NumOfPackagesDropEdit.Name = "NumOfPackagesDropEdit";
			this.NumOfPackagesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.NumOfPackagesDropEdit.TabIndex = 8;
			// 
			// QuantityDropEdit
			// 
			this.QuantityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuantityDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity)));
			this.QuantityDropEdit.BindToAmount = "CSI_Quantity";
			this.QuantityDropEdit.BindToUnit = "CSI_UnitOfQuantity";
			this.QuantityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 147, true);
			this.QuantityDropEdit.Name = "QuantityDropEdit";
			this.QuantityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.QuantityDropEdit.TabIndex = 9;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_LineNo)));
			this.LineNoCalcEdit.CaptionResourceString = null;
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 202, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.LineNoCalcEdit.TabIndex = 10;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// StatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.StatusTextBox, "CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Status)));
			this.StatusTextBox.CaptionResourceString = null;
			this.StatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(101, 228, true);
			this.StatusTextBox.Name = "StatusTextBox";
			this.StatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 20, true);
			this.StatusTextBox.TabIndex = 11;
			// 
			// PreviousDocumentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StatusTextBox);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Controls.Add(this.QuantityDropEdit);
			this.Controls.Add(this.NumOfPackagesDropEdit);
			this.Controls.Add(this.ComplementTextBox);
			this.Controls.Add(this.ItemNumberCalcEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.TypeCodeFindBox);
			this.Name = "PreviousDocumentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 289, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TypeCodeFindBox.ResumeLayout(true);
			this.TypeCodeFindBox.PerformLayout();
			this.NumOfPackagesDropEdit.ResumeLayout(true);
			this.NumOfPackagesDropEdit.PerformLayout();
			this.QuantityDropEdit.ResumeLayout(true);
			this.QuantityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox TypeCodeFindBox;
		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.ZCalcEdit ItemNumberCalcEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit NumOfPackagesDropEdit;
		internal ZArchitecture.GUI.ZCalcDropEdit QuantityDropEdit;
		internal ZArchitecture.ZTextBox ComplementTextBox;
		internal ZArchitecture.ZCalcEdit LineNoCalcEdit;
		internal ZArchitecture.ZTextBox StatusTextBox;
	}
}
