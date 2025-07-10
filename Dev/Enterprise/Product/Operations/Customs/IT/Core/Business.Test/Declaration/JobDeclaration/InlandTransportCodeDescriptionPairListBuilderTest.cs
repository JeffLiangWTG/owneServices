namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class InlandTransportCodeDescriptionPairListBuilderTest
{
	sealed class InlandTransportCodeDescriptionPairListBuilderForImportTest : EU.Business.Declaration.Testing.InlandTransportCodeDescriptionPairListBuilderForImportTest
	{
		protected override string ExpectedTransportCodeList_IWT => "10, 11, 80, 81";

		protected override string ExpectedTransportCodeList_SEA => "10, 11, 80, 81";

		protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetNewBuilder(EU.Business.Declaration.JobDeclaration declaration)
			=> new InlandTransportCodeDescriptionPairListBuilder(declaration);
	}

	sealed class InlandTransportCodeDescriptionPairListBuilderForExportTest : EU.Business.Declaration.Testing.InlandTransportCodeDescriptionPairListBuilderForExportTest
	{
		protected override string ExpectedTransportCodeList_IWT => "10, 11, 80, 81";

		protected override string ExpectedTransportCodeList_SEA => "10, 11, 80, 81";

		protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetNewBuilder(EU.Business.Declaration.JobDeclaration declaration)
			=> new InlandTransportCodeDescriptionPairListBuilder(declaration);
	}
}
