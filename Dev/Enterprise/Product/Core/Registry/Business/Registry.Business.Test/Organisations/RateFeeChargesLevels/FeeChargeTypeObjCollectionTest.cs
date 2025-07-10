using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(FeeChargeTypeCollection))]
	sealed class FeeChargeTypeObjCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FeeChargeTypeCollection>
	{
		public void TestAddFeeChargeTypeObject()
		{
			var collection = new FeeChargeTypeCollection();
			AssertEquals(0, collection.Count);

			collection.AddFeeChargeType(
				"ABC",
				(NoResString)"Test type description",
				new FeeChargeLevelCollection()
				{
					new FeeChargeLevel()
					{
						Code = "SIL", Amount1 = 30, Amount1Currency = "AUD", Amount1Type = "Min", Amount2 = 40, Amount2Currency = "CNY", Amount2Type = "Exc", Description = (NoResString)"test fee charge level"
					}
				});

			AssertEquals(1, collection.Count);
			AssertEquals("ABC", collection[0].Code);

			AssertEquals("Test type description", collection[0].Description);
			var levels = collection[0].FeeChargeLevels;

			AssertEquals("SIL", levels[0].Code);
			AssertEquals("test fee charge level", levels[0].Description);
			AssertEquals((ZDecimal)30, levels[0].Amount1);
			AssertEquals("AUD", levels[0].Amount1Currency);
			AssertEquals("Min", levels[0].Amount1Type);
			AssertEquals((ZDecimal)40, levels[0].Amount2);
			AssertEquals("CNY", levels[0].Amount2Currency);
			AssertEquals("Exc", levels[0].Amount2Type);
		}

		public void TestDuplicatedFeeChargeTypesObject()
		{
			var collection = new FeeChargeTypeCollection();
			AssertEquals(0, collection.Count);

			collection.AddFeeChargeType(
				"ABC",
				(NoResString)"Test type description",
				new FeeChargeLevelCollection()
				{
					new FeeChargeLevel()
					{
						Code = "SIL", Amount1 = 30, Amount1Currency = "AUD", Amount1Type = "Min", Amount2 = 40, Amount2Currency = "CNY", Amount2Type = "Exc", Description = (NoResString)"test fee charge level"
					}
				});

			collection.AddFeeChargeType(
				"ABC",
				(NoResString)"Test type description",
				new FeeChargeLevelCollection()
				{
					new FeeChargeLevel()
					{
						Code = "SIL", Amount1 = 30, Amount1Currency = "AUD", Amount1Type = "Min", Amount2 = 40, Amount2Currency = "CNY", Amount2Type = "Exc", Description = (NoResString)"test fee charge level"
					}
			});

			AssertEquals(2, collection.Count);
			AssertHasError(collection[1].CodeInfo, "The Code has been duplicated and must be unique.");
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FeeChargeTypeCollection GetCollectionToTest()
		{
			return new FeeChargeTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FeeChargeType();
		}
	}
}
