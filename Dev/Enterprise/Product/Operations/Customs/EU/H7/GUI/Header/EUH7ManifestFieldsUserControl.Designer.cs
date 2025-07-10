using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ManifestFieldsUserControl
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
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsOfficeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PresentationOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SubmitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PresenterAddressControl = new ZArchitecture.GUI.ZAddressControl();
			this.ConsolidatedStatusSeparatorUserControl = new ZArchitecture.GUI.SeparatorUserControl();
			this.ConsolidatedCustomsStatusDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader);
			// 
			// CustomsOfficeLabel
			//
			this.CustomsOfficeLabel.Name = "CustomsOfficeLabel";
			this.CustomsOfficeLabel.CaptionResourceString = Res.GetData("e5df0221-4159-4c8f-b001-1b6f326cb248", "Customs Offices");
			this.CustomsOfficeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.CustomsOfficeLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(10, 0, 0, 0);
			this.CustomsOfficeLabel.IsFontBold = true;
			this.CustomsOfficeLabel.TabIndex = 0;
			// 
			// CustomsOfficeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "AMA_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader)(null)).AMA_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.CaptionResourceString = Res.GetData("2cacbcfb-6b1f-43cc-9fb6-1cc899d305fb", "Lodgement", "Lodgement", "Lodgement", "Code identifying the Customs Office of Lodgement of the declaration.");
			this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// PresentationOfficeFindBox
			// 
			this.BindingSource.SetBindingMember(this.PresentationOfficeCodeFindBox, "PresentationOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader)(null)).PresentationOffice)));
			this.PresentationOfficeCodeFindBox.Name = "PresentationOfficeCodeFindBox";
			this.PresentationOfficeCodeFindBox.CaptionResourceString = Res.GetData("feb8ed1e-7d35-46b4-b61a-afa8ff12e223", "Presentation", "Presentation", "Presentation", "Code identifying the Customs Office of goods presentation.");
			this.PresentationOfficeCodeFindBox.TabIndex = 2;
			// 
			// SubmitTypeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.SubmitTypeDropEdit, "AMA_ApplicationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader)(null)).AMA_ApplicationCode)));
			this.SubmitTypeDropEdit.Name = "SubmitTypeDropEdit";
			this.SubmitTypeDropEdit.TabIndex = 3;
			// 
			// PresenterAddressControl
			// 
			this.PresenterAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresenterAddressControl, "AMA_OA_Presenter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader)(null)).AMA_OA_Presenter)));
			this.PresenterAddressControl.Name = "PresenterAddressControl";
			this.PresenterAddressControl.ShowAddress = false;
			this.PresenterAddressControl.TabIndex = 4;
			//
			// ConsolidatedStatusSeparatorUserControl
			//
			this.ConsolidatedStatusSeparatorUserControl.Name = "ConsolidatedStatusSeparatorUserControl";
			this.ConsolidatedStatusSeparatorUserControl.CaptionResourceString = Res.GetData("54bdd366-a01b-45ee-9103-190caef6a12c", "Consolidated Status");
			this.ConsolidatedStatusSeparatorUserControl.TabIndex = 5;
			//
			// ConsolidatedCustomsStatusDropEdit
			//
			this.BindingSource.SetBindingMember(this.ConsolidatedCustomsStatusDropEdit, "RegistrationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.H7.Business.AsycudaManifestHeader)(null)).RegistrationStatus)));
			this.ConsolidatedCustomsStatusDropEdit.Name = "ConsolidatedCustomsStatusDropEdit";
			this.ConsolidatedCustomsStatusDropEdit.CaptionResourceString = Res.GetData("1066c692-42a4-41f4-a5d4-5822137ea2d1", "Cus. Status", "Cus. Status", "Customs Status", "Code identifying the customs status of the declaration.");
			this.ConsolidatedCustomsStatusDropEdit.TabIndex = 6;
			// 
			// EUH7ManifestFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EUH7ManifestFieldsUserControl";
			this.Controls.Add(this.CustomsOfficeLabel);
			this.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.Controls.Add(this.PresentationOfficeCodeFindBox);
			this.Controls.Add(this.SubmitTypeDropEdit);
			this.Controls.Add(this.PresenterAddressControl);
			this.Controls.Add(this.ConsolidatedStatusSeparatorUserControl);
			this.Controls.Add(this.ConsolidatedCustomsStatusDropEdit);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.ZLabel CustomsOfficeLabel;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZCodeFindBox PresentationOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit SubmitTypeDropEdit;
		internal ZArchitecture.GUI.ZAddressControl PresenterAddressControl;
		internal ZArchitecture.GUI.SeparatorUserControl ConsolidatedStatusSeparatorUserControl;
		internal ZArchitecture.GUI.ZDropEdit ConsolidatedCustomsStatusDropEdit;
	}
}
