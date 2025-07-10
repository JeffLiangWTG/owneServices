#if DEBUG
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Testing
{
	public partial class LoadTestListForm : ZChildForm
	{
		Enterprise.ZArchitecture.ZLabel TestStartedTimeLabel;
		Enterprise.ZArchitecture.GUI.ZButton loadTestsButton;
		Enterprise.ZArchitecture.GUI.ZCheckBox groupTestsByAssemblyCheckBox;
		ZOpenFileDialog openFileDialog;
		Enterprise.ZArchitecture.ZTextBox FileNameTextBox;
		CargoWise.Windows.UI.KButton OpenFileDialogButton;

		System.ComponentModel.Container components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		new void InitializeComponent()
		{
			this.TestStartedTimeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.loadTestsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.groupTestsByAssemblyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.FileNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.openFileDialog = new ZOpenFileDialog();
			this.OpenFileDialogButton = new CargoWise.Windows.UI.KButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 22, true);
			this.MainStatusBar.TabIndex = 5;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(252);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(253);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Testing.TestListLoader);
			// 
			// TestStartedTimeLabel
			// 
			this.TestStartedTimeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 30, true);
			this.TestStartedTimeLabel.Name = "TestStartedTimeLabel";
			this.TestStartedTimeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 21, true);
			this.TestStartedTimeLabel.TabIndex = 1;
			this.TestStartedTimeLabel.Text = "Filename:";
			// 
			// loadTestsButton
			// 
			this.loadTestsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 67, true);
			this.loadTestsButton.Name = "loadTestsButton";
			this.loadTestsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 21, true);
			this.loadTestsButton.TabIndex = 0;
			this.loadTestsButton.Text = "Load Tests";
			this.loadTestsButton.Click += new System.EventHandler(this.loadTestsButton_Click);
			// 
			// groupTestsByAssemblyCheckBox
			// 
			this.BindingSource.SetBindingMember(this.groupTestsByAssemblyCheckBox, "GroupTestsByAssembly");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Testing.TestListLoader)(null)).GroupTestsByAssembly)));
			this.groupTestsByAssemblyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.groupTestsByAssemblyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.groupTestsByAssemblyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 67, true);
			this.groupTestsByAssemblyCheckBox.Name = "groupTestsByAssemblyCheckBox";
			this.groupTestsByAssemblyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 21, true);
			this.groupTestsByAssemblyCheckBox.TabIndex = 4;
			this.groupTestsByAssemblyCheckBox.Text = "Group Tests By Assembly";
			// 
			// FileNameTextBox
			// 
			this.FileNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.FileNameTextBox, "FileName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Testing.TestListLoader)(null)).FileName)));
			this.FileNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 30, true);
			this.FileNameTextBox.Name = "FileNameTextBox";
			this.FileNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 19, true);
			this.FileNameTextBox.TabIndex = 2;
			// 
			// openFileDialog
			// 
			this.openFileDialog.RestoreDirectory = true;
			// 
			// OpenFileDialogButton
			// 
			this.OpenFileDialogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(482, 29, true);
			this.OpenFileDialogButton.Name = "OpenFileDialogButton";
			this.OpenFileDialogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 21, true);
			this.OpenFileDialogButton.TabIndex = 3;
			this.OpenFileDialogButton.Text = "...";
			this.OpenFileDialogButton.Click += new System.EventHandler(this.OpenFileDialogButton_Click);
			// 
			// LoadTestListForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(534, 126, true);
			this.Controls.Add(this.OpenFileDialogButton);
			this.Controls.Add(this.FileNameTextBox);
			this.Controls.Add(this.loadTestsButton);
			this.Controls.Add(this.groupTestsByAssemblyCheckBox);
			this.Controls.Add(this.TestStartedTimeLabel);
			this.DataSourceType = typeof(Enterprise.Testing.TestListLoader);
			this.DataSourceTypeName = "Enterprise.Testing.TestListLoader";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 163, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 163, true);
			this.Name = "LoadTestListForm";
			this.Text = "Select the testrun file";
			this.Controls.SetChildIndex(this.TestStartedTimeLabel, 0);
			this.Controls.SetChildIndex(this.groupTestsByAssemblyCheckBox, 0);
			this.Controls.SetChildIndex(this.loadTestsButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FileNameTextBox, 0);
			this.Controls.SetChildIndex(this.OpenFileDialogButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
#endif
