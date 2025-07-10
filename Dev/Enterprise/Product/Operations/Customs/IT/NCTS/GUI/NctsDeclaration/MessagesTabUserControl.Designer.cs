
namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class MessagesTabUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).BeginInit();
			this.MessageGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessageEdifactTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageEdifactTextBox, "EM_MessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsHeader)(null)).Messages)).SyncRoot)).EM_MessageText)));
			this.MessageEdifactTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageEdifactTextBox.WordWrap = false;
			// 
			// MessagesTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "MessagesTabUserControl";
			((System.ComponentModel.ISupportInitialize)(this.MessageGrid)).EndInit();
			this.MessageGrid.ResumeLayout(false);
			this.MessageGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
