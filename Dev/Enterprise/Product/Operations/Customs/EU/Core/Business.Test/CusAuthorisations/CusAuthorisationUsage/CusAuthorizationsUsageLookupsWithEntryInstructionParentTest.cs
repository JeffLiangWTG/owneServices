namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class CusAuthorizationsUsageLookupsWithEntryInstructionParentTest : CusAuthorizationsUsageLookupsAbstractTest<CusAuthorizationUsageLookups>
	{
		protected override CusAuthorizationUsageLookups GetLookups() => jobDeclaration.CustomsEntryInstructions.AddNew().CusAuthorizationUsages.AddNew().Lookups;
	}
}
