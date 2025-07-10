using CargoWise.EntityFramework.Testing;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	internal class ClientIncidentQuoteLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTerms()
		{
			var quote = Factory.New<ClientIncidentQuote>();
			var lookups = new ClientIncidentQuoteLookups(quote);
			var list = lookups.PaymentTerms;

			Assert("Precondition", quote.CIQ_Type.IsEmpty);
			Assert("No MonthlyTerm", !list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm));
			Assert("No OneOffTerm", !list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm));

			quote.CIQ_Type = PaymentTypesAndTermsList.PaymentTypes.Codes.MonthlyType;
			list = lookups.PaymentTerms;
			Assert("MonthlyTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.MonthlyTerm));

			quote.CIQ_Type = PaymentTypesAndTermsList.PaymentTypes.Codes.OneOffType;
			list = lookups.PaymentTerms;
			Assert("OneOffTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTerms.Codes.OneOffTerm));
		}

		public void TestPaymentTypes()
		{
			var lookups = new ClientIncidentQuoteLookups(Factory.New<ClientIncidentQuote>());
			var list = lookups.PaymentTypes;

			Assert("MonthlyTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTypes.Codes.MonthlyType));
			Assert("OneOffTerm", list.ContainsCode(PaymentTypesAndTermsList.PaymentTypes.Codes.OneOffType));
		}
	}
}