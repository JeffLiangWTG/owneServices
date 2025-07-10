
namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	partial class MoveLicencesToNewEnterpriseIDPopupForm
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
		protected override void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoveLicencesToNewEnterpriseIDPopupForm));
			this.ButtonOk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.LicenceEnterpriseIDFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicenceEnterpriseIDFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 228, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 0, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.MoveLicencesToNewEnterpriseIDBizO);
			// 
			// ButtonOk
			// 
			this.ButtonOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 185, true);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonOk.TabIndex = 1;
			this.ButtonOk.Text = "Ok";
			this.ButtonOk.ToolTipCaption = null;
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(284, 185, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonCancel.TabIndex = 2;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(377, 161, true);
			this.zLabel1.TabIndex = 4;
			this.zLabel1.Text = resources.GetString("zLabel1.Text");
			// 
			// LicenceEnterpriseIDFindBox
			// 
			this.LicenceEnterpriseIDFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LicenceEnterpriseIDFindBox, "LicenceEnterpriseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.MasterFiles.Business.MoveLicencesToNewEnterpriseIDBizO)(null)).LicenceEnterpriseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.MasterFiles.Business.MoveLicencesToNewEnterpriseIDBizO)(null)).LicenceEnterpriseList)));
			this.LicenceEnterpriseIDFindBox.BindToList = "LicenceEnterpriseList";
			this.LicenceEnterpriseIDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(49, 185, true);
			this.LicenceEnterpriseIDFindBox.Name = "LicenceEnterpriseIDFindBox";
			this.LicenceEnterpriseIDFindBox.ShouldResize = true;
			this.LicenceEnterpriseIDFindBox.ShowDescriptionBox = false;
			this.LicenceEnterpriseIDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 17, true);
			this.LicenceEnterpriseIDFindBox.TabIndex = 0;
			// 
			// MoveLicencesToNewEnterpriseIDPopupForm
			// 
			this.AcceptButton = this.ButtonOk;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.ButtonCancel;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 228, true);
			this.Controls.Add(this.LicenceEnterpriseIDFindBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOk);
			this.DataSourceType = typeof(Enterprise.Client.EDI.MasterFiles.Business.MoveLicencesToNewEnterpriseIDBizO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MoveLicencesToNewEnterpriseIDPopupForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Text = "Move licences to";
			this.Controls.SetChildIndex(this.ButtonOk, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LicenceEnterpriseIDFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicenceEnterpriseIDFindBox.ResumeLayout(true);
			this.LicenceEnterpriseIDFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ButtonOk;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LicenceEnterpriseIDFindBox;
	}
}
