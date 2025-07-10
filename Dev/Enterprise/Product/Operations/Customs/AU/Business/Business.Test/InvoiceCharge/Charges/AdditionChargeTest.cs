namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AdditionChargeTest : Common.Testing.AdditionChargeTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
