using System;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.JobInvoicing.BatchPosting
{
	public partial class BatchPostingForm
	{


		#region Windows Form Designer generated code

		protected ZButton CloseButton;
		protected ZArchitecture.ZLabel RecordsUpdatedLabel;
		protected ZArchitecture.ZCalcEdit ObjectsPostedCalcEdit;
		protected ZArchitecture.ZCalcEdit RecordsUpdatedCalcEdit;
		protected KRichTextBox ProgressTextBox;
		private KProgressBar ProgressBar;
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.ObjectsPostedCalcEdit = new ZArchitecture.ZCalcEdit();
			this.ProgressTextBox = new KRichTextBox();
			this.ProgressBar = new KProgressBar();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 26, true);
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
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BatchPostingBusinessObject);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BatchPostingForm|2446f0c4-f410-4424-aab1-75d369cb376d", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 496, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
			this.CloseButton.TabIndex = 12;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// ObjectsPostedCalcEdit
			// 
			this.ObjectsPostedCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.ObjectsPostedCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("BatchPostingForm|2bdb94ec-b28c-4d09-97f8-8a67379576c6", "Objects Posted");
			this.BindingSource.SetBindingMember(this.ObjectsPostedCalcEdit, "ObjectsPosted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((BatchPostingBusinessObject)(null)).ObjectsPosted)));
			this.ObjectsPostedCalcEdit.DecimalPlaces = 0;
			this.ObjectsPostedCalcEdit.Decimals = 0;
			this.ObjectsPostedCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 496, true);
			this.ObjectsPostedCalcEdit.Name = "ObjectsPostedCalcEdit";
			this.ObjectsPostedCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ObjectsPostedCalcEdit.TabIndex = 8;
			this.ObjectsPostedCalcEdit.Text = "1,000";
			this.ObjectsPostedCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProgressTextBox
			// 
			this.ProgressTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.ProgressTextBox.Name = "ProgressTextBox";
			this.ProgressTextBox.ReadOnly = true;
			this.ProgressTextBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.ProgressTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 476, true);
			this.ProgressTextBox.TabIndex = 5;
			this.ProgressTextBox.Text = "";
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 497, true);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 20, true);
			this.ProgressBar.TabIndex = 14;
			// 
			// BatchPostingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Controls.Add(this.ProgressBar);
			this.Controls.Add(this.ProgressTextBox);
			this.Controls.Add(this.ObjectsPostedCalcEdit);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.GUI";
			this.DataSourceType = typeof(BatchPostingBusinessObject);
			this.DataSourceTypeName = "Enterprise.Accounting.GUI.JobInvoicing.BatchPosting.BatchPostingBusinessObject";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "BatchPostingForm";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ObjectsPostedCalcEdit, 0);
			this.Controls.SetChildIndex(this.ProgressTextBox, 0);
			this.Controls.SetChildIndex(this.ProgressBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}