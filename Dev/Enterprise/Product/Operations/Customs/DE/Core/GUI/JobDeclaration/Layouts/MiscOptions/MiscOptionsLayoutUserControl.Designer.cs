using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.DutyAccountNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VatPaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VATAccountNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.VATClaimBackDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StatisticStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DutyAccountNumberDropEdit.SuspendLayout();
            this.VatPaymentPartyDropEdit.SuspendLayout();
            this.VATAccountNumberDropEdit.SuspendLayout();
            this.VATClaimBackDropEdit.SuspendLayout();
            this.StatisticStatusDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.JobDeclaration);
            // 
            // DutyAccountNumberDropEdit
            // 
            this.DutyAccountNumberDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DutyAccountNumberDropEdit, "JE_DefermentAccountNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).JE_DefermentAccountNumber)));
            this.DutyAccountNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 42, true);
            this.DutyAccountNumberDropEdit.Name = "DutyAccountNumberDropEdit";
            this.DutyAccountNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.DutyAccountNumberDropEdit.TabIndex = 0;
            // 
            // VatPaymentPartyDropEdit
            // 
            this.VatPaymentPartyDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VatPaymentPartyDropEdit, "ZG_VATDeferType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferType)));
            this.VatPaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 69, true);
            this.VatPaymentPartyDropEdit.Name = "VatPaymentPartyDropEdit";
            this.VatPaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.VatPaymentPartyDropEdit.TabIndex = 1;
            // 
            // VATAccountNumberDropEdit
            // 
            this.VATAccountNumberDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VATAccountNumberDropEdit, "ZG_VATDeferNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).ZG_VATDeferNumber)));
            this.VATAccountNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 16, true);
            this.VATAccountNumberDropEdit.Name = "VATAccountNumberDropEdit";
            this.VATAccountNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.VATAccountNumberDropEdit.TabIndex = 2;
            // 
            // VATClaimBackDropEdit
            // 
            this.VATClaimBackDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VATClaimBackDropEdit, "JE_VATClaimBack");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).JE_VATClaimBack)));
            this.VATClaimBackDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.VATClaimBackDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 93, true);
            this.VATClaimBackDropEdit.Name = "VATClaimBackDropEdit";
            this.VATClaimBackDropEdit.PreBoundMaxLength = 1;
            this.VATClaimBackDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(146, 20, true);
            this.VATClaimBackDropEdit.TabIndex = 3;
            // 
            // StatisticStatusDropEdit
            // 
            this.StatisticStatusDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StatisticStatusDropEdit, "JE_StatisticStatus");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.JobDeclaration)(null)).JE_StatisticStatus)));
            this.StatisticStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 118, true);
            this.StatisticStatusDropEdit.Name = "StatisticStatusDropEdit";
            this.StatisticStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
            this.StatisticStatusDropEdit.TabIndex = 4;
            // 
            // MiscOptionsLayoutUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.DutyAccountNumberDropEdit);
            this.Controls.Add(this.VatPaymentPartyDropEdit);
            this.Controls.Add(this.VATAccountNumberDropEdit);
            this.Controls.Add(this.VATClaimBackDropEdit);
            this.Controls.Add(this.StatisticStatusDropEdit);
            this.Name = "MiscOptionsLayoutUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 163, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DutyAccountNumberDropEdit.ResumeLayout(true);
            this.DutyAccountNumberDropEdit.PerformLayout();
            this.VatPaymentPartyDropEdit.ResumeLayout(true);
            this.VatPaymentPartyDropEdit.PerformLayout();
            this.VATAccountNumberDropEdit.ResumeLayout(true);
            this.VATAccountNumberDropEdit.PerformLayout();
            this.VATClaimBackDropEdit.ResumeLayout(true);
            this.VATClaimBackDropEdit.PerformLayout();
            this.StatisticStatusDropEdit.ResumeLayout(true);
            this.StatisticStatusDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit VATAccountNumberDropEdit;
		internal ZArchitecture.GUI.ZDropEdit VatPaymentPartyDropEdit;
		internal ZArchitecture.GUI.ZDropEdit DutyAccountNumberDropEdit;
		internal ZArchitecture.GUI.ZDropEdit VATClaimBackDropEdit;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit StatisticStatusDropEdit;
	}
}
