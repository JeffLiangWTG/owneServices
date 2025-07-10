using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	partial class SendCancellationMessageOperationalApplicatorControl
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

		internal ZDropEditWithFixedWidth invalidationMotivationDropEditWithFixedWidth;
		internal ZArchitecture.ZTextBox invalidationReasonTextBox;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.invalidationMotivationDropEditWithFixedWidth = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.invalidationReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.invalidationMotivationDropEditWithFixedWidth.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.OperationalActions.SendCancellationMessageApplicator);
			// 
			// invalidationMotivationDropEditWithFixedWidth
			// 
			this.invalidationMotivationDropEditWithFixedWidth.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.invalidationMotivationDropEditWithFixedWidth, "CancellationMessage.InvalidationMotivation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.FR.Business.OperationalActions.SendCancellationMessageApplicator)(null)).CancellationMessage.InvalidationMotivation)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.invalidationMotivationDropEditWithFixedWidth, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.invalidationMotivationDropEditWithFixedWidth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 25, true);
			this.invalidationMotivationDropEditWithFixedWidth.Name = "invalidationMotivationDropEditWithFixedWidth";
			this.invalidationMotivationDropEditWithFixedWidth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.invalidationMotivationDropEditWithFixedWidth.TabIndex = 1;
			// 
			// invalidationReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.invalidationReasonTextBox, "CancellationMessage.InvalidationReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.FR.Business.OperationalActions.SendCancellationMessageApplicator)(null)).CancellationMessage.InvalidationReason)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.invalidationReasonTextBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.invalidationReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(46, 68, true);
			this.invalidationReasonTextBox.Multiline = true;
			this.invalidationReasonTextBox.Name = "invalidationReasonTextBox";
			this.invalidationReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 138, true);
			this.invalidationReasonTextBox.TabIndex = 2;
			// 
			// SendCancellationMessageOperationalApplicatorControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.invalidationMotivationDropEditWithFixedWidth);
			this.Controls.Add(this.invalidationReasonTextBox);
			this.Name = "SendCancellationMessageOperationalApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 421, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.invalidationMotivationDropEditWithFixedWidth.ResumeLayout(true);
			this.invalidationMotivationDropEditWithFixedWidth.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		#endregion
	}
}
