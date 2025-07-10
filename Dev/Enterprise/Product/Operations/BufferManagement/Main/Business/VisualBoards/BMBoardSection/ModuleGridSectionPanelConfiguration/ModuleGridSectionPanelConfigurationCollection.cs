using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionPanelConfigurationCollection : NonPersistentBusinessObjectCollection<ModuleGridSectionPanelConfiguration>
	{
		readonly BMBoardSection section;

		public ModuleGridSectionPanelConfigurationCollection(BMBoardSection section)
		{
			this.section = section;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ModuleGridSectionPanelConfiguration(section);
		}
	}
}
