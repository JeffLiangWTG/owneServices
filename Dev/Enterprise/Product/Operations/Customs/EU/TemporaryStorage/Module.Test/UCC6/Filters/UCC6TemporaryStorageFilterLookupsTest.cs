using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.TemporaryStorage.Module;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.Module.Testing
{
	sealed class UCC6TemporaryStorageFilterLookupsTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When FilterStripBusinessObject is null", () => new UCC6TemporaryStorageFilterLookups(null));

			FilterStripBusinessObject filterBizObject = new UCC6TemporaryStorageFilterStripBusinessObject();
			AssertNoExceptionThrown("When FilterStripBusinessObject is present", () => new UCC6TemporaryStorageFilterLookups(filterBizObject));
		}

		public void TestSupportingDocumentsType()
		{
			AssertDocumentsType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentsTemporaryStorageUCC6, (UCC6TemporaryStorageFilterLookups lookups) => lookups.SupportingDocumentsType);
		}

		public void TestPreviousDocumentsType()
		{
			AssertDocumentsType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, (UCC6TemporaryStorageFilterLookups lookups) => lookups.PreviousDocumentsType);
		}

		void AssertDocumentsType(ZString codeType, Func<UCC6TemporaryStorageFilterLookups, ZZRefCusCodeListCombinedCollection> getCodeList)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			_ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "9001", "9001 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, codeType, "9002", "9002 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, "9003", "9003 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			_ = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, codeType, "9004", "9004 DES", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			CombineAssertions(() =>
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
				{
					AssertContainsExactElementsInAnyOrder("LV List", new ZString[] { "9001", "9002", "9003" }, GetCodeList().Select(x => x.ZZD_Code).ToArray());
				}

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
				{
					AssertContainsExactElementsInAnyOrder("DE List", new ZString[] { "9001", "9002", "9004" }, GetCodeList().Select(x => x.ZZD_Code).ToArray());
				}
			});

			ZZRefCusCodeListCombinedCollection GetCodeList()
			{
				var lookups = new UCC6TemporaryStorageFilterLookups(new UCC6TemporaryStorageFilterStripBusinessObject());
				var codeList = getCodeList(lookups);
				codeList.Load();
				return codeList;
			}
		}
	}
}
