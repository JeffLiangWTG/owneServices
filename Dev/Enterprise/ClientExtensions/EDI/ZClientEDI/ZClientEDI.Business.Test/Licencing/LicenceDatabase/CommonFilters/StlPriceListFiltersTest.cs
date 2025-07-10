using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Business.Test;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class StlPriceListFiltersTest : TestCaseWithFactory
	{
		public void TestPriceFilters()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			var priceList1 = BillingTestHelper.CreatePriceHeader(lic1.Company, "STL", "STL v1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price1 = BillingTestHelper.AddPriceItem(priceList1, "C01", "TRA", "", 100m);
			var link1 = BillingTestHelper.CreatePriceLink(lic1.Database, priceList1, new ZDateTime(2016, 1, 1));
			link1.PHL_ValidTo = new ZDateTime(2017, 1, 1);
			link1.PHL_L6 = priceList1.PK;
			link1.PHL_RX_NKCurrency = "AUD";
			link1.PHL_VolumeCode = "HV";
			link1.PHL_CorePackCode = "INC";
			link1.PHL_SystemCreateUser = "U01";
			link1.PHL_SystemCreateTimeUtc = new ZDateTime(2016, 2, 1);
			var link2 = BillingTestHelper.CreatePriceLink(lic1.Database, priceList1, new ZDateTime(2014, 1, 1));
			link2.PHL_ValidTo = new ZDateTime(2015, 1, 1);
			link2.PHL_L6 = priceList1.PK;
			link2.PHL_RX_NKCurrency = "USD";
			link2.PHL_VolumeCode = "LV";
			link2.PHL_CorePackCode = "EX";
			link2.PHL_SystemCreateUser = "U02";
			link2.PHL_SystemCreateTimeUtc = new ZDateTime(2015, 2, 1);
			Factory.Save();

			var filters = new ModuleFilterCollection();
			new StlPriceListFilters().AddPriceListFilters(filters, null, null);

			var validFrom = filters["STL Prices Valid From"] as ModuleDateFilter;
			validFrom.IsActive = true;
			validFrom.PropertySearch = "Date range";
			validFrom.Property1 = new ZDateTime(2016, 01, 1);
			validFrom.Property2 = new ZDateTime(2016, 01, 1);

			var validTo = filters["STL Prices Valid To"] as ModuleDateFilter;
			validTo.IsActive = true;
			validTo.PropertySearch = "Date range";
			validTo.Property1 = new ZDateTime(2017, 01, 1);
			validTo.Property2 = new ZDateTime(2017, 01, 1);

			var pricesVersion = filters["STL Prices Version"] as ModuleTextFilter;
			pricesVersion.IsActive = true;
			pricesVersion.Property = "STL v1";

			var pricesCurrency = filters["STL Prices Currency"] as ModuleNkFilter;
			pricesCurrency.IsActive = true;
			pricesCurrency.Property = "AUD";

			var pricesVolume = filters["STL Prices Volume"] as ModuleTextFilter;
			pricesVolume.IsActive = true;
			pricesVolume.Property = "HV";

			var pricesCorePack = filters["STL Prices Core Pack"] as ModuleTextFilter;
			pricesCorePack.IsActive = true;
			pricesCorePack.Property = "INC";

			var pricesCreatedBy = filters["STL Prices Created By"] as ModuleNkFilter;
			pricesCreatedBy.IsActive = true;
			pricesCreatedBy.Property = "U01";

			var createdDate = filters["STL Prices Created Date"] as ModuleDateFilter;
			createdDate.IsActive = true;
			createdDate.PropertySearch = "Date range";
			createdDate.Property1 = new ZDateTime(2016, 02, 1);
			createdDate.Property2 = new ZDateTime(2016, 02, 1);

			var query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, pricesVersion, pricesCurrency,
				pricesVolume, pricesCorePack, pricesCreatedBy, createdDate });
			AssertEquals(link1.PK, Factory.Load<EdiPriceHeaderLink>(query).Single().PK);
		}

		public void TestSettingFilters()
		{
			var lic1 = BillingTestHelper.CreateLicence(Factory, "EN1", "AU1", "DB1");
			var priceList1 = BillingTestHelper.CreatePriceHeader(lic1.Company, "STL", "STL v1", "AUD", new ZDateTime(2016, 1, 1), false);
			var price1 = BillingTestHelper.AddPriceItem(priceList1, "C01", "TRA", "", 100m);

			var settingDIS = lic1.Database.LicenceSettings.AddNew();
			settingDIS.LS9_ValidFrom = settingDIS.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingDIS.LS9_Type = "DIS";
			settingDIS.LS9_Name = "SPECIAL";
			settingDIS.LS9_Percent = 10;
			settingDIS.LS9_Comment = "Comment002";

			var settingDIS2 = lic1.Database.LicenceSettings.AddNew();
			settingDIS2.LS9_ValidFrom = settingDIS2.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingDIS2.LS9_Type = "DIS";
			settingDIS2.LS9_Name = "WISESPECIAL";
			settingDIS2.LS9_Percent = 11;
			settingDIS2.LS9_Comment = "Comment003";

			var settingBUY = lic1.Database.LicenceSettings.AddNew();
			settingBUY.LS9_ValidFrom = settingBUY.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingBUY.LS9_Type = "BUY";
			settingBUY.LS9_Name = "Buy001";

			var settingBUY2 = lic1.Database.LicenceSettings.AddNew();
			settingBUY2.LS9_ValidFrom = settingBUY2.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingBUY2.LS9_Type = "BUY";
			settingBUY2.LS9_Name = "Buy002";

			var settingCOM = lic1.Database.LicenceSettings.AddNew();
			settingCOM.LS9_ValidFrom = settingCOM.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingCOM.LS9_Type = "COM";
			settingCOM.LS9_Price = 12;

			var settingCOM2 = lic1.Database.LicenceSettings.AddNew();
			settingCOM2.LS9_ValidFrom = settingCOM2.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingCOM2.LS9_Type = "COM";
			settingCOM2.LS9_Price = 22;

			var settingPRI = lic1.Database.LicenceSettings.AddNew();
			settingPRI.LS9_ValidFrom = settingPRI.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingPRI.LS9_Type = "PRI";
			settingPRI.LS9_Price = 45.67;
			settingPRI.PriceKey = new UsageCodeKey("STL", "#P1");

			var settingPRI2 = lic1.Database.LicenceSettings.AddNew();
			settingPRI2.LS9_ValidFrom = settingPRI2.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingPRI2.LS9_Type = "PRI";
			settingPRI2.LS9_Price = 62.34;
			settingPRI2.PriceKey = new UsageCodeKey("STL", "#P2");

			var settingHVF1 = lic1.Database.LicenceSettings.AddNew();
			settingHVF1.LS9_ValidFrom = settingHVF1.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingHVF1.LS9_Type = "HVF";
			settingHVF1.PriceKey = new UsageCodeKey("STL", "HV1");

			var settingHVF2 = lic1.Database.LicenceSettings.AddNew();
			settingHVF2.LS9_ValidFrom = settingHVF2.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingHVF2.LS9_Type = "HVF";
			settingHVF2.PriceKey = new UsageCodeKey("STL", "HV2");

			var settingMIN = lic1.Database.LicenceSettings.AddNew();
			settingMIN.LS9_ValidFrom = settingMIN.LS9_ValidTo = new ZDateTime(2016, 1, 1);
			settingMIN.LS9_Type = "MIN";
			settingMIN.PriceKey = new UsageCodeKey("STL", "#P3");

			Factory.Save();

			var filters = new ModuleFilterCollection();
			new StlPriceListFilters().AddPriceListFilters(filters, null, null);

			var validFrom = filters["STL Setting Valid From"] as ModuleDateFilter;
			validFrom.IsActive = true;
			validFrom.PropertySearch = "Date range";
			validFrom.Property1 = new ZDateTime(2016, 01, 1);
			validFrom.Property2 = new ZDateTime(2016, 01, 1);

			var validTo = filters["STL Setting Valid To"] as ModuleDateFilter;
			validTo.IsActive = true;
			validTo.PropertySearch = "Date range";
			validTo.Property1 = new ZDateTime(2016, 01, 1);
			validTo.Property2 = new ZDateTime(2016, 01, 1);

			var discountName = filters["STL Setting Discount Name"] as ModuleTextFilter;
			discountName.IsActive = true;
			discountName.Property = "SPECIAL";

			var discountPercent = filters["STL Setting Discount Percent"] as ModuleNumberRangeFilter;
			discountPercent.IsActive = true;
			discountPercent.Property1 = 8;
			discountPercent.Property2 = 11;

			var buyingGroup = filters["STL Setting Buying Group Name"] as ModuleTextFilter;
			buyingGroup.IsActive = true;
			buyingGroup.Property = "Buy001";

			var commitmentUnits = filters["STL Setting Commitment Units"] as ModuleNumberRangeFilter;
			commitmentUnits.IsActive = true;
			commitmentUnits.Property1 = 10;
			commitmentUnits.Property2 = 15;

			var priceCode = filters["STL Setting Price Code"] as ModuleTextFilter;
			priceCode.IsActive = true;
			priceCode.Property = "#P1";

			var price = filters["STL Setting Price"] as ModuleNumberRangeFilter;
			price.IsActive = true;
			price.Property1 = 40;
			price.Property2 = 48;

			var comment = filters["STL Setting Comment"] as ModuleTextFilter;
			comment.IsActive = true;
			comment.Property = "Comment002";

			var hfv = filters["STL Setting High Volume Feature"] as ModuleTextFilter;
			hfv.IsActive = true;
			hfv.Property = "HV2";

			var min = filters["STL Setting Min. Spend Feature"] as ModuleTextFilter;
			min.IsActive = true;
			min.Property = "#P3";

			var query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, discountName, discountPercent, comment });
			AssertEquals(settingDIS.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);

			query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, buyingGroup });
			AssertEquals(settingBUY.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);

			query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, commitmentUnits });
			AssertEquals(settingCOM.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);

			query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, priceCode, price });
			AssertEquals(settingPRI.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);

			query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, hfv });
			AssertEquals(settingHVF2.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);

			query = filters.GetFilterQuery(new ModuleFilter[] { validFrom, validTo, min });
			AssertEquals(settingMIN.PK, Factory.Load<EdiLicenceSetting>(query).Single().PK);
		}

		public void TestSTLDiscountFilterLimits_AboveMaxValue()
		{
			var filters = new ModuleFilterCollection();
			new StlPriceListFilters().AddPriceListFilters(filters, null, null);
			var discountPercent = filters["STL Setting Discount Percent"] as ModuleNumberRangeFilter;

			discountPercent.IsActive = true;
			discountPercent.Property1 = 1000;
			discountPercent.Property2 = 1000;

			AssertHasError("From value can't be above 100", discountPercent.Property1Info, "Please enter a value less than or equal to 999.99.");
			AssertHasError("To value can't be above 100", discountPercent.Property2Info, "Please enter a value less than or equal to 999.99.");

			discountPercent.Property1 = 10;
			discountPercent.Property2 = 30;

			AssertNoError("From Value has no validation message correctly", discountPercent.Property1Info, "Please enter a value less than or equal to 999.99.");
			AssertNoError("To Value has no validation message correctly", discountPercent.Property2Info, "Please enter a value less than or equal to 999.99.");
		}

		public void TestSTLDiscountFilterLimits_BelowMinValue()
		{
			var filters = new ModuleFilterCollection();
			new StlPriceListFilters().AddPriceListFilters(filters, null, null);
			var discountPercent = filters["STL Setting Discount Percent"] as ModuleNumberRangeFilter;

			discountPercent.IsActive = true;
			discountPercent.Property1 = -1000;
			discountPercent.Property2 = -1000;

			AssertHasError("From value can't be below zero", discountPercent.Property1Info, "Please enter a value greater than or equal to -999.99.");
			AssertHasError("To value can't be below zero", discountPercent.Property2Info, "Please enter a value greater than or equal to -999.99.");

			discountPercent.Property1 = 10;
			discountPercent.Property2 = 30;

			AssertNoError("From Value has no lower limit validation message", discountPercent.Property1Info, "Please enter a value greater than or equal to -999.99.");
			AssertNoError("To Value has no lower limit validation message", discountPercent.Property2Info, "Please enter a value greater than or equal to -999.99.");
		}

		public void TestSTLDiscountFilterLimits_InValidRange()
		{
			var filters = new ModuleFilterCollection();
			new StlPriceListFilters().AddPriceListFilters(filters, null, null);
			var discountPercent = filters["STL Setting Discount Percent"] as ModuleNumberRangeFilter;

			discountPercent.IsActive = true;
			discountPercent.Property1 = 10;
			discountPercent.Property2 = 30;

			AssertNoErrors("From value is valid", discountPercent.Property1Info);
			AssertNoErrors("To value is valid", discountPercent.Property2Info);
		}
	}
}
