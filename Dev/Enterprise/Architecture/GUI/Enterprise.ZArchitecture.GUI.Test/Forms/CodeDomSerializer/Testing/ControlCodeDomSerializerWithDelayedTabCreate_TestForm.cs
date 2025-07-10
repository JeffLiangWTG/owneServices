using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[WTG.StaticAnalysis.Annotation.CodeAlive("This form is for test purposes.")]
	[SuppressFormDesignerAnalysis]
	class ControlCodeDomSerializerWithDelayedTabCreate_TestForm : ControlCodeDomSerializerWithDelayedTabCreate_TestForm_Base
	{
		public ControlCodeDomSerializerWithDelayedTabCreate_TestForm()
		{
			InitializeComponent();
		}

		private readonly System.ComponentModel.IContainer components;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.TabControl = new CargoWise.Windows.UI.KTabControl();
			this.ATabPage = new MockTabPage();
			this.ContainerControlWithinTabPage = new CargoWise.Windows.UI.KGroupBox();
			this.TextBoxWithinTabPage = new CargoWise.Windows.UI.KTextBox();
			this.NestedTabControl = new CargoWise.Windows.UI.KTabControl();
			this.NestedTabPage = new MockTabPage();
			this.ContainerControlInNestedTabPage = new CargoWise.Windows.UI.KGroupBox();
			this.TextBoxWithinNestedTabPage = new CargoWise.Windows.UI.KTextBox();
			this.ContainerControlOutsideTabPage = new CargoWise.Windows.UI.KGroupBox();
			this.TextBoxOutsideTabPage = new CargoWise.Windows.UI.KTextBox();
			this.TabControl.SuspendLayout();
			this.ATabPage.SuspendLayout();
			this.ContainerControlWithinTabPage.SuspendLayout();
			this.NestedTabControl.SuspendLayout();
			this.NestedTabPage.SuspendLayout();
			this.ContainerControlInNestedTabPage.SuspendLayout();
			this.ContainerControlOutsideTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.ATabPage);
			this.TabControl.Location = new System.Drawing.Point(12, 139);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = new System.Drawing.Size(313, 375);
			this.TabControl.TabIndex = 0;
			// 
			// ATabPage
			// 
			this.ATabPage.Controls.Add(this.NestedTabControl);
			this.ATabPage.Controls.Add(this.ContainerControlWithinTabPage);
			this.ATabPage.Location = new System.Drawing.Point(4, 22);
			this.ATabPage.Name = "ATabPage";
			this.ATabPage.Padding = new System.Windows.Forms.Padding(3);
			this.ATabPage.Size = new System.Drawing.Size(305, 349);
			this.ATabPage.TabIndex = 0;
			this.ATabPage.Text = "TabPage";
			this.ATabPage.UseVisualStyleBackColor = true;
			// 
			// ContainerControlWithinTabPage
			// 
			this.ContainerControlWithinTabPage.Controls.Add(this.TextBoxWithinTabPage);
			this.ContainerControlWithinTabPage.Location = new System.Drawing.Point(6, 6);
			this.ContainerControlWithinTabPage.Name = "ContainerControlWithinTabPage";
			this.ContainerControlWithinTabPage.Size = new System.Drawing.Size(289, 158);
			this.ContainerControlWithinTabPage.TabIndex = 0;
			this.ContainerControlWithinTabPage.TabStop = false;
			this.ContainerControlWithinTabPage.Text = "Container Control nested within TabPage";
			// 
			// TextBoxWithinTabPage
			// 
			this.TextBoxWithinTabPage.Location = new System.Drawing.Point(34, 45);
			this.TextBoxWithinTabPage.Name = "TextBoxWithinTabPage";
			this.TextBoxWithinTabPage.Size = new System.Drawing.Size(100, 20);
			this.TextBoxWithinTabPage.TabIndex = 0;
			// 
			// NestedTabControl
			// 
			this.NestedTabControl.Controls.Add(this.NestedTabPage);
			this.NestedTabControl.Location = new System.Drawing.Point(19, 181);
			this.NestedTabControl.Name = "NestedTabControl";
			this.NestedTabControl.SelectedIndex = 0;
			this.NestedTabControl.Size = new System.Drawing.Size(265, 149);
			this.NestedTabControl.TabIndex = 1;
			// 
			// NestedTabPage
			// 
			this.NestedTabPage.Controls.Add(this.ContainerControlInNestedTabPage);
			this.NestedTabPage.Location = new System.Drawing.Point(4, 22);
			this.NestedTabPage.Name = "NestedTabPage";
			this.NestedTabPage.Padding = new System.Windows.Forms.Padding(3);
			this.NestedTabPage.Size = new System.Drawing.Size(257, 123);
			this.NestedTabPage.TabIndex = 0;
			this.NestedTabPage.Text = "NestedTabPage";
			this.NestedTabPage.UseVisualStyleBackColor = true;
			// 
			// ContainerControlInNestedTabPage
			// 
			this.ContainerControlInNestedTabPage.Controls.Add(this.TextBoxWithinNestedTabPage);
			this.ContainerControlInNestedTabPage.Location = new System.Drawing.Point(17, 16);
			this.ContainerControlInNestedTabPage.Name = "ContainerControlInNestedTabPage";
			this.ContainerControlInNestedTabPage.Size = new System.Drawing.Size(221, 90);
			this.ContainerControlInNestedTabPage.TabIndex = 0;
			this.ContainerControlInNestedTabPage.TabStop = false;
			this.ContainerControlInNestedTabPage.Text = "groupBox1";
			// 
			// TextBoxWithinNestedTabPage
			// 
			this.TextBoxWithinNestedTabPage.Location = new System.Drawing.Point(19, 31);
			this.TextBoxWithinNestedTabPage.Name = "TextBoxWithinNestedTabPage";
			this.TextBoxWithinNestedTabPage.Size = new System.Drawing.Size(100, 20);
			this.TextBoxWithinNestedTabPage.TabIndex = 1;
			// 
			// ContainerControlOutsideTabPage
			// 
			this.ContainerControlOutsideTabPage.Controls.Add(this.TextBoxOutsideTabPage);
			this.ContainerControlOutsideTabPage.Location = new System.Drawing.Point(354, 166);
			this.ContainerControlOutsideTabPage.Name = "ContainerControlOutsideTabPage";
			this.ContainerControlOutsideTabPage.Size = new System.Drawing.Size(257, 158);
			this.ContainerControlOutsideTabPage.TabIndex = 1;
			this.ContainerControlOutsideTabPage.TabStop = false;
			this.ContainerControlOutsideTabPage.Text = "Container Control outside of TabPage";
			// 
			// TextBoxOutsideTabPage
			// 
			this.TextBoxOutsideTabPage.Location = new System.Drawing.Point(25, 44);
			this.TextBoxOutsideTabPage.Name = "TextBoxOutsideTabPage";
			this.TextBoxOutsideTabPage.Size = new System.Drawing.Size(100, 20);
			this.TextBoxOutsideTabPage.TabIndex = 0;
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate_TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(718, 526);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.ContainerControlOutsideTabPage);
			this.Name = "ControlCodeDomSerializerWithDelayedTabCreate_TestForm";
			this.Controls.SetChildIndex(this.ContainerControlOutsideTabPage, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.TabControl.ResumeLayout(false);
			this.ATabPage.ResumeLayout(false);
			this.ContainerControlWithinTabPage.ResumeLayout(false);
			this.ContainerControlWithinTabPage.PerformLayout();
			this.NestedTabControl.ResumeLayout(false);
			this.NestedTabPage.ResumeLayout(false);
			this.ContainerControlInNestedTabPage.ResumeLayout(false);
			this.ContainerControlInNestedTabPage.PerformLayout();
			this.ContainerControlOutsideTabPage.ResumeLayout(false);
			this.ContainerControlOutsideTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		private void ATabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ContainerControlWithinTabPage = new CargoWise.Windows.UI.KGroupBox();
			this.TextBoxWithinTabPage = new CargoWise.Windows.UI.KTextBox();
			this.NestedTabControl = new CargoWise.Windows.UI.KTabControl();
			this.ATabPage.SuspendLayout();
			this.ContainerControlWithinTabPage.SuspendLayout();
			this.NestedTabControl.SuspendLayout();
			this.ATabPage.Controls.Add(this.NestedTabControl);
			this.ATabPage.Controls.Add(this.ContainerControlWithinTabPage);
			// 
			// ContainerControlWithinTabPage
			// 
			this.ContainerControlWithinTabPage.Controls.Add(this.TextBoxWithinTabPage);
			this.ContainerControlWithinTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6);
			this.ContainerControlWithinTabPage.Name = "ContainerControlWithinTabPage";
			this.ContainerControlWithinTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(289, 158);
			this.ContainerControlWithinTabPage.TabIndex = 0;
			this.ContainerControlWithinTabPage.TabStop = false;
			this.ContainerControlWithinTabPage.Text = "Container Control nested within TabPage";
			// 
			// TextBoxWithinTabPage
			// 
			this.TextBoxWithinTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(34, 45);
			this.TextBoxWithinTabPage.Name = "TextBoxWithinTabPage";
			this.TextBoxWithinTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.TextBoxWithinTabPage.TabIndex = 0;
			// 
			// NestedTabControl
			// 
			this.NestedTabControl.Controls.Add(this.NestedTabPage);
			this.NestedTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 181);
			this.NestedTabControl.Name = "NestedTabControl";
			this.NestedTabControl.SelectedIndex = 0;
			this.NestedTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 149);
			this.NestedTabControl.TabIndex = 1;
			this.ContainerControlWithinTabPage.ResumeLayout(false);
			this.ContainerControlWithinTabPage.PerformLayout();
			this.NestedTabControl.ResumeLayout(false);
			this.ATabPage.ResumeLayout(true);
		}

		private void NestedTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ContainerControlInNestedTabPage = new CargoWise.Windows.UI.KGroupBox();
			this.TextBoxWithinNestedTabPage = new CargoWise.Windows.UI.KTextBox();
			this.NestedTabPage.SuspendLayout();
			this.ContainerControlInNestedTabPage.SuspendLayout();
			this.NestedTabPage.Controls.Add(this.ContainerControlInNestedTabPage);
			// 
			// ContainerControlInNestedTabPage
			// 
			this.ContainerControlInNestedTabPage.Controls.Add(this.TextBoxWithinNestedTabPage);
			this.ContainerControlInNestedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 16);
			this.ContainerControlInNestedTabPage.Name = "ContainerControlInNestedTabPage";
			this.ContainerControlInNestedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 90);
			this.ContainerControlInNestedTabPage.TabIndex = 0;
			this.ContainerControlInNestedTabPage.TabStop = false;
			this.ContainerControlInNestedTabPage.Text = "groupBox1";
			// 
			// TextBoxWithinNestedTabPage
			// 
			this.TextBoxWithinNestedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 31);
			this.TextBoxWithinNestedTabPage.Name = "TextBoxWithinNestedTabPage";
			this.TextBoxWithinNestedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.TextBoxWithinNestedTabPage.TabIndex = 1;
			this.ContainerControlInNestedTabPage.ResumeLayout(false);
			this.ContainerControlInNestedTabPage.PerformLayout();
			this.NestedTabPage.ResumeLayout(true);
		}

		void AdditionalCodeMemberThatShouldntBeDeleted()
		{
		}

		private CargoWise.Windows.UI.KTabControl TabControl;

		private MockTabPage ATabPage;

		private CargoWise.Windows.UI.KGroupBox ContainerControlWithinTabPage;

		private CargoWise.Windows.UI.KTextBox TextBoxWithinTabPage;

		private CargoWise.Windows.UI.KGroupBox ContainerControlOutsideTabPage;

		private CargoWise.Windows.UI.KTextBox TextBoxOutsideTabPage;

		private CargoWise.Windows.UI.KTabControl NestedTabControl;

		private MockTabPage NestedTabPage;

		private CargoWise.Windows.UI.KGroupBox ContainerControlInNestedTabPage;

		private CargoWise.Windows.UI.KTextBox TextBoxWithinNestedTabPage;
	}
#endif
}
