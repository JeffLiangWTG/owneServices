using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PricingPageChargeCodeCollection))]
	sealed class PricingPageChargeCodeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<PricingPageChargeCodeCollection>
	{
		public void TestCustomClone()
		{
			var fallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);

			var chargeCodeForPricingPageSectionsConfiguration = new ChargeCodeForPricingPageSectionsConfiguration();

			var element1 = Collection.AddNew();
			var element2 = Collection.AddNew();

			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			element1.ChargeCodePK = guid1;
			element2.ChargeCodePK = guid2;

			var clone = Collection.Clone(chargeCodeForPricingPageSectionsConfiguration, fallbackLevel, Factory);

			AssertEquals("Clone.CurrentFactory", Factory, CurrentFactoryPropertyInfo(typeof(PricingPageChargeCodeCollection)).GetValue(clone, null));
			AssertEquals("Clone.CurrentFallbackLevel", fallbackLevel, clone.CurrentFallbackLevel);
			AssertEquals("Clone.ParentConfiguration", chargeCodeForPricingPageSectionsConfiguration, clone.ParentConfiguration);

			AssertEquals("Clone.Count", 2, clone.Count);
			AssertEquals("Element1 should be in the Clone.", guid1, clone[0].ChargeCodePK);
			AssertEquals("Element2 should be in the Clone.", guid2, clone[1].ChargeCodePK);
		}

		#region Implementation

		protected override PricingPageChargeCodeCollection GetCollectionToTest()
		{
			return new PricingPageChargeCodeCollection(null, null, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new PricingPageChargeCodeGroup(null, Factory, null);
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
