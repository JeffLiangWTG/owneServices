
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class UCC6TemporaryStorageAdditionalInformationDetailsUserControl
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
			this.AmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CurrencyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DetailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.KindDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.FullTypeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CurrencyDropEdit.SuspendLayout();
			this.KindDropEdit.SuspendLayout();
			this.FullTypeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo);
			// 
			// AmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AmountCalcEdit, "CSI_Value");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_Value)));
			this.AmountCalcEdit.DecimalPlaces = 2;
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 212, true);
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.AmountCalcEdit.TabIndex = 6;
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CurrencyDropEdit
			// 
			this.CurrencyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyDropEdit, "CSI_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_RX_NKCurrency)));
			this.CurrencyDropEdit.CaptionResourceString = Res.GetData("38AE9A51-11AB-45E0-9FFA-59871D4F888F", "Currency");
			this.CurrencyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 186, true);
			this.CurrencyDropEdit.Name = "CurrencyDropEdit";
			this.CurrencyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.CurrencyDropEdit.TabIndex = 5;
			// 
			// DetailTextBox
			// 
			this.BindingSource.SetBindingMember(this.DetailTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_ReferenceNumber2)));
			this.DetailTextBox.CaptionResourceString = Res.GetData("17AB30A3-5D29-4765-8065-E2596DC66518", "Detail");
			this.DetailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 160, true);
			this.DetailTextBox.Name = "DetailTextBox";
			this.DetailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.DetailTextBox.TabIndex = 4;
			this.DetailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_ReferenceNumber)));
			this.ReferenceTextBox.CaptionResourceString = Res.GetData("008FB891-2734-48A2-BB67-4863D35EC380", "Reference");
			this.ReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 85, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.ReferenceTextBox.TabIndex = 2;
			// 
			// KindDropEdit
			// 
			this.KindDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.KindDropEdit, "CSI_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_SubType)));
			this.KindDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 33, true);
			this.KindDropEdit.Name = "KindDropEdit";
			this.KindDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.KindDropEdit.TabIndex = 0;
			// 
			// FullTypeCodeFindBox
			// 
			this.FullTypeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FullTypeCodeFindBox, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_Code)));
			this.FullTypeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 59, true);
			this.FullTypeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.FullTypeCodeFindBox.Name = "FullTypeCodeFindBox";
			this.FullTypeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FullTypeCodeFindBox.ParentType = null;
			this.FullTypeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 15, true);
			this.FullTypeCodeFindBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(null)).CSI_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 111, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 43, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// AdditionalInformationDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AmountCalcEdit);
			this.Controls.Add(this.CurrencyDropEdit);
			this.Controls.Add(this.DetailTextBox);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.KindDropEdit);
			this.Controls.Add(this.FullTypeCodeFindBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Name = "AdditionalInformationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(658, 264, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CurrencyDropEdit.ResumeLayout(true);
			this.CurrencyDropEdit.PerformLayout();
			this.KindDropEdit.ResumeLayout(true);
			this.KindDropEdit.PerformLayout();
			this.FullTypeCodeFindBox.ResumeLayout(true);
			this.FullTypeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZCalcEdit AmountCalcEdit;
		internal ZArchitecture.GUI.ZDropEdit CurrencyDropEdit;
		internal ZArchitecture.ZTextBox DetailTextBox;
		internal ZArchitecture.ZTextBox ReferenceTextBox;
		internal ZArchitecture.GUI.ZDropEdit KindDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox FullTypeCodeFindBox;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
	}
}
