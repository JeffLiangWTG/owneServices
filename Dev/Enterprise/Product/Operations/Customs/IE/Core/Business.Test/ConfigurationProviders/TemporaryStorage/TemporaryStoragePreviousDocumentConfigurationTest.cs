using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStoragePreviousDocumentConfiguration))]
	sealed class TemporaryStoragePreviousDocumentConfigurationTest : TemporaryStoragePreviousDocumentConfigurationAbstractTest<TemporaryStoragePreviousDocumentConfiguration>
	{
		public override void TestGetCodeList()
		{
			RefCusCodeList codeLists1, codeLists2, codeLists3, codeLists4, codeLists5;
			GenerateCodeList(out codeLists1, out codeLists2, out codeLists3, out codeLists4, out codeLists5);
			var previousDocument = Factory.NewWithValidTestData<TemporaryStorageHeader>().PreviousDocuments.AddNew();
			var codeList = configuration.GetCodeList(previousDocument) as ZZRefCusCodeListCombinedCollection;
			var completeFilter = codeList.CompleteFilter;
			AssertEquals("Matched Purpose: Presentation", true, Factory.Load<ZZRefCusCodeListCombined>(codeLists1.PK).MatchesFilter(completeFilter));
			AssertEquals("Matched Purpose: Presentation", true, Factory.Load<ZZRefCusCodeListCombined>(codeLists2.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched Purpose: Presentation", false, Factory.Load<ZZRefCusCodeListCombined>(codeLists3.PK).MatchesFilter(completeFilter));
			AssertEquals("Matched Purpose: Presentation", true, Factory.Load<ZZRefCusCodeListCombined>(codeLists4.PK).MatchesFilter(completeFilter));
			AssertEquals("Unmatched Purpose: Presentation", false, Factory.Load<ZZRefCusCodeListCombined>(codeLists5.PK).MatchesFilter(completeFilter));
			AssertSame("Cached", codeList, (ZZRefCusCodeListCombinedCollection)previousDocument.Lookups.CodeList);
		}

		void GenerateCodeList(out RefCusCodeList codeLists1, out RefCusCodeList codeLists2, out RefCusCodeList codeLists3, out RefCusCodeList codeLists4, out RefCusCodeList codeLists5)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection,
				"Previous Documents Imports (BOX40)");
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment,
				"Transport Charges Method Of Payment");

			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			codeLists1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "A", "Test 1",
				yesterday, tomorrow);

			codeLists2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "B", "Test 1",
				yesterday, tomorrow);

			codeLists3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "C", "Test 1",
				yesterday, tomorrow);

			codeLists4 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfImportDirection, "D", "Test 1",
				yesterday, tomorrow);
			codeLists5 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Ireland,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "E", "Test 2",
				yesterday, tomorrow);
			Factory.Save();
		}
	}
}
