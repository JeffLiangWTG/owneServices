using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	partial class ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm
	{
		private void InitializeComponent()
		{
			this.groupBox1 = new CargoWise.Windows.UI.KGroupBox();
			this.TabControlInBaseClass = new CargoWise.Windows.UI.KTabControl();
			this.TabPageInBaseClass = new MockTabPage();
			this.TextBoxInBaseClass = new CargoWise.Windows.UI.KTextBox();
			this.groupBox1.SuspendLayout();
			this.TabControlInBaseClass.SuspendLayout();
			this.TabPageInBaseClass.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.TabControlInBaseClass);
			this.groupBox1.Location = new System.Drawing.Point(478, 12);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(228, 142);
			this.groupBox1.TabIndex = 2;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "TabPage in base class of inherited form ";
			// 
			// TabControlInBaseClass
			// 
			this.TabControlInBaseClass.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right);
			this.TabControlInBaseClass.Controls.Add(this.TabPageInBaseClass);
			this.TabControlInBaseClass.Location = new System.Drawing.Point(17, 20);
			this.TabControlInBaseClass.Name = "TabControlInBaseClass";
			this.TabControlInBaseClass.SelectedIndex = 0;
			this.TabControlInBaseClass.Size = new System.Drawing.Size(202, 106);
			this.TabControlInBaseClass.TabIndex = 0;
			// 
			// TabPageInBaseClass
			// 
			this.TabPageInBaseClass.Controls.Add(this.TextBoxInBaseClass);
			this.TabPageInBaseClass.Location = new System.Drawing.Point(4, 22);
			this.TabPageInBaseClass.Name = "TabPageInBaseClass";
			this.TabPageInBaseClass.Padding = new System.Windows.Forms.Padding(3);
			this.TabPageInBaseClass.Size = new System.Drawing.Size(194, 80);
			this.TabPageInBaseClass.TabIndex = 1;
			this.TabPageInBaseClass.Text = "TabPageInBaseClass";
			this.TabPageInBaseClass.UseVisualStyleBackColor = true;
			// 
			// TextBoxInBaseClass
			// 
			this.TextBoxInBaseClass.Location = new System.Drawing.Point(21, 17);
			this.TextBoxInBaseClass.Name = "TextBoxInBaseClass";
			this.TextBoxInBaseClass.Size = new System.Drawing.Size(100, 20);
			this.TextBoxInBaseClass.TabIndex = 0;
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(718, 526);
			this.Controls.Add(this.groupBox1);
			this.Name = "ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm";
			this.Text = "ControlCodeDomSerializerWithDelayedTabCreate_TestForm";
			this.groupBox1.ResumeLayout(false);
			this.TabControlInBaseClass.ResumeLayout(false);
			this.TabPageInBaseClass.ResumeLayout(false);
			this.TabPageInBaseClass.PerformLayout();
			this.ResumeLayout(false);
		}
	}
#endif
}
