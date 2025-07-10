using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	sealed class ExportAddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryOfDestinations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			var auCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cnCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var ieCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var list = (ZZRefCusCodeListCombinedCollection)invoiceLine.AddInfoLookups.CountriesOfDestination;

			CombineAssertions(() =>
			{
				AssertSame("Cached", list, invoiceLine.AddInfoLookups.CountriesOfDestination);
				var filter = list.CompleteFilter;
				AssertEquals("AU", true, Factory.Load<ZZRefCusCodeListCombined>(auCode.PK).MatchesFilter(filter));
				AssertEquals("CN", true, Factory.Load<ZZRefCusCodeListCombined>(cnCode.PK).MatchesFilter(filter));
				AssertEquals("IE", true, Factory.Load<ZZRefCusCodeListCombined>(ieCode.PK).MatchesFilter(filter));
			});
		}

		public void TestCountryOfDestinations_B1871_B1872()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");

			var code_value = UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL140;

			helper.CreateNewOrGetExistingCusCodeType(code_value, "CL140");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "QQ", "QQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AD", "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "SM", "SM", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "DE", "DE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IT", "IT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			code_value = UniversalReferenceConstants.RefCusCodeListTypes.Codes.CL063;
			helper.CreateNewOrGetExistingCusCodeType(code_value, "CL063");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "XX", "XX", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "YY", "YY", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, code_value, "AD", "AD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var list = invoiceLine.AddInfoLookups;
			var lookups = (ExportAddInfoJobComInvoiceLineLookups)invoiceLine.AddInfoLookups;

			CombineAssertions(() =>
			{
				var codeListCL140 = lookups.CountryOfDestinationCL140;
				Assert("CodeListCL140 AU", codeListCL140.ContainsCode("AU"));
				Assert("CodeListCL140 CN", codeListCL140.ContainsCode("CN"));
				Assert("CodeListCL140 IE", codeListCL140.ContainsCode("IE"));
				Assert("CodeListCL140 exclude QQ", !codeListCL140.ContainsCode("QQ"));
				Assert("CodeListCL140 include", codeListCL140.ContainsCode("AD"));
				Assert("CodeListCL140 include", codeListCL140.ContainsCode("SM"));
				Assert("CodeListCL140 include", codeListCL140.ContainsCode("DE"));
				Assert("CodeListCL140 include", codeListCL140.ContainsCode("IT"));

				var codeList063 = lookups.CountryOfDestinationCL063;
				Assert("CodeListCL063 XX", codeList063.ContainsCode("XX"));
				Assert("CodeListCL063 YY", codeList063.ContainsCode("YY"));
				Assert("CodeListCL063 exclude AD", !codeList063.ContainsCode("AD"));
			});
		}
	}
}
