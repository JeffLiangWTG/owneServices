using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeForPricingPageSectionsRegistryDataType))]
	sealed class ChargeCodeForPricingPageSectionsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<ChargeCodeForPricingPageSectionsRegistryDataType>
	{
		protected override ChargeCodeForPricingPageSectionsRegistryDataType GetNewDataType()
		{
			return new ChargeCodeForPricingPageSectionsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "ChargeCodeForPricingPageSectionsRegistryItemEditor"; }
		}

		protected override void AssertValuesEqual(string message, NonPersistentBusinessObject lhs, NonPersistentBusinessObject rhs)
		{
			base.AssertValuesEqual(message, lhs, rhs);

			var chargeCodeForPricingPageSectionsConfiguration1 = (ChargeCodeForPricingPageSectionsConfiguration)lhs;
			var chargeCodeForPricingPageSectionsConfiguration2 = (ChargeCodeForPricingPageSectionsConfiguration)rhs;

			AssertEquals("Charges.Count", chargeCodeForPricingPageSectionsConfiguration1.Charges.Count, chargeCodeForPricingPageSectionsConfiguration2.Charges.Count);

			for (int i = 0; i < chargeCodeForPricingPageSectionsConfiguration1.Charges.Count; ++i)
			{
				foreach (ZPropertyInfo propertyInfo in chargeCodeForPricingPageSectionsConfiguration1.Charges[i].ZPropertyInfoHash)
				{
					AssertEquals(chargeCodeForPricingPageSectionsConfiguration1.Charges[i][propertyInfo.Name], chargeCodeForPricingPageSectionsConfiguration2.Charges[i][propertyInfo.Name]);
				}
			}
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new ChargeCodeForPricingPageSectionsConfigurationCollection();

			var chargeCodeForPricingPageSectionsConfiguration1 = collection.AddNew();
			chargeCodeForPricingPageSectionsConfiguration1.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			chargeCodeForPricingPageSectionsConfiguration1.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.OriginPickupCharges;
			var charge1 = chargeCodeForPricingPageSectionsConfiguration1.Charges.AddNew();
			charge1.ChargeCodePK = new ZGuid(new Guid("2ce31dd2-0e0f-49b4-8b4b-02068fc82c81"));
			var charge2 = chargeCodeForPricingPageSectionsConfiguration1.Charges.AddNew();
			charge2.ChargeCodePK = new ZGuid(new Guid("45df7203-1582-4714-9911-f64ef1c7beda"));

			var chargeCodeForPricingPageSectionsConfiguration2 = collection.AddNew();
			chargeCodeForPricingPageSectionsConfiguration2.PricingPage = ChargeCodeForPricingPageSectionsHelper.PricingPage.Code.ForwardingConcise;
			chargeCodeForPricingPageSectionsConfiguration2.Section = ChargeCodeForPricingPageSectionsHelper.Section.Code.DestinationDeliveryCharges;
			var charge3 = chargeCodeForPricingPageSectionsConfiguration2.Charges.AddNew();
			charge3.ChargeCodePK = new ZGuid(new Guid("1b72d306-3089-4eb5-b1d7-8570ef33c0cf"));
			var charge4 = chargeCodeForPricingPageSectionsConfiguration2.Charges.AddNew();
			charge4.ChargeCodePK = new ZGuid(new Guid("cbebff81-de09-4421-bc51-2bca3cf9d861"));

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, GetNewDataType().Serialise(collection)),
				new ValidSampleAndBinaryValueInDB(new ChargeCodeForPricingPageSectionsConfigurationCollection(), System.Array.Empty<byte>())
			};
		}
	}
}
