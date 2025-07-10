using System;
using System.Globalization;
using System.IO;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageMinimumFeeSettings))]
	public class UsageMinimumFeeSettingsTest : RegistryBusinessObjectTemplateTestCase<UsageMinimumFeeSettings>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override UsageMinimumFeeSettings GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override UsageMinimumFeeSettings GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		UsageMinimumFeeSettings NewPopulatedBusinessObject() => new UsageMinimumFeeSettings(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(AutoUsageMinimumFeeSettings.Schema));
		}

		public void TestValidation()
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

			var collection = new UsageMinimumFeeCollection();
			var usageMinimumFee1 = collection.AddNew();
			usageMinimumFee1.RunPreSaveValidation();
			AssertHasError(usageMinimumFee1.ProductCodeInfo, "Please enter a Product Code.");
			AssertHasError(usageMinimumFee1.PriceListCodeInfo, "Please enter a Price List Code.");
			AssertHasError(usageMinimumFee1.MinimumFeeCodeInfo, "Please enter a Minimum Fee Code.");

			var usageMinimumFee2 = collection.AddNew();
			usageMinimumFee2.ProductCode = "ABC";
			usageMinimumFee2.PriceListCode = "AXX";
			usageMinimumFee2.MinimumFeeCode = "MF1";
			usageMinimumFee1.RunPreSaveValidation();
			AssertNoErrors(usageMinimumFee2.ProductCodeInfo);
			AssertHasError(usageMinimumFee2.PriceListCodeInfo, "Enter a valid Price List Code.");
			AssertNoErrors(usageMinimumFee2.MinimumFeeCodeInfo);

			var usageMinimumFee3 = collection.AddNew();
			usageMinimumFee3.ProductCode = "DEF";
			usageMinimumFee3.PriceListCode = "ART";
			usageMinimumFee3.MinimumFeeCode = "MF2";
			usageMinimumFee3.RunPreSaveValidation();
			AssertNoErrors(usageMinimumFee3.ProductCodeInfo);
			AssertNoErrors(usageMinimumFee3.PriceListCodeInfo);
			AssertNoErrors(usageMinimumFee3.MinimumFeeCodeInfo);
		}

		public void TestSerialization()
		{
			var settings = NewPopulatedBusinessObject();
			var minimumFee = settings.MinimumFeeList.AddNew();
			minimumFee.ProductCode = "ABC";
			minimumFee.PriceListCode = "AXT";
			minimumFee.MinimumFeeCode = "MF1";

			var serializer = ZXmlSerializer.New(typeof(UsageMinimumFeeSettings));
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, settings);
				var xml = writer.ToString();

				using (var reader = new StringReader(xml))
				{
					var clone = (UsageMinimumFeeSettings)serializer.Deserialize(reader);
					AssertEquals(1, clone.MinimumFeeList.Count);
					AssertEquals("ABC", clone.MinimumFeeList[0].ProductCode);
					AssertEquals("AXT", clone.MinimumFeeList[0].PriceListCode);
					AssertEquals("MF1", clone.MinimumFeeList[0].MinimumFeeCode);
				}
			}
		}
	}
}
