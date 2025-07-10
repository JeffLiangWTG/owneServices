
namespace Enterprise.Customs.ES.NCTS.GUI
{
	partial class EditClearanceInfoForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.ClearanceNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.ClearanceDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalLimitDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.ClearanceInfo);
			// 
			// ClearanceNumber
			// 
			this.BindingSource.SetBindingMember(this.ClearanceNumber, "ClearanceNumber");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.NCTS.Business.ClearanceInfo)(null)).ClearanceNumber)));
			this.ClearanceNumber.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("4301F34E-BB47-4999-9C15-B787237517E6", "Clearance Number");
			this.ClearanceNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 40, true);
			this.ClearanceNumber.Name = "ClearanceNumber";
			this.ClearanceNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ClearanceNumber.TabIndex = 1;
			// 
			// ClearanceDate
			// 
			this.ClearanceDate.AllowDrop = true;
			this.ClearanceDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ClearanceDate, "ClearanceDate");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.ClearanceInfo)(null)).ClearanceDate)));
			this.ClearanceDate.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("AC095D8D-EE70-4D83-A5DA-337E4FFE6E8A", "Clearance Date");
			this.ClearanceDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ClearanceDate, true);
			this.ClearanceDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 66, true);
			this.ClearanceDate.Name = "ClearanceDate";
			this.ClearanceDate.TabIndex = 2;
			// 
			// ArrivalLimitDate
			// 
			this.ArrivalLimitDate.AllowDrop = true;
			this.ArrivalLimitDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ArrivalLimitDate, "ArrivalLimitDate");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.NCTS.Business.ClearanceInfo)(null)).ArrivalLimitDate)));
			this.ArrivalLimitDate.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("15666A6A-F75D-4898-9C85-D5165E9B969B", "Arrival Limit Date");
			this.ArrivalLimitDate.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.LongIncludingSeconds;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ArrivalLimitDate, true);
			this.ArrivalLimitDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 92, true);
			this.ArrivalLimitDate.Name = "ArrivalLimitDate";
			this.ArrivalLimitDate.TabIndex = 3;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("16FBF1B4-17AB-46E1-8DBC-C62B36CD1C31", "&OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 141, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("C96EE0DE-8CDA-42AF-8F98-96BFB30AD68E", "&Cancel");
			this.Cancel_Button.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 141, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(76, 23, true);
			this.Cancel_Button.TabIndex = 5;
			this.Cancel_Button.ToolTipCaption = null;
			this.Cancel_Button.UseVisualStyleBackColor = true;
			// 
			// EditClearanceInfoForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 194, true);
			this.Controls.Add(this.ClearanceNumber);
			this.Controls.Add(this.ClearanceDate);
			this.Controls.Add(this.ArrivalLimitDate);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.Cancel_Button);
			this.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.ClearanceInfo);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EditClearanceInfoForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Edit Clearance Info";
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.ClearanceNumber, 0);
			this.Controls.SetChildIndex(this.ClearanceDate, 0);
			this.Controls.SetChildIndex(this.ArrivalLimitDate, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected ZArchitecture.ZTextBox ClearanceNumber;
		protected ZArchitecture.GUI.ZDateEdit ClearanceDate;
		protected ZArchitecture.GUI.ZDateEdit ArrivalLimitDate;
		ZArchitecture.GUI.ZButton OKButton;
		ZArchitecture.GUI.ZButton Cancel_Button;
	}
}
