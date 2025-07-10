using Enterprise.ZArchitecture.GUI;

#if DEBUG

namespace Enterprise.Testing
{
	public partial class LoadPreviousDatTestForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel UserTestPKLabel;
		Enterprise.ZArchitecture.GUI.ZButton loadTestsButton;
		ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZTextBox UserTestPKTextBox;

		new void InitializeComponent()
		{
			this.UserTestPKLabel = new Enterprise.ZArchitecture.ZLabel();
			this.loadTestsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UserTestPKTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 143, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
#if !WINZOR
			this.MessageStatusBarPanel.Width = 252;
#else
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(252);
#endif
			// 
			// ErrorStatusBarPanel
			// 
#if !WINZOR
			this.ErrorStatusBarPanel.Width = 253;
#else
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(253);
#endif
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Testing.TestListLoader);
			// 
			// UserTestPKLabel
			// 
			this.UserTestPKLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 56, true);
			this.UserTestPKLabel.Name = "UserTestPKLabel";
			this.UserTestPKLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.UserTestPKLabel.TabIndex = 2;
			this.UserTestPKLabel.Text = "UserTestPK:";
			// 
			// loadTestsButton
			// 
			this.loadTestsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.loadTestsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(581, 107, true);
			this.loadTestsButton.Name = "loadTestsButton";
			this.loadTestsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
			this.loadTestsButton.TabIndex = 1;
			this.loadTestsButton.Text = "Load Tests";
			this.loadTestsButton.Click += new System.EventHandler(this.loadTestsButton_Click);
			// 
			// UserTestPKTextBox
			// 
			this.UserTestPKTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.UserTestPKTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.UserTestPKTextBox, "FileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Testing.TestListLoader)(null)).FileName)));
			this.UserTestPKTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 55, true);
			this.UserTestPKTextBox.Name = "UserTestPKTextBox";
			this.UserTestPKTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 20, true);
			this.UserTestPKTextBox.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(419, 35, true);
			this.zLabel1.TabIndex = 4;
			this.zLabel1.Text = "You may now paste the full hyperlink to the DAT test run. Finally :)";
			// 
			// LoadPreviousDatTestForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(713, 165, true);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.UserTestPKTextBox);
			this.Controls.Add(this.loadTestsButton);
			this.Controls.Add(this.UserTestPKLabel);
			this.DataSourceType = typeof(Enterprise.Testing.TestListLoader);
			this.DataSourceTypeName = "Enterprise.Testing.TestListLoader";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "LoadPreviousDatTestForm";
			this.Text = "Load Previous Dat Test";
			this.Activated += new System.EventHandler(this.LoadPreviousDatTestForm_Activated);
			this.Controls.SetChildIndex(this.UserTestPKLabel, 0);
			this.Controls.SetChildIndex(this.loadTestsButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.UserTestPKTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
#endif
