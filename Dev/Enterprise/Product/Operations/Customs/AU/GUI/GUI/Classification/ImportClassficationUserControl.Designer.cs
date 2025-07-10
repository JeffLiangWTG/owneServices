using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ImportClassificationUserControl
	{
		void InitializeComponent()
		{
			this.otherGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.instrumentNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.cMRDefaultCPDecAnswersButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cC_AddInfoBoundAddInfoCMRControl = new Enterprise.Customs.AU.Declaration.GUI.AddInfoControlOptionalCMR();
			this.instrumentTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.treatmentCodeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.tariffFindBox = new Enterprise.Customs.AU.Declaration.GUI.UniversalTariffImportFindBox();
			this.tariffFindBoxAUCClass = new Enterprise.Customs.AU.Declaration.GUI.AUCClassFindBox();
			this.BaseClassificationGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.otherGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.Controls.Add(this.tariffFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.tariffFindBoxAUCClass);
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 155, true);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.LastAuditDateEdit, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.AuditStaffCodeFindBox, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffFindBoxAUCClass, 0);
			this.BaseClassificationGroupBox.Controls.SetChildIndex(this.tariffFindBox, 0);
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
			// OtherGroupBox
			// 
			this.otherGroupBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|7d94bcb2-442b-4025-a7dd-d321cf35a8fa", "Additional Information");
			this.otherGroupBox.Controls.Add(this.instrumentNumberCodeFindBox);
			this.otherGroupBox.Controls.Add(this.cMRDefaultCPDecAnswersButton);
			this.otherGroupBox.Controls.Add(this.cC_AddInfoBoundAddInfoCMRControl);
			this.otherGroupBox.Controls.Add(this.instrumentTypeBoundDropEdit);
			this.otherGroupBox.Controls.Add(this.treatmentCodeBoundDropEdit);
			this.otherGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.otherGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 155, true);
			this.otherGroupBox.Name = "OtherGroupBox";
			this.otherGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 103, true);
			this.otherGroupBox.TabIndex = 1;
			this.otherGroupBox.TabStop = false;
			// 
			// InstrumentNumberCodeFindBox
			// 
			this.instrumentNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.instrumentNumberCodeFindBox, "InstrumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).InstrumentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).AddInfo.Lookups.CMRInstrumentNumberList)));
			this.instrumentNumberCodeFindBox.BindToList = "AddInfo+Lookups+CMRInstrumentNumberList";
			this.instrumentNumberCodeFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|ff0e1317-fae2-4549-8b16-daece57e4151", "Instrument Code", "When quoted this number identifies the instrument which provides the means of obtaining a concessional rate of duty (from that which would normally be payable.) It is always used in conjunction with Instrument Type. \r\n\r\nExample: Tariff / Stat: 3906.90.00. 08, Treatment: 505, Treatment Instrument Type: TC, Treatment Instrument Number: 8908359");
			this.instrumentNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 44, true);
			this.instrumentNumberCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.InstrumentNumber;
			this.instrumentNumberCodeFindBox.Name = "InstrumentNumberCodeFindBox";
			this.instrumentNumberCodeFindBox.PopupCaption = "Instrument Number";
			this.instrumentNumberCodeFindBox.PreBoundMaxLength = 8;
			this.instrumentNumberCodeFindBox.ShowDescriptionBox = false;
			this.instrumentNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(98, 20, true);
			this.instrumentNumberCodeFindBox.TabIndex = 2;
			this.instrumentNumberCodeFindBox.Tag = "";
			// 
			// CMRDefaultCPDecAnswersButton
			// 
			this.cMRDefaultCPDecAnswersButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(382, 72, true);
			this.cMRDefaultCPDecAnswersButton.Name = "CMRDefaultCPDecAnswersButton";
			this.cMRDefaultCPDecAnswersButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.cMRDefaultCPDecAnswersButton.TabIndex = 4;
			this.cMRDefaultCPDecAnswersButton.Text = "CP Dec Answers";
			this.cMRDefaultCPDecAnswersButton.Click += new System.EventHandler(this.CMRDefaultCPDecAnswersButton_Click);
			// 
			// CC_AddInfoBoundAddInfoCMRControl
			// 
			this.cC_AddInfoBoundAddInfoCMRControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.cC_AddInfoBoundAddInfoCMRControl, "AddInfo+AddInfoLine");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).AddInfo.AddInfoLine)));
			this.cC_AddInfoBoundAddInfoCMRControl.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|439129c4-7bfa-4064-9748-5caea6cee945", "CMR Add Info");
			this.cC_AddInfoBoundAddInfoCMRControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 16, true);
			this.cC_AddInfoBoundAddInfoCMRControl.Name = "CC_AddInfoBoundAddInfoCMRControl";
			this.cC_AddInfoBoundAddInfoCMRControl.ShowCMRAddInfo = true;
			this.cC_AddInfoBoundAddInfoCMRControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 20, true);
			this.cC_AddInfoBoundAddInfoCMRControl.TabIndex = 0;
			// 
			// InstrumentTypeBoundDropEdit
			// 
			this.instrumentTypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.instrumentTypeBoundDropEdit, "AddInfo+ZA_InstrumentType_Hidden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).AddInfo.ZA_InstrumentType_Hidden)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).AddInfo.Lookups.ZA_InstrumentType_List)));
			this.instrumentTypeBoundDropEdit.BindToList = "AddInfo+Lookups+ZA_InstrumentType_List";
			this.instrumentTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 44, true);
			this.instrumentTypeBoundDropEdit.Name = "InstrumentTypeBoundDropEdit";
			this.instrumentTypeBoundDropEdit.PreBoundMaxLength = 3;
			this.instrumentTypeBoundDropEdit.ShowDescriptionBox = false;
			this.instrumentTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.instrumentTypeBoundDropEdit.TabIndex = 1;
			// 
			// TreatmentCodeBoundDropEdit
			// 
			this.treatmentCodeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.treatmentCodeBoundDropEdit, "TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).TreatmentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).TreatmentCode_List)));
			this.treatmentCodeBoundDropEdit.BindToList = "TreatmentCode_List";
			this.treatmentCodeBoundDropEdit.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|72995dda-a2f9-4c31-827e-1545e7522c37", "Treatment Code");
			this.treatmentCodeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(144, 72, true);
			this.treatmentCodeBoundDropEdit.Name = "TreatmentCodeBoundDropEdit";
			this.treatmentCodeBoundDropEdit.PreBoundMaxLength = 3;
			this.treatmentCodeBoundDropEdit.ShowDescriptionBox = false;
			this.treatmentCodeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.treatmentCodeBoundDropEdit.TabIndex = 3;
			// 
			// tariffFindBox
			// 
			this.tariffFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffFindBox, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).CC_TariffNum)));
			this.tariffFindBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|5dc61254-6323-4abd-a3a6-ebc4d1c82a16", "Tariff Number");
			this.tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.tariffFindBox.Name = "tariffFindBox";
			this.tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 21, true);
			this.tariffFindBox.TabIndex = 1;
			this.tariffFindBox.Visible = false;
			// 
			// tariffFindBoxAUCClass
			// 
			this.tariffFindBoxAUCClass.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.tariffFindBoxAUCClass, "CC_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.Classification)(null)).CC_TariffNum)));
			this.tariffFindBoxAUCClass.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("ImportClassificationUserControl|5dc61254-6323-4abd-a3a6-ebc4d1c82a16", "Tariff Number");
			this.tariffFindBoxAUCClass.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 52, true);
			this.tariffFindBoxAUCClass.Name = "tariffFindBoxAUCClass";
			this.tariffFindBoxAUCClass.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 21, true);
			this.tariffFindBoxAUCClass.TabIndex = 1;
			this.tariffFindBoxAUCClass.Visible = false;
			// 
			// ImportClassificationUserControl
			// 
			this.Controls.Add(this.otherGroupBox);
			this.Name = "ImportClassificationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 258, true);
			this.Controls.SetChildIndex(this.BaseClassificationGroupBox, 0);
			this.Controls.SetChildIndex(this.otherGroupBox, 0);
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.otherGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		protected internal UniversalTariffImportFindBox tariffFindBox;
		protected internal AUCClassFindBox tariffFindBoxAUCClass;
		ZGroupBox otherGroupBox;
		ZDropEdit instrumentTypeBoundDropEdit;
		ZDropEdit treatmentCodeBoundDropEdit;
		AddInfoControlOptionalCMR cC_AddInfoBoundAddInfoCMRControl;
		ZButton cMRDefaultCPDecAnswersButton;
		ZCodeFindBox instrumentNumberCodeFindBox;
	}
}
