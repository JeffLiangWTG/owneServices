using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ConsolidatedBillingSettingCollection))]
	public class ConsolidatedBillingSettingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ConsolidatedBillingSettingCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override ConsolidatedBillingSettingCollection GetCollectionToTest() => new ConsolidatedBillingSettingCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new ConsolidatedBillingSetting(NewFallbackLevel(), Factory);
	}
}
