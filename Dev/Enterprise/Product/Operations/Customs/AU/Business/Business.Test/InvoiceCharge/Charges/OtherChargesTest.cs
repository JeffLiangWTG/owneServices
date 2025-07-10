namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class OtherChargesTest : Common.Testing.OtherChargesTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
