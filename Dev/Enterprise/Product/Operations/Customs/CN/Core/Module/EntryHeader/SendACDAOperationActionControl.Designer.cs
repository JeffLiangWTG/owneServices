using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CN.Module
{
	partial class SendACDAOperationActionControl
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
			this.SendGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.NotSendIfACDANumberExistCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SendWithErrorsRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.SendRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SendGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Module.SendACDAOperationalActionMethodApplicator);
			// 
			// SendGroupBox
			// 
			this.SendGroupBox.CaptionResourceString = Enterprise.Customs.CN.Module.Res.GetData("DE88F556-608A-411C-A571-0BF506BD82F7", "Send Messages");
			this.SendGroupBox.Controls.Add(this.NotSendIfACDANumberExistCheckBox);
			this.SendGroupBox.Controls.Add(this.SendWithErrorsRadioButton);
			this.SendGroupBox.Controls.Add(this.SendRadioButton);
			this.SendGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.SendGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SendGroupBox.Name = "SendGroupBox";
			this.SendGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 146, true);
			this.SendGroupBox.TabIndex = 0;
			this.SendGroupBox.TabStop = false;
			// 
			// NotSendIfACDANumberExistCheckBox
			// 
			this.BindingSource.SetBindingMember(this.NotSendIfACDANumberExistCheckBox, "NotSendIfACDANumberExist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Module.SendACDAOperationalActionMethodApplicator)(null)).NotSendIfACDANumberExist)));
			this.NotSendIfACDANumberExistCheckBox.CaptionResourceString = Enterprise.Customs.CN.Module.Res.GetData("1840E894-6138-4909-A58F-E3748EB0CF80", "Do not send if ACDA number already exists");
			this.NotSendIfACDANumberExistCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 69, true);
			this.NotSendIfACDANumberExistCheckBox.Name = "NotSendIfACDANumberExistCheckBox";
			this.NotSendIfACDANumberExistCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 16, true);
			this.NotSendIfACDANumberExistCheckBox.TabIndex = 2;
			this.NotSendIfACDANumberExistCheckBox.UseVisualStyleBackColor = true;
			// 
			// SendWithErrorsRadioButton
			// 
			this.SendWithErrorsRadioButton.AutoCheck = false;
			this.SendWithErrorsRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SendWithErrorsRadioButton, "SendWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Module.SendACDAOperationalActionMethodApplicator)(null)).SendWithMessageErrors)));
			this.SendWithErrorsRadioButton.CaptionResourceString = Enterprise.Customs.CN.Module.Res.GetData("4AB0E559-408E-44DC-9AC6-10F0D406C367", "Send ignoring errors");
			this.SendWithErrorsRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 44, true);
			this.SendWithErrorsRadioButton.Name = "SendWithErrorsRadioButton";
			this.SendWithErrorsRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 18, true);
			this.SendWithErrorsRadioButton.TabIndex = 1;
			this.SendWithErrorsRadioButton.UseVisualStyleBackColor = true;
			// 
			// NotSendWithErrorsRadioButton
			// 
			this.SendRadioButton.AutoCheck = false;
			this.SendRadioButton.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(this.SendRadioButton, "NotSendWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Module.SendACDAOperationalActionMethodApplicator)(null)).NotSendWithMessageErrors)));
			this.SendRadioButton.CaptionResourceString = Enterprise.Customs.CN.Module.Res.GetData("33BF7473-B67A-46F9-AB27-038A38C45160", "Send (send only where there are no errors)");
			this.SendRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(83, 19, true);
			this.SendRadioButton.Name = "SendRadioButton";
			this.SendRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 19, true);
			this.SendRadioButton.TabIndex = 0;
			this.SendRadioButton.UseVisualStyleBackColor = true;
			// 
			// SendACDAOperationActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SendGroupBox);
			this.Name = "SendACDAOperationActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 283, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SendGroupBox.ResumeLayout(false);
			this.SendGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZRadioButton SendWithErrorsRadioButton;
		internal ZRadioButton SendRadioButton;
		internal ZCheckBox NotSendIfACDANumberExistCheckBox;
		ZGroupBox SendGroupBox;
	}
}
