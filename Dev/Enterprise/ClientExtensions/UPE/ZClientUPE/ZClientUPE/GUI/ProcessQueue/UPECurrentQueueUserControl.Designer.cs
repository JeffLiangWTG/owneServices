using System;
using CargoWise.EntityFramework;
using Enterprise.Client.UPE.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class UPECurrentQueueUserControl : CurrentQueueUserControl
	{
		public Enterprise.ZArchitecture.ZLabel AccountNumberLabel;
		public Enterprise.ZArchitecture.ZTextBox AccountNumberTextBox;
		public Enterprise.ZArchitecture.GUI.ZDateEdit EIRRaisedDateDateEdit;
		public Enterprise.ZArchitecture.ZLabel EIRRaisedDateLabel;

		void InitializeComponent()
		{
			this.AccountNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.EIRRaisedDateLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AccountNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EIRRaisedDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.UPEActiveProcessQueue);
			// 
			// AccountNumberLabel
			// 
			this.AccountNumberLabel.AutoSize = true;
			this.AccountNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(667, 12, true);
			this.AccountNumberLabel.Name = "AccountNumberLabel";
			this.AccountNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.AccountNumberLabel.TabIndex = 5;
			this.AccountNumberLabel.Text = "Account Number:";
			// 
			// EIRRaisedDateLabel
			// 
			this.EIRRaisedDateLabel.AutoSize = true;
			this.EIRRaisedDateLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(667, 36, true);
			this.EIRRaisedDateLabel.Name = "EIRRaisedDateLabel";
			this.EIRRaisedDateLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 13, true);
			this.EIRRaisedDateLabel.TabIndex = 6;
			this.EIRRaisedDateLabel.Text = "EIR Raised Date:";
			// 
			// AccountNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccountNumberTextBox, "P4_CustomAttrib8");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Client.UPE.Business.UPEActiveProcessQueue)(null)).P4_CustomAttrib8)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AccountNumberTextBox, false);
			this.AccountNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(780, 8, true);
			this.AccountNumberTextBox.Name = "AccountNumberTextBox";
			this.AccountNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 20, true);
			this.AccountNumberTextBox.TabIndex = 7;
			// 
			// EIRRaisedDateDateEdit
			// 
			this.EIRRaisedDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.EIRRaisedDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EIRRaisedDateDateEdit, "P4_CustomDate4");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.UPEActiveProcessQueue)(null)).P4_CustomDate4)));
			this.EIRRaisedDateDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.EIRRaisedDateDateEdit, false);
			this.EIRRaisedDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(780, 32, true);
			this.EIRRaisedDateDateEdit.Name = "EIRRaisedDateDateEdit";
			this.EIRRaisedDateDateEdit.TabIndex = 8;
			// 
			// UPECurrentQueueUserControl
			// 
			this.Controls.Add(this.EIRRaisedDateDateEdit);
			this.Controls.Add(this.AccountNumberTextBox);
			this.Controls.Add(this.EIRRaisedDateLabel);
			this.Controls.Add(this.AccountNumberLabel);
			this.Name = "UPECurrentQueueUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(986, 88, true);
			this.Controls.SetChildIndex(this.AccountNumberLabel, 0);
			this.Controls.SetChildIndex(this.EIRRaisedDateLabel, 0);
			this.Controls.SetChildIndex(this.AccountNumberTextBox, 0);
			this.Controls.SetChildIndex(this.EIRRaisedDateDateEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
