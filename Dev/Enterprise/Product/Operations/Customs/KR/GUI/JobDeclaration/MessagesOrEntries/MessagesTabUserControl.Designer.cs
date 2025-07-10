using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class MessagesTabUserControl
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
			components = new System.ComponentModel.Container();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.HtmlInterpretationBox);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 539, true);
			this.MessageDetailsTabPage.Controls.SetChildIndex(this.HtmlInterpretationBox, 0);
			// 
			// InterpretedMessageTextBox
			// 
			this.InterpretedMessageTextBox.Visible = false;
			//
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.EDIMessageCollection);
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.HtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(496, 533, true);
			this.HtmlInterpretationBox.TabIndex = 1;
		}

		#endregion

		Enterprise.Messaging.GUI.HtmlInterpretationBox HtmlInterpretationBox;
	}
}
