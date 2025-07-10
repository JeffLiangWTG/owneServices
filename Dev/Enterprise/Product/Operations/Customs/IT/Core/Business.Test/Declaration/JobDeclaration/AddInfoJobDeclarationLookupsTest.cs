using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

abstract class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
{
	public override void TestCommunityTransitStatusListExport()
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
		AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", "T2L, T2LF, T2LSM", Declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
	}

	public override void TestCommunityTransitStatusListImport()
	{
		Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
		AssertEquals("lookups.CommunityTransitStatusIDList.CodesAsString", ZString.Empty, Declaration.AddInfoLookups.CommunityTransitStatusIDList.CodesAsString);
	}

	public override void TestSpecificCircumstanceIndicatorList()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType("C296E", "Specific Circumstance Indicator", "IT");
		helper.CreateCusCodeList("IT", "C296E", "A10", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
		helper.CreateCusCodeList("IT", "C296E", "B20", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
		helper.CreateCusCodeList("IT", "C296E", "C30", ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1));
		Factory.Save();

		var declaration = Factory.New<JobDeclaration>();
		var lookups = declaration.AddInfoLookups;

		declaration.JE_MessageType = "IMP";
		AssertEquals("When declaration is IMP, SpecificCircumstanceIndicatorList", "E, A, D, C, B", lookups.SpecificCircumstanceIndicatorList.CodesAsString);

		declaration.JE_MessageType = "EXP";
		declaration.MessageVersion = "TXT";
		AssertEquals("When declaration is EXP TXT, SpecificCircumstanceIndicatorList", "E, A, D, C, B", lookups.SpecificCircumstanceIndicatorList.CodesAsString);

		declaration.MessageVersion = "XML";
		var specificCircumstanceIndicatorList = lookups.SpecificCircumstanceIndicatorList;
		AssertEquals("When declaration is EXP XML, SpecificCircumstanceIndicatorList", "A10, B20, C30", specificCircumstanceIndicatorList.CodesAsString);
		AssertSame("SpecificCircumstanceIndicatorList cached", specificCircumstanceIndicatorList, lookups.SpecificCircumstanceIndicatorList);
	}

	public void TestBorderTransportMeansList_ExportUCC6()
	{
		Declaration.JE_MessageType = "EXP";
		Declaration.MessageVersion = "XML";
		var lookups = Declaration.AddInfoLookups;
		CombineAssertions("For EXP UCC6", () =>
		{
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: string.Empty, expectedBorderMeansOfTransportCodes: "10, 11, 21, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "AIR", expectedBorderMeansOfTransportCodes: "40, 41", expectedDefaultCode: "40");
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "FIX", expectedBorderMeansOfTransportCodes: "10, 11, 21, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "IWT", expectedBorderMeansOfTransportCodes: "10, 11, 80, 81", expectedDefaultCode: "81");
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "OWN", expectedBorderMeansOfTransportCodes: "10, 11, 21, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "MAI", expectedBorderMeansOfTransportCodes: "10, 11, 21, 30, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "RAI", expectedBorderMeansOfTransportCodes: "21", expectedDefaultCode: "21");
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "ROA", expectedBorderMeansOfTransportCodes: "30", expectedDefaultCode: "30");
			AssertBorderMeansOfTransportListIsCorrectForDecTransport(transportMode: "SEA", expectedBorderMeansOfTransportCodes: "10, 11, 80, 81", expectedDefaultCode: "11");
			AssertSame("BorderTransportMeansList cache", lookups.BorderTransportMeansList, lookups.BorderTransportMeansList);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		Declaration = Factory.New<JobDeclaration>();
	}

	protected JobDeclaration Declaration { get; private set; }

	#region Implementation

	void AssertBorderMeansOfTransportListIsCorrectForDecTransport(string transportMode, string expectedBorderMeansOfTransportCodes, string expectedDefaultCode)
	{
		Declaration.JE_TransportMode = transportMode;
		AssertEquals($"For {transportMode}, CodesAsString", expectedBorderMeansOfTransportCodes, Declaration.AddInfoLookups.BorderTransportMeansList.CodesAsString);
		AssertEquals($"For {transportMode}, DefaultCode", expectedDefaultCode, Declaration.AddInfoLookups.BorderTransportMeansList.DefaultCode);
	}

	#endregion

}
