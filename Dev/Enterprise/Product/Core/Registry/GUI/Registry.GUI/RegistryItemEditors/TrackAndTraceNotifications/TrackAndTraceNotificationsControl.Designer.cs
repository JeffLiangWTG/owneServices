namespace Enterprise.Registry.GUI
{
	partial class TrackAndTraceNotificationsControl
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
		void InitializeComponent()
		{
			this.notifySenderOnSuccessOrAcknowledgementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.groupForSuccessOrAcknowledgementFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.successOrAcknowledgementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupForSuccessOrAcknowledgementGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.errorAndDiscrepancyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupForErrorsAndDiscrepanciesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.groupForErrorsAndDiscrepanciesFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.notifyGroupOnDiscrepancyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifyGroupOnErrorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifySenderOnDiscrepancyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.notifySenderOnErrorCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.successOrAcknowledgementGroupBox.SuspendLayout();
			this.groupForSuccessOrAcknowledgementGroupBox.SuspendLayout();
			this.errorAndDiscrepancyGroupBox.SuspendLayout();
			this.groupForErrorsAndDiscrepanciesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.TrackAndTraceNotificationsRule);
			// 
			// notifySenderOnSuccessOrAcknowledgementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifySenderOnSuccessOrAcknowledgementCheckBox, "NotifySenderOnSuccessOrAcknowledgement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifySenderOnSuccessOrAcknowledgement)));
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("3e48a55e-9732-434b-b180-b2b3c2a5d2b1", "Notify Sender On Success Or Acknowledgement");
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.Name = "notifySenderOnSuccessOrAcknowledgementCheckBox";
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.TabIndex = 0;
			this.notifySenderOnSuccessOrAcknowledgementCheckBox.UseVisualStyleBackColor = true;
			// 
			// groupForSuccessOrAcknowledgementFindBox
			// 
			this.groupForSuccessOrAcknowledgementFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.groupForSuccessOrAcknowledgementFindBox, "GroupForSuccessOrAcknowledgement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).GroupForSuccessOrAcknowledgement)));
			this.groupForSuccessOrAcknowledgementFindBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.groupForSuccessOrAcknowledgementFindBox, false);
			this.groupForSuccessOrAcknowledgementFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.groupForSuccessOrAcknowledgementFindBox.Name = "groupForSuccessOrAcknowledgementFindBox";
			this.groupForSuccessOrAcknowledgementFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.groupForSuccessOrAcknowledgementFindBox.TabIndex = 1;
			// 
			// successOrAcknowledgementGroupBox
			// 
			this.successOrAcknowledgementGroupBox.AutoSize = true;
			this.successOrAcknowledgementGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.successOrAcknowledgementGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6043a319-0e30-448b-9ae3-6cce6e270488", "Success Or Acknowledgement Notifications");
			this.successOrAcknowledgementGroupBox.Controls.Add(this.groupForSuccessOrAcknowledgementGroupBox);
			this.successOrAcknowledgementGroupBox.Controls.Add(this.notifyGroupOnSuccessOrAcknowledgementCheckBox);
			this.successOrAcknowledgementGroupBox.Controls.Add(this.notifySenderOnSuccessOrAcknowledgementCheckBox);
			this.successOrAcknowledgementGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.successOrAcknowledgementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.successOrAcknowledgementGroupBox.Name = "successOrAcknowledgementGroupBox";
			this.successOrAcknowledgementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 110, true);
			this.successOrAcknowledgementGroupBox.TabIndex = 1;
			this.successOrAcknowledgementGroupBox.TabStop = false;
			// 
			// groupForSuccessOrAcknowledgementGroupBox
			// 
			this.groupForSuccessOrAcknowledgementGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("99901e3c-d12d-44ae-a5c7-218624827b06", "Group For Success Or Acknowledgement");
			this.groupForSuccessOrAcknowledgementGroupBox.Controls.Add(this.groupForSuccessOrAcknowledgementFindBox);
			this.groupForSuccessOrAcknowledgementGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.groupForSuccessOrAcknowledgementGroupBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.groupForSuccessOrAcknowledgementGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 64, true);
			this.groupForSuccessOrAcknowledgementGroupBox.Name = "groupForSuccessOrAcknowledgementGroupBox";
			this.groupForSuccessOrAcknowledgementGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 43, true);
			this.groupForSuccessOrAcknowledgementGroupBox.TabIndex = 2;
			this.groupForSuccessOrAcknowledgementGroupBox.TabStop = false;
			// 
			// notifyGroupOnSuccessOrAcknowledgementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifyGroupOnSuccessOrAcknowledgementCheckBox, "NotifyGroupOnSuccessOrAcknowledgement");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifyGroupOnSuccessOrAcknowledgement)));
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0a5a5f73-20d6-4896-8846-8ed42280ca2d", "Notify Group On Success Or Acknowledgement");
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.Name = "notifyGroupOnSuccessOrAcknowledgementCheckBox";
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.TabIndex = 1;
			this.notifyGroupOnSuccessOrAcknowledgementCheckBox.UseVisualStyleBackColor = true;
			// 
			// errorAndDiscrepancyGroupBox
			// 
			this.errorAndDiscrepancyGroupBox.AutoSize = true;
			this.errorAndDiscrepancyGroupBox.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.errorAndDiscrepancyGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d1a65bb4-d09e-4842-8e84-d6a738b61ae5", "Error And Discrepancy Notifications");
			this.errorAndDiscrepancyGroupBox.Controls.Add(this.groupForErrorsAndDiscrepanciesGroupBox);
			this.errorAndDiscrepancyGroupBox.Controls.Add(this.notifyGroupOnDiscrepancyCheckBox);
			this.errorAndDiscrepancyGroupBox.Controls.Add(this.notifyGroupOnErrorCheckBox);
			this.errorAndDiscrepancyGroupBox.Controls.Add(this.notifySenderOnDiscrepancyCheckBox);
			this.errorAndDiscrepancyGroupBox.Controls.Add(this.notifySenderOnErrorCheckBox);
			this.errorAndDiscrepancyGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.errorAndDiscrepancyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 110, true);
			this.errorAndDiscrepancyGroupBox.Name = "errorAndDiscrepancyGroupBox";
			this.errorAndDiscrepancyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 158, true);
			this.errorAndDiscrepancyGroupBox.TabIndex = 2;
			this.errorAndDiscrepancyGroupBox.TabStop = false;
			// 
			// groupForErrorsAndDiscrepanciesGroupBox
			// 
			this.groupForErrorsAndDiscrepanciesGroupBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8dcae944-fed2-48f5-b2a9-a8d08b03d3d4", "Group For Errors And Discrepancies");
			this.groupForErrorsAndDiscrepanciesGroupBox.Controls.Add(this.groupForErrorsAndDiscrepanciesFindBox);
			this.groupForErrorsAndDiscrepanciesGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.groupForErrorsAndDiscrepanciesGroupBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.groupForErrorsAndDiscrepanciesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 112, true);
			this.groupForErrorsAndDiscrepanciesGroupBox.Name = "groupForErrorsAndDiscrepanciesGroupBox";
			this.groupForErrorsAndDiscrepanciesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 43, true);
			this.groupForErrorsAndDiscrepanciesGroupBox.TabIndex = 4;
			this.groupForErrorsAndDiscrepanciesGroupBox.TabStop = false;
			// 
			// groupForErrorsAndDiscrepanciesFindBox
			// 
			this.groupForErrorsAndDiscrepanciesFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.groupForErrorsAndDiscrepanciesFindBox, "GroupForErrorsAndDiscrepancies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).GroupForErrorsAndDiscrepancies)));
			this.groupForErrorsAndDiscrepanciesFindBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.groupForErrorsAndDiscrepanciesFindBox, false);
			this.groupForErrorsAndDiscrepanciesFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 18, true);
			this.groupForErrorsAndDiscrepanciesFindBox.Name = "groupForErrorsAndDiscrepanciesFindBox";
			this.groupForErrorsAndDiscrepanciesFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.groupForErrorsAndDiscrepanciesFindBox.TabIndex = 1;
			// 
			// notifyGroupOnDiscrepancyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifyGroupOnDiscrepancyCheckBox, "NotifyGroupOnDiscrepancy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifyGroupOnDiscrepancy)));
			this.notifyGroupOnDiscrepancyCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ad3d6b4e-f005-4272-ad6d-363e46bced80", "Notify Group On Discrepancy");
			this.notifyGroupOnDiscrepancyCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifyGroupOnDiscrepancyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyGroupOnDiscrepancyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 88, true);
			this.notifyGroupOnDiscrepancyCheckBox.Name = "notifyGroupOnDiscrepancyCheckBox";
			this.notifyGroupOnDiscrepancyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifyGroupOnDiscrepancyCheckBox.TabIndex = 3;
			this.notifyGroupOnDiscrepancyCheckBox.UseVisualStyleBackColor = true;
			// 
			// notifyGroupOnErrorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifyGroupOnErrorCheckBox, "NotifyGroupOnError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifyGroupOnError)));
			this.notifyGroupOnErrorCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("35471dd1-baa5-49af-88c9-98ca3de4cc09", "Notify Group On Error");
			this.notifyGroupOnErrorCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifyGroupOnErrorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifyGroupOnErrorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 64, true);
			this.notifyGroupOnErrorCheckBox.Name = "notifyGroupOnErrorCheckBox";
			this.notifyGroupOnErrorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifyGroupOnErrorCheckBox.TabIndex = 2;
			this.notifyGroupOnErrorCheckBox.UseVisualStyleBackColor = true;
			// 
			// notifySenderOnDiscrepancyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifySenderOnDiscrepancyCheckBox, "NotifySenderOnDiscrepancy");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifySenderOnDiscrepancy)));
			this.notifySenderOnDiscrepancyCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4362d010-8121-453f-9dfc-0c8b35b24680", "Notify Sender On Discrepancy");
			this.notifySenderOnDiscrepancyCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifySenderOnDiscrepancyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifySenderOnDiscrepancyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 40, true);
			this.notifySenderOnDiscrepancyCheckBox.Name = "notifySenderOnDiscrepancyCheckBox";
			this.notifySenderOnDiscrepancyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifySenderOnDiscrepancyCheckBox.TabIndex = 1;
			this.notifySenderOnDiscrepancyCheckBox.UseVisualStyleBackColor = true;
			// 
			// notifySenderOnErrorCheckBox
			// 
			this.BindingSource.SetBindingMember(this.notifySenderOnErrorCheckBox, "NotifySenderOnError");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.TrackAndTraceNotificationsRule)(null)).NotifySenderOnError)));
			this.notifySenderOnErrorCheckBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5880bbca-44f4-43f7-86dd-ac6bab01edcf", "Notify Sender On Error");
			this.notifySenderOnErrorCheckBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.notifySenderOnErrorCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.notifySenderOnErrorCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.notifySenderOnErrorCheckBox.Name = "notifySenderOnErrorCheckBox";
			this.notifySenderOnErrorCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(321, 24, true);
			this.notifySenderOnErrorCheckBox.TabIndex = 0;
			this.notifySenderOnErrorCheckBox.UseVisualStyleBackColor = true;
			// 
			// TrackAndTraceNotificationsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.errorAndDiscrepancyGroupBox);
			this.Controls.Add(this.successOrAcknowledgementGroupBox);
			this.Name = "TrackAndTraceNotificationsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.successOrAcknowledgementGroupBox.ResumeLayout(false);
			this.groupForSuccessOrAcknowledgementGroupBox.ResumeLayout(false);
			this.errorAndDiscrepancyGroupBox.ResumeLayout(false);
			this.groupForErrorsAndDiscrepanciesGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox notifySenderOnSuccessOrAcknowledgementCheckBox;
		private ZArchitecture.GUI.ZGuidFindBox groupForSuccessOrAcknowledgementFindBox;
		private ZArchitecture.GUI.ZGroupBox successOrAcknowledgementGroupBox;
		private ZArchitecture.GUI.ZCheckBox notifyGroupOnSuccessOrAcknowledgementCheckBox;
		private ZArchitecture.GUI.ZGroupBox groupForSuccessOrAcknowledgementGroupBox;
		private ZArchitecture.GUI.ZGroupBox errorAndDiscrepancyGroupBox;
		private ZArchitecture.GUI.ZGroupBox groupForErrorsAndDiscrepanciesGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox groupForErrorsAndDiscrepanciesFindBox;
		private ZArchitecture.GUI.ZCheckBox notifySenderOnDiscrepancyCheckBox;
		private ZArchitecture.GUI.ZCheckBox notifySenderOnErrorCheckBox;
		private ZArchitecture.GUI.ZCheckBox notifyGroupOnDiscrepancyCheckBox;
		private ZArchitecture.GUI.ZCheckBox notifyGroupOnErrorCheckBox;
	}
}
