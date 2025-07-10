using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeGroupAndChargeCodeCollection))]
	sealed class ChargeGroupAndChargeCodeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeGroupAndChargeCodeCollection>
	{
		public void TestCustomClone()
		{
			FallbackLevel fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			LandedCostingGroup landedCostingGroup = new LandedCostingGroup();

			ChargeGroupAndChargeCode element1 = Collection.AddNew();
			ChargeGroupAndChargeCode element2 = Collection.AddNew();

			element1.ChargeGroupCode = "ABC";
			element2.ChargeGroupCode = "XYZ";

			ChargeGroupAndChargeCodeCollection clone = Collection.Clone(landedCostingGroup, fallbackLevel, Factory);

			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(ChargeGroupAndChargeCodeCollection)).GetValue(clone, null));
			AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("Clone.ParentLandedCostingGroup", landedCostingGroup, clone.ParentLandedCostingGroup);

			AssertEquals("Clone.Count", 2, clone.Count);
			AssertEquals("Element1 should be in the Clone.", "ABC", clone[0].ChargeGroupCode);
			AssertEquals("Element2 should be in the Clone.", "XYZ", clone[1].ChargeGroupCode);
		}

		#region Implementation

		protected override ChargeGroupAndChargeCodeCollection GetCollectionToTest()
		{
			return new ChargeGroupAndChargeCodeCollection(null, null, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeGroupAndChargeCode(null, Factory);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
