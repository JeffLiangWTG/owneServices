namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class IM413DeclarationProviderTest : IM413AndIM415DeclarationProviderTest<IM413DeclarationProvider>
	{
		protected override string ExpectedLRN => "LRN1231";

		public void TestMRN()
		{
			SetUpTestData();
			bill.MovementReferenceNumber = "MRN9987";
			AssertEquals("MRN9987", Provider.MRN);
		}

		public void TestDetailsAmended()
		{
			SetUpTestData();
			messageSendingObject.AmendmentInvalidationReason = "This is the amendment invalidation reason";
			AssertEquals("This is the amendment invalidation reason", Provider.DetailsAmended);
		}

		protected override IM413DeclarationProvider GetProvider()
		{
			SetUpTestData();
			return new IM413DeclarationProvider(messageSendingObject);
		}
	}
}
