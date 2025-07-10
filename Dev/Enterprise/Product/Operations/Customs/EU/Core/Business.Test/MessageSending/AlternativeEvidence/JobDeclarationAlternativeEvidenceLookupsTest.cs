using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing;

[TestedType(typeof(JobDeclarationAlternativeEvidenceLookups))]
sealed class JobDeclarationAlternativeEvidenceLookupsTest : BusinessObjectLookupsTestCase
{
	public void TestAlternativeEvidenceTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL170;
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, "3333", "description1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, "4444", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Iraq, codeType, "5555", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var lookups = CreateJobDeclarationAlternativeEvidence().Lookups;
		var codes = lookups.AlternativeEvidenceTypeList;
		CombineAssertions(() =>
		{
			AssertNotEquals("Not Contains 5555", true, codes.ContainsCode("5555"));
			AssertEquals("Contains 3333", true, codes.ContainsCode("3333"));
			AssertEquals("Contains 4444", true, codes.ContainsCode("4444"));
			AssertEquals("1 Description", "description1", codes.GetDescriptionFromCode("3333"));
			AssertEquals("2 Description", "description2", codes.GetDescriptionFromCode("4444"));
		});
	}

	public void TestTransportDocumentTypeList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		const string codeType = EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.ExportTransportDocument;
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, "3333", "description1", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Latvia, codeType, "4444", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Iraq, codeType, "5555", "description2", ZDateTime.Today.AddDays(-1), ZDateTime.Today.AddDays(1));
		Factory.Save();

		var lookups = CreateJobDeclarationAlternativeEvidence().Lookups;
		var codes = (CodeDescriptionPairList)lookups.TransportDocumentTypeList;
		CombineAssertions(() =>
		{
			AssertNotEquals("Not Contains 5555", true, codes.ContainsCode("5555"));
			AssertEquals("Contains 3333", true, codes.ContainsCode("3333"));
			AssertEquals("Contains 4444", true, codes.ContainsCode("4444"));
			AssertEquals("1 Description", "description1", codes.GetDescriptionFromCode("3333"));
			AssertEquals("2 Description", "description2", codes.GetDescriptionFromCode("4444"));
		});
	}

	JobDeclarationAlternativeEvidence CreateJobDeclarationAlternativeEvidence() => new(Factory);
}
