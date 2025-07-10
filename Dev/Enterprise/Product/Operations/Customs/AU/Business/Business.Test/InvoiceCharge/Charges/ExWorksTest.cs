namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class ExWorksTest : Common.Testing.ExWorksTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
