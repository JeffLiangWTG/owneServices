using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.EU.ExitControl.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitSeal))]
	class CusExitSealTest : CusExitSealAbstractTest<CusExitSeal, CusExitContainer, CusExitHeader>
	{
		public void TestLookups()
		{
			var exitSeal = (CusExitSeal)GetNewBusinessObject();
			AssertType<CusExitSealUcc6Lookups>("Lookups Type", exitSeal.Lookups);
		}

		public void TestValidation()
		{
			var exitSeal = (CusExitSeal)GetNewBusinessObject();
			AssertType<CusExitSealValidation>("Validation Type", exitSeal.Validation);
		}
	}
}
