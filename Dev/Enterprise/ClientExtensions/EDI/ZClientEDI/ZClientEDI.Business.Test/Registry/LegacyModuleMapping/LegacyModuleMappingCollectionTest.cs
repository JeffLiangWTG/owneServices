using CargoWise.EntityFramework;
using Enterprise.CustomerService.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(LegacyModuleMappingCollection))]
	internal sealed class LegacyModuleMappingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<LegacyModuleMappingCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override LegacyModuleMappingCollection GetCollectionToTest()
		{
			return new LegacyModuleMappingCollection(ModuleListType.MenuSection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new LegacyModuleMapping(ModuleListType.MenuSection);
		}

		#endregion
	}
}
