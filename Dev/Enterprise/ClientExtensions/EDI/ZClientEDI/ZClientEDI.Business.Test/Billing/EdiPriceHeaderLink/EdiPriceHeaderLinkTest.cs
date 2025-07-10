using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiPriceHeaderLink))]
	internal class EdiPriceHeaderLinkTest : EnterpriseBusinessObjectTestCase
	{
		public void TestPHL_VolumeCode()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_VolumeCode = "AA";
			AssertEquals(100m, link.PHL_VolumePercent);
			link.PHL_VolumeCode = "HV";
			AssertEquals(50m, link.PHL_VolumePercent);
			link.PHL_VolumeCode = "LV";
			AssertEquals(150m, link.PHL_VolumePercent);
			link.PHL_VolumeCode = "STD";
			AssertEquals(100m, link.PHL_VolumePercent);
		}

		public void TestPHL_CorePackCode()
		{
			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_CorePackCode = "AA";
			AssertEquals(0m, link.PHL_CoreUpliftPercent);
			link.PHL_CorePackCode = "EX";
			AssertEquals(0m, link.PHL_CoreUpliftPercent);
			link.PHL_CorePackCode = "INC";
			AssertEquals(0m, link.PHL_CoreUpliftPercent);
			link.PHL_CorePackCode = "UP";
			AssertEquals(50m, link.PHL_CoreUpliftPercent);
		}

		public void TestShouldIncludeDiscount()
		{
			var price = Factory.NewWithValidTestData<ClientLicencePriceItem>();
			price.L7_Code = "USR";

			var link = Factory.NewWithValidTestData<EdiPriceHeaderLink>();
			link.PHL_CorePackCode = "EX";

			var headerDiscount = Factory.New<EdiPriceHeaderDiscount>();
			headerDiscount.PHD_Type = "VOL";
			var info = new DiscountInfo();
			info.Init(headerDiscount, null, null);
			var discount = new VolumeStlDiscount(info);

			AssertEquals(false, link.ShouldIncludeDiscount(price, null, discount));

			link.PHL_CorePackCode = "INC";
			AssertEquals(true, link.ShouldIncludeDiscount(price, null, discount));

			link.PHL_CorePackCode = "UP";
			AssertEquals(true, link.ShouldIncludeDiscount(price, null, discount));

			link.PHL_CorePackCode = "EX";
			price.L7_Code = "C01";
			AssertEquals(true, link.ShouldIncludeDiscount(price, null, discount));

			link.PHL_CorePackCode = "EX";
			price.L7_Code = "USR";
			headerDiscount.PHD_Type = "SPE";
			AssertEquals(true, link.ShouldIncludeDiscount(price, null, discount));
		}
	}
}
