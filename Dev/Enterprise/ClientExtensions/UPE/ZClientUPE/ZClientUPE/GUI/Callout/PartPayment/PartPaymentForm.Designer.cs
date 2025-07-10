using System;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class PartPaymentForm : ZChildForm
	{
		protected override void InitializeComponent()
		{
			this.AmountToCollectCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReasonTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TypeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RemarksTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RemarksLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 245, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(146);
			// 
			// AmountToCollectCalcEdit
			// 
			this.AmountToCollectCalcEdit.BindTo = "AmountToCollect";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.INumericZType)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).AmountToCollect)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).AmountToCollectInfo)));
			this.AmountToCollectCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 41, true);
			this.AmountToCollectCalcEdit.Name = "AmountToCollectCalcEdit";
			this.AmountToCollectCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.AmountToCollectCalcEdit.TabIndex = 3;
			this.AmountToCollectCalcEdit.Text = "0.00";
			this.AmountToCollectCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 216, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.Text = "&OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ReasonTypeDropEdit
			// 
			this.ReasonTypeDropEdit.BindTo = "ReasonType";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).ReasonTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).ReasonType)));
			this.ReasonTypeDropEdit.BindToList = "CalloutPartPaymentCodeDescriptionPairList";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).CalloutPartPaymentCodeDescriptionPairList)));
			this.ReasonTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 15, true);
			this.ReasonTypeDropEdit.Name = "ReasonTypeDropEdit";
			this.ReasonTypeDropEdit.PreBoundMaxLength = 3;
			this.ReasonTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ReasonTypeDropEdit.TabIndex = 1;
			// 
			// TypeLabel
			// 
			this.TypeLabel.AutoSize = true;
			this.TypeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 19, true);
			this.TypeLabel.Name = "TypeLabel";
			this.TypeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 13, true);
			this.TypeLabel.TabIndex = 0;
			this.TypeLabel.Text = "Reason Type:";
			// 
			// RemarksTextBox
			// 
			this.RemarksTextBox.BindTo = "Remarks";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).RemarksInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutPartPayment)(null)).Remarks)));
			this.RemarksTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RemarksTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 67, true);
			this.RemarksTextBox.Multiline = true;
			this.RemarksTextBox.Name = "RemarksTextBox";
			this.RemarksTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 142, true);
			this.RemarksTextBox.TabIndex = 5;
			// 
			// RemarksLabel
			// 
			this.RemarksLabel.AutoSize = true;
			this.RemarksLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 68, true);
			this.RemarksLabel.Name = "RemarksLabel";
			this.RemarksLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(52, 13, true);
			this.RemarksLabel.TabIndex = 4;
			this.RemarksLabel.Text = "Remarks:";
			// 
			// zLabel3
			// 
			this.zLabel3.AutoSize = true;
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 44, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 13, true);
			this.zLabel3.TabIndex = 2;
			this.zLabel3.Text = "Amount to Collect:";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(275, 216, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "&Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// PartPaymentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 269, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.RemarksLabel);
			this.Controls.Add(this.RemarksTextBox);
			this.Controls.Add(this.TypeLabel);
			this.Controls.Add(this.ReasonTypeDropEdit);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.AmountToCollectCalcEdit);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.CalloutPartPayment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "PartPaymentForm";
			this.Controls.SetChildIndex(this.AmountToCollectCalcEdit, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.ReasonTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.TypeLabel, 0);
			this.Controls.SetChildIndex(this.RemarksTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.RemarksLabel, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.ZCalcEdit AmountToCollectCalcEdit;
		internal ZButton OKButton;
		ZDropEdit ReasonTypeDropEdit;
		Enterprise.ZArchitecture.ZLabel TypeLabel;
		Enterprise.ZArchitecture.ZTextBox RemarksTextBox;
		Enterprise.ZArchitecture.ZLabel RemarksLabel;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		internal ZButton cancelButton;
	}
}
