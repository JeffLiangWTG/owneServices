using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MarketingManager.GUI.Testing
{
	sealed class LicenceUsageFilterValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateProperty1()
		{
			var filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			var validation = new LicenceUsageFilterValidation(filter);
			filter.Property1 = ZDateTime.Empty;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, true);
			filter.Property1 = new ZDateTime(2014, 1, 1);
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);
			filter.Property1 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);
			filter.Property1 = ZDateTime.Empty;
			filter.Property2 = new ZDateTime(2014, 1, 1);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			validation.ValidateProperty1();
			AssertMandatoryValidationError(filter.Property1Info, false);
		}

		public void TestValidateProperty2()
		{
			var filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			var validation = new LicenceUsageFilterValidation(filter);
			filter.Property2 = ZDateTime.Empty;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, true);
			filter.Property2 = new ZDateTime(2014, 1, 1);
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.DateRangeSearchTexts.Yesterday;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);
			filter.Property1 = new ZDateTime(2014, 1, 1);
			filter.Property2 = ZDateTime.Empty;
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			validation.ValidateProperty2();
			AssertMandatoryValidationError(filter.Property2Info, false);
		}

		public void TestValidatePriceHeaderCode()
		{
			var filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			var validation = new LicenceUsageFilterValidation(filter);
			filter.PriceHeaderCode = "";
			validation.ValidatePriceHeaderCode();
			AssertMandatoryValidationError(filter.PriceHeaderCodeInfo, true);
			AssertListValidationInvalidCodeError(filter.PriceHeaderCodeInfo, false);
			filter.PriceHeaderCode = "COR";
			validation.ValidatePriceHeaderCode();
			AssertListValidationInvalidCodeError(filter.PriceHeaderCodeInfo, true);
			filter.PriceHeaderCode = "ODM";
			validation.ValidatePriceHeaderCode();
			AssertListValidationInvalidCodeError(filter.PriceHeaderCodeInfo, false);
			filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: false);
			validation = new LicenceUsageFilterValidation(filter);
			filter.PriceHeaderCode = "";
			validation.ValidatePriceHeaderCode();
			AssertMandatoryValidationError(filter.PriceHeaderCodeInfo, false);
		}

		public void TestValidateUsageCount()
		{
			var filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			var validation = new LicenceUsageFilterValidation(filter);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			filter.UsageCount = -1;
			validation.ValidateUsageCount();
			AssertHasErrors(filter.UsageCountInfo);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			filter.UsageCount = 0;
			validation.ValidateUsageCount();
			AssertNoErrors(filter.UsageCountInfo);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			filter.UsageCount = 3;
			validation.ValidateUsageCount();
			AssertNoErrors(filter.UsageCountInfo);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.LessThanOrEqualTo;
			filter.UsageCount = 0;
			validation.ValidateUsageCount();
			AssertHasErrors(filter.UsageCountInfo);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.GreaterThanOrEqualTo;
			filter.UsageCount = 0;
			validation.ValidateUsageCount();
			AssertHasErrors(filter.UsageCountInfo);
		}

		public void TestValidateUsageCountComparisonOperator()
		{
			var filter = new LicenceUsageFilter("Dummy", ViewCampaignContactSchema.Constants.VCC_OH, isBilledFilter: true);
			var validation = new LicenceUsageFilterValidation(filter);
			filter.UsageCountComparisonOperator = "";
			validation.ValidateUsageCountComparisonOperator();
			AssertHasErrors(filter.UsageCountComparisonOperatorInfo);
			filter.UsageCountComparisonOperator = "AAA";
			validation.ValidateUsageCount();
			AssertHasErrors(filter.UsageCountComparisonOperatorInfo);
			filter.UsageCountComparisonOperator = LicenceUsageFilter.CountComparisonConstants.Exact;
			validation.ValidateUsageCount();
			AssertNoErrors(filter.UsageCountComparisonOperatorInfo);
		}
	}
}
