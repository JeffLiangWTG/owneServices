using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.GUI
{
	public class JobBranchDefaultOrderRuleRegistryItemEditor : NonPersistentBusinessObjectBindingRegistryItemEditor
	{
		public JobBranchDefaultOrderRuleRegistryItemEditor(IRegistryDataType dataType, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(dataType, fallbackLevel, factory)
		{
		}

		protected override RegistryZUserControl NewBoundWinFormsEditorPane()
		{
			return new JobBranchDefaultOrderRuleControl();
		}
	}
}
