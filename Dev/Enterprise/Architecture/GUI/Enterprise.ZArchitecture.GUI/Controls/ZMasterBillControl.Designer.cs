using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.Common.Testing;
using CargoWise.ComponentModel;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Notifications;

namespace Enterprise.ZArchitecture
{
	public abstract partial class OMasterBillNumber
	{
		ZTextBox DummyTextBox;
		protected OMasterBill1TextBox MasterBill1TextBox;
		protected OMasterBill2TextBox MasterBill2TextBox;
		protected OMasterBillTextBox MasterBillOtherTextBox;
		protected ZLabel SeparatorLabel;

		void InitializeComponent()
		{
			this.DummyTextBox = new ZTextBox();
			this.MasterBill1TextBox = new OMasterBill1TextBox();
			this.MasterBill2TextBox = new OMasterBill2TextBox();
			this.SeparatorLabel = new ZLabel();
			this.MasterBillOtherTextBox = new OMasterBillTextBox();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// DummyTextBox
			//
			this.DummyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.DummyTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.DummyTextBox.Enabled = false;
			this.DummyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DummyTextBox.Name = "DummyTextBox";
			this.DummyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.DummyTextBox.TabIndex = 0;
			//
			// MasterBill1TextBox
			//
			this.MasterBill1TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MasterBill1TextBox, false);
			this.MasterBill1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MasterBill1TextBox.MasterBill2TextBox = null;
			this.MasterBill1TextBox.Name = "MasterBill1TextBox";
			this.MasterBill1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 13, true);
			this.MasterBill1TextBox.TabIndex = 0;
			this.MasterBill1TextBox.TabStop = false;
			//
			// MasterBill2TextBox
			//
			this.MasterBill2TextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.MasterBill2TextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MasterBill2TextBox, false);
			this.MasterBill2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 3, true);
			this.MasterBill2TextBox.MasterBill1TextBox = null;
			this.MasterBill2TextBox.Name = "MasterBill2TextBox";
			this.MasterBill2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 13, true);
			this.MasterBill2TextBox.TabIndex = 1;
			//
			// SeparatorLabel
			//
			this.SeparatorLabel.BackColor = System.Drawing.SystemColors.Window;
			this.SeparatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 3, true);
			this.SeparatorLabel.Name = "SeparatorLabel";
			this.SeparatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 13, true);
			this.SeparatorLabel.TabIndex = 3;
			this.SeparatorLabel.Text = "-";
			//
			// MasterBillOtherTextBox
			//
			this.MasterBillOtherTextBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.MasterBillOtherTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MasterBillOtherTextBox.Name = "MasterBillOtherTextBox";
			this.MasterBillOtherTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.MasterBillOtherTextBox.TabIndex = 4;
			this.MasterBillOtherTextBox.Visible = false;
			//
			// OMasterBillNumber
			//
			this.CaptionRenderingEnabled = true;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.Controls.Add(this.MasterBill1TextBox);
			this.Controls.Add(this.MasterBill2TextBox);
			this.Controls.Add(this.SeparatorLabel);
			this.Controls.Add(this.DummyTextBox);
			this.Controls.Add(this.MasterBillOtherTextBox);
			this.Name = "OMasterBillNumber";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
