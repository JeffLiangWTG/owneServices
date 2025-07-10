using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed partial class FormWithTabPageNotifications
	{
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			this.TabControl = new ZTabControl();
			this.TabPage1 = new ZTabPage();
			this.TabPage2 = new ZTabPage();
			this.zLabel1 = new ZLabel();
			this.txtNumber = new ZCalcEdit();
			this.Grid = new ZGrid();
			this.TabPage3 = new TestBindingTabPage();
			this.InnerControl = new UserControlForTestTabPage();
			this.TabPage4 = new TestBindingTabPage();
			this.TabControlWithDynamicCreationControl = new ZTabControl();
			this.TabPageWithDynamicCreationControl = new ZTabPage();
			this.DynamicControlCreationUserControl = new ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.TabPage2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).BeginInit();
			this.Grid.SuspendLayout();
			this.TabPage3.SuspendLayout();
			this.InnerControl.SuspendLayout();
			this.TabPage4.SuspendLayout();
			this.TabControlWithDynamicCreationControl.SuspendLayout();
			this.TabPageWithDynamicCreationControl.SuspendLayout();
			this.DynamicControlCreationUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = new Point(0, 481);
			this.MainStatusBar.Size = new Size(579, 24);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CargoWise.EntityFramework.Testing.DummyWithDependentsBusinessObject);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.TabPage1);
			this.TabControl.Controls.Add(this.TabPage2);
			this.TabControl.Controls.Add(this.TabPage3);
			this.TabControl.Controls.Add(this.TabPage4);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(555, 687, true);
			this.TabControl.TabIndex = 1;
			// 
			// TabPage1
			// 
			this.TabPage1.Location = new Point(4, 23);
			this.TabPage1.Name = "TabPage1";
			this.TabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.TabPage1.Size = new Size(547, 443);
			this.TabPage1.TabIndex = 0;
			this.TabPage1.Text = "TabPage1";
			this.TabPage1.UseVisualStyleBackColor = true;
			// 
			// TabPage2
			// 
			this.TabPage2.Controls.Add(this.zLabel1);
			this.TabPage2.Controls.Add(this.txtNumber);
			this.TabPage2.Controls.Add(this.Grid);
			this.TabPage2.Location = new Point(4, 23);
			this.TabPage2.Name = "TabPage2";
			this.TabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.TabPage2.Size = new Size(547, 443);
			this.TabPage2.TabIndex = 1;
			this.TabPage2.Text = "TabPage2";
			this.TabPage2.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = new Point(6, 415);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = new Size(70, 23);
			this.zLabel1.TabIndex = 2;
			this.zLabel1.Text = "Number:";
			// 
			// txtNumber
			// 
			this.BindingSource.SetBindingMember(this.txtNumber, "Dependents.ZD1_Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.Testing.DummyDependantBusinessObject)(((System.Collections.IList)(((CargoWise.EntityFramework.Testing.DummyWithDependentsBusinessObject)(null)).Dependents)).SyncRoot)).ZD1_Number);
			this.txtNumber.DecimalPlaces = 2;
			this.txtNumber.Location = new Point(82, 417);
			this.txtNumber.Name = "txtNumber";
			this.txtNumber.Size = new Size(100, 20);
			this.txtNumber.TabIndex = 1;
			this.txtNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// Grid
			// 
			this.Grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.Grid, "Dependents");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.Testing.DummyWithDependentsBusinessObject)(null)).Dependents);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.Testing.DummyDependantBusinessObject)(((System.Collections.IList)(((CargoWise.EntityFramework.Testing.DummyWithDependentsBusinessObject)(null)).Dependents)).SyncRoot)).ZD1_Code);
			this.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo2.Caption = "Code";
			zTextBoxColumnStyleInfo2.ColumnName = "ZD1_Code";
			zTextBoxColumnStyleInfo2.Width = 120;
			this.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.Grid.GridId = "92d01668-d6d6-47a6-87a5-b2bb715b604b";
			this.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.Grid.LayoutKey = "Grid";
			this.Grid.Location = new Point(6, 6);
			this.Grid.Name = "Grid";
			this.Grid.Size = new Size(535, 405);
			this.Grid.TabIndex = 0;
			// 
			// TabPage3
			// 
			this.TabPage3.Controls.Add(this.InnerControl);
			this.TabPage3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPage3.Name = "TabPage3";
			this.TabPage3.Padding = new System.Windows.Forms.Padding(3);
			this.TabPage3.Size = new Size(547, 443);
			this.TabPage3.TabIndex = 2;
			this.TabPage3.Text = "TabPage3";
			this.TabPage3.UseVisualStyleBackColor = true;
			// 
			// InnerControl
			// 
			this.InnerControl.AllowDrop = true;
			this.InnerControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InnerControl.Name = "InnerControl";
			this.InnerControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 150, true);
			this.InnerControl.TabIndex = 0;
			// 
			// TabPage4
			// 
			this.TabPage4.Controls.Add(this.TabControlWithDynamicCreationControl);
			this.TabPage4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPage4.Name = "TabPage4";
			this.TabPage4.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TabPage4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(547, 660, true);
			this.TabPage4.TabIndex = 3;
			this.TabPage4.Text = "TabPage4";
			this.TabPage4.UseVisualStyleBackColor = true;
			// 
			// TabControlWithDynamicCreationControl
			// 
			this.TabControlWithDynamicCreationControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.TabControlWithDynamicCreationControl.Controls.Add(this.TabPageWithDynamicCreationControl);
			this.TabControlWithDynamicCreationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControlWithDynamicCreationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.TabControlWithDynamicCreationControl.Name = "TabControlWithDynamicCreationControl";
			this.TabControlWithDynamicCreationControl.SelectedIndex = 0;
			this.TabControlWithDynamicCreationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(541, 654, true);
			this.TabControlWithDynamicCreationControl.TabIndex = 0;
			// 
			// TabPageWithDynamicCreationControl
			// 
			this.TabPageWithDynamicCreationControl.Controls.Add(this.DynamicControlCreationUserControl);
			this.TabPageWithDynamicCreationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TabPageWithDynamicCreationControl.Name = "TabPageWithDynamicCreationControl";
			this.TabPageWithDynamicCreationControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TabPageWithDynamicCreationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 627, true);
			this.TabPageWithDynamicCreationControl.TabIndex = 0;
			this.TabPageWithDynamicCreationControl.UseVisualStyleBackColor = true;
			// 
			// DynamicControlCreationUserControl
			// 
			this.DynamicControlCreationUserControl.AllowDrop = true;
			this.DynamicControlCreationUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicControlCreationUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.DynamicControlCreationUserControl.Name = "DynamicControlCreationUserControl";
			this.DynamicControlCreationUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 621, true);
			this.DynamicControlCreationUserControl.TabIndex = 0;
			this.DynamicControlCreationUserControl.UserControlType = typeof(UserControlForTestTabPageWithWrappedProperty);
			// 
			// FormWithTabPageNotifications
			// 
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new Size(579, 505);
			this.Controls.Add(this.TabControl);
			this.DataSourceType = typeof(CargoWise.EntityFramework.Testing.DummyWithDependentsBusinessObject);
			this.Name = "FormWithTabPageNotifications";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "FormWithTabPageNotifications";
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.TabControl.PerformLayout();
			this.TabPage2.ResumeLayout(false);
			this.TabPage2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Grid)).EndInit();
			this.Grid.ResumeLayout(false);
			this.Grid.PerformLayout();
			this.TabPage3.ResumeLayout(false);
			this.TabPage3.PerformLayout();
			this.InnerControl.ResumeLayout(true);
			this.InnerControl.PerformLayout();
			this.TabPage4.ResumeLayout(false);
			this.TabPage4.PerformLayout();
			this.TabControlWithDynamicCreationControl.ResumeLayout(false);
			this.TabControlWithDynamicCreationControl.PerformLayout();
			this.TabPageWithDynamicCreationControl.ResumeLayout(false);
			this.TabPageWithDynamicCreationControl.PerformLayout();
			this.DynamicControlCreationUserControl.ResumeLayout(true);
			this.DynamicControlCreationUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
