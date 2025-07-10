using System.Linq;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ModuleGuidModuleSpecifiedFilter))]
	public class ModuleGuidModuleSpecifiedFilterWithCodeDescriptionModuleFilterPairTest : ModuleGuidModuleSpecifiedFilterTest
	{
		protected override ModuleGuidModuleSpecifiedFilter GetNewModuleFilter()
		{
			var mainModules = ModuleIDs.AllIncludingClientModules.Select(x => new CodeDescriptionModuleFilterPair(x.Name, x.ExtendedDescription, x));
			var modulesIncludingDummy = mainModules.Concat(new[] { new CodeDescriptionModuleFilterPair("Dummy", "Dummy", DummyModuleIDs.Dummy) });

			return new ModuleGuidModuleSpecifiedFilter("moo", JobHeaderSchema.JH_ParentID, () => modulesIncludingDummy);
		}
	}
}
