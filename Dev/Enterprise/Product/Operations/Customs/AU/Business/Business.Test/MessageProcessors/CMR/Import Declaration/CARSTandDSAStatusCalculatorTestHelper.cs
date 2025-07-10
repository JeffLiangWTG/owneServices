namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CARSTandDSAStatusCalculatorTestHelper : CARSTandDSAStatusCalculator
	{
		public CARSTandDSAStatusCalculatorTestHelper(PackingGroup pivot)
			: base(pivot)
		{
		}

		public void DeriveStatusForTesting()
		{
			DeriveStatus();
		}
	}
}
