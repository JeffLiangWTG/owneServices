namespace Enterprise.Customs.CN.GUI
{
	partial class CNSWClientSettingRegistryItemUserControl
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
			this.MachineNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RunningIntervalInSecondsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiveFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ErrorResponseFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArchiveFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EHubClientIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EHubClientStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACDAGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AcdaSendFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcdaReceiveFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcdaArchiveFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcdaErrorResponseFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DeclarationGroupBox.SuspendLayout();
			this.ACDAGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CNSWClientSetting);
			// 
			// MachineNameTextBox
			// 
			this.MachineNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MachineNameTextBox, "MachineName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).MachineName)));
			this.MachineNameTextBox.CaptionResourceString = null;
			this.MachineNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 26, true);
			this.MachineNameTextBox.Name = "MachineNameTextBox";
			this.MachineNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.MachineNameTextBox.TabIndex = 0;
			// 
			// RunningIntervalInSecondsCalcEdit
			// 
			this.RunningIntervalInSecondsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RunningIntervalInSecondsCalcEdit, "RunningIntervalInSeconds");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).RunningIntervalInSeconds)));
			this.RunningIntervalInSecondsCalcEdit.CaptionResourceString = null;
			this.RunningIntervalInSecondsCalcEdit.DecimalPlaces = 0;
			this.RunningIntervalInSecondsCalcEdit.Decimals = 0;
			this.RunningIntervalInSecondsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 104, true);
			this.RunningIntervalInSecondsCalcEdit.Name = "RunningIntervalInSecondsCalcEdit";
			this.RunningIntervalInSecondsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.RunningIntervalInSecondsCalcEdit.TabIndex = 3;
			this.RunningIntervalInSecondsCalcEdit.Text = "0";
			this.RunningIntervalInSecondsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SendFolderTextBox
			// 
			this.SendFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SendFolderTextBox, "SendFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).SendFolder)));
			this.SendFolderTextBox.CaptionResourceString = null;
			this.SendFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 19, true);
			this.SendFolderTextBox.Name = "SendFolderTextBox";
			this.SendFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.SendFolderTextBox.TabIndex = 0;
			// 
			// ReceiveFolderTextBox
			// 
			this.ReceiveFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReceiveFolderTextBox, "ReceiveFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).ReceiveFolder)));
			this.ReceiveFolderTextBox.CaptionResourceString = null;
			this.ReceiveFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 45, true);
			this.ReceiveFolderTextBox.Name = "ReceiveFolderTextBox";
			this.ReceiveFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.ReceiveFolderTextBox.TabIndex = 1;
			// 
			// ErrorResponseFolderTextBox
			// 
			this.ErrorResponseFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ErrorResponseFolderTextBox, "ErrorResponseFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).ErrorResponseFolder)));
			this.ErrorResponseFolderTextBox.CaptionResourceString = null;
			this.ErrorResponseFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 71, true);
			this.ErrorResponseFolderTextBox.Name = "ErrorResponseFolderTextBox";
			this.ErrorResponseFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.ErrorResponseFolderTextBox.TabIndex = 2;
			// 
			// ArchiveFolderTextBox
			// 
			this.ArchiveFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ArchiveFolderTextBox, "ArchiveFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).ArchiveFolder)));
			this.ArchiveFolderTextBox.CaptionResourceString = null;
			this.ArchiveFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 97, true);
			this.ArchiveFolderTextBox.Name = "ArchiveFolderTextBox";
			this.ArchiveFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.ArchiveFolderTextBox.TabIndex = 3;
			// 
			// EHubClientIDTextBox
			// 
			this.EHubClientIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EHubClientIDTextBox, "EHubClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).EHubClientID)));
			this.EHubClientIDTextBox.CaptionResourceString = null;
			this.EHubClientIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 52, true);
			this.EHubClientIDTextBox.Name = "EHubClientIDTextBox";
			this.EHubClientIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.EHubClientIDTextBox.TabIndex = 1;
			// 
			// EHubClientStatusTextBox
			// 
			this.EHubClientStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EHubClientStatusTextBox, "EHubClientStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).EHubClientStatus)));
			this.EHubClientStatusTextBox.CaptionResourceString = null;
			this.EHubClientStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 78, true);
			this.EHubClientStatusTextBox.Name = "EHubClientStatusTextBox";
			this.EHubClientStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.EHubClientStatusTextBox.TabIndex = 2;
			// 
			// DeclarationGroupBox
			// 
			this.DeclarationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("08b1179d-f695-4db4-b096-61d720426a9a", "Customs Declaration");
			this.DeclarationGroupBox.Controls.Add(this.SendFolderTextBox);
			this.DeclarationGroupBox.Controls.Add(this.ReceiveFolderTextBox);
			this.DeclarationGroupBox.Controls.Add(this.ArchiveFolderTextBox);
			this.DeclarationGroupBox.Controls.Add(this.ErrorResponseFolderTextBox);
			this.DeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 130, true);
			this.DeclarationGroupBox.Name = "DeclarationGroupBox";
			this.DeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 126, true);
			this.DeclarationGroupBox.TabIndex = 4;
			this.DeclarationGroupBox.TabStop = false;
			// 
			// ACDAGroupBox
			// 
			this.ACDAGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ACDAGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("ff7aca45-43f6-48ba-a9b5-6f78dd5ba96f", "Agreement of Customs Declaration Agent");
			this.ACDAGroupBox.Controls.Add(this.AcdaSendFolderTextBox);
			this.ACDAGroupBox.Controls.Add(this.AcdaReceiveFolderTextBox);
			this.ACDAGroupBox.Controls.Add(this.AcdaArchiveFolderTextBox);
			this.ACDAGroupBox.Controls.Add(this.AcdaErrorResponseFolderTextBox);
			this.ACDAGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 262, true);
			this.ACDAGroupBox.Name = "ACDAGroupBox";
			this.ACDAGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 126, true);
			this.ACDAGroupBox.TabIndex = 5;
			this.ACDAGroupBox.TabStop = false;
			// 
			// AcdaSendFolderTextBox
			// 
			this.AcdaSendFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcdaSendFolderTextBox, "AcdaSendFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).AcdaSendFolder)));
			this.AcdaSendFolderTextBox.CaptionResourceString = null;
			this.AcdaSendFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 19, true);
			this.AcdaSendFolderTextBox.Name = "AcdaSendFolderTextBox";
			this.AcdaSendFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.AcdaSendFolderTextBox.TabIndex = 0;
			// 
			// AcdaReceiveFolderTextBox
			// 
			this.AcdaReceiveFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcdaReceiveFolderTextBox, "AcdaReceiveFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).AcdaReceiveFolder)));
			this.AcdaReceiveFolderTextBox.CaptionResourceString = null;
			this.AcdaReceiveFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 45, true);
			this.AcdaReceiveFolderTextBox.Name = "AcdaReceiveFolderTextBox";
			this.AcdaReceiveFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.AcdaReceiveFolderTextBox.TabIndex = 1;
			// 
			// AcdaArchiveFolderTextBox
			// 
			this.AcdaArchiveFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcdaArchiveFolderTextBox, "AcdaArchiveFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).AcdaArchiveFolder)));
			this.AcdaArchiveFolderTextBox.CaptionResourceString = null;
			this.AcdaArchiveFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 97, true);
			this.AcdaArchiveFolderTextBox.Name = "AcdaArchiveFolderTextBox";
			this.AcdaArchiveFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.AcdaArchiveFolderTextBox.TabIndex = 3;
			// 
			// AcdaErrorResponseFolderTextBox
			// 
			this.AcdaErrorResponseFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcdaErrorResponseFolderTextBox, "AcdaErrorResponseFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CNSWClientSetting)(null)).AcdaErrorResponseFolder)));
			this.AcdaErrorResponseFolderTextBox.CaptionResourceString = null;
			this.AcdaErrorResponseFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 71, true);
			this.AcdaErrorResponseFolderTextBox.Name = "AcdaErrorResponseFolderTextBox";
			this.AcdaErrorResponseFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(267, 20, true);
			this.AcdaErrorResponseFolderTextBox.TabIndex = 2;
			// 
			// CNSWClientSettingRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ACDAGroupBox);
			this.Controls.Add(this.EHubClientStatusTextBox);
			this.Controls.Add(this.EHubClientIDTextBox);
			this.Controls.Add(this.MachineNameTextBox);
			this.Controls.Add(this.RunningIntervalInSecondsCalcEdit);
			this.Controls.Add(this.DeclarationGroupBox);
			this.Name = "CNSWClientSettingRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 390, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DeclarationGroupBox.ResumeLayout(false);
			this.DeclarationGroupBox.PerformLayout();
			this.ACDAGroupBox.ResumeLayout(false);
			this.ACDAGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZTextBox MachineNameTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit RunningIntervalInSecondsCalcEdit;

		#endregion

		private ZArchitecture.ZTextBox SendFolderTextBox;
		private ZArchitecture.ZTextBox ReceiveFolderTextBox;
		private ZArchitecture.ZTextBox ErrorResponseFolderTextBox;
		private ZArchitecture.ZTextBox ArchiveFolderTextBox;
		private ZArchitecture.ZTextBox EHubClientIDTextBox;
		private ZArchitecture.ZTextBox EHubClientStatusTextBox;
		private ZArchitecture.GUI.ZGroupBox DeclarationGroupBox;
		private ZArchitecture.GUI.ZGroupBox ACDAGroupBox;
		private ZArchitecture.ZTextBox AcdaSendFolderTextBox;
		private ZArchitecture.ZTextBox AcdaReceiveFolderTextBox;
		private ZArchitecture.ZTextBox AcdaArchiveFolderTextBox;
		private ZArchitecture.ZTextBox AcdaErrorResponseFolderTextBox;
	}
}
