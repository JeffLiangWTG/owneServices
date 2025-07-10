using Enterprise.Customs.CA.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class LVSDeclarationRatingAdaptersProviderTest : RatingAdaptersProviderTest
	{
		public void TestGetAdapters_ForCostWhenNoLegs_LogWarning()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.Invoices.RemoveAll();

			var interactor = new TestUIInteractor();
			var adaptersProvider = new LVSDeclarationRatingAdaptersProvider(declaration);
			var adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);

			AssertEquals("Adapters count", 0, adapters.Count);
			AssertCollectionContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No invoices found"), interactor.errors);

			interactor.errors.Clear();

			declaration.Invoices.AddNew();

			adaptersProvider = new LVSDeclarationRatingAdaptersProvider(declaration);
			adapters = adaptersProvider.GetAdapters(interactor, AutoRateOptions.AutorateCosts);

			AssertEquals("Adapters count", 1, adapters.Count);
			AssertEquals(AdapterType.LVSDeclaration, adapters[0].AdapterType);
			AssertEquals(declaration.JE_DeclarationReference, adapters[0].OperationalJobCode);
			AssertCollectionNotContains(RatingAdaptersProvider.LogMessages.RatingAdaptersCannotBeCreated("No invoices found"), interactor.errors);
		}
	}
}
