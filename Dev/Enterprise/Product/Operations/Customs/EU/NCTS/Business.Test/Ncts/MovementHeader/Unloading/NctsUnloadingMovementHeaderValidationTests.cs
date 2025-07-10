namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class NctsUnloadingMovementHeaderValidationTests : NctsCommonMovementHeaderValidationAbstractTest<INctsDepartureMovementHeaderPhase5ValidationDecider>
	{
		protected override NctsCommonMovementHeader GetMovementHeaderForTest()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			return header.UnloadingMovementHeader;
		}
	}
}
