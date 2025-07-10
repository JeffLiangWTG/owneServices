using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageMinimumFeeSettingsRegistryItem))]
	public class UsageMinimumFeeSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<UsageMinimumFeeSettings, UsageMinimumFeeSettings>
	{
		protected override StronglyTypedRegistryItem<UsageMinimumFeeSettings, UsageMinimumFeeSettings> GetNewRegistryItem()
		{
			return new UsageMinimumFeeSettingsRegistryItem("UsageMinimumFeeSettings",
							(NoResString)"LicenceBillingCategory",
							(NoResString)"Products (non-CW1) Enabled for Minimum Fee usage",
							(NoResString)"System Pricelist added to this registry will trigger a minimum fee usage to Non-CW1 production license DBs that do not have usage for the billing period. The Usage Code column must match the code of the System Pricelist in WISGLOSYD2.",
							RegistryStorageFlags.System);
		}

		protected override UsageMinimumFeeSettings ValidValue
		{
			get
			{
				var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
				mappings.AddNew("ABC", "ABC Product", true);
				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

				var categories = new CodeDescriptionPairList(EDIDataRegistry.Instance.BillingUsageCategoryCodes.Value);
				categories.AddPair("SMF", "SMF Category");
				EDIDataRegistry.Instance.BillingUsageCategoryCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, categories);

				var settings = new UsageBillingSettings();
				var priceLists = settings.PriceLists;
				var priceList1 = priceLists.AddNew();
				priceList1.ProductCode = "ABC";
				priceList1.RawUsageCategory = "SMF";
				priceList1.PriceListCode = "AXT";
				priceList1.Description = "ABC-SMF-AXT";
				EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);

				var minimumFeeSettings = new UsageMinimumFeeSettings();
				var minimumFeeUsage1 = minimumFeeSettings.MinimumFeeList.AddNew();
				minimumFeeUsage1.ProductCode = "ABC";
				minimumFeeUsage1.PriceListCode = "AXT";
				minimumFeeUsage1.MinimumFeeCode = "TTT";
				EDIDataRegistry.Instance.UsageMinimumFeeSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, minimumFeeSettings);

				return minimumFeeSettings;
			}
		}
	}
}
