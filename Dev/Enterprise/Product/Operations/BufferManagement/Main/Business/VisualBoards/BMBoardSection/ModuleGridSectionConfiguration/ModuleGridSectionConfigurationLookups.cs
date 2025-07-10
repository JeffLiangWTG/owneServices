using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionConfigurationLookups : ZLookups
	{
		public ModuleGridSectionConfigurationLookups(ModuleGridSectionConfiguration parent)
			: base(parent)
		{
		}

		new ModuleGridSectionConfiguration Parent
		{
			get { return (ModuleGridSectionConfiguration)base.Parent; }
		}

		public CodeDescriptionPairList AllPanels
		{
			get
			{
				var list = new CodeDescriptionPairList();
				foreach (var config in Parent.PanelConfigurations.OrderBy(c => c.Sequence))
				{
					list.AddPair(config.Sequence.ToString(), config.PanelName);
				}
				return list;
			}
		}
	}
}
