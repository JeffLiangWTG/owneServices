namespace Enterprise.Customs.CA.Module.OperationalActions
{
	partial class CARNSQueryOperationalActionControl
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
			this.CARNSQuerySendingOptionsGroupBOx = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SuppressNotificationPopoutCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IgnoreAllWarningsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CARNSQuerySendingOptionsGroupBOx.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Module.OperationalActions.CARNSQueryOperationalActionMethodApplicator);
			// 
			// CARNSQuerySendingOptionsGroupBOx
			// 
			this.CARNSQuerySendingOptionsGroupBOx.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("1C1BA1FF-918C-4C75-ABF1-AA65CDB81D8B", "CA Release Status Query Sending Options");
			this.CARNSQuerySendingOptionsGroupBOx.Controls.Add(this.SuppressNotificationPopoutCheckBox);
			this.CARNSQuerySendingOptionsGroupBOx.Controls.Add(this.IgnoreAllWarningsCheckBox);
			this.CARNSQuerySendingOptionsGroupBOx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.CARNSQuerySendingOptionsGroupBOx.Name = "CARNSQuerySendingOptionsGroupBOx";
			this.CARNSQuerySendingOptionsGroupBOx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(617, 108, true);
			this.CARNSQuerySendingOptionsGroupBOx.TabIndex = 0;
			this.CARNSQuerySendingOptionsGroupBOx.TabStop = false;
			// 
			// SuppressNotificationPopoutCheckBox
			// 
			this.SuppressNotificationPopoutCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SuppressNotificationPopoutCheckBox, "SuppressNotificationPopout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CARNSQueryOperationalActionMethodApplicator)(null)).SuppressNotificationPopout)));
			this.SuppressNotificationPopoutCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("A4CBBB69-C550-49EF-A022-82B73AFFB1F5", "Suppress Notification Dialog from popping-out");
			this.SuppressNotificationPopoutCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuppressNotificationPopoutCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 66, true);
			this.SuppressNotificationPopoutCheckBox.Name = "SuppressNotificationPopoutCheckBox";
			this.SuppressNotificationPopoutCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.SuppressNotificationPopoutCheckBox.TabIndex = 2;
			this.SuppressNotificationPopoutCheckBox.UseVisualStyleBackColor = true;
			// 
			// IgnoreAllWarningsCheckBox
			// 
			this.IgnoreAllWarningsCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IgnoreAllWarningsCheckBox, "IgnoreAllWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CA.Module.OperationalActions.CARNSQueryOperationalActionMethodApplicator)(null)).IgnoreAllWarnings)));
			this.IgnoreAllWarningsCheckBox.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("FF1CFCF6-03D7-4E5C-9B53-B36D9D78CD68", "Ignore warnings and continue sending", "Ignore all warnings and continue sending (e.g. Validation Message Errors, Awaiting for responses warning)");
			this.IgnoreAllWarningsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IgnoreAllWarningsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 31, true);
			this.IgnoreAllWarningsCheckBox.Name = "IgnoreAllWarningsCheckBox";
			this.IgnoreAllWarningsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IgnoreAllWarningsCheckBox.TabIndex = 0;
			this.IgnoreAllWarningsCheckBox.UseVisualStyleBackColor = true;
			// 
			// CARNSQueryOperationalActionControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CARNSQuerySendingOptionsGroupBOx);
			this.Name = "CARNSQueryOperationalActionControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 158, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CARNSQuerySendingOptionsGroupBOx.ResumeLayout(false);
			this.CARNSQuerySendingOptionsGroupBOx.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CARNSQuerySendingOptionsGroupBOx;
		private ZArchitecture.GUI.ZCheckBox IgnoreAllWarningsCheckBox;
		private ZArchitecture.GUI.ZCheckBox SuppressNotificationPopoutCheckBox;
	}
}
