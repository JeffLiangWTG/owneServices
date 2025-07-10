using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	partial class InterchangeNumberDetailsForm
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
			this.CompanyEDISiteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsEDISiteTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CurrentInterchangeNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NewInterchangeNumberCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ButtonIncrement = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonOK = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 176, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails);
			// 
			// CompanyEDISiteTextBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyEDISiteTextBox, "CompanyEDISite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails)(null)).CompanyEDISite)));
			this.CompanyEDISiteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 12, true);
			this.CompanyEDISiteTextBox.Name = "CompanyEDISiteTextBox";
			this.CompanyEDISiteTextBox.ReadOnly = true;
			this.CompanyEDISiteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 17, true);
			this.CompanyEDISiteTextBox.TabIndex = 1;
			// 
			// CustomsEDISiteTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsEDISiteTextBox, "CustomsEDISite");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails)(null)).CustomsEDISite)));
			this.CustomsEDISiteTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 44, true);
			this.CustomsEDISiteTextBox.Name = "CustomsEDISiteTextBox";
			this.CustomsEDISiteTextBox.ReadOnly = true;
			this.CustomsEDISiteTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(344, 17, true);
			this.CustomsEDISiteTextBox.TabIndex = 2;
			// 
			// CurrentInterchangeNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CurrentInterchangeNumberCalcEdit, "CurrentInterchangeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails)(null)).CurrentInterchangeNumber)));
			this.CurrentInterchangeNumberCalcEdit.DecimalPlaces = 0;
			this.CurrentInterchangeNumberCalcEdit.Decimals = 0;
			this.CurrentInterchangeNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 76, true);
			this.CurrentInterchangeNumberCalcEdit.Name = "CurrentInterchangeNumberCalcEdit";
			this.CurrentInterchangeNumberCalcEdit.ReadOnly = true;
			this.CurrentInterchangeNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.CurrentInterchangeNumberCalcEdit.TabIndex = 3;
			this.CurrentInterchangeNumberCalcEdit.Text = "0";
			this.CurrentInterchangeNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.CurrentInterchangeNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// NewInterchangeNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.NewInterchangeNumberCalcEdit, "NewInterchangeNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails)(null)).NewInterchangeNumber)));
			this.NewInterchangeNumberCalcEdit.DecimalPlaces = 0;
			this.NewInterchangeNumberCalcEdit.Decimals = 0;
			this.NewInterchangeNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 108, true);
			this.NewInterchangeNumberCalcEdit.Name = "NewInterchangeNumberCalcEdit";
			this.NewInterchangeNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.NewInterchangeNumberCalcEdit.TabIndex = 4;
			this.NewInterchangeNumberCalcEdit.Text = "0";
			this.NewInterchangeNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.NewInterchangeNumberCalcEdit.TrackDisposedAccess = true;
			// 
			// ButtonIncrement
			// 
			this.ButtonIncrement.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("2a3c4ca3-e139-497f-aaa5-afabafdff68f", "Increment (M)");
			this.ButtonIncrement.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 76, true);
			this.ButtonIncrement.Name = "ButtonIncrement";
			this.ButtonIncrement.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 50, true);
			this.ButtonIncrement.TabIndex = 5;
			this.ButtonIncrement.ToolTipCaption = null;
			this.ButtonIncrement.Click += ButtonIncrement_Click;
			// 
			// ButtonOK
			// 
			this.ButtonOK.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("e98d00f4-f258-425a-87b4-db129a843015", "OK");
			this.ButtonOK.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 140, true);
			this.ButtonOK.Name = "ButtonOK";
			this.ButtonOK.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonOK.TabIndex = 6;
			this.ButtonOK.ToolTipCaption = null;
			this.ButtonOK.Click += ButtonOK_Click;
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("11734b2c-c0f5-429b-9a45-05cf249d1657", "Cancel");
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(469, 140, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonCancel.TabIndex = 7;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.Click += ButtonCancel_Click;
			// 
			// IncreaseInterchangeNumberForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 200, true);
			this.Controls.Add(this.CustomsEDISiteTextBox);
			this.Controls.Add(this.CompanyEDISiteTextBox);
			this.Controls.Add(this.CurrentInterchangeNumberCalcEdit);
			this.Controls.Add(this.NewInterchangeNumberCalcEdit);
			this.Controls.Add(this.ButtonIncrement);
			this.Controls.Add(this.ButtonOK);
			this.Controls.Add(this.ButtonCancel);
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.InterchangeNumberDetails);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "InterchangeNumberDetailsForm";
			this.Text = Enterprise.Customs.AU.Declaration.GUI.Res.GetString("c5c45b2c-5dc6-4228-9056-7b243bee958d", "Increase Interchange Number");
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			this.Controls.SetChildIndex(this.ButtonOK, 0);
			this.Controls.SetChildIndex(this.ButtonIncrement, 0);
			this.Controls.SetChildIndex(this.NewInterchangeNumberCalcEdit, 0);
			this.Controls.SetChildIndex(this.CurrentInterchangeNumberCalcEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CompanyEDISiteTextBox, 0);
			this.Controls.SetChildIndex(this.CustomsEDISiteTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CompanyEDISiteTextBox;
		private ZArchitecture.ZTextBox CustomsEDISiteTextBox;
		private ZArchitecture.ZCalcEdit CurrentInterchangeNumberCalcEdit;
		private ZArchitecture.ZCalcEdit NewInterchangeNumberCalcEdit;
		internal ZButton ButtonIncrement;
		internal ZButton ButtonOK;
		internal ZButton ButtonCancel;
	}
}
