using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoJobComInvoiceLineLookups))]
	sealed class AddInfoJobComInvoiceLineLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryOfSupply()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Ireland, "Ireland");
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "Country");
			var auCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "AU", "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var cnCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "CN", "CN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var ieCode = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland, UniversalReferenceConstants.RefCusCodeListTypes.Codes.Country, "IE", "IE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var list = (ZZRefCusCodeListCombinedCollection)invoiceLine.AddInfoLookups.CountryOfSupplyList;
			AssertCodeList();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertCodeList();

			void AssertCodeList()
			{
				CombineAssertions($"JE_MessageType: {declaration.JE_MessageType}", () =>
				{
					AssertSame("Cached", list, invoiceLine.AddInfoLookups.CountryOfSupplyList);
					var filter = list.CompleteFilter;
					AssertEquals("AU", true, Factory.Load<ZZRefCusCodeListCombined>(auCode.PK).MatchesFilter(filter));
					AssertEquals("CN", true, Factory.Load<ZZRefCusCodeListCombined>(cnCode.PK).MatchesFilter(filter));
					AssertEquals("IE", true, Factory.Load<ZZRefCusCodeListCombined>(ieCode.PK).MatchesFilter(filter));
				});
			}
		}
	}
}
