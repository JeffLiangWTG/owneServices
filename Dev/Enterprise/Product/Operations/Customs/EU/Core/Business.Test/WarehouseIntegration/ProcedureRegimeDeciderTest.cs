using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class ProcedureRegimeDeciderTest : TestCaseWithFactory
	{
		public void TestIsIntoRegime()
		{
			var procedure = Factory.New<RefCusProcedure>();
			var decider = new ProcedureRegimeDecider();
			AssertEquals("Precondition: returns false by default", false, decider.IsIntoRegime(procedure));

			procedure.ZZ6_IntoVATWarehouse = "Y";
			AssertEquals("Should return true when ZZ6_IntoVATWarehouse is Y", true, decider.IsIntoRegime(procedure));
		}
	}
}
