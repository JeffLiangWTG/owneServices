using System;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingSettingsRegistryItem))]
	class UsageBillingSettingsRegistryItemTest : StronglyTypedRegistryItemTestCase<UsageBillingSettings, UsageBillingSettings>
	{
		protected override StronglyTypedRegistryItem<UsageBillingSettings, UsageBillingSettings> GetNewRegistryItem()
		{
			return new UsageBillingSettingsRegistryItem("ProductsEnabledForUsageBilling",
							(NoResString)"LicenceBillingCategory",
							(NoResString)"Products (non-CW1) Enabled for Usage Billing",
							(NoResString)"The non CargoWiseOne products enabled for generic usage billing via STL billing engine.\r\nThis enables invoice delivery instructions, independent pricelist and usage calculation in STL billing.",
							RegistryStorageFlags.System);
		}

		protected override UsageBillingSettings ValidValue
		{
			get
			{
				var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
				mappings.AddNew("ABC", "ABC Product", true);
				EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

				var settings = new UsageBillingSettings();

				var priceLists = settings.PriceLists;
				var priceList1 = priceLists.AddNew();
				priceList1.ProductCode = "ABC";
				priceList1.RawUsageCategory = "SAT";
				priceList1.PriceListCode = "PL0";
				priceList1.Description = "ABC - Price List #0";

				var priceList2 = priceLists.AddNew();
				priceList2.ProductCode = "ABC";
				priceList2.RawUsageCategory = "3GT";
				priceList2.PriceListCode = "PL1";
				priceList2.Description = "ABC - Price List #1";

				var branchRestriction = settings.BranchRestrictions.AddNew();
				branchRestriction.ProductCode = "ABC";
				branchRestriction.InvoicingBranch = Env.CurrentBranchPK;

				return settings;
			}
		}
	}

	[TestedType(typeof(UsageBillingSettingsRegistryDataType))]
	class UsageBillingSettingsDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<UsageBillingSettingsRegistryDataType>
	{
		protected override UsageBillingSettingsRegistryDataType GetNewDataType()
		{
			return new UsageBillingSettingsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "UsageBillingSettingsRegistryEditor"; }
		}

		protected override bool HasEditor
		{
			get { return true; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var sample = GetTestSettings(1);
			var sample2 = GetTestSettings(5);

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(sample, DataType.Serialise(sample)),
				new ValidSampleAndBinaryValueInDB(sample2, DataType.Serialise(sample2)),
			};
		}

		UsageBillingSettings GetTestSettings(int idx)
		{
			var settings = new UsageBillingSettings();

			var priceLists = settings.PriceLists;
			var priceList1 = priceLists.AddNew();
			priceList1.ProductCode = "ABC";
			priceList1.RawUsageCategory = "SAT";
			priceList1.PriceListCode = $"E_{idx}";

			var priceList2 = priceLists.AddNew();
			priceList2.ProductCode = "ABC";
			priceList2.RawUsageCategory = "3GT";
			priceList2.PriceListCode = $"E_{idx + 1}";

			var branchRestriction = settings.BranchRestrictions.AddNew();
			branchRestriction.ProductCode = "ABC";
			branchRestriction.InvoicingBranch = Env.CurrentBranchPK;

			return settings;
		}
	}
}
