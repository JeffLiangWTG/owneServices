using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.Business.Testing;

[TestedType(typeof(TemporaryStoragePreviousDocumentConfiguration))]
sealed class TemporaryStoragePreviousDocumentConfigurationTest : EU.Business.CusTempStorage.Testing.TemporaryStoragePreviousDocumentConfigurationAbstractTest<TemporaryStoragePreviousDocumentConfiguration>
{
	public override void TestGetCodeList()
	{
		var (itDC40TCodeList, itTCMOPCodeList, eunDC40TCodeList, expiredITDC40TCodeList) = GenerateCodeList();
		var previousDocument = Factory.New<TemporaryStorageHeader>().PreviousDocuments.AddNew();
		var codeList = (ZZRefCusCodeListCombinedCollection)configuration.GetCodeList(previousDocument);
		var completeFilter = codeList.CompleteFilter;
		AssertCodeListMatchesFilter("Matched: IT DC40T", true, itDC40TCodeList.PK);
		AssertCodeListMatchesFilter("Mismatched: IT TCMOP", false, itTCMOPCodeList.PK);
		AssertCodeListMatchesFilter("Mismatched: EUN DC40T", false, eunDC40TCodeList.PK);
		AssertCodeListMatchesFilter("Mismatched: Expired IT DC40T", false, expiredITDC40TCodeList.PK);
		AssertSame("Cached", codeList, previousDocument.Lookups.CodeList);

		void AssertCodeListMatchesFilter(string message, bool expected, ZGuid codeListPk)
			=> AssertEquals(message, expected, Factory.Load<ZZRefCusCodeListCombined>(codeListPk).MatchesFilter(completeFilter));
	}

	(RefCusCodeList itDC40TCodeList, RefCusCodeList itTCMOPCodeList, RefCusCodeList eunDC40TCodeList, RefCusCodeList expiredITDC40TCodeList) GenerateCodeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		_ = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "Document Type (EU Box 40 Temporary Storage)");
		_ = helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "Transport Charges Method Of Payment");

		var twoDaysAgo = ZDateTime.Today.AddDays(-2);
		var yesterday = ZDateTime.Today.AddDays(-1);
		var maxDate = ZDateTime.MaxSmallDateTimeValue;

		var itDC40TCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "235", "Elenco dei container",
			yesterday, maxDate);

		var itTCMOPCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EUTransportChargesMethodOfPayment, "A", "Pagamento in contanti",
			yesterday, maxDate);

		var eunDC40TCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "C620", "T2LF document",
			yesterday, maxDate);

		var expiredITDC40TCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PreviousDocumentOfPNTS, "270", "Bolle di consegna",
			twoDaysAgo, yesterday);

		Factory.Save();

		return (itDC40TCodeList, itTCMOPCodeList, eunDC40TCodeList, expiredITDC40TCodeList);
	}

	#region CollectionMaxCount

	protected override bool ExpectedIsCollectionMaxCountValidationEnabled => false;

	#endregion
}
