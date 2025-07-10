using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.GUI;

namespace Enterprise.Client.EDI.Registry.GUI
{
	public partial class CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl : RegistryZUserControl
	{
		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl()
		{
			InitializeComponent();
		}

		public CodeDescriptionIncidentEmailTemplatePairCollectionRegistryControl(bool isNeedUpgradeColumnVisible) : this()
		{
			CategoryGrid.GetColumnStyle(nameof(CodeDescriptionIncidentEmailTemplatePair.NeedUpgrade)).IsUnavailable = !isNeedUpgradeColumnVisible;
		}

		protected override void SetControlOrBusinessEntityReadOnly(bool readOnly)
		{
			base.SetControlOrBusinessEntityReadOnly(readOnly);

			CategoryGrid.ReadOnly = readOnly;
			IncidentEmailTemplatePairRegistryControl.ReadOnly = readOnly;
		}
	}
}
