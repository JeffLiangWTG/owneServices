using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

public class AddInfoJobDeclarationLookupsTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationLookupsTest
{
	public override void TestSpecificCircumstanceIndicatorList()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var codeList = declaration.AddInfoLookups.SpecificCircumstanceIndicatorList;
		CombineAssertions(() =>
		{
			AssertSame("Cached", codeList, declaration.AddInfoLookups.SpecificCircumstanceIndicatorList);
			AssertEquals("A20", codeList.CodesAsString);
		});
	}

	public void TestBorderTransportMeansListForSeaExport()
	{
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		declaration.JE_TransportMode = ModeOfTransportCodeList.Codes._SEA;
		var codeList = declaration.AddInfoLookups.BorderTransportMeansList;
		AssertEquals("11", codeList.DefaultCode);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
