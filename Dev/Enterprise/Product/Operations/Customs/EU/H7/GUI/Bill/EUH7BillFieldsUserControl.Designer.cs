using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7BillFieldsUserControl
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
			this.LocalReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LocationOfGoodsUserControl = new EU.GUI.LocationOfGoodsUserControl();
			this.GoodsValueConvertToLocalCurrencyControl = new Enterprise.Customs.GUI.ConvertToLocalCurrencyControl();
			this.AdditionalProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalProcedureCodesUserControl = new AdditionalProcedureCodesUserControl();
			this.StandAloneDeclarationUserControl = new EUH7StandAloneDeclarationUserControl();
			this.ContainerUserControl = new EUH7ContainerUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaBill);
			//
			// LocalReferenceNumberTextBox
			//
			this.LocalReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalReferenceNumberTextBox, "LocalReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).LocalReferenceNumber)));
			this.LocalReferenceNumberTextBox.Name = "LocalReferenceNumberTextBox";
			//
			// MovementReferenceNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.MovementReferenceNumberTextBox, "MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).MovementReferenceNumber)));
			this.MovementReferenceNumberTextBox.Name = "MovementReferenceNumberTextBox";
			//
			// LocationOfGoodsUserControl
			//
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			//
			// GoodsValueConvertToLocalCurrencyControl
			//
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_GoodsValue)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaBill)(null)).ABL_RX_NKGoodsValueCurrency)));
			this.GoodsValueConvertToLocalCurrencyControl.AllowDrop = true;
			this.GoodsValueConvertToLocalCurrencyControl.BindToAmount = AsycudaBill.Schema.ABL_GoodsValue;
			this.GoodsValueConvertToLocalCurrencyControl.BindToUnit = AsycudaBill.Schema.ABL_RX_NKGoodsValueCurrency;
			this.GoodsValueConvertToLocalCurrencyControl.FindBoxType = Enterprise.ZArchitecture.GUI.FindBoxType.Code;
			this.GoodsValueConvertToLocalCurrencyControl.Name = "GoodsValueConvertToLocalCurrencyControl";
			//
			// AdditionalProcedureDropEdit
			//
			this.BindingSource.SetBindingMember(this.AdditionalProcedureDropEdit, "ABL_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).ABL_Procedure)));
			this.AdditionalProcedureDropEdit.AllowDrop = true;
			this.AdditionalProcedureDropEdit.Name = "AdditionalProcedureDropEdit";
			//
			// MessageStatusDropEdit
			//
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "ABL_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ASYCUDA.Business.AsycudaBill)(null)).ABL_MessageStatus)));
			this.MessageStatusDropEdit.AllowDrop = true;
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			//
			// AdditionalProcedureCodesUserControl
			//
			this.AdditionalProcedureCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalProcedureCodesUserControl, ".");
			this.AdditionalProcedureCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 196, true);
			this.AdditionalProcedureCodesUserControl.Name = "AdditionalProcedureCodesUserControl";
			this.AdditionalProcedureCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(388, 20, true);
			this.AdditionalProcedureCodesUserControl.TabIndex = 6;
			//
			// StandAloneDeclarationUserControl
			//
			this.StandAloneDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StandAloneDeclarationUserControl, ".");
			this.StandAloneDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StandAloneDeclarationUserControl.Name = "StandAloneDeclarationUserControl";
			this.StandAloneDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.StandAloneDeclarationUserControl.TabIndex = 7;
			//
			// ContainerUserControl
			//
			this.ContainerUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ContainerUserControl, ".");
			this.ContainerUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ContainerUserControl.Name = "ContainerUserControl";
			this.ContainerUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 20, true);
			this.ContainerUserControl.TabIndex = 8;
			//
			// EUH7BillFieldsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.LocalReferenceNumberTextBox);
			this.Controls.Add(this.MovementReferenceNumberTextBox);
			this.Controls.Add(this.GoodsValueConvertToLocalCurrencyControl);
			this.Controls.Add(this.AdditionalProcedureDropEdit);
			this.Controls.Add(this.MessageStatusDropEdit);
			this.Controls.Add(this.AdditionalProcedureCodesUserControl);
			this.Controls.Add(this.StandAloneDeclarationUserControl);
			this.Controls.Add(this.ContainerUserControl);
			this.Name = "EUH7BillFieldsUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox LocalReferenceNumberTextBox;
		internal Enterprise.ZArchitecture.ZTextBox MovementReferenceNumberTextBox;
		internal EU.GUI.LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal Enterprise.Customs.GUI.ConvertToLocalCurrencyControl GoodsValueConvertToLocalCurrencyControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit AdditionalProcedureDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal AdditionalProcedureCodesUserControl AdditionalProcedureCodesUserControl;
		internal EUH7StandAloneDeclarationUserControl StandAloneDeclarationUserControl;
		internal EUH7ContainerUserControl ContainerUserControl;
	}
}
