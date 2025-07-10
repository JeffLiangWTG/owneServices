namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public class InlandTransportCodeDescriptionPairListBuilderForImportTest : InlandTransportCodeDescriptionPairListBuilderBaseTest<InlandTransportCodeDescriptionPairListBuilder>
	{
		protected override string ExpectedTransportCodeList_AIR => "40, 41";

		protected override string ExpectedTransportCodeList_FIX => "10, 11, 20, 30, 40, 41, 80, 81";

		protected override string ExpectedTransportCodeList_IWT => "80, 81";

		protected override string ExpectedTransportCodeList_OWN => "10, 11, 20, 30, 40, 41, 80, 81";

		protected override string ExpectedTransportCodeList_MAI => "10, 11, 20, 30, 40, 41, 80, 81";

		protected override string ExpectedTransportCodeList_RAI => "20, 21";

		protected override string ExpectedTransportCodeList_ROA => "30, 31";

		protected override string ExpectedTransportCodeList_SEA => "10, 11";

		protected override string ExpectedTransportCodeList_Default => "10, 11, 20, 30, 40, 41, 80, 81";

		protected override InlandTransportCodeDescriptionPairListBuilder GetNewBuilder(JobDeclaration declaration)
			=> new InlandTransportCodeDescriptionPairListBuilder(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
		}
	}
}
