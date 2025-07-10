using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	partial class OverrideRevokeReasonUserControl
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
			this.overrideRevokeReasonCheckBox = new ZCheckBox();
			this.revokeReasonDropEdit = new ZDropEdit();
			this.revokeReasonDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.revokeReasonDropEdit.SuspendLayout();
			this.revokeReasonDescriptionTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(G3MessageSendingObjectParent);
			// 
			// overrideRevokeReasonCheckBox
			// 
			this.overrideRevokeReasonCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.overrideRevokeReasonCheckBox, nameof(G3MessageSendingObjectParent.OverrideDefaultRevokeReason));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((G3MessageSendingObjectParent)(null)).OverrideDefaultRevokeReason)));
			this.overrideRevokeReasonCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 22, true);
			this.overrideRevokeReasonCheckBox.Name = "overrideRevokeReasonCheckBox";
			this.overrideRevokeReasonCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.overrideRevokeReasonCheckBox.TabIndex = 0;
			this.overrideRevokeReasonCheckBox.UseVisualStyleBackColor = true;
			// 
			// revokeReasonDropEdit
			// 
			this.revokeReasonDropEdit.AllowDrop = true;
			this.revokeReasonDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.revokeReasonDropEdit, nameof(G3MessageSendingObjectParent.RevokeReason));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((G3MessageSendingObjectParent)(null)).RevokeReason)));
			this.revokeReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 22, true);
			this.revokeReasonDropEdit.Name = "revokeReasonDropEdit";
			this.revokeReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.revokeReasonDropEdit.TabIndex = 1;
			this.revokeReasonDropEdit.CodeBox.Width = 80;
			this.revokeReasonDropEdit.DescriptionBox.Width = 140;
			this.revokeReasonDropEdit.ShowDescriptionBox = false;
			// 
			// revokeReasonDescriptionTextBox
			// 
			this.revokeReasonDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.revokeReasonDescriptionTextBox, nameof(G3MessageSendingObjectParent.RevokeReasonDescription));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((G3MessageSendingObjectParent)(null)).RevokeReasonDescription)));
			this.revokeReasonDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(550, 22, true);
			this.revokeReasonDescriptionTextBox.Name = "revokeReasonDescriptionTextBox";
			this.revokeReasonDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.revokeReasonDescriptionTextBox.TabIndex = 2;
			// 
			// OverrideRevokeReasonUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.overrideRevokeReasonCheckBox);
			this.Controls.Add(this.revokeReasonDropEdit);
			this.Controls.Add(this.revokeReasonDescriptionTextBox);
			this.Name = "OverrideRevokeReasonUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.revokeReasonDropEdit.ResumeLayout(true);
			this.revokeReasonDropEdit.PerformLayout();
			this.revokeReasonDescriptionTextBox.ResumeLayout(true);
			this.revokeReasonDescriptionTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZCheckBox overrideRevokeReasonCheckBox;
		ZDropEdit revokeReasonDropEdit;
		ZTextBox revokeReasonDescriptionTextBox;

		#endregion
	}
}
