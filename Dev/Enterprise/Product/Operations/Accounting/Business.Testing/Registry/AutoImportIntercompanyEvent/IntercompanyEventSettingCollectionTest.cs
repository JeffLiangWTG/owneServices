using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyEventSettingCollection))]
	public class IntercompanyEventSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IntercompanyEventSettingCollection>
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override IntercompanyEventSettingCollection GetCollectionToTest()
		{
			return new IntercompanyEventSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IntercompanyEventSetting();
		}
	}
}
