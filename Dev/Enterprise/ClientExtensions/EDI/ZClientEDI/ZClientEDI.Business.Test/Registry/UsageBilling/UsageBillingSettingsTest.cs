using System;
using System.Globalization;
using System.IO;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(UsageBillingSettings))]
	internal class UsageBillingSettingsTest : RegistryBusinessObjectTemplateTestCase<UsageBillingSettings>
	{
		public void TestGetClone()
		{
			var settings = NewPopulatedBusinessObject();
			var priceList = settings.PriceLists.AddNew();
			priceList.ProductCode = "E2E";
			priceList.PriceListCode = "EX1";
			var branchRestriction = settings.BranchRestrictions.AddNew();
			branchRestriction.ProductCode = "E2E";
			branchRestriction.InvoicingBranch = Env.CurrentBranchPK;

			var clone = (UsageBillingSettings)settings.Clone(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			AssertEquals(1, clone.PriceLists.Count);
			AssertEquals(1, clone.BranchRestrictions.Count);
			AssertEquals("E2E", clone.PriceLists[0].ProductCode);
			AssertEquals("EX1", clone.PriceLists[0].PriceListCode);
			AssertEquals("E2E", clone.BranchRestrictions[0].ProductCode);
			AssertEquals(Env.CurrentBranchPK, clone.BranchRestrictions[0].InvoicingBranch);
		}

		public void TestValidation()
		{
			var mappings = EDIDataRegistry.Instance.SystemProductMappings.Value;
			mappings.AddNew("ABC", "ABC Product", true);
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var collection = new UsageBillingPriceListCollection();
			var product1 = collection.AddNew();
			product1.RunPreSaveValidation();
			AssertHasError(product1.ProductCodeInfo, "Please enter a Product Code.");
			product1.ProductCode = "XXX";
			AssertHasError(product1.ProductCodeInfo, "Enter a valid Product Code.");
			product1.ProductCode = "ABC";
			AssertNoErrors(product1.ProductCodeInfo);

			AssertHasError(product1.PriceListCodeInfo, "Please enter a Price List Code.");
			product1.PriceListCode = "E#1";
			AssertNoErrors(product1.PriceListCodeInfo);

			var product2 = collection.AddNew();
			product2.ProductCode = "ABC";
			product2.PriceListCode = "E#1";
			AssertHasError(product2.PriceListCodeInfo, "The price list system code must be unique.");
			product2.PriceListCode = "E#2";
			AssertNoErrors(product2.PriceListCodeInfo);

			product2.PriceListCode = "WCU";
			AssertHasError(product2.PriceListCodeInfo, "The price list system code must be unique.");
			product2.PriceListCode = "E#3";
			AssertNoErrors(product2.PriceListCodeInfo);
		}

		public void TestSerialization()
		{
			var settings = NewPopulatedBusinessObject();
			var priceList = settings.PriceLists.AddNew();
			priceList.ProductCode = "E2E";
			priceList.PriceListCode = "EX1";
			var branchRestriction = settings.BranchRestrictions.AddNew();
			branchRestriction.ProductCode = "E2E";
			branchRestriction.InvoicingBranch = Env.CurrentBranchPK;

			var serializer = ZXmlSerializer.New(typeof(UsageBillingSettings));
			using (var writer = new StringWriter(CultureInfo.InvariantCulture))
			{
				serializer.Serialize(writer, settings);
				var xml = writer.ToString();

				using (var reader = new StringReader(xml))
				{
					var clone = (UsageBillingSettings)serializer.Deserialize(reader);
					AssertEquals(1, clone.PriceLists.Count);
					AssertEquals(1, clone.BranchRestrictions.Count);
					AssertEquals("E2E", clone.PriceLists[0].ProductCode);
					AssertEquals("EX1", clone.PriceLists[0].PriceListCode);
					AssertEquals("E2E", clone.BranchRestrictions[0].ProductCode);
					AssertEquals(Env.CurrentBranchPK, clone.BranchRestrictions[0].InvoicingBranch);
				}
			}
		}

		public void TestCodeAlive()
		{
			AssertNotNull("CodeAlive", typeof(AutoUsageBillingSettings.Schema));
		}

		public static void SetupValidTestRegistry()
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

			EDIDataRegistry.Instance.UsageBillingSettings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, settings);
		}

		protected override bool RequiresFactory => true;
		protected override bool RequiresFallbackLevel => true;

		protected override UsageBillingSettings GetBusinessObjectToClone() => NewPopulatedBusinessObject();

		protected override UsageBillingSettings GetBusinessObjectToSerialise() => NewPopulatedBusinessObject();

		UsageBillingSettings NewPopulatedBusinessObject() => new UsageBillingSettings(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
	}
}
