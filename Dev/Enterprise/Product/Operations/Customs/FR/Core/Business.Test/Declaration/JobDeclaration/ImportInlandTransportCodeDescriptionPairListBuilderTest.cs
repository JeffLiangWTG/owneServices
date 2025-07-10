using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class ImportInlandTransportCodeDescriptionPairListBuilderTest : EU.Business.Declaration.Testing.InlandTransportCodeDescriptionPairListBuilderForImportTest
	{
		protected override string ExpectedDefaultCode_IWT => MeansOfTransportList.Codes.EuropeanVesselIdentificationNumberEniCode;

		protected override string ExpectedDefaultCode_SEA => MeansOfTransportList.Codes.ImoShipIdentificationNumber;

		protected override EU.Business.Declaration.InlandTransportCodeDescriptionPairListBuilder GetNewBuilder(EU.Business.Declaration.JobDeclaration declaration)
			=> new InlandTransportCodeDescriptionPairListBuilder(declaration);
	}
}
