using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.GUI
{
	public partial class CodeDescriptionWithEnabledAndDefaultsRegistryControl : RegistryZUserControl
	{
		public CodeDescriptionWithEnabledAndDefaultsRegistryControl()
		{
			InitializeComponent();
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);
			Grid.ReadOnly = readOnly;
		}

		public void SetupCodeColumn(MultilingualString codeColumnCaption)
		{
			if (codeColumnCaption != null)
			{
				Grid.SetColumnCaption("Code", codeColumnCaption);
			}
		}

		public void SetupDescriptionColumn(MultilingualString descriptionColumnCaption)
		{
			if (descriptionColumnCaption != null)
			{
				Grid.SetColumnCaption("EnglishDescription", descriptionColumnCaption);
			}
		}
	}
}
