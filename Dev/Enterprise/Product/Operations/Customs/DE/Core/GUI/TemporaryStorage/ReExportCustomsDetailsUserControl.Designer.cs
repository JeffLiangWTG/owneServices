namespace Enterprise.Customs.DE.GUI
{
	partial class ReExportCustomsDetailsUserControl
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
			this.CustomsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AdditionalInfoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsDetailsGroupBox.SuspendLayout();
			this.CustomsOfficeCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// CustomsDetailsGroupBox
			//
			this.CustomsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c8eb951a-c27b-4cc3-beb5-da3bcc16ea34", "Customs Details");
			this.CustomsDetailsGroupBox.Controls.Add(this.CustomsOfficeCodeFindBox);
			this.CustomsDetailsGroupBox.Controls.Add(this.AdditionalInfoTextBox);
			this.CustomsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomsDetailsGroupBox.Name = "CustomsDetailsGroupBox";
			this.CustomsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 122, true);
			this.CustomsDetailsGroupBox.TabIndex = 102;
			this.CustomsDetailsGroupBox.TabStop = false;
			// 
			// CustomsOfficeCodeFindBox
			// 
			this.CustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeCodeFindBox, "SJH_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_CustomsOffice)));
			this.CustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 17, true);
			this.CustomsOfficeCodeFindBox.Name = "CustomsOfficeCodeFindBox";
			this.CustomsOfficeCodeFindBox.ShouldResize = false;
			this.CustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 20, true);
			this.CustomsOfficeCodeFindBox.TabIndex = 1;
			// 
			// AdditionalInfoTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalInfoTextBox, "SJH_AdditionalInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_AdditionalInformation)));
			this.AdditionalInfoTextBox.CaptionResourceString = null;
			this.AdditionalInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 42, true);
			this.AdditionalInfoTextBox.Multiline = true;
			this.AdditionalInfoTextBox.Name = "AdditionalInfoTextBox";
			this.AdditionalInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 63, true);
			this.AdditionalInfoTextBox.TabIndex = 4;
			// 
			// REXCustomsDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CustomsDetailsGroupBox);
			this.Name = "REXCustomsDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(781, 122, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsDetailsGroupBox.ResumeLayout(false);
			this.CustomsDetailsGroupBox.PerformLayout();
			this.CustomsOfficeCodeFindBox.ResumeLayout(true);
			this.CustomsOfficeCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox CustomsDetailsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox CustomsOfficeCodeFindBox;
		private ZArchitecture.ZTextBox AdditionalInfoTextBox;
	}
}
