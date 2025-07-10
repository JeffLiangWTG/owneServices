using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(DiscountSuspensionPolicy))]
	public class DiscountSuspensionPolicyTest : RegistryBusinessObjectTemplateTestCase<DiscountSuspensionPolicy>
	{
		protected override DiscountSuspensionPolicy GetBusinessObjectToClone()
		{
			return new DiscountSuspensionPolicy();
		}

		protected override DiscountSuspensionPolicy GetBusinessObjectToSerialise()
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

		public void TestPolicyCode_Lookups()
		{
			var setting = new DiscountSuspensionPolicy();
			var info = setting.PolicyCodeInfo;
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(setting, info.PropertyDescriptor);
			AssertEquals(3, list.Count);
		}

		public void TestProductCode_Lookups()
		{
			var setting = new DiscountSuspensionPolicy();
			var info = setting.ProductCodeInfo;
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(setting, info.PropertyDescriptor);
			AssertEquals(4, list.Count);
		}
	}

	public class DiscountSuspensionPolicyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProductCode()
		{
			var collection = new DiscountSuspensionPolicyCollection();
			var item1 = collection.AddNew("ENT", "ALW");
			AssertNoErrors(item1.ProductCodeInfo);

			item1.ProductCode = "";
			AssertHasError(item1.ProductCodeInfo, "Please enter a Product.");

			item1.ProductCode = "#@#";
			AssertHasError(item1.ProductCodeInfo, "Enter a valid Product.");

			item1.ProductCode = "ENT";
			AssertNoErrors(item1.ProductCodeInfo);

			var item2 = collection.AddNew("ENT", "ALW");
			item2.ValidateProductCode();
			AssertPropertyIsUniqueInCollectionValidationError(item2.ProductCodeInfo, true);

			item2.ProductCode = "BOR";
			AssertPropertyIsUniqueInCollectionValidationError(item2.ProductCodeInfo, false);
		}

		public void TestValidatePolicyCode()
		{
			var collection = new DiscountSuspensionPolicyCollection();
			var item1 = collection.AddNew("ENT", "ALW");
			item1.ValidatePolicyCode();
			AssertNoErrors(item1.PolicyCodeInfo);

			item1.PolicyCode = "";
			AssertHasError(item1.PolicyCodeInfo, "Please enter a Policy.");

			item1.PolicyCode = "#@#";
			AssertHasError(item1.PolicyCodeInfo, "Enter a valid Policy.");

			item1.PolicyCode = "NVR";
			AssertNoErrors(item1.PolicyCodeInfo);
		}
	}
}
