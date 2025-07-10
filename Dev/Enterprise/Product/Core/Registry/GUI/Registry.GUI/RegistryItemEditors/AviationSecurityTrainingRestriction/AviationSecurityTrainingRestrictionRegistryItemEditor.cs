using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.GUI
{
	public class AviationSecurityTrainingRestrictionRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public AviationSecurityTrainingRestrictionRegistryItemEditor(AviationSecurityTrainingRestrictionDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(dataType, fallbackLevel, factory)
		{
		}
		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new AviationSecurityTrainingRestrictionControl();
		}
	}
}
