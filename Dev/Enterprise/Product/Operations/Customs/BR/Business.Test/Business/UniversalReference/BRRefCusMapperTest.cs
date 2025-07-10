using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class BRRefCusMapperTest : TestCaseWithFactory
	{
		public void TestMapCW1CurrencyCodeToCustomsCode()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			AssertEquals("USD Code should be", "220", BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Factory, "USD"));
			AssertEquals("BRL Code should be", "790", BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Factory, "BRL"));
			AssertEquals("EUR Code should be", ZString.Empty, BRRefCusMapper.MapCW1CurrencyCodeToCustomsCode(Factory, "EUR"));
		}

		public void TestMapCW1CountryCodeToCustomsCode()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("US", "220"),
				new KeyValuePair<string, string>("BR", "790"),
				new KeyValuePair<string, string>("CH", "87")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			AssertEquals("US Code should be", "220", BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Factory, "US"));
			AssertEquals("BR Code should be", "790", BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Factory, "BR"));
			AssertEquals("CH Code should be", "087", BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Factory, "CH"));
			AssertEquals("DE Code should be", ZString.Empty, BRRefCusMapper.MapCW1CountryCodeToCustomsCode(Factory, "DE"));
		}

		public void TestMapCustomsCodeCountryToCW1Code()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("US", "220"),
				new KeyValuePair<string, string>("BR", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Country, "Country Codes Mapping", codes);

			AssertEquals("220 Code should be", "US", BRRefCusMapper.MapCustomsCodeCountryToCW1Code(Factory, "220"));
			AssertEquals("790 Code should be", "BR", BRRefCusMapper.MapCustomsCodeCountryToCW1Code(Factory, "790"));
			AssertEquals("000 Code should be", ZString.Empty, BRRefCusMapper.MapCustomsCodeCountryToCW1Code(Factory, "000"));
		}

		public void TestMapCW1ModalTransportCodeToCustomsCode()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("SEA", "01"),
				new KeyValuePair<string, string>("ROA", "07")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, Constants.RefCusMapType.ModalTransport, "Modal Transport Mapping", codes);

			AssertEquals("SEA Code should be", "01", BRRefCusMapper.MapCW1ModalTransportCodeToCustomsCode(Factory, "SEA"));
			AssertEquals("ROA Code should be", "07", BRRefCusMapper.MapCW1ModalTransportCodeToCustomsCode(Factory, "ROA"));
			AssertEquals("AIR Code should be", ZString.Empty, BRRefCusMapper.MapCW1ModalTransportCodeToCustomsCode(Factory, "AIR"));
		}

		public void TestMapCustomsCodeCurrencyToCW1Code()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("USD", "220"),
				new KeyValuePair<string, string>("BRL", "790")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.Currency, "Currency Codes Mapping", codes);

			AssertEquals("220 Code should be", "USD", BRRefCusMapper.MapCustomsCodeCurrencyToCW1Code(Factory, "220"));
			AssertEquals("790 Code should be", "BRL", BRRefCusMapper.MapCustomsCodeCurrencyToCW1Code(Factory, "790"));
			AssertEquals("000 Code should be", ZString.Empty, BRRefCusMapper.MapCustomsCodeCurrencyToCW1Code(Factory, "000"));
		}

		public void TestGetTaxRevenueCodeMapping()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("DTY", "0086"),
				new KeyValuePair<string, string>("IPI", "1038"),
				new KeyValuePair<string, string>("F1ND", "5149"),
				new KeyValuePair<string, string>("F1D5", "2185"),
				new KeyValuePair<string, string>("SUF", "7811"),
				new KeyValuePair<string, string>("COF", "5629"),
				new KeyValuePair<string, string>("PIS", "5602")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.RCODE, "Tax Revenue Code", codes);

			var mappings = BRRefCusMapper.GetTaxRevenueCodeMapping(Factory).OrderBy(x => x.Key);
			AssertContainsExactElementsInExactOrder(new[] { "COF", "DTY", "F1D5", "F1ND", "IPI", "PIS", "SUF" }, mappings.Select(x => x.Key).ToArray());
			AssertContainsExactElementsInExactOrder(new[] { "5629", "0086", "2185", "5149", "1038", "5602", "7811" }, mappings.Select(x => x.Value).ToArray());
		}

		public void TestMapCW1RateTypeToCustomsCode()
		{
			var codes = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("DTY", "II"),
				new KeyValuePair<string, string>("IPI", "IPI"),
				new KeyValuePair<string, string>("ADD", "ANTIDUMPING"),
				new KeyValuePair<string, string>("COF", "COFINS"),
				new KeyValuePair<string, string>("PIS", "PIS")
			};
			ReferenceTestDataHelper.CreateRefCusMap(Factory, RefCusMapTypeList.Codes.RateType, "BR Rate Types", codes);

			AssertEquals("DTY Code should be ", "II", BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "DTY"));
			AssertEquals("IPI Code should be ", "IPI", BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "IPI"));
			AssertEquals("ADD Code should be ", "ANTIDUMPING", BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "ADD"));
			AssertEquals("COF Code should be ", "COFINS", BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "COF"));
			AssertEquals("PIS Code should be ", "PIS", BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "PIS"));
			AssertEquals("SUF Code should be ", ZString.Empty, BRRefCusMapper.MapCW1RateTypeToCustomsCode(Factory, "SUF"));
		}
	}
}
