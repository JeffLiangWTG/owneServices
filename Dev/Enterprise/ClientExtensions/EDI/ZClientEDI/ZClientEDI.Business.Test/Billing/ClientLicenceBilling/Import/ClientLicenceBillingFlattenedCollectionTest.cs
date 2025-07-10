using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(ClientLicenceBillingFlattenedCollection))]
	public class ClientLicenceBillingFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ClientLicenceBillingFlattenedCollection>
	{
		protected override ClientLicenceBillingFlattenedCollection GetCollectionToTest()
		{
			return new ClientLicenceBillingFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ClientLicenceBillingFlattened();
		}

		public void TestCollectionIsIImportWizardProvider()
		{
			var collection = GetCollectionToTest();
			Assert(collection is IImportWizardProvider);
			AssertType<ClientLicenceBillingImportWizard>(((IImportWizardProvider)collection).GetImportWizard(new ImportCollectionInfoImpl(collection), null, null));
		}
	}
}
