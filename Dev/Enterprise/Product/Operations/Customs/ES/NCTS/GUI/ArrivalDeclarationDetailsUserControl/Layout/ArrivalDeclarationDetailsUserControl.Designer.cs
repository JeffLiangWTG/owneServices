namespace Enterprise.Customs.ES.NCTS.GUI;

partial class ArrivalDeclarationDetailsUserControl
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
			this.CircuitTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalSummaryDeclarationUserControl = new Enterprise.Customs.ES.NCTS.GUI.ArrivalSummaryDeclarationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalSummaryDeclarationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// CircuitTextBox
			// 
			this.CircuitTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CircuitTextBox, "Circuit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.NctsHeader)(null)).Circuit)));
			this.CircuitTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 24, true);
			this.CircuitTextBox.Name = "CircuitTextBox";
			this.CircuitTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.CircuitTextBox.TabIndex = 1;
			// 
			// ArrivalSummaryDeclaration
			// 
			this.ArrivalSummaryDeclarationUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ArrivalSummaryDeclarationUserControl, ".");
			this.ArrivalSummaryDeclarationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 51, true);
			this.ArrivalSummaryDeclarationUserControl.Name = "ArrivalSummaryDeclarationUserControl";
			this.ArrivalSummaryDeclarationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ArrivalSummaryDeclarationUserControl.TabIndex = 0;
			// 
			// ArrivalDeclarationDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalSummaryDeclarationUserControl);
			this.Controls.Add(this.CircuitTextBox);
			this.Name = "ArrivalDeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 151, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalSummaryDeclarationUserControl.ResumeLayout(true);
			this.ArrivalSummaryDeclarationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}

	#endregion

	internal Enterprise.ZArchitecture.ZTextBox CircuitTextBox;
	internal ArrivalSummaryDeclarationUserControl ArrivalSummaryDeclarationUserControl;
}
