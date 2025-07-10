using System;
using System.ComponentModel;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZPreviousNextControl
	{

		#region Component Designer generated code

		Container components = null;
		ZButton PreviousButton;
		ZButton NextButton;
		ZCalcEdit CurrentRecordNumberCalcEdit;
		ZLabel zLabel1;
		ZCalcEdit NumberOfResults;

		void InitializeComponent()
		{
			this.PreviousButton = new ZButton();
			this.NextButton = new ZButton();
			this.CurrentRecordNumberCalcEdit = new ZCalcEdit();
			this.zLabel1 = new ZLabel();
			this.NumberOfResults = new ZCalcEdit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// PreviousButton
			//
			this.PreviousButton.BackColor = System.Drawing.Color.Transparent;
			this.PreviousButton.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.PreviousButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 1, true);
			this.PreviousButton.Name = "PreviousButton";
			this.PreviousButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.PreviousButton.TabIndex = 0;
			this.PreviousButton.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PreviousButton.UseVisualStyleBackColor = false;
			this.PreviousButton.Click += new EventHandler(this.PreviousButton_Click);
			this.PreviousButton.ToolTipCaption = ResString.GetMultilingualString("C6E18F41-893A-4894-9A36-F3F528A21A8D", "Previous");
			//
			// NextButton
			//
			this.NextButton.BackColor = System.Drawing.Color.Transparent;
			this.NextButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.NextButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(225, 1, true);
			this.NextButton.Name = "NextButton";
			this.NextButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 23, true);
			this.NextButton.TabIndex = 1;
			this.NextButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.NextButton.UseVisualStyleBackColor = false;
			this.NextButton.Click += new EventHandler(this.NextButton_Click);
			this.NextButton.ToolTipCaption = ResString.GetMultilingualString("B7494DB1-9027-498A-AEA4-CB2A3C4CB94E", "Next");
			//
			// CurrentRecordNumberCalcEdit
			//
			this.BindingSource.SetBindingMember(this.CurrentRecordNumberCalcEdit, "CurrentRecordNumberAllowingInvalid");
			this.CurrentRecordNumberCalcEdit.Decimals = 0;
			this.CurrentRecordNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(79, 3, true);
			this.CurrentRecordNumberCalcEdit.Name = "CurrentRecordNumberCalcEdit";
			this.CurrentRecordNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 18, true);
			this.CurrentRecordNumberCalcEdit.TabIndex = 2;
			this.CurrentRecordNumberCalcEdit.Text = "10,00";
			this.CurrentRecordNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// zLabel1
			//
			this.zLabel1.CaptionResourceString = Enterprise.ZArchitecture.GUI.Res.GetData("ZPreviousNextControl|349b9bc8-62cc-4d01-8c17-ed61f2efddd5", "of");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 2, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 20, true);
			this.zLabel1.TabIndex = 3;
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			//
			// NumberOfResults
			//
			this.BindingSource.SetBindingMember(this.NumberOfResults, "NumberOfResults");
			this.NumberOfResults.Decimals = 0;
			this.NumberOfResults.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 3, true);
			this.NumberOfResults.Name = "NumberOfResults";
			this.NumberOfResults.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(44, 18, true);
			this.NumberOfResults.TabIndex = 4;
			this.NumberOfResults.Text = "10,000";
			this.NumberOfResults.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			//
			// ZPreviousNextControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.NumberOfResults);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CurrentRecordNumberCalcEdit);
			this.Controls.Add(this.NextButton);
			this.Controls.Add(this.PreviousButton);
			this.Name = "ZPreviousNextControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 26, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
