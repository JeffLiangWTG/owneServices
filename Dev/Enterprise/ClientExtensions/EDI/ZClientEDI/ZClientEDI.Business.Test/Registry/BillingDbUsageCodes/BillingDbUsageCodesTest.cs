using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingDbUsageCodes))]
	public class BillingDbUsageCodesTest : RegistryBusinessObjectTemplateTestCase<BillingDbUsageCodes>
	{
		protected override BillingDbUsageCodes GetBusinessObjectToClone()
		{
			return new BillingDbUsageCodes();
		}

		protected override BillingDbUsageCodes GetBusinessObjectToSerialise()
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

		public void TestPriceHeaderCode_Lookups()
		{
			var mapping = new BillingDbUsageCodes();
			var info = mapping.PriceHeaderCodeInfo;
			var list = (ReadOnlyCodeDescriptionPairList)MetaData.GetListDataSource(mapping, info.PropertyDescriptor);
			var expected = BillingConstants.PriceHeaderType.GetPriceHeaderTypeList();
			AssertEquals(expected.Count, list.Count);
		}
	}

	public class BillingDbUsageCodesValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateCategory()
		{
			var codes = new BillingDbUsageCodes();
			codes.Category = "XXX";
			AssertMandatoryValidationError(codes.CategoryInfo, false);

			codes.Category = "";
			AssertMandatoryValidationError(codes.CategoryInfo, true);
		}

		public void TestValidatePriceItemCode()
		{
			var codes = new BillingDbUsageCodes();
			codes.PriceItemCode = "XXX";
			AssertMandatoryValidationError(codes.PriceItemCodeInfo, false);

			codes.PriceItemCode = "";
			AssertMandatoryValidationError(codes.PriceItemCodeInfo, true);
		}

		public void TestValidatePriceHeaderCode()
		{
			var codes = new BillingDbUsageCodes();
			codes.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			AssertMandatoryValidationError(codes.PriceHeaderCodeInfo, false);
			AssertListValidationInvalidCodeError(codes.PriceHeaderCodeInfo, false);

			codes.PriceHeaderCode = "";
			AssertMandatoryValidationError(codes.PriceHeaderCodeInfo, true);

			codes.PriceHeaderCode = "ZZZ";
			AssertListValidationInvalidCodeError(codes.PriceHeaderCodeInfo, true);
		}
	}
}
