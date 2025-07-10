using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IT.GUI
{
	sealed partial class AmountAndUnitControl
	{
		void InitializeComponent()
		{
			this.AmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.UnitOfMeasureTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AmountCalcEdit
			// 
			this.AmountCalcEdit.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AmountCalcEdit, false);
			this.AmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AmountCalcEdit.ReadOnly = true;
			this.AmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.AmountCalcEdit.TabIndex = 1;
			this.AmountCalcEdit.Name = "AmountCalcEdit";
			this.AmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitOfMeasureTextBox
			// 
			this.UnitOfMeasureTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UnitOfMeasureTextBox, false);
			this.UnitOfMeasureTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 0, true);
			this.UnitOfMeasureTextBox.ReadOnly = true;
			this.UnitOfMeasureTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(35, 20, true);
			this.UnitOfMeasureTextBox.TabIndex = 2;
			this.UnitOfMeasureTextBox.Name = "UnitOfMeasureTextBox";
			// 
			// AmountAndUnitControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnitOfMeasureTextBox);
			this.Controls.Add(this.AmountCalcEdit);
			this.Name = "AmountAndUnitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 22, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZCalcEdit AmountCalcEdit;
		internal ZTextBox UnitOfMeasureTextBox;
	}
}
