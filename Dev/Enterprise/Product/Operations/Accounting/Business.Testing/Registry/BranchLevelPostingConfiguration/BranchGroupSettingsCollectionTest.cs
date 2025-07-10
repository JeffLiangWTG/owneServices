using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BranchGroupSettingsCollection))]
	public class BranchGroupSettingsCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<BranchGroupSettingsCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override BranchGroupSettingsCollection GetCollectionToTest()
		{
			return new BranchGroupSettingsCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new BranchGroupSettings();
		}
	}
}
