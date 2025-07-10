using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeLevelCollection))]
	sealed class FeeChargeLevelCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FeeChargeLevelCollection>
	{
		public void TestDuplicatedLevelEntered()
		{
			var collection = new FeeChargeLevelCollection();
			AssertEquals(0, collection.Count);

			var level1 = collection.AddNew();
			level1.Code = "SIL";

			var level2 = collection.AddNew();
			level2.Code = "SIL";

			AssertHasError(level2.CodeInfo, "The Code has been duplicated and must be unique.");
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FeeChargeLevelCollection GetCollectionToTest()
		{
			return new FeeChargeLevelCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FeeChargeLevel();
		}
	}
}
