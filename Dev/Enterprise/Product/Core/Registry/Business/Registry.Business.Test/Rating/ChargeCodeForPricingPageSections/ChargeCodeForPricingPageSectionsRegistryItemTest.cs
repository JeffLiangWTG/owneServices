using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsRegistryItem))]
	sealed class ChargeCodeForPricingPageSectionsRegistryItemTest : StronglyTypedRegistryItemTestCase<ChargeCodeForPricingPageSectionsConfigurationCollection>
	{
		static readonly ZGuid[] OriginPickupChargeCodePKs = { ZGuid.NewZGuid(), ZGuid.NewZGuid() };
		static readonly ZGuid[] DestinationDeliveryChargeCodePKs = { ZGuid.NewZGuid(), ZGuid.NewZGuid() };

		ChargeCodeForPricingPageSectionsConfigurationCollection Collection;

		protected override StronglyTypedRegistryItem<ChargeCodeForPricingPageSectionsConfigurationCollection, ChargeCodeForPricingPageSectionsConfigurationCollection> GetNewRegistryItem()
			=> new ChargeCodeForPricingPageSectionsRegistryItem("", null, null, null, RegistryStorageFlags.System, new ChargeCodeForPricingPageSectionsConfigurationCollection());

		public void TestGetChargeCodeConfiguration()
		{
			AssertGetChargeCodeConfiguration(
				"Load charge codes when Pricing Page is ForwardingConcise and Section is OriginPickupCharges",
				ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise,
				ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges,
				OriginPickupChargeCodePKs);

			AssertGetChargeCodeConfiguration(
				"Load charge codes when Pricing Page is ForwardingConcise and Section is DestinationDeliveryCharges",
				ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise,
				ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges,
				DestinationDeliveryChargeCodePKs);
		}

		void AssertGetChargeCodeConfiguration(string caseMessage, string pricingPage, string section, IEnumerable<ZGuid> exceptedChargeCodes)
		{
			var registryItem = (ChargeCodeForPricingPageSectionsRegistryItem)GetNewRegistryItem();

			using (registryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Collection))
			{
				var actualChargeCodes = registryItem.GetChargeCodeConfiguration(
					pricingPage: pricingPage,
					section: section
				);

				var actualChargeCodePKs = actualChargeCodes.Charges
					.Cast<PricingPageChargeCodeGroup>()
					.Select(x => x.ChargeCodePK)
					.ToList();

				AssertContainsExactElementsInAnyOrder(caseMessage, exceptedChargeCodes, actualChargeCodePKs);
			}
		}

		ChargeCodeForPricingPageSectionsConfigurationCollection SetupChargeCodeForPricingPageSectionsConfigurationCollection()
		{
			var collection = new ChargeCodeForPricingPageSectionsConfigurationCollection(null, null);

			var configuration1 = collection.AddNew();
			configuration1.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			configuration1.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;

			foreach (var chargeCode in OriginPickupChargeCodePKs)
			{
				var charge = configuration1.Charges.AddNew();
				charge.ChargeCodePK = chargeCode;
			}

			var configuration2 = collection.AddNew();
			configuration2.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			configuration2.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;

			foreach (var chargeCode in DestinationDeliveryChargeCodePKs)
			{
				var charge = configuration2.Charges.AddNew();
				charge.ChargeCodePK = chargeCode;
			}

			return collection;
		}

		protected override void SetUp()
		{
			Collection = SetupChargeCodeForPricingPageSectionsConfigurationCollection();
		}
	}
}
