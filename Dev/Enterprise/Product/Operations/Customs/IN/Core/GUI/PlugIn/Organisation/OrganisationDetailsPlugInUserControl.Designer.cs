namespace Enterprise.Customs.IN.GUI;

partial class OrganisationDetailsPlugInUserControl
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
			this.IsDiplomatCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IN.Business.INOrgImpAddInfo);
			// 
			// IsDiplomatCheckBox
			// 
			this.IsDiplomatCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsDiplomatCheckBox, "ZO_IsDiplomat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IN.Business.INOrgImpAddInfo)(null)).ZO_IsDiplomat)));
			this.IsDiplomatCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(94, 18, true);
			this.IsDiplomatCheckBox.Name = "IsDiplomatCheckBox";
			this.IsDiplomatCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 14, true);
			this.IsDiplomatCheckBox.TabIndex = 0;
			// 
			// OrganisationDetailsPlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.IsDiplomatCheckBox);
			this.Name = "OrganisationDetailsPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(347, 123, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal ZArchitecture.GUI.ZCheckBox IsDiplomatCheckBox;
}
