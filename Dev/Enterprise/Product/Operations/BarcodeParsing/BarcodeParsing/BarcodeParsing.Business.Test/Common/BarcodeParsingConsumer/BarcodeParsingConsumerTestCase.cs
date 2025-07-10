using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestsSubclassesOf(typeof(IBarcodeParsingConsumer))]
	public abstract class BarcodeParsingConsumerTestCase : BarcodeParsingTestCase
	{
		#region Captions

		#region TestBuyerCaption

		public void TestBuyerCaption()
		{
			AssertEquals(ExpectedBuyerCaption, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).BuyerCaption);
		}

		protected abstract string ExpectedBuyerCaption { get; }

		#endregion

		#region TestRelatedEntityCaption

		public void TestRelatedEntityCaption()
		{
			AssertEquals(ExpectedRelatedEntityCaption, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).RelatedEntityCaption);
		}

		protected abstract string ExpectedRelatedEntityCaption { get; }

		#endregion

		#region TestSupplierCaption

		public void TestSupplierCaption()
		{
			AssertEquals(ExpectedSupplierCaption, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).SupplierCaption);
		}

		protected abstract string ExpectedSupplierCaption { get; }

		#endregion

		#endregion

		#region Collections

		#region TestBuyers

		public void TestBuyers()
		{
			AssertEquals(ExpectedTypeOfBuyers, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).Buyers.GetType());
		}

		protected abstract Type ExpectedTypeOfBuyers { get; }

		#endregion

		#region TestSuppliers

		public void TestSuppliers()
		{
			AssertEquals(ExpectedTypeOfSuppliers, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).Suppliers.GetType());
		}

		protected abstract Type ExpectedTypeOfSuppliers { get; }

		#endregion

		#endregion

		#region Flags

		#region TestIsBuyerAvailable

		public void TestIsBuyerAvailable()
		{
			AssertEquals(ExpectedIsBuyerAvailable, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).IsBuyerAvailable);
		}

		protected abstract bool ExpectedIsBuyerAvailable { get; }

		#endregion

		#region TestIsRelatedEntityAvailable

		public void TestIsRelatedEntityAvailable()
		{
			AssertEquals(ExpectedIsRelatedEntityAvailable, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).IsRelatedEntityAvailable);
		}

		protected abstract bool ExpectedIsRelatedEntityAvailable { get; }

		#endregion

		#region TestIsSupplierAvailable

		public void TestIsSupplierAvailable()
		{
			AssertEquals(ExpectedIsSupplierAvailable, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).IsSupplierAvailable);
		}

		protected abstract bool ExpectedIsSupplierAvailable { get; }

		#endregion

		#endregion

		#region TestGS1TargetFieldsToDefault

		public void TestGS1TargetFieldsToDefault()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedGS1TargetFieldsToDefault, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).GS1TargetFieldsToDefault);
		}

		protected abstract ZString[] ExpectedGS1TargetFieldsToDefault { get; }

		#endregion

		#region TestRelatedEntityRequirements

		public void TestRelatedEntityRequirements()
		{
			AssertEquals(ExpectedRelatedEntityRequirements, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).RelatedEntityRequirements);
		}

		protected abstract RelatedEntityRequirements ExpectedRelatedEntityRequirements { get; }

		#endregion

		#region TestTargetFields

		public void TestTargetFields()
		{
			AssertContainsExactElementsInAnyOrder(ExpectedTargetFields, Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode).TargetFields);
		}

		protected abstract CodeDescriptionPairList ExpectedTargetFields { get; }

		#endregion

		#region TestTargetFieldsMatchesEnumInBarcodeParser

		public void TestTargetFieldsMatchesEnumInBarcodeParser()
		{
			if (TestedTypeHelper.GetTestedType(GetType()) != typeof(DefaultBarcodeParsingConsumer))
			{
				var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
				var consumerGenericType = consumer.GetType().BaseType;
				var enumType = consumerGenericType.GetGenericArguments()[0];

				AssertContainsExactElementsInAnyOrder("Target Fields list to choose should match the Enum that the Barcode Parser uses.",
					ExpectedTargetFieldsForEnum.Cast<CodeDescriptionPair>().Select(c => c.Code),
					Enum.GetValues(enumType).Cast<object>().Select(e => e.ToString()));
			}
			else
			{
				// test does not apply because DefaultBarcodeParsingConsumer has no target fields yet the .NET enum type requires at least one field to be defined
				Assert(true);
			}
		}

		protected virtual CodeDescriptionPairList ExpectedTargetFieldsForEnum => ExpectedTargetFields;

		#endregion

		//

		#region TestConsumerIsSpringLoadable

		public void TestConsumerIsSpringLoadable()
		{
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
			AssertEquals("Module code returned by springed Consumer object should be the same as the code that sprang it.", ModuleCode, consumer.ModuleCode);
			AssertEquals("Factory passed into the Consumer Object should be returned by the Consumer object.", Factory, consumer.Factory);
		}

		#endregion

		#region TestGenericParameterIsAnEnum

		public void TestGenericParameterIsAnEnum()
		{
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
			var consumerGenericType = consumer.GetType().BaseType;
			Assert("Generic arguments should be an enumeration", consumerGenericType.GetGenericArguments()[0].IsEnum);
		}

		#endregion

		#region TestIfRelatedEntityIsAvailableThenRelatedEntityListShouldReturnCollection

		public void TestIfRelatedEntityIsAvailableThenRelatedEntityListShouldReturnCollection()
		{
			var buyer = Helper.CreateOrg("Buyer");
			var supplier = Helper.CreateOrg("Supplier");
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
			if (consumer.IsRelatedEntityAvailable)
			{
				AssertNotNull("RelatedEntityList should not be null if consumer supports Related Entity.", consumer.GetRelatedEntityList(buyer, supplier));
			}
			else
			{
				AssertNull("RelatedEntityList should be null if consumer does not support Related Entity.", consumer.GetRelatedEntityList(buyer, supplier));
			}
		}

		#endregion

		#region TestModuleCodeShouldNotBeEmpty

		public void TestModuleCodeShouldNotBeEmpty()
		{
			if (TestedTypeHelper.GetTestedType(GetType()) != typeof(DefaultBarcodeParsingConsumer))
			{
				AssertEquals("Module code Should not be empty.", false, ModuleCode.IsEmpty);
			}
			else
			{
				// test does not apply
				Assert(true);
			}
		}

		#endregion

		#region TestPropertiesDoNotThrowExceptionOrReturnNull

		public void TestPropertiesDoNotThrowExceptionOrReturnNull()
		{
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);

			object poke;
			AssertNoExceptionThrown("Buyer Caption property threw Exception.", () => poke = consumer.BuyerCaption);
			AssertNoExceptionThrown("Buyers property threw Exception.", () => poke = consumer.Buyers);
			AssertNoExceptionThrown("GS1TargetFieldsToDefault property threw Exception.", () => poke = consumer.GS1TargetFieldsToDefault);
			AssertNoExceptionThrown("Fallback property threw Exception.", () => poke = consumer.RelatedEntityRequirements);
			AssertNoExceptionThrown("IsRelatedEntityAvailable property threw Exception.", () => poke = consumer.IsRelatedEntityAvailable);
			AssertNoExceptionThrown("RelatedEntityCaption property threw Exception.", () => poke = consumer.RelatedEntityCaption);
			AssertNoExceptionThrown("SupplierCaption property threw Exception.", () => poke = consumer.SupplierCaption);
			AssertNoExceptionThrown("Suppliers property threw Exception.", () => poke = consumer.Suppliers);
			AssertNoExceptionThrown("TargetFields property threw Exception.", () => poke = consumer.TargetFields);

			//AssertNotNull("Buyers", consumer.Buyers);	temporary removed until buyers is removed from LocationBarcodeParsingConsumer
			AssertNotNull("GS1TargetFieldsToDefault", consumer.GS1TargetFieldsToDefault);
			//AssertNotNull("Suppliers", consumer.Suppliers);	temporary removed until suppiler is removed from LocationBarcodeParsingConsumer
			AssertNotNull("TargetFields", consumer.TargetFields);

			if (TestedTypeHelper.GetTestedType(GetType()) != typeof(DefaultBarcodeParsingConsumer))
			{
				AssertNotEquals("Should have some Target Fields otherwise Barcode Parsing cannot be used.", 0, consumer.TargetFields.Count);
			}
		}

		#endregion

		#region TestRelatedEntityListShouldReturnNullIfBuyerIsRequiredAndBuyerIsNull

		public void TestRelatedEntityListShouldReturnNullIfBuyerIsRequiredAndBuyerIsNull()
		{
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
			if (consumer.IsBuyerRequiredForRelatedEntity())
			{
				AssertNull("Related Entity List should return null if no buyer supplied, but buyer is required for related entity.",
					consumer.GetRelatedEntityList(null, Helper.CreateOrg("Supplier")));
			}
			else
			{
				// test does not apply
				Assert(true);
			}
		}

		#endregion

		#region TestRelatedEntityRequirementCannotBeRequiresBuyerIfRelatedEntityIsNotAvailable

		public void TestRelatedEntityRequirementCannotBeRequiresBuyerIfRelatedEntityIsNotAvailable()
		{
			var consumer = Factory.GetBarcodeParsingConsumerFromModuleCode(ModuleCode);
			if (!consumer.IsRelatedEntityAvailable)
			{
				AssertEquals("Cannot have a requirement for Related Entity if Related Entity is not supported by the module.",
					RelatedEntityRequirements.None, consumer.RelatedEntityRequirements);
			}
			else
			{
				// test does not apply
				Assert(true);
			}
		}

		#endregion

		#region Implementation

		protected abstract ZString ModuleCode { get; }

		#endregion
	}
}
