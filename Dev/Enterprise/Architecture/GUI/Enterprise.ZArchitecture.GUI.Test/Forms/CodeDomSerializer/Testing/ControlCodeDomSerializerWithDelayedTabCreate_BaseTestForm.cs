using CargoWise.Windows.UI.Testing;

namespace Enterprise.ZArchitecture.GUI.Testing
{
#if !WINZOR
	[SuppressFormDesignerAnalysis]
	partial class ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm : ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm_Base
	{
		public ControlCodeDomSerializerWithDelayedTabCreate_BaseTestForm()
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

		private void TabPageInBaseClass_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TextBoxInBaseClass = new CargoWise.Windows.UI.KTextBox();
			this.TabPageInBaseClass.SuspendLayout();
			this.TabPageInBaseClass.Controls.Add(this.TextBoxInBaseClass);
			// 
			// TextBoxInBaseClass
			// 
			this.TextBoxInBaseClass.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 17);
			this.TextBoxInBaseClass.Name = "TextBoxInBaseClass";
			this.TextBoxInBaseClass.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20);
			this.TextBoxInBaseClass.TabIndex = 0;
			this.TabPageInBaseClass.PerformLayout();
			this.TabPageInBaseClass.ResumeLayout(true);
		}

		void AdditionalCodeMemberThatShouldntBeDeleted()
		{
		}

		private CargoWise.Windows.UI.KGroupBox groupBox1;

		private CargoWise.Windows.UI.KTabControl TabControlInBaseClass;

		internal MockTabPage TabPageInBaseClass;

		private CargoWise.Windows.UI.KTextBox TextBoxInBaseClass;
	}
#endif
}
