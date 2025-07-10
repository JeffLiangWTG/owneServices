using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class TemplateRegistryItemControl : RegistryZUserControl
	{
		public TemplateRegistryItemControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			CheckBox.ReadOnly = readOnly;
			Criterion1Box.ReadOnly = readOnly;
			Criterion2Box.ReadOnly = readOnly;
			Criterion3Box.ReadOnly = readOnly;
			Criterion4Box.ReadOnly = readOnly;
			Criterion5Box.ReadOnly = readOnly;
		}

		ZGroupBox GroupBox;
		ZCheckBox CheckBox;
		ZLabel Criterion1Label;
		ZDropEdit Criterion1Box;
		ZLabel Criterion2Label;
		ZDropEdit Criterion2Box;
		ZLabel Criterion3Label;
		ZDropEdit Criterion3Box;
		ZLabel Criterion4Label;
		ZDropEdit Criterion4Box;
		ZLabel Criterion5Label;
		ZDropEdit Criterion5Box;
	}
}
