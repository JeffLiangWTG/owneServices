using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingSystemChargeCodeMapping))]
	public class BillingSystemChargeCodeMappingTest : RegistryBusinessObjectTemplateTestCase<BillingSystemChargeCodeMapping>
	{
		protected override BillingSystemChargeCodeMapping GetBusinessObjectToClone()
		{
			return new BillingSystemChargeCodeMapping();
		}

		protected override BillingSystemChargeCodeMapping GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		public void TestSystemCode_Lookups()
		{
			var mapping = new BillingSystemChargeCodeMapping();
			var info = mapping.SystemCodeInfo;
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(mapping, info.PropertyDescriptor);
			AssertEquals(0, list.Count);

			UsageBillingSettingsTest.SetupValidTestRegistry();

			var mapping2 = new BillingSystemChargeCodeMapping();
			mapping2.ProductCode = "ENT";
			var info2 = mapping2.SystemCodeInfo;
			var list2 = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(mapping2, info2.PropertyDescriptor);
			var expected = BillingConstants.GetBillingSystemList();
			expected.AddPair("PL0", "ABC - Price List #0");
			expected.AddPair("PL1", "ABC - Price List #1");
			AssertContainsExactElementsInAnyOrder(expected, list2);
		}

		public void TestProductCode_Lookups()
		{
			var mapping = new BillingSystemChargeCodeMapping();
			var info = mapping.ProductCodeInfo;
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(mapping, info.PropertyDescriptor);
			var expected = new ProductTypes(true);
			AssertEquals(expected.Count, list.Count);
		}
	}

	public class BillingSystemChargeCodeMappingValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProductCode()
		{
			var billingSystemChargeCode = new BillingSystemChargeCodeMapping();
			billingSystemChargeCode.ProductCode = "XXX";
			AssertMandatoryValidationError(billingSystemChargeCode.ProductCodeInfo, false);

			billingSystemChargeCode.ProductCode = "";
			AssertMandatoryValidationError(billingSystemChargeCode.ProductCodeInfo, true);
		}

		public void TestValidateSubModule()
		{
			var collection = new BillingSystemChargeCodeMappingCollection();
			var item1 = collection.AddNew("ENT", "ABM", "AAA");
			item1.ValidateSubModule();
			AssertPropertyIsUniqueInCollectionValidationError(item1.SubModuleInfo, false);

			var item2 = collection.AddNew("ENT", "ABM", "AAA");
			item2.ValidateSubModule();
			AssertPropertyIsUniqueInCollectionValidationError(item2.SubModuleInfo, true);

			item2.SubModule = "BBB";
			AssertPropertyIsUniqueInCollectionValidationError(item2.SubModuleInfo, false);

			var item3 = collection.AddNew("SPH", "ABM", "AAA");
			item3.ValidateSubModule();
			AssertPropertyIsUniqueInCollectionValidationError(item2.SubModuleInfo, false);

			var item4 = collection.AddNew("ENT", "HOS", "AAA");
			item4.ValidateSubModule();
			AssertPropertyIsUniqueInCollectionValidationError(item2.SubModuleInfo, false);
		}
	}
}
