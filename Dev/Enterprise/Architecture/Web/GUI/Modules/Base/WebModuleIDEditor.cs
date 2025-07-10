using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.ZArchitecture.Web.GUI.Internal
{
	public class WebModuleIDEditor : ModuleIDEditor
	{
		protected override IEnumerable<ModuleIdentifier> GetModuleIDs()
		{
			return WebModuleIDs.All;
		}

		protected internal IEnumerable<ModuleIdentifier> GetModuleIDsInternal() => GetModuleIDs();
	}
}
