
namespace Enterprise.Customs.CH.GUI;

partial class TareSupplementUserControl
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
			this.TareSupplementCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TareSupplementCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CH.Business.JobComInvoiceLine);
			// 
			// TareSupplementCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TareSupplementCheckBox, "JI_TareSupplementConfirmation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_TareSupplementConfirmation)));
			this.TareSupplementCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 0, true);
			this.TareSupplementCheckBox.Name = "TareSupplementCheckBox";
			this.TareSupplementCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.TareSupplementCheckBox.TabIndex = 5;
			this.TareSupplementCheckBox.UseVisualStyleBackColor = true;
			// 
			// TareSupplementCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.TareSupplementCalcEdit, "JI_TareSupplementPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CH.Business.JobComInvoiceLine)(null)).JI_TareSupplementPercentage)));
			this.TareSupplementCalcEdit.DecimalPlaces = 1;
			this.TareSupplementCalcEdit.Decimals = 1;
			this.TareSupplementCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TareSupplementCalcEdit.Name = "TareSupplementCalcEdit";
			this.TareSupplementCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.TareSupplementCalcEdit.TabIndex = 4;
			this.TareSupplementCalcEdit.Text = "0.0";
			this.TareSupplementCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.TareSupplementCalcEdit.TrackDisposedAccess = true;
			// 
			// TareSupplementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TareSupplementCheckBox);
			this.Controls.Add(this.TareSupplementCalcEdit);
			this.Name = "TareSupplementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

    }

    #endregion

    internal ZArchitecture.GUI.ZCheckBox TareSupplementCheckBox;
    internal ZArchitecture.ZCalcEdit TareSupplementCalcEdit;
}
