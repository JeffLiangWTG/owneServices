using System;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageMinimumFeeSettingsRegistryDataType))]
	public class UsageMinimumFeeSettingsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UsageMinimumFeeSettingsRegistryDataType>
	{
		protected override UsageMinimumFeeSettingsRegistryDataType GetNewDataType()
		{
			return new UsageMinimumFeeSettingsRegistryDataType();
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			mappings.AddNew("DEF", "DEF Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
			categories.AddPair("SMF", "SMF Category");
			categories.AddPair("BMF", "BMF Category");
			EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

			var settings = new UsageBillingSettings();
			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SMF";
			priceList1.PriceListCode = "AXT";
			priceList1.Description = "ABC-SMF-AXT";

			var priceList2 = priceLists.AddNew();
			priceList2.ProductCode = "DEF";
			priceList2.RawUsageCategory = "BMF";
			priceList2.PriceListCode = "ART";
			priceList2.Description = "DEF-BMF-ART";
			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

			var settings1 = new UsageMinimumFeeSettings();
			var minimumFee1 = settings1.MinimumFeeList.AddNew();
			minimumFee1.ProductCode = "ABC";
			minimumFee1.PriceListCode = "AXT";
			minimumFee1.MinimumFeeCode = "MF1";

			var settings2 = new UsageMinimumFeeSettings();
			var minimumFee2 = settings2.MinimumFeeList.AddNew();
			minimumFee2.ProductCode = "DEF";
			minimumFee2.PriceListCode = "ART";
			minimumFee2.MinimumFeeCode = "MF2";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(settings1, DataType.Serialise(settings1)),
				new ValidSampleAndBinaryValueInDB(settings2, DataType.Serialise(settings2)),
			};
		}

		protected override string ExpectedEditorName => "UsageMinimumFeeSettingsRegistryEditor";

		protected override bool HasEditor => true;
	}
}
