namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DeductionChargeTest : Common.Testing.DeductionChargeTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
