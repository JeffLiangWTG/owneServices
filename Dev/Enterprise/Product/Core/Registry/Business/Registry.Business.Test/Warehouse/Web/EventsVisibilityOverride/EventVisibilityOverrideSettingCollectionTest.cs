using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(EventVisibilityOverrideSettingCollection))]
	sealed class EventVisibilityOverrideSettingCollectionTest : RegistryBusinessObjectCollectionTestCase<EventVisibilityOverrideSettingCollection>
	{
		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override EventVisibilityOverrideSettingCollection GetCollectionToTest()
		{
			return new EventVisibilityOverrideSettingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EventVisibilityOverrideSetting();
		}
	}
}
