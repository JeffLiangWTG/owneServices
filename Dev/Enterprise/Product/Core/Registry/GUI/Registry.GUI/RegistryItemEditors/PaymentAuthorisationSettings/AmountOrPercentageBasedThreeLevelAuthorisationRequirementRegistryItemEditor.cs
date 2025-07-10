using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AmountOrPercentageBasedThreeLevelAuthorisationRequirementRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new AmountOrPercentageAuthorisationSettingsControl();
		}

		protected override EditorPaneAnchor Anchor
		{
			get { return EditorPaneAnchor.All; }
		}
	}
}
