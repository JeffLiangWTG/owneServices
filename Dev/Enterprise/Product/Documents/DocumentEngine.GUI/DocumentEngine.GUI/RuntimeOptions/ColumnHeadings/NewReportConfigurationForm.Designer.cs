namespace Enterprise.DocumentEngine.GUI.RuntimeOptions
{
	partial class NewReportConfigurationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected sealed override void InitializeComponent()
		{
			this.zButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.textBoxConfigName = new Enterprise.ZArchitecture.ZTextBox();
			this.LinkedFieldUserControl = new Enterprise.DocumentEngine.GUI.RuntimeOptions.LookupFieldUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LinkedFieldUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 92, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.NewConfiguration);
			// 
			// zButtonOK
			// 
			this.zButtonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonOK.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NewReportConfigurationForm|cf827dcf-b0e3-487d-8afd-aca0f2c14c79", "&OK");
			this.zButtonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.zButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(301, 66, true);
			this.zButtonOK.Name = "zButtonOK";
			this.zButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.zButtonOK.TabIndex = 3;
			this.zButtonOK.UseVisualStyleBackColor = true;
			this.zButtonOK.Click += new System.EventHandler(this.zButtonOK_Click);
			// 
			// zButtonCancel
			// 
			this.zButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.zButtonCancel.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NewReportConfigurationForm|63afcfa5-3441-40e2-be0d-0bb1d4669dca", "&Cancel");
			this.zButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.zButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 66, true);
			this.zButtonCancel.Name = "zButtonCancel";
			this.zButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 20, true);
			this.zButtonCancel.TabIndex = 4;
			this.zButtonCancel.UseVisualStyleBackColor = true;
			// 
			// textBoxConfigName
			// 
			this.BindingSource.SetBindingMember(this.textBoxConfigName, "NewName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngine.RuntimeOptions.NewConfiguration)(null)).NewName)));
			this.textBoxConfigName.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.textBoxConfigName.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NewReportConfigurationForm|b2b00999-6ccd-459c-9677-7b142ce791c0", "Description");
			this.textBoxConfigName.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 42, true);
			this.textBoxConfigName.Name = "textBoxConfigName";
			this.textBoxConfigName.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 20, true);
			this.textBoxConfigName.TabIndex = 2;
			// 
			// LinkedFieldUserControl
			// 
			this.LinkedFieldUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 9, true);
			this.LinkedFieldUserControl.Name = "LinkedFieldUserControl";
			this.LinkedFieldUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(444, 27, true);
			this.LinkedFieldUserControl.TabIndex = 1;
			// 
			// NewReportConfigurationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 116, true);
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("NewReportConfigurationForm|d3211b43-ed0a-4105-8f7a-01a7c95044f9", "Report Configuration");
			this.Controls.Add(this.LinkedFieldUserControl);
			this.Controls.Add(this.textBoxConfigName);
			this.Controls.Add(this.zButtonCancel);
			this.Controls.Add(this.zButtonOK);
			this.DataSourceAssemblyName = "Enterprise.DocumentEngine";
			this.DataSourceType = typeof(Enterprise.DocumentEngine.RuntimeOptions.NewConfiguration);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.RuntimeOptions.NewConfiguration";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "NewReportConfigurationForm";
			this.Controls.SetChildIndex(this.zButtonOK, 0);
			this.Controls.SetChildIndex(this.zButtonCancel, 0);
			this.Controls.SetChildIndex(this.textBoxConfigName, 0);
			this.Controls.SetChildIndex(this.LinkedFieldUserControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LinkedFieldUserControl.ResumeLayout(true);
			this.LinkedFieldUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZTextBox textBoxConfigName;
		internal Enterprise.ZArchitecture.GUI.ZButton zButtonOK;
		internal Enterprise.ZArchitecture.GUI.ZButton zButtonCancel;
		internal LookupFieldUserControl LinkedFieldUserControl;
	}
}
