using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsConfigurationCollection))]
	sealed class ChargeCodeForPricingPageSectionsConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeCodeForPricingPageSectionsConfigurationCollection>
	{
		public void TestIndexerAndAddNew()
		{
			var chargeCodeForPricingPageSectionsConfiguration1 = Collection.AddNew();
			var chargeCodeForPricingPageSectionsConfiguration2 = Collection.AddNew();

			AssertEquals("Collection[0]", chargeCodeForPricingPageSectionsConfiguration1, Collection[0]);
			AssertEquals("Collection[1]", chargeCodeForPricingPageSectionsConfiguration2, Collection[1]);
		}

		#region Implementation

		protected override ChargeCodeForPricingPageSectionsConfigurationCollection GetCollectionToTest()
		{
			return new ChargeCodeForPricingPageSectionsConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeCodeForPricingPageSectionsConfiguration(null, Factory);
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
