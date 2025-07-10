namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class ExportClassificationUserControl
	{
		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.tariffFindBoxAHECC = new Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox();
			this.tariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			this.LastAuditDateEdit.SuspendLayout();
			this.AuditStaffCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.tariffFindBoxAHECC.SuspendLayout();
			this.tariffFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.tariffFindBoxAHECC);
			this.BaseClassificationGroupBox.Controls.Add(this.tariffFindBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 264, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffFindBoxAHECC, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LookupCodeTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.DescriptionTextBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.CC_IsActiveCheckBox, 0);
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.Classification);
			// 
			// tariffFindBoxAHECC
			// 
			this.tariffFindBoxAHECC.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffFindBoxAHECC, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).CC_TariffNum)));
			this.tariffFindBoxAHECC.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ExportClassificationUserControl|5dc61254-6323-4abd-a3a6-ebc4d1c82a16", "Lookup");
			this.tariffFindBoxAHECC.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.tariffFindBoxAHECC.Name = "tariffFindBoxAHECC";
			this.tariffFindBoxAHECC.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.tariffFindBoxAHECC.ParentType = null;
			this.tariffFindBoxAHECC.PreBoundMaxLength = 10;
			this.tariffFindBoxAHECC.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 20, true);
			this.tariffFindBoxAHECC.TabIndex = 1;
			this.tariffFindBoxAHECC.Visible = false;
			// 
			// tariffFindBox
			// 
			this.tariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).CC_TariffNum)));
			this.tariffFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ExportClassificationUserControl|5dc61254-6323-4abd-a3a6-ebc4d1c82a16", "Lookup");
			this.tariffFindBox.ErrorForUnsupportedCountry = null;
			this.tariffFindBox.GetEffectiveDate = null;
			this.tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.tariffFindBox.Name = "tariffFindBox";
			this.tariffFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.tariffFindBox.ParentType = null;
			this.tariffFindBox.PreBoundMaxLength = 10;
			this.tariffFindBox.SelectNomenclatureModes = null;
			this.tariffFindBox.ShowDescriptionFilterOnNonNomenclatureTariffModule = false;
			this.tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 20, true);
			this.tariffFindBox.TabIndex = 1;
			this.tariffFindBox.TariffType = "EXP";
			this.tariffFindBox.Visible = false;
			// 
			// ExportClassificationUserControl
			// 
			this.Name = "ExportClassificationUserControl";
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			this.LastAuditDateEdit.ResumeLayout(true);
			this.LastAuditDateEdit.PerformLayout();
			this.AuditStaffCodeFindBox.ResumeLayout(true);
			this.AuditStaffCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.tariffFindBoxAHECC.ResumeLayout(true);
			this.tariffFindBoxAHECC.PerformLayout();
			this.tariffFindBox.ResumeLayout(true);
			this.tariffFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		#endregion

		protected internal Enterprise.Customs.AU.Declaration.GUI.AHECCFindBox tariffFindBoxAHECC;
		protected internal Enterprise.Customs.AU.Declaration.GUI.UniversalTariffExportFindBox tariffFindBox;
	}
}
