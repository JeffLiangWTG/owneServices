namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DiscountTest : Common.Testing.DiscountTest
	{
		protected override string GetCountryContext() => JobDeclaration.AUEdifice;
	}
}
