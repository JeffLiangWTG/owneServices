using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(HBLDeliveryPrioritySettingCollection))]
	sealed class HBLDeliveryPrioritySettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<HBLDeliveryPrioritySettingCollection>
	{
		#region Implementation

		protected override HBLDeliveryPrioritySettingCollection GetCollectionToTest()
			=> new HBLDeliveryPrioritySettingCollection(null, Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> new HBLDeliveryPrioritySetting(null, Factory);

		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		#endregion
	}
}
