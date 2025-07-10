using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageMinimumFeeCollection))]
	public class UsageMinimumFeeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UsageMinimumFeeCollection>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override UsageMinimumFeeCollection GetCollectionToTest() => new UsageMinimumFeeCollection(NewFallbackLevel(), Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new UsageMinimumFee();
	}
}
