using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CL.Manifest.GUI
{
	partial class CLSMSMessageSendingRegistryItemUserControl
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
			this.ApplicationNodeNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ApplicationNodePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RunningIntervalInSecondsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UnknownFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InvalidFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RejectedFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReceiveFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcceptedFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EnableSMSMessageSendingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.XtCredentialStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.XtCredentialStatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending);
			// 
			// MachineNameTextBox
			// 
			this.MachineNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MachineNameTextBox, "MachineName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).MachineName)));
			this.MachineNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 26, true);
			this.MachineNameTextBox.Name = "MachineNameTextBox";
			this.MachineNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.MachineNameTextBox.TabIndex = 0;
			// 
			// ApplicationNodeNameTextBox
			// 
			this.ApplicationNodeNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ApplicationNodeNameTextBox, "ApplicationNodeName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).ApplicationNodeName)));
			this.ApplicationNodeNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 52, true);
			this.ApplicationNodeNameTextBox.Name = "ApplicationNodeNameTextBox";
			this.ApplicationNodeNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.ApplicationNodeNameTextBox.TabIndex = 1;
			// 
			// ApplicationNodePasswordTextBox
			// 
			this.ApplicationNodePasswordTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ApplicationNodePasswordTextBox, "ApplicationNodePassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).ApplicationNodePassword)));
			this.ApplicationNodePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 78, true);
			this.ApplicationNodePasswordTextBox.Name = "ApplicationNodePasswordTextBox";
			this.ApplicationNodePasswordTextBox.PasswordChar = '*';
			this.ApplicationNodePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.ApplicationNodePasswordTextBox.TabIndex = 2;
			// 
			// RunningIntervalInSecondsCalcEdit
			// 
			this.RunningIntervalInSecondsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RunningIntervalInSecondsCalcEdit, "RunningIntervalInSeconds");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).RunningIntervalInSeconds)));
			this.RunningIntervalInSecondsCalcEdit.DecimalPlaces = 2;
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).SendFolder)));
			this.SendFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 130, true);
			this.SendFolderTextBox.Name = "SendFolderTextBox";
			this.SendFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.SendFolderTextBox.TabIndex = 4;
			// 
			// UnknownFolderTextBox
			// 
			this.UnknownFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UnknownFolderTextBox, "UnknownFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).UnknownFolder)));
			this.UnknownFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 156, true);
			this.UnknownFolderTextBox.Name = "UnknownFolderTextBox";
			this.UnknownFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.UnknownFolderTextBox.TabIndex = 5;
			// 
			// InvalidFolderTextBox
			// 
			this.InvalidFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.InvalidFolderTextBox, "InvalidFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).InvalidFolder)));
			this.InvalidFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 182, true);
			this.InvalidFolderTextBox.Name = "InvalidFolderTextBox";
			this.InvalidFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.InvalidFolderTextBox.TabIndex = 6;
			// 
			// RejectedFolderTextBox
			// 
			this.RejectedFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RejectedFolderTextBox, "RejectedFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).RejectedFolder)));
			this.RejectedFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 208, true);
			this.RejectedFolderTextBox.Name = "RejectedFolderTextBox";
			this.RejectedFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.RejectedFolderTextBox.TabIndex = 7;
			// 
			// ReceiveFolderTextBox
			// 
			this.ReceiveFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReceiveFolderTextBox, "ReceiveFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).ReceiveFolder)));
			this.ReceiveFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 234, true);
			this.ReceiveFolderTextBox.Name = "ReceiveFolderTextBox";
			this.ReceiveFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.ReceiveFolderTextBox.TabIndex = 8;
			// 
			// AcceptedFolderTextBox
			// 
			this.AcceptedFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AcceptedFolderTextBox, "AcceptedFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).AcceptedFolder)));
			this.AcceptedFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 260, true);
			this.AcceptedFolderTextBox.Name = "AcceptedFolderTextBox";
			this.AcceptedFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.AcceptedFolderTextBox.TabIndex = 9;
			// 
			// EnableSMSMessageSendingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.EnableSMSMessageSendingCheckBox, "EnableSMSMessageSending");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).EnableSMSMessageSending)));
			this.EnableSMSMessageSendingCheckBox.CaptionResourceString = Enterprise.Customs.CL.Manifest.GUI.Res.GetData("c2c72135-341d-4a4a-82c5-6a5aa38bdea7", "Enable SMS Message Sending");
			this.EnableSMSMessageSendingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.EnableSMSMessageSendingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 312, true);
			this.EnableSMSMessageSendingCheckBox.Name = "EnableSMSMessageSendingCheckBox";
			this.EnableSMSMessageSendingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 24, true);
			this.EnableSMSMessageSendingCheckBox.TabIndex = 11;
			// 
			// XtCredentialStatusDropEdit
			// 
			this.XtCredentialStatusDropEdit.AllowDrop = true;
			this.XtCredentialStatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.XtCredentialStatusDropEdit, "XtCredentialStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CL.Manifest.Business.CLSMSMessageSending)(null)).XtCredentialStatus)));
			this.XtCredentialStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 286, true);
			this.XtCredentialStatusDropEdit.Name = "XtCredentialStatusDropEdit";
			this.XtCredentialStatusDropEdit.PreBoundMaxLength = 3;
			this.XtCredentialStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.XtCredentialStatusDropEdit.TabIndex = 10;
			// 
			// CLSMSMessageSendingRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.XtCredentialStatusDropEdit);
			this.Controls.Add(this.EnableSMSMessageSendingCheckBox);
			this.Controls.Add(this.MachineNameTextBox);
			this.Controls.Add(this.ApplicationNodePasswordTextBox);
			this.Controls.Add(this.ApplicationNodeNameTextBox);
			this.Controls.Add(this.RunningIntervalInSecondsCalcEdit);
			this.Controls.Add(this.SendFolderTextBox);
			this.Controls.Add(this.UnknownFolderTextBox);
			this.Controls.Add(this.InvalidFolderTextBox);
			this.Controls.Add(this.RejectedFolderTextBox);
			this.Controls.Add(this.ReceiveFolderTextBox);
			this.Controls.Add(this.AcceptedFolderTextBox);
			this.Name = "CLSMSMessageSendingRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 359, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.XtCredentialStatusDropEdit.ResumeLayout(true);
			this.XtCredentialStatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZTextBox MachineNameTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit RunningIntervalInSecondsCalcEdit;

		#endregion

		private ZArchitecture.ZTextBox ApplicationNodeNameTextBox;
		private ZArchitecture.ZTextBox ApplicationNodePasswordTextBox;
		private ZArchitecture.ZTextBox SendFolderTextBox;
		private ZArchitecture.ZTextBox UnknownFolderTextBox;
		private ZArchitecture.ZTextBox InvalidFolderTextBox;
		private ZArchitecture.ZTextBox RejectedFolderTextBox;
		private ZArchitecture.ZTextBox ReceiveFolderTextBox;
		private ZArchitecture.ZTextBox AcceptedFolderTextBox;
		private ZDropEdit XtCredentialStatusDropEdit;
		private ZCheckBox EnableSMSMessageSendingCheckBox;
	}
}
