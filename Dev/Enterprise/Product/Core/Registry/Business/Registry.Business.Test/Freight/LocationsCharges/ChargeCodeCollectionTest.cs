using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeCollection))]
	sealed class ChargeCodeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeCodeCollection>
	{
		public void TestCustomClone()
		{
			FallbackLevel fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			LocationsChargesGroup locationsChargesGroup = new LocationsChargesGroup();

			ChargeCodeGroup element1 = Collection.AddNew();
			ChargeCodeGroup element2 = Collection.AddNew();

			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			element1.ChargeCodePK = guid1;
			element2.ChargeCodePK = guid2;

			ChargeCodeCollection clone = Collection.Clone(locationsChargesGroup, fallbackLevel, Factory);

			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(ChargeCodeCollection)).GetValue(clone, null));
			AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("Clone.ParentLocationsChargesGroup", locationsChargesGroup, clone.ParentLocationsChargesGroup);

			AssertEquals("Clone.Count", 2, clone.Count);
			AssertEquals("Element1 should be in the Clone.", guid1, clone[0].ChargeCodePK);
			AssertEquals("Element2 should be in the Clone.", guid2, clone[1].ChargeCodePK);
		}

		#region Implementation

		protected override ChargeCodeCollection GetCollectionToTest()
		{
			return new ChargeCodeCollection(null, null, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeCodeGroup(null, Factory);
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
