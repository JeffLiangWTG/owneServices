
namespace Enterprise.Customs.IE.GUI
{
	partial class CopyAndSendToCustomsForm
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
		new void InitializeComponent()
		{
			this.NumberOfCopiesTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NumberOfInvoiceLinesCopiesTextBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendToCustomsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MessageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessageTypeDropEdit.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 113, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 23, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData);
			// 
			// NumberOfCopiesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NumberOfCopiesTextBox, "NumberOfCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData)(null)).NumberOfCopies)));
			this.NumberOfCopiesTextBox.CaptionResourceString = null;
			this.NumberOfCopiesTextBox.DecimalPlaces = 0;
			this.NumberOfCopiesTextBox.Decimals = 0;
			this.NumberOfCopiesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 11, true);
			this.NumberOfCopiesTextBox.Name = "NumberOfCopiesTextBox";
			this.NumberOfCopiesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.NumberOfCopiesTextBox.TabIndex = 0;
			this.NumberOfCopiesTextBox.Text = "0";
			this.NumberOfCopiesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NumberOfInvoiceLinesCopiesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NumberOfInvoiceLinesCopiesTextBox, "NumberOfInvoiceLinesCopies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData)(null)).NumberOfInvoiceLinesCopies)));
			this.NumberOfInvoiceLinesCopiesTextBox.CaptionResourceString = null;
			this.NumberOfInvoiceLinesCopiesTextBox.DecimalPlaces = 0;
			this.NumberOfInvoiceLinesCopiesTextBox.Decimals = 0;
			this.NumberOfInvoiceLinesCopiesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 31, true);
			this.NumberOfInvoiceLinesCopiesTextBox.Name = "NumberOfInvoiceLinesCopiesTextBox";
			this.NumberOfInvoiceLinesCopiesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 20, true);
			this.NumberOfInvoiceLinesCopiesTextBox.TabIndex = 0;
			this.NumberOfInvoiceLinesCopiesTextBox.Text = "0";
			this.NumberOfInvoiceLinesCopiesTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SendToCustomsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SendToCustomsCheckBox, "SendToCustoms");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData)(null)).SendToCustoms)));
			this.SendToCustomsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 53, true);
			this.SendToCustomsCheckBox.Name = "SendToCustomsCheckBox";
			this.SendToCustomsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 24, true);
			this.SendToCustomsCheckBox.TabIndex = 1;
			// 
			// MessageTypeDropEdit
			// 
			this.MessageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageTypeDropEdit, "MessageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData)(null)).MessageType)));
			this.MessageTypeDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 80, true);
			this.MessageTypeDropEdit.Name = "MessageTypeDropEdit";
			this.MessageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.MessageTypeDropEdit.TabIndex = 2;
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("17FDEC36-7794-4029-B9C7-0BC861CF2069", "Create");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 106, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.SendButton.TabIndex = 3;
			this.SendButton.ToolTipCaption = null;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelButton2
			// 
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.IE.GUI.Res.GetData("7644EF01-46EB-4DA4-BE4D-DCB664AA96F8", "Cancel");
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(291, 106, true);
			this.CancelButton2.Name = "CancelButton2";
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 21, true);
			this.CancelButton2.TabIndex = 4;
			this.CancelButton2.ToolTipCaption = null;
			this.CancelButton2.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.CancelButton2);
			this.MainGroupBox.Controls.Add(this.NumberOfCopiesTextBox);
			this.MainGroupBox.Controls.Add(this.NumberOfInvoiceLinesCopiesTextBox);
			this.MainGroupBox.Controls.Add(this.SendToCustomsCheckBox);
			this.MainGroupBox.Controls.Add(this.SendButton);
			this.MainGroupBox.Controls.Add(this.MessageTypeDropEdit);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 113, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// CopyAndSendToCustomsForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 156, true);
			this.Controls.Add(this.MainGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.IE.Business.CopyAndSendToCustomsFormData);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 175, true);
			this.Name = "CopyAndSendToCustomsForm";
			this.Text = "Copy And Send To Customs";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessageTypeDropEdit.ResumeLayout(true);
			this.MessageTypeDropEdit.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton SendButton;
		protected Enterprise.ZArchitecture.GUI.ZButton CancelButton2;
		internal Enterprise.ZArchitecture.ZCalcEdit NumberOfCopiesTextBox;
		internal Enterprise.ZArchitecture.ZCalcEdit NumberOfInvoiceLinesCopiesTextBox;
		internal ZArchitecture.GUI.ZCheckBox SendToCustomsCheckBox;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth MessageTypeDropEdit;
		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
	}
}
