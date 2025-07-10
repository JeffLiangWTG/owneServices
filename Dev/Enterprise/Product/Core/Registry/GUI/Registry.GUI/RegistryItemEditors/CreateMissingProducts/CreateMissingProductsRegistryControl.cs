using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	[SuppressBindingMemberBashingTestAttribute]
	public partial class CreateMissingProductsRegistryControl : RegistryBusinessObjectTemplateZUserControl
	{
		public CreateMissingProductsRegistryControl()
		{
			InitializeComponent();

			this.yesRadioButton.CheckedChanged += YesRadioButton_CheckedChanged;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			this.Enabled = !readOnly;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is CreateMissingProductsInfo info)
			{
				yesRadioButton.Checked = info.IsOverrideToYes;
				noRadioButton.Checked = !info.IsOverrideToYes;
				defaultRelationshipDropEdit.Visible = info.IsOverrideToYes;
				defaultRelationshipLabel.Visible = info.IsOverrideToYes;
			}
		}

		void YesRadioButton_CheckedChanged(object sender, System.EventArgs e)
		{
			defaultRelationshipDropEdit.Visible = yesRadioButton.Checked;
			defaultRelationshipLabel.Visible = yesRadioButton.Checked;
		}
	}
}
