namespace Enterprise.Messaging.GUI
{
	partial class EDIMessageInterpretationUserControl
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
			this.InterpretationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InterpretationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Messaging.Business.EDIMessage);
			// 
			// InterpretationGroupBox
			// 
			this.InterpretationGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDIMessageInterpretationUserControl|a1843151-5d52-49e5-a881-fcf139319bb4", "Message Interpretation");
			this.InterpretationGroupBox.Controls.Add(this.HtmlInterpretationBox);
			this.InterpretationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InterpretationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InterpretationGroupBox.Name = "InterpretationGroupBox";
			this.InterpretationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 236, true);
			this.InterpretationGroupBox.TabIndex = 0;
			this.InterpretationGroupBox.TabStop = false;
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.BindingSource.SetBindingMember(this.HtmlInterpretationBox, "EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(null)).EM_MessageInterpretation)));
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.HtmlInterpretationBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 217, true);
			this.HtmlInterpretationBox.TabIndex = 0;
			// 
			// EDIMessageInterpretationUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.InterpretationGroupBox);
			this.Name = "EDIMessageInterpretationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InterpretationGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox InterpretationGroupBox;
		private HtmlInterpretationBox HtmlInterpretationBox;
	}
}
