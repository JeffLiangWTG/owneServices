using Enterprise.MasterFiles.GUI.Organisation.UserControls.Address;

namespace Enterprise.Accounting.GUI
{
	partial class UpdateJobLocalClientUserControl
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
			this.LocalClientControl = new ZAddressWithContactControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UpdateJobStatusActionMethodApplicator);
			// 
			// LocalClientFindBox
			// 
			this.LocalClientControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocalClientControl, "LocalZAddressWithContact");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((UpdateJobLocalClientActionMethodApplicator)(null)).LocalZAddressWithContact)));
			this.LocalClientControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.LocalClientControl.CaptionResourceString = Res.GetData("UpdateJobLocalClientUserControl|5c27c962-a16f-4db2-9468-b1887623dea0", "Local Client");
			this.LocalClientControl.ContactInfoTabVisible = true;
			this.LocalClientControl.InvoiceContactTabCaption = Enterprise.Accounting.GUI.Res.GetData("ZAddressWithContactControl|79ad8d2f-19b7-4049-a41c-3a36c594be57", "Invoice Contact");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LocalClientControl, false);
			this.LocalClientControl.Name = "LocalClientControl";
			this.LocalClientControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.LocalClientControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.LocalClientControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 162, true);
			this.LocalClientControl.OnlyStopOnDebtor = false;
			this.LocalClientControl.TabIndex = 0;
			// 
			// UpdateJobLocalClientUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.LocalClientControl);
			this.Name = "UpdateJobLocalClientUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(307, 187, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		ZAddressWithContactControl LocalClientControl;
	}
}
