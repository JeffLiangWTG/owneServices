using Enterprise.Customs.EU.Business.Declaration.Testing;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

public class ImportInlandTransportCodeDescriptionPairListBuilderTest : InlandTransportCodeDescriptionPairListBuilderBaseTest<ImportInlandTransportCodeDescriptionPairListBuilder>
{
	protected override string ExpectedTransportCodeList_AIR => "40, 41";

	protected override string ExpectedTransportCodeList_FIX => "10, 11, 20, 21, 30, 31, 40, 41, 80, 81";

	protected override string ExpectedTransportCodeList_IWT => "80, 81";

	protected override string ExpectedTransportCodeList_OWN => "10, 11, 20, 21, 30, 31, 40, 41, 80, 81";

	protected override string ExpectedTransportCodeList_MAI => "10, 11, 20, 21, 30, 31, 40, 41, 80, 81";

	protected override string ExpectedTransportCodeList_RAI => "20, 21";

	protected override string ExpectedTransportCodeList_ROA => "30, 31";

	protected override string ExpectedTransportCodeList_SEA => "10, 11";

	protected override string ExpectedTransportCodeList_Default => "10, 11, 20, 21, 30, 31, 40, 41, 80, 81";

	protected override ImportInlandTransportCodeDescriptionPairListBuilder GetNewBuilder(EU.Business.Declaration.JobDeclaration declaration)
	{
		return new ImportInlandTransportCodeDescriptionPairListBuilder(declaration);
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
	}
}
