using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public sealed class DeltaGJobComInvoiceHeaderValueSetStrategyTest : TestCaseWithFactory
	{
		public void TestValueSet()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var invoice = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var valueSetStrategy = new DeltaGJobComInvoiceHeaderValueSetStrategy(invoice);

			invoice.ShouldClearIncoTermPlacesIfNeeded = false;
			valueSetStrategy.ValueSet(invoice.JZ_IncoTermInfo, invoice.JZ_IncoTerm);
			AssertEquals("ShouldClearIncoTermPlacesIfNeeded should be true", true, invoice.ShouldClearIncoTermPlacesIfNeeded);
		}
	}
}
