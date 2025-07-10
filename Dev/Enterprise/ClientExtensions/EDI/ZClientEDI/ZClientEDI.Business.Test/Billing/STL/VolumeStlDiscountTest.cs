using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	internal class VolumeStlDiscountTest : TestCaseWithFactory
	{
		public void TestPercentage()
		{
			AssertEquals(0m, VolumeStlDiscount.CalculatePercentage(0));
			AssertEquals(5m, VolumeStlDiscount.CalculatePercentage(10001m));
			AssertEquals(10m, VolumeStlDiscount.CalculatePercentage(20001m));
			AssertEquals(15m, VolumeStlDiscount.CalculatePercentage(40001m));
			AssertEquals(20m, VolumeStlDiscount.CalculatePercentage(80001m));
			AssertEquals(24m, VolumeStlDiscount.CalculatePercentage(160001m));
			AssertEquals(28m, VolumeStlDiscount.CalculatePercentage(320001m));
			AssertEquals(32m, VolumeStlDiscount.CalculatePercentage(640001m));
			AssertEquals(36m, VolumeStlDiscount.CalculatePercentage(1280001m));
			AssertEquals(40m, VolumeStlDiscount.CalculatePercentage(2560001m));
			AssertEquals(44m, VolumeStlDiscount.CalculatePercentage(5120001m));
			AssertEquals(47m, VolumeStlDiscount.CalculatePercentage(10240001m));
			AssertEquals(50m, VolumeStlDiscount.CalculatePercentage(20480001m));

			var discountConfig = new VolumeDiscount();
			var line = discountConfig.Lines.AddNew();
			line.UnitCount = 1000;
			line.Percent = 10m;
			line = discountConfig.Lines.AddNew();
			line.UnitCount = 9000000000m;
			line.Percent = 50m;
			AssertEquals(50m, VolumeStlDiscount.CalculatePercentage(9000000001m, discountConfig));

			var closerTo5 = VolumeStlDiscount.CalculatePercentage(9900);
			Assert(closerTo5.ToString(), closerTo5 < 5 && closerTo5 > 0 && (5 - closerTo5) < closerTo5);

			var closerTo10 = VolumeStlDiscount.CalculatePercentage(19900);
			Assert(closerTo10.ToString(), closerTo10 < 10 && closerTo10 > 5 && (10 - closerTo10) < (closerTo10 - 5));

			var closerTo50 = VolumeStlDiscount.CalculatePercentage(20000000m);
			Assert(closerTo50.ToString(), closerTo50 < 50 && closerTo50 > 47 && (50 - closerTo50) < (closerTo50 - 47));

			var info = new DiscountInfo();
			var headerDiscount = Factory.New<EdiPriceHeaderDiscount>();
			var setting = Factory.New<DiscountLicenceSetting>();
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");

			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2015, 10, 1));
			monthlyUsage.BuyingGroup = new StlBilling.BuyingGroupVolume() { TotalLicenceUnits = 790000m };

			info.Init(headerDiscount, setting, monthlyUsage);
			var discount = new VolumeStlDiscount(info);
			AssertEquals(32m, discount.Percentage);

			info.Init(headerDiscount, null, monthlyUsage);
			discount = new VolumeStlDiscount(info);
			AssertEquals(32m, discount.Percentage);

			headerDiscount.PHD_Type = BillingConstants.DiscountCalculator.Volume;
			var volumeDiscount = ((VolumeDiscount)headerDiscount.Config);
			volumeDiscount.Lines.RemoveAndDeleteAll();
			var line1 = volumeDiscount.Lines.AddNew();
			line1.UnitCount = 32000;
			line1.Percent = 28.26m;
			var line2 = volumeDiscount.Lines.AddNew();
			line2.UnitCount = 64000;
			line2.Percent = 32.35m;
			var line3 = volumeDiscount.Lines.AddNew();
			line3.UnitCount = 1280000;
			line3.Percent = 36.47m;
			info.Init(headerDiscount, null, monthlyUsage);
			discount = new VolumeStlDiscount(info);
			AssertEquals(32.35m, discount.Percentage);

			setting.LS9_Percent = 77m;
			info.Init(headerDiscount, setting, monthlyUsage);
			discount = new VolumeStlDiscount(info);
			AssertEquals(77m, discount.Percentage);
		}
	}

	internal class DomesticEntityStlDiscountTest : TestCaseWithFactory
	{
		public void TestMonthExpiry()
		{
			// They create a second entity the month after this and no longer meet the Domestic Discount criteria
			var lastPeriodWithSingleEntity = EdiDateTest.MonthToday;

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "CN";
			lic2.ClientCompany.LCC_RN_NKCountryCode = "US";
			lic1.ClientCompany.LCC_CreateTimeUtc = lastPeriodWithSingleEntity.AddMonths(-12);
			lic2.ClientCompany.LCC_CreateTimeUtc = lastPeriodWithSingleEntity.AddMonths(1).AddDays(2);
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_SystemCode = "STL";
			priceHeader.L6_ValidFrom = lastPeriodWithSingleEntity.AddMonths(-12);

			var headerDevelopingDiscount = priceHeader.StlDiscounts.AddNew();
			headerDevelopingDiscount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;
			headerDevelopingDiscount.PHD_Name = "Developing Country";
			var countryDiscount = (CountryDiscount)headerDevelopingDiscount.Config;
			countryDiscount.Lines.RemoveAll();
			var chinaLine = countryDiscount.Lines.AddNew();
			chinaLine.Country = "CN";
			chinaLine.Percent = 40;
			chinaLine.RequiresDomesticDiscount = true;

			var headerDomesticDiscount = priceHeader.StlDiscounts.AddNew();
			headerDomesticDiscount.PHD_Type = BillingConstants.DiscountCalculator.DomesticEntity;
			headerDomesticDiscount.PHD_Name = "Domestic Entity";
			var domesticDiscount = (DomesticDiscount)headerDomesticDiscount.Config;
			domesticDiscount.MaxForeignCompanyCount = 0;
			domesticDiscount.ExpiryMonthCount = 3;
			domesticDiscount.RequiresDevelopingCountry = true;
			var line1 = domesticDiscount.Lines.AddNew();
			line1.UserCount = 0;
			line1.Percent = 30m;
			var line2 = domesticDiscount.Lines.AddNew();
			line2.UserCount = 20;
			line2.Percent = 20m;

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity, lic1.ClientCompany, line2.UserCount - 1);

			var usage2a = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(1), lic1.ClientCompany, line2.UserCount - 1);
			var usage2b = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(1), lic2.ClientCompany, 1);

			var usage3a = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(2), lic1.ClientCompany, line2.UserCount - 1);
			var usage3b = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(2), lic2.ClientCompany, 1);

			var usage4a = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(3), lic1.ClientCompany, line2.UserCount - 1);
			var usage4b = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(3), lic2.ClientCompany, 1);

			var usage5a = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(4), lic1.ClientCompany, line2.UserCount - 1);
			var usage5b = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", lastPeriodWithSingleEntity.AddMonths(4), lic2.ClientCompany, 1);

			Factory.Save();

			var clientCompanies = new ClientCompany[] { lic1.ClientCompany, lic2.ClientCompany };

			var delivery = lic1.Company.InvoiceDeliveries.First();
			var invoicedCompany = lic1.Company;

			var dbUsers = new DatabaseCountryUserSet();
			List<IStlDiscount> availableDiscounts = new List<IStlDiscount>();

			// last month of meeting the criteria
			{
				var usages = new Usage[] { new Usage(usage1) };

				var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic1), delivery, invoicedCompany),
					usages, null, null, lic1.Database.LicenceSettings.ToArray(), ZDateTime.Empty, true, new Dictionary<string, PriceList>(), new ClientCompany[] { lic1.ClientCompany });

				var monthlyUsage = new StlMonthlyUsage(lic1.Factory, databaseUsage, lastPeriodWithSingleEntity, new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
					new SystemBill.TaxGroup(delivery));

				var info1 = new DiscountInfo();
				info1.Init(headerDomesticDiscount, null, monthlyUsage, dbUsers);
				var info2 = new DiscountInfo();
				info2.Init(headerDevelopingDiscount, null, monthlyUsage, dbUsers);
				var stlDiscount1 = new DomesticEntityStlDiscount(info1);
				var stlDiscount2 = new DevelopingCountryStlDiscount(info2);
				var potentialDiscounts = new IStlDiscount[] { stlDiscount1, stlDiscount2 };

				StlDiscount.BuildAvailableDiscounts(potentialDiscounts, availableDiscounts);
				AssertEquals("both discounts apply", 2, availableDiscounts.Count);
				AssertEquals("domestic discount", 30m, stlDiscount1.Percentage);
				AssertEquals("developing discount", 40m, stlDiscount2.Percentage);
			}

			// 3 months after failing the criteria
			{
				var usages = new Usage[] { new Usage(usage4a), new Usage(usage4b) };
				var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic1), delivery, invoicedCompany),
					usages, null, null, lic1.Database.LicenceSettings.ToArray(), ZDateTime.Empty, true, new Dictionary<string, PriceList>(), clientCompanies);

				var monthlyUsage = new StlMonthlyUsage(lic1.Factory, databaseUsage, lastPeriodWithSingleEntity.AddMonths(3), new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
					new SystemBill.TaxGroup(delivery));

				var info1 = new DiscountInfo();
				info1.Init(headerDomesticDiscount, null, monthlyUsage, dbUsers);
				var info2 = new DiscountInfo();
				info2.Init(headerDevelopingDiscount, null, monthlyUsage, dbUsers);
				var stlDiscount1 = new DomesticEntityStlDiscount(info1);
				var stlDiscount2 = new DevelopingCountryStlDiscount(info2);
				var potentialDiscounts = new IStlDiscount[] { stlDiscount1, stlDiscount2 };

				StlDiscount.BuildAvailableDiscounts(potentialDiscounts, availableDiscounts);
				AssertEquals("both discounts apply", 2, availableDiscounts.Count);
				AssertEquals("domestic discount applies at multi-entity rate", 10m, stlDiscount1.Percentage);
				AssertEquals("developing discount", 40m, stlDiscount2.Percentage);
			}

			// 4 months after failing the criteria
			{
				var usages = new Usage[] { new Usage(usage5a), new Usage(usage5b) };

				var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic1), delivery, invoicedCompany),
					usages, null, null, lic1.Database.LicenceSettings.ToArray(), ZDateTime.Empty, true, new Dictionary<string, PriceList>(), clientCompanies);

				var monthlyUsage = new StlMonthlyUsage(lic1.Factory, databaseUsage, lastPeriodWithSingleEntity.AddMonths(4), new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
					new SystemBill.TaxGroup(delivery));

				var info1 = new DiscountInfo();
				info1.Init(headerDomesticDiscount, null, monthlyUsage, dbUsers);
				var info2 = new DiscountInfo();
				info2.Init(headerDevelopingDiscount, null, monthlyUsage, dbUsers);
				var stlDiscount1 = new DomesticEntityStlDiscount(info1);
				var stlDiscount2 = new DevelopingCountryStlDiscount(info2);
				var potentialDiscounts = new IStlDiscount[] { stlDiscount1, stlDiscount2 };

				StlDiscount.BuildAvailableDiscounts(potentialDiscounts, availableDiscounts);
				AssertEquals("both discounts expire", 0, availableDiscounts.Count);
			}
		}
	}

	internal class WiseCloudStlDiscountTest : TestCaseWithFactory
	{
		[TestDate(2016, 8, 5)]
		public void TestPercentage()
		{
			var info = new DiscountInfo();
			var headerDiscount = Factory.New<EdiPriceHeaderDiscount>();
			headerDiscount.PHD_Type = BillingConstants.DiscountCalculator.WiseCloud;
			headerDiscount.PHD_Percent = 5.99m;
			var wiseCloudDiscount = ((WiseCloudDiscount)headerDiscount.Config);
			var lic = BillingTestHelper.CreateLicence(Factory, "AAA");

			var monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2016, 8, 1));
			monthlyUsage.BuyingGroup = new StlBilling.BuyingGroupVolume() { TotalLicenceUnits = 790000m };

			info.Init(headerDiscount, null, monthlyUsage);
			var discount = new WiseCloudStlDiscount(info);
			AssertEquals(5.99m, discount.Percentage);

			//E.g. 2) Customer goes live on 1 Feb. Expiry is 6 months Last day of discount is July 31. August usage is entirely outside the discount and is not discounted.
			wiseCloudDiscount.ExpiryMonthsFromAgreedGoLive = 6;
			lic.LA_AgreedLiveDate = new ZDateTime(2016, 2, 1);
			monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2016, 8, 1), new ZDateTime(2016, 2, 1));
			monthlyUsage.BuyingGroup = new StlBilling.BuyingGroupVolume() { TotalLicenceUnits = 790000m };
			info.Init(headerDiscount, null, monthlyUsage);
			discount = new WiseCloudStlDiscount(info);
			AssertEquals(0m, discount.Percentage);

			//E.g. 1) Customer goes live on 15 Feb. Expiry is 6 months. Last day of discount is 14 Aug. Since August usage is partially covered, the discount applies in August and then ends.
			lic.LA_AgreedLiveDate = new ZDateTime(2016, 2, 15);
			monthlyUsage = StlMonthlyUsageTest.CreateMonthlyUsage(lic, new ZDateTime(2016, 8, 1), new ZDateTime(2016, 2, 15));
			monthlyUsage.BuyingGroup = new StlBilling.BuyingGroupVolume() { TotalLicenceUnits = 790000m };
			info.Init(headerDiscount, null, monthlyUsage);
			discount = new WiseCloudStlDiscount(info);
			AssertEquals(5.99m, discount.Percentage);
		}
	}

	internal class DevelopingCountryStlDiscountTest : TestCaseWithFactory
	{
		public void TestSouthAfricaDevelopingCountryDiscount()
		{
			var today = ZDateTime.Today;
			var thisPeriod = new ZDateTime(today.Year, today.Month, 1);
			var lastYear = today.AddYears(-1);

			var lic1 = BillingTestHelper.CreateLicence(Factory, "AAA");
			var lic2 = BillingTestHelper.CreateAnotherLicence(lic1, "BBB");
			lic1.ClientCompany.LCC_RN_NKCountryCode = "ZA";
			lic2.ClientCompany.LCC_RN_NKCountryCode = "CI";
			lic1.ClientCompany.LCC_CreateTimeUtc = lastYear;
			lic2.ClientCompany.LCC_CreateTimeUtc = lastYear;
			BillingTestHelper.SetInvoicing(lic1, Env.CurrentBranch.PK);

			var priceHeader = lic1.Company.PriceHeaders.AddNew();
			priceHeader.L6_DiscountCode = "STL1";
			priceHeader.L6_SystemCode = "STL";
			priceHeader.L6_ValidFrom = lastYear;

			var headerDevelopingDiscount = priceHeader.StlDiscounts.AddNew();
			headerDevelopingDiscount.PHD_Type = BillingConstants.DiscountCalculator.DevelopingCountry;
			headerDevelopingDiscount.PHD_Name = "Developing Country";
			var countryDiscount = (CountryDiscount)headerDevelopingDiscount.Config;
			countryDiscount.Lines.RemoveAll();
			var line = countryDiscount.Lines.AddNew();
			line.Country = "ZA";
			line.Percent = 35;
			line.RequiresDomesticDiscount = false;

			var usage1 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", thisPeriod, lic1.ClientCompany, 100);
			var usage2 = BillingTestHelper.CreateChargeableUsage(Factory, "STL", "USR", thisPeriod, lic2.ClientCompany, 200);
			Factory.Save();

			var clientCompanies = new ClientCompany[] { lic1.ClientCompany, lic2.ClientCompany };

			var delivery = lic1.Company.InvoiceDeliveries.First();
			var invoicedCompany = lic1.Company;

			var dbUsers = new DatabaseCountryUserSet();
			List<IStlDiscount> availableDiscounts = new List<IStlDiscount>();

			var list = new CodeDescriptionBoolCollection();
			list.Add("ZA", (NoResString)"ZA", false);
			list.Add("CI", (NoResString)"ZA", false);
			EDIDataRegistry.Instance.BillingCountryGroups.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);

			// All ZA region only entities
			{
				var usages = new Usage[] { new Usage(usage1), new Usage(usage2) };

				var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic1), delivery, invoicedCompany),
					usages, null, null, lic1.Database.LicenceSettings.ToArray(), ZDateTime.Empty, true, new Dictionary<string, PriceList>(), clientCompanies);

				var monthlyUsage = new StlMonthlyUsage(lic1.Factory, databaseUsage, thisPeriod, new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
					new SystemBill.TaxGroup(delivery));

				var devInfo = new DiscountInfo();
				devInfo.Init(headerDevelopingDiscount, null, monthlyUsage, dbUsers);
				var devDiscount = new DevelopingCountryStlDiscount(devInfo);
				var potentialDiscounts = new IStlDiscount[] { devDiscount };

				StlDiscount.BuildAvailableDiscounts(potentialDiscounts, availableDiscounts);
				AssertEquals(1, availableDiscounts.Count);
				AssertEquals(35m, devDiscount.Percentage);
			}

			// ZA & AU entities
			{
				lic2.ClientCompany.LCC_RN_NKCountryCode = "AU";
				Factory.Save();

				var usages = new Usage[] { new Usage(usage1), new Usage(usage2) };

				var databaseUsage = new DatabaseUsage(new UsageOwnerDelivery(new UsageOwner(lic1), delivery, invoicedCompany),
					usages, null, null, lic1.Database.LicenceSettings.ToArray(), ZDateTime.Empty, true, new Dictionary<string, PriceList>(), clientCompanies);

				var monthlyUsage = new StlMonthlyUsage(lic1.Factory, databaseUsage, thisPeriod, new InvoiceGroup(delivery, invoicedCompany?.LC_OH ?? ZGuid.Empty),
					new SystemBill.TaxGroup(delivery));

				var devInfo = new DiscountInfo();
				devInfo.Init(headerDevelopingDiscount, null, monthlyUsage, dbUsers);
				var devDiscount = new DevelopingCountryStlDiscount(devInfo);
				var potentialDiscounts = new IStlDiscount[] { devDiscount };

				StlDiscount.BuildAvailableDiscounts(potentialDiscounts, availableDiscounts);
				AssertEquals(0, availableDiscounts.Count);
			}
		}
	}
}
