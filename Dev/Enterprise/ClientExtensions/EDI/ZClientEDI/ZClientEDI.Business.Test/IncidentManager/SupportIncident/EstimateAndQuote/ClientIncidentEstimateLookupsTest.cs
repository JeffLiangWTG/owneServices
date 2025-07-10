using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ClientIncidentEstimateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTerms()
		{
			var lookups = new ClientIncidentEstimateLookups(Factory.New<ClientIncidentEstimate>());
			var list = lookups.PaymentTerms;

			Assert("MonthlyTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm));
			Assert("OneOffTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm));
		}
	}
}