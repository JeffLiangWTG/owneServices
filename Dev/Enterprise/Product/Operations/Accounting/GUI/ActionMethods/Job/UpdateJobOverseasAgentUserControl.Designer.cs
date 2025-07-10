
namespace Enterprise.Accounting.GUI
{
	partial class UpdateJobOverseasAgentUserControl
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
			this.OverseasAgentControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateJobStatusActionMethodApplicator);
			// 
			// OverseasAgentControl
			// 
			this.OverseasAgentControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverseasAgentControl, "AgentCollectAddr_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UpdateJobOverseasAgentActionMethodApplicator)(null)).AgentCollectAddr_ZAddress)));
			this.OverseasAgentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.OverseasAgentControl.Name = "OverseasAgentControl";
			this.OverseasAgentControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.OverseasAgentControl.TabIndex = 0;
			this.OverseasAgentControl.CaptionResourceString = Res.GetData("UpdateJobOverseasAgentUserControl|2602a4fe-e76e-4ae0-9add-44d9b4043412", "Overseas Agent");
			// 
			// UpdateJobOverseasAgentUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OverseasAgentControl);
			this.Name = "UpdateJobOverseasAgentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		Enterprise.MasterFiles.GUI.ZOrgAddressControl OverseasAgentControl;
	}
}
