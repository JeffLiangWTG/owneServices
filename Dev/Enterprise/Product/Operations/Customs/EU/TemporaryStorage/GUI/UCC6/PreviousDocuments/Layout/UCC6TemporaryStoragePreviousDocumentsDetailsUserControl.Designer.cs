using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStoragePreviousDocumentsDetailsUserControl
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
			this.PackageCalcDropEdit = new ZCalcDropEdit();
			this.QuantityCalcDropEdit = new ZCalcDropEdit();
			this.GoodItemIdentifierCalcEdit = new ZCalcEdit();
			this.TypeCodeFindBox = new ZCodeFindBox();
			this.ReferenceNumberTextBox = new ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeCodeFindBox.SuspendLayout();
			this.PackageCalcDropEdit.SuspendLayout();
			this.QuantityCalcDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TemporaryStoragePreviousDocument);
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TemporaryStoragePreviousDocument)(null)).CSI_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = Res.GetData("ba650148-07b8-4c50-aced-be485adbd03b", "Reference Number");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 17, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 17, true);
			this.ReferenceNumberTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.ReferenceNumberTextBox.TabIndex = 1;
			// 
			// GoodItemIdentifierCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.GoodItemIdentifierCalcEdit, "CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TemporaryStoragePreviousDocument)(null)).CSI_LineNo)));
			this.GoodItemIdentifierCalcEdit.CaptionResourceString = Res.GetData("70686752-0b66-4116-99c0-c3f01d98edeb", "Goods Item Identifier");
			this.GoodItemIdentifierCalcEdit.DecimalPlaces = 2;
			this.GoodItemIdentifierCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 45, true);
			this.GoodItemIdentifierCalcEdit.Name = "GoodItemIdentifierCalcEdit";
			this.GoodItemIdentifierCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.GoodItemIdentifierCalcEdit.TabIndex = 3;
			this.GoodItemIdentifierCalcEdit.MaxValue = 99999;
			this.GoodItemIdentifierCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.GoodItemIdentifierCalcEdit.TrackDisposedAccess = true;
			// 
			// TypeCodeFindBox
			// 
			this.TypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TemporaryStoragePreviousDocument)(null)).CSI_Code)));
			this.TypeCodeFindBox.CaptionResourceString = Res.GetData("6B0E7C1D-79BD-4111-BAF5-73DE9F2F1924", "Type");
			this.TypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 75, true);
			this.TypeCodeFindBox.Name = "TypeCodeFindBox";
			this.TypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TypeCodeFindBox.ParentType = null;
			this.TypeCodeFindBox.PreBoundMaxLength = 4;
			this.TypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.TypeCodeFindBox.TabIndex = 4;
			// 
			// PackageCalcDropEdit
			//
			this.PackageCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TemporaryStoragePreviousDocument)(null)).CSI_PackQty)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TemporaryStoragePreviousDocument)(null)).CSI_PackType)));
			this.PackageCalcDropEdit.BindToAmount = "CSI_PackQty";
			this.PackageCalcDropEdit.BindToUnit = "CSI_PackType";
			this.PackageCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 105, true);
			this.PackageCalcDropEdit.Name = "PackageCalcDropEdit";
			this.PackageCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.PackageCalcDropEdit.TabIndex = 5;
			this.PackageCalcDropEdit.UnitPreBoundMaxLength = 2;
			this.PackageCalcDropEdit.MaxValue = 99999999;
			// 
			// QuantityCalcDropEdit
			//
			this.QuantityCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuantityCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((TemporaryStoragePreviousDocument)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TemporaryStoragePreviousDocument)(null)).CSI_UnitOfQuantity)));
			this.QuantityCalcDropEdit.BindToAmount = "CSI_Quantity";
			this.QuantityCalcDropEdit.BindToUnit = "CSI_UnitOfQuantity";
			this.QuantityCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 135, true);
			this.QuantityCalcDropEdit.Name = "QuantityCalcDropEdit";
			this.QuantityCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.QuantityCalcDropEdit.TabIndex = 6;
			this.QuantityCalcDropEdit.UnitPreBoundMaxLength = 2;
			this.QuantityCalcDropEdit.MaxValue = 9999999999m;
			// 
			// UCC6TemporaryStoragePreviousDocumentsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.GoodItemIdentifierCalcEdit);
			this.Controls.Add(this.PackageCalcDropEdit);
			this.Controls.Add(this.QuantityCalcDropEdit);
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.TypeCodeFindBox);
			this.Name = "UCC6TemporaryStoragePreviousDocumentsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 160, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackageCalcDropEdit.ResumeLayout(true);
			this.PackageCalcDropEdit.PerformLayout();
			this.QuantityCalcDropEdit.ResumeLayout(true);
			this.QuantityCalcDropEdit.PerformLayout();
			this.TypeCodeFindBox.ResumeLayout(true);
			this.TypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZCalcDropEdit PackageCalcDropEdit;
		internal ZCalcDropEdit QuantityCalcDropEdit;
		internal ZCalcEdit GoodItemIdentifierCalcEdit;
		internal ZCodeFindBox TypeCodeFindBox;
		internal ZTextBox ReferenceNumberTextBox;
	}
}
