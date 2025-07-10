using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Business.Testing
{
	sealed class CusLineTariffDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var cusLineTariffDetail2 = invoiceLine.CusLineTariffDetails.AddNew();
			var list = lookups.TariffTypeList;
			AssertSame("TariffTypeList", cusLineTariffDetail2.Lookups.TariffTypeList, list);
		}

		public void TestTariffList_WhenBZ_TypeIsSelected()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X2", "X203", "Cigarettes containing tobaco (a)", "402.32 * [MIL] + 0.0873 + [RSP]", "MIL", "RSP", "2402209000");
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X1", "X110", "Liquefied Petroleum Gas - used as propellant", "63.95 * [KLT]", "VCT", null, "1234567890");

			lookups.Parent.BZ_Type = "X2";
			var list = lookups.TariffList;
			list.Load();

			CombineAssertions(() =>
			{
				AssertType<ChildTariffViewCollection>(list);
				AssertEquals(1, list.Count);
				AssertEquals("X203", list[0].ZZ1_TariffCode);
				AssertSame("Cached", list, lookups.TariffList);
			});
		}

		public void TestTariffList_WhenBZ_TypeIsEmpty()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X2", "X203", "Cigarettes containing tobaco (a)", "402.32 * [MIL] + 0.0873 + [RSP]", "MIL", "RSP", "2402209000");
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "X1", "X110", "Liquefied Petroleum Gas - used as propellant", "63.95 * [KLT]", "VCT", null, "1234567890");
			CreateTariffData(helper, Core.Constants.CountryCodes.Ireland, "F1", "F110", "I shouldn't appear in the collection!", "3.9 * [KLT]", "VCT", null, "951258963");

			var list = lookups.TariffList;
			list.Load();

			CombineAssertions(() =>
			{
				AssertType<IEChildTariffViewCollection>(list);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "X203", "X110" }, list.Cast<TariffView>().Select(x => x.ZZ1_TariffCode));
				AssertSame("Cached", list, lookups.TariffList);
			});
		}

		void CreateTariffData(UniversalReferenceTestDataHelper helper, string countryCode, string tariffTypeCode, string tariffCode, string description, string rateFormula, string rateUOM1, string rateUOM2, string tariffRelationshipCode)
		{
			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(countryCode, "Ireland", euGrouping);
			var tariffType = helper.CreateNewOrGetExistingTariffType(countryCode, tariffTypeCode);
			var tariff = helper.LoadOrCreateNewTariff(countryCode, tariffType.PK, tariffCode, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, description);
			var rateType = helper.CreateCusRateType(countryCode, tariffTypeCode, description: $"{description} Tax");
			var rateCode = helper.CreateCusRateCode(Factory, "EXC", rateType.PK, description: "Excise", countryCode: countryCode);
			var rate = helper.CreateRate(tariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, rateFormula, dataGrouping: countryCode);
			helper.CreateRateUOM(rate.PK, rateUOM1);
			if (rateUOM2 != null)
			{
				helper.CreateRateUOM(rate.PK, rateUOM2);
			}
			helper.CreateTariffRelationship(tariff.PK, tariffType.PK, tariffRelationshipCode);
		}

		public void TestQuantityUnitList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var euGrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland", euGrouping);

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "Customs Declaration Units of Quantity");
			helper.CreateCusCodeList(Core.Constants.CountryCodes.EuropeanUnion, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UM1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Ireland, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, "UM2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			Factory.Save();

			var list = lookups.QuantityUnitList;
			CombineAssertions(() =>
			{
				AssertType<CodeDescriptionPairList>(list);
				AssertContainsExactElementsInAnyOrder(new ZString[] { "UM1", "UM2" }, list.Cast<ICodeDescription>().OrderBy(x => x.Code).Select(x => x.Code));
			});
		}

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			var cusLineTariffDetail = invoiceLine.CusLineTariffDetails.AddNew();
			lookups = new CusLineTariffDetailLookups(cusLineTariffDetail);
		}
		JobComInvoiceLine invoiceLine;
		CusLineTariffDetailLookups lookups;
	}
}
