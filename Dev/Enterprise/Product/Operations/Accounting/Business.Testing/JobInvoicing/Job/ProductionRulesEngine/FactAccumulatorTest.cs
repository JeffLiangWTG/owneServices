using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.JobInvoicing
{
	public class FactAccumulatorTest : TestCaseWithFactory
	{
		public void TestCreateUniqueOrganisationFact_MainAddressHasNoCountry()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.MainAddress.OA_RN_NKCountryCode = "";

			var factAccumulator = new FactAccumulator();

			AssertNoExceptionThrown("The result should be null due to the MainAddress has no country.", () => AssertNull(factAccumulator.CreateUniqueOrganisationFact(orgHeader)));
		}

		public void TestCreateUniqueUNLOCOFact_RefUNLOCOHasNoCountry()
		{
			var refUNLOCO = Factory.New<RefUNLOCO>();
			refUNLOCO.RL_RN_NKCountryCode = "";

			var factAccumulator = new FactAccumulator();

			AssertNoExceptionThrown("The result should be null due to the RefUNLOCO has no country.", () => AssertNull(factAccumulator.CreateUniqueUNLOCOFact(refUNLOCO)));
		}
	}
}
