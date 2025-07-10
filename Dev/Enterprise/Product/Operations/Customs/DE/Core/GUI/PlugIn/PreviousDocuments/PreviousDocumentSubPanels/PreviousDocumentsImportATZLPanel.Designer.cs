using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class PreviousDocumentsImportATZLPanel
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
			this.localReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.authorizationNumberDropDown = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction);
			// 
			// localReferenceTextBox
			// 
			this.localReferenceTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.localReferenceTextBox, "PreviousDocumentMaster.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).PreviousDocumentMaster.CSI_ReferenceNumber2)));
			this.localReferenceTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("B5B9DE42-9098-47E5-BD17-A73CF75ECC5F", "Local Reference");
			this.localReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.localReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 3, true);
			this.localReferenceTextBox.Name = "localReferenceTextBox";
			this.localReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.localReferenceTextBox.TabIndex = 1;
			// 
			// authorizationNumberTextBox
			// 
			this.authorizationNumberDropDown.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.authorizationNumberDropDown, "PreviousDocumentMaster.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).PreviousDocumentMaster.AuthorizationNumber)));
			this.authorizationNumberDropDown.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("D29558F1-BE04-4246-B34F-37DC3E95EB78", "Auth. No.");
			this.authorizationNumberDropDown.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(429, 3, true);
			this.authorizationNumberDropDown.Name = "authorizationNumberTextBox";
			this.authorizationNumberDropDown.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(222, 20, true);
			this.authorizationNumberDropDown.TabIndex = 2;
			// 
			// PreviousDocumentsImportATZLPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.localReferenceTextBox);
			this.Controls.Add(this.authorizationNumberDropDown);
			this.Name = "PreviousDocumentsImportATZLPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(661, 27, true);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 43, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZTextBox localReferenceTextBox;
		public ZDropEdit authorizationNumberDropDown;

	}
}
