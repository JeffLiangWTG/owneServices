using System.Windows.Forms;
using Enterprise.Accounting.Business.GLAccountFormat;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GLAccountFormat
{
	public partial class FormatChangeForm
	{
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.zTextBox1 = new ZArchitecture.ZTextBox();
			this.zTextBox2 = new ZArchitecture.ZTextBox();
			this.ContinueButton = new ZButton();
			this.CancelBoundButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 114, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 22, true);
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
			this.BindingSource.DataSourceType = typeof(GLAccountFormatter);
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "CurrentFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((GLAccountFormatter)(null)).CurrentFormat)));
			this.zTextBox1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FormatChangeForm|a6f10784-0a7e-4ec5-bef1-b896d212cb4c", "Current  format");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 15, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.zTextBox1.TabIndex = 1;
			this.zTextBox1.Text = "ZTEXTBOX1";
			// 
			// zTextBox2
			// 
			this.BindingSource.SetBindingMember(this.zTextBox2, "NewFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((GLAccountFormatter)(null)).NewFormat)));
			this.zTextBox2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FormatChangeForm|ae10b74d-0ddd-49a7-8375-96f83097b9a4", "New format");
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 20, true);
			this.zTextBox2.TabIndex = 2;
			this.zTextBox2.Text = "ZTEXTBOX2";
			// 
			// ContinueButton
			// 
			this.ContinueButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FormatChangeForm|2396a840-db73-44a2-a0b7-a1a0f6b8b6dc", "Continue");
			this.ContinueButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 74, true);
			this.ContinueButton.Name = "ContinueButton";
			this.ContinueButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ContinueButton.TabIndex = 5;
			this.ContinueButton.Click += new System.EventHandler(this.ContinueButton_Click);
			// 
			// CancelBoundButton
			// 
			this.CancelBoundButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FormatChangeForm|965c7315-e5fc-4c40-9a72-bb372a9f069c", "Cancel");
			this.CancelBoundButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(240, 74, true);
			this.CancelBoundButton.Name = "CancelBoundButton";
			this.CancelBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CancelBoundButton.TabIndex = 6;
			this.CancelBoundButton.Click += new System.EventHandler(this.CancelBoundButton_Click);
			// 
			// FormatChangeForm
			// 
			this.AcceptButton = this.ContinueButton;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 136, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("FormatChangeForm|15d893d8-8304-4cac-81cc-80c2b3bd8a13", "GL Account Format");
			this.Controls.Add(this.CancelBoundButton);
			this.Controls.Add(this.ContinueButton);
			this.Controls.Add(this.zTextBox2);
			this.Controls.Add(this.zTextBox1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(GLAccountFormatter);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GLAccountFormat.GLAccountFormatter";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FormatChangeForm";
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.zTextBox2, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ContinueButton, 0);
			this.Controls.SetChildIndex(this.CancelBoundButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
