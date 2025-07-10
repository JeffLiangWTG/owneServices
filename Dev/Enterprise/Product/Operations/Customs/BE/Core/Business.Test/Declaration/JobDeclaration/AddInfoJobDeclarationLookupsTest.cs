namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
{
	public override void TestSpecificCircumstanceIndicatorList()
	{
		var codeList = Lookups.SpecificCircumstanceIndicatorList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", codeList, Lookups.SpecificCircumstanceIndicatorList);
			AssertEquals("CodesAsString", "A20", codeList.CodesAsString);
		});
	}

	public void TestInlandTransportCodeList_ImportUCC6()
	{
		Declaration.JE_MessageType = "IMP";
		CombineAssertions("For IMP UCC6", () =>
		{
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: string.Empty, expectedInlandTransportCodes: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "AIR", expectedInlandTransportCodes: "40, 41", expectedDefaultCode: "40");
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "FIX", expectedInlandTransportCodes: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "IWT", expectedInlandTransportCodes: "80, 81", expectedDefaultCode: "81");
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "OWN", expectedInlandTransportCodes: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "MAI", expectedInlandTransportCodes: "10, 11, 20, 21, 30, 31, 40, 41, 80, 81", expectedDefaultCode: null);
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "RAI", expectedInlandTransportCodes: "20, 21", expectedDefaultCode: "20");
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "ROA", expectedInlandTransportCodes: "30, 31", expectedDefaultCode: "30");
			AssertInlandTransportCodeListIsCorrectForDecTransport(transportMode: "SEA", expectedInlandTransportCodes: "10, 11", expectedDefaultCode: "11");
			AssertSame("BorderTransportMeansList cache", Lookups.BorderTransportMeansList, Lookups.BorderTransportMeansList);
		});
	}

	void AssertInlandTransportCodeListIsCorrectForDecTransport(string transportMode, string expectedInlandTransportCodes, string expectedDefaultCode)
	{
		Declaration.JE_TransportModeInland = transportMode;
		AssertEquals($"For {transportMode}, CodesAsString", expectedInlandTransportCodes, Declaration.AddInfoLookups.InlandTransportCodeList.CodesAsString);
		AssertEquals($"For {transportMode}, DefaultCode", expectedDefaultCode, Declaration.AddInfoLookups.InlandTransportCodeList.DefaultCode);
	}

	public override void TestRegionOfDestinationList()
	{
		var list = Declaration.AddInfoLookups.RegionOfDestinationList;
		CombineAssertions(() =>
		{
			AssertEquals("Codes from list", new BERegionList().CodesAsString, list.CodesAsString);
			AssertSame("Cached", list, Declaration.AddInfoLookups.RegionOfDestinationList);
		});
	}

	JobDeclarationLookups Lookups => Declaration.Lookups;

	JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
	JobDeclaration declaration;
}
