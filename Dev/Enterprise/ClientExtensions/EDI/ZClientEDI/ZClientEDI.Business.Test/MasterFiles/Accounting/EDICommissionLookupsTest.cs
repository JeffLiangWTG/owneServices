using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	class EDICommissionLookupsTest : CommissionLookupsTestCase<EDICommissionLookups>
	{
		#region Products

		public void TestProducts()
		{
			var lookups = EDICommissionLookups.New(Factory);

			Converter<ICodeDescription, string> textProvider = (x) => x.Code + " " + x.Description;
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionComparer(), textProvider, new ProductTypes().Cast<ICodeDescription>(), lookups.GetProducts().Cast<ICodeDescription>());
		}

		#endregion

		#region Services

		public void TestServices()
		{
			var billingSystemChargeCodeMappings = new BillingSystemChargeCodeMappingCollection();
			billingSystemChargeCodeMappings.AddNew("ENT", "ABM", "");
			billingSystemChargeCodeMappings.AddNew("ENT", "ABM", "CUSMES");
			billingSystemChargeCodeMappings.AddNew("ENT", "ABM", "MOVMES");
			billingSystemChargeCodeMappings.AddNew("ENT", "AAA", "AAAAAA");
			billingSystemChargeCodeMappings.AddNew("XXX", "BBB", "");
			EDIDataRegistry.Instance.BillingSystemChargeCodeMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, billingSystemChargeCodeMappings);

			var lookups = EDICommissionLookups.New(Factory);

			Converter<ICodeDescription, string> textProvider = (x) => x.Code + " " + x.Description;
			CodeDescriptionComparer comparer = new CodeDescriptionComparer();
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetServices("").Cast<ICodeDescription>());
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetServices("ALL").Cast<ICodeDescription>());

			var expectedEntServices = BillingConstants.GetBillingSystemList();
			expectedEntServices.AddPair("AAA", "AAA");
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, expectedEntServices.Cast<ICodeDescription>(), lookups.GetServices(ProductTypes.Codes.Enterprise).Cast<ICodeDescription>());

			var expectedXxxServices = new CodeDescriptionPairList();
			expectedXxxServices.AddPair("BBB", "BBB");
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, expectedXxxServices.Cast<ICodeDescription>(), lookups.GetServices("XXX").Cast<ICodeDescription>());
		}

		#endregion

		#region SubModules

		public void TestSubModules()
		{
			var odmPriceHeader = Factory.New<ClientLicencePriceHeader>();
			odmPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			var odmPriceItem1 = odmPriceHeader.Items.AddNew();
			odmPriceItem1.L7_Code = "111";
			odmPriceItem1.L7_Description = "111 Description";
			var odmPriceItem2 = odmPriceHeader.Items.AddNew();
			odmPriceItem2.L7_Code = "222";
			odmPriceItem2.L7_Description = "222 Description";

			var stlPriceHeader = Factory.New<ClientLicencePriceHeader>();
			stlPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			var stlPriceItemA = stlPriceHeader.Items.AddNew();
			stlPriceItemA.L7_Code = "AAA";
			stlPriceItemA.L7_Description = "AAA Description";
			var stlPriceItemB = stlPriceHeader.Items.AddNew();
			stlPriceItemB.L7_Code = "BBB";
			stlPriceItemB.L7_Description = "BBB Description";

			Factory.Save();

			var lookups = EDICommissionLookups.New(Factory);

			Converter<ICodeDescription, string> textProvider = (x) => x.Code + " " + x.Description;
			CodeDescriptionComparer comparer = new CodeDescriptionComparer();
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetSubModules("", "").Cast<ICodeDescription>());
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetSubModules("XXX", "XXX").Cast<ICodeDescription>());
			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetSubModules("ALL", "ALL").Cast<ICodeDescription>());
			AssertContainsExactElementsInAnyOrder(comparer, textProvider,
				new[]
				{
					new CodeDescriptionPair("111", "111 Description"),
					new CodeDescriptionPair("222", "222 Description"),
				},
				lookups.GetSubModules(ProductTypes.Codes.Enterprise, BillingConstants.BillingSystem.ODM).Cast<ICodeDescription>());

			AssertContainsExactElementsInAnyOrder(comparer, textProvider,
				new[]
				{
					new CodeDescriptionPair("AAA", "AAA Description"),
					new CodeDescriptionPair("BBB", "BBB Description"),
				},
				lookups.GetSubModules(ProductTypes.Codes.Enterprise, BillingConstants.BillingSystem.STL).Cast<ICodeDescription>());

			AssertContainsExactElementsInAnyOrder(comparer, textProvider,
				new ClientLicenceFeeLookups(null).FeeTypes.Cast<ICodeDescription>(),
				lookups.GetSubModules(ProductTypes.Codes.Enterprise, BillingConstants.BillingSystem.Fee).Cast<ICodeDescription>());

			AssertContainsExactElementsInAnyOrder(comparer, textProvider, Enumerable.Empty<ICodeDescription>(), lookups.GetSubModules("XXX", BillingConstants.BillingSystem.ODM).Cast<ICodeDescription>());
		}

		public void TestSubModules_PremiumServices()
		{
			var stdHeader = ClientLicencePriceHeaderCollectionTest.CreateStandardPricesHeader(Factory);
			var stdLicCompany = stdHeader.Company;
			var ldsPriceHeader = stdLicCompany.PriceHeaders.AddNew();
			var goldenTaxPriceHeader = stdLicCompany.PriceHeaders.AddNew();

			var licence1a = BillingTestHelper.CreateLicence(Factory, "DDD", "AAA", "SYD");
			var licence1b = BillingTestHelper.CreateAnotherLicence(licence1a, "BNE");
			var licence2 = BillingTestHelper.CreateLicence(Factory, "EEE", "BBB", "MEL");

			var org1a = licence1a.Company.Header;
			var org1b = licence1b.Company.Header;
			var org2 = licence2.Company.Header;

			var db1 = licence1a.Database;
			var db2 = licence2.Database;

			var periodStart = new ZDateTime(2017, 1, 1);
			var stlPriceHeader = Factory.New<ClientLicencePriceHeader>();
			stlPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.STL;
			stlPriceHeader.L6_TestDbPriceCode = "#NP";

			ldsPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.LDaaS;
			var piceItem1 = ldsPriceHeader.Items.AddNew();
			piceItem1.L7_Code = "AA1";
			piceItem1.L7_Description = "AA1 Description";
			var priceItem2 = ldsPriceHeader.Items.AddNew();
			priceItem2.L7_Code = "AA2";
			priceItem2.L7_Description = "AA2 Description";
			var priceItem3 = ldsPriceHeader.Items.AddNew();
			priceItem3.L7_Code = "BB1";
			priceItem3.L7_Description = "BB1 Description";

			goldenTaxPriceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.GoldenTax;
			var goldItem1 = goldenTaxPriceHeader.Items.AddNew();
			goldItem1.L7_Code = "GTD";
			goldItem1.L7_Description = "Golden Tax Device";

			var service1a = BillingTestHelper.CreatePremiumService(db1, "#HC", periodStart, ZDateTime.Empty);
			var service1b = BillingTestHelper.CreatePremiumService(db1, "AA1", periodStart, ZDateTime.Empty);
			service1b.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;
			var service1c = BillingTestHelper.CreatePremiumService(db1, "AA2", periodStart, ZDateTime.Empty);
			service1c.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			var service2a = BillingTestHelper.CreatePremiumService(db2, "#HC", periodStart, ZDateTime.Empty);
			var service2b = BillingTestHelper.CreatePremiumService(db2, "BB1", periodStart, ZDateTime.Empty);
			service2b.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.LDaaS;

			var service3 = BillingTestHelper.CreatePremiumService(db2, "GTD", periodStart, ZDateTime.Empty);
			service3.CPS_PriceHeaderCode = BillingConstants.PriceHeaderType.GoldenTax;

			Factory.Save();

			var lookups = EDICommissionLookups.New(Factory);

			Converter<ICodeDescription, string> textProvider = (x) => x.Code + " " + x.Description;
			CodeDescriptionComparer comparer = new CodeDescriptionComparer();

			AssertContainsExactElementsInAnyOrder(comparer, textProvider,
				new[]
				{
					new CodeDescriptionPair("#HC", ""),
					new CodeDescriptionPair("AA1", "AA1 Description"),
					new CodeDescriptionPair("AA2", "AA2 Description"),
					new CodeDescriptionPair("BB1", "BB1 Description"),
					new CodeDescriptionPair("GTD", "Golden Tax Device"),
				},
				lookups.GetSubModules(ProductTypes.Codes.Enterprise, BillingConstants.BillingSystem.Service).Cast<ICodeDescription>());
		}

		#endregion

		#region ShouldShowServicesAndSubModules

		public void TestGetShouldShowServicesAndSubModules()
		{
			var lookups = EDICommissionLookups.New(Factory);
			AssertEquals(true, lookups.GetShouldShowServicesAndSubModules());
		}

		#endregion

		#region Overrides

		protected override EDICommissionLookups GetNewLookups()
		{
			return EDICommissionLookups.New(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			LicenceCompany.ClearStandardPricesCompanyCache();
		}

		protected override void TearDown()
		{
			LicenceCompany.ClearStandardPricesCompanyCache();
			base.TearDown();
		}

		#endregion

		#region Classes

		class CodeDescriptionComparer : IEqualityComparer<ICodeDescription>
		{
			public bool Equals(ICodeDescription x, ICodeDescription y)
			{
				return
					x.Code == y.Code &&
					x.Description == y.Description;
			}

			public int GetHashCode(ICodeDescription obj)
			{
				unchecked
				{
					var hash = 13;
					hash += (hash * 7) + obj.Code.GetHashCode();
					hash += (hash * 7) + obj.Description.GetHashCode();

					return hash;
				}
			}
		}

		#endregion
	}
}