using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	partial class FrDeclarationSendPrelodgeAmendmentActionApplicatorControl
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
			this.sendMessagesEvenWithMessageErrorsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.OperationalActions.FrDeclarationSendPrelodgeAmendmentMessageApplicator);
			// 
			// sendMessagesEvenWithMessageErrorsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.sendMessagesEvenWithMessageErrorsCheckBox, "SendMessagesEvenWithMessageErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.FR.Business.OperationalActions.FrDeclarationSendPrelodgeAmendmentMessageApplicator)(null)).SendMessagesEvenWithMessageErrors)));
			this.sendMessagesEvenWithMessageErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 34, true);
			this.sendMessagesEvenWithMessageErrorsCheckBox.Name = "sendMessagesEvenWithMessageErrorsCheckBox";
			this.sendMessagesEvenWithMessageErrorsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(280, 24, true);
			this.sendMessagesEvenWithMessageErrorsCheckBox.TabIndex = 2;
			// 
			// FrDeclarationSendPrelodgeAmendmentActionApplicatorControl
			// 
			this.Controls.Add(this.sendMessagesEvenWithMessageErrorsCheckBox);
			this.Name = "FrDeclarationSendPrelodgeAmendmentActionApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 103, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZCheckBox sendMessagesEvenWithMessageErrorsCheckBox;

		#endregion
	}
}
