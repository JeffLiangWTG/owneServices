using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.H7.Business.Test;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	public class IM413HeaderProviderTest : DataProviderTestCase<IM413HeaderProvider>
	{
		public void TestDeclaration()
		{
			AssertType<IM413DeclarationProvider>(Provider.Declaration);
			CombineAssertions("Declaration Contents", () =>
			{
				AssertEquals("Provider.Declaration.LRN", "TestLRN", Provider.Declaration.LRN);
				AssertEquals("Provider.Declaration.AdditionalDeclarationType", "A", Provider.Declaration.AdditionalDeclarationType);
				AssertEquals("Provider.Declaration.PreferredPaymentMethod", "E", Provider.Declaration.PreferredPaymentMethod);
				AssertEquals("Provider.Declaration.CustomsOfficeLodgement", "TestOffice", Provider.Declaration.CustomsOfficeLodgement);
				AssertEquals("Provider.Declaration.Parties.Declarant.Name", "declarant", Provider.Declaration.Parties.Declarant.Name);
				AssertEquals("Provider.Declaration.Parties.Representative.Id", "IE1234412", Provider.Declaration.Parties.Representative.Id);
				AssertEquals("Provider.Declaration.Parties.Representative.Status", "2", Provider.Declaration.Parties.Representative.Status);
				AssertEquals("Provider.Declaration.DeferredPayment", "12345", Provider.Declaration.DeferredPayment);
				AssertEquals("Provider.Declaration.MRN", "TestMRN", Provider.Declaration.MRN);
				AssertEquals("Provider.Declaration.DetailsAmended", "This is the amendment invalidation reason", Provider.Declaration.DetailsAmended);
			});
		}

		public void TestGoodsShipment()
		{
			AssertType<IM413AndIM415GoodsShipmentProvider>(Provider.GoodsShipment);
		}

		protected override IM413HeaderProvider GetProvider()
		{
			SetUpTestData();
			return new IM413HeaderProvider(messageSendingObject);
		}

		void SetUpTestData()
		{
			if (messageSendingObject == null)
			{
				messageSendingObject = MessageDataProviderTestHelper.SetUpMessageSendingObject(Factory);
				messageSendingObject.SubStyle = SubStyleCodeList.Codes.NormalDeclaration;
				messageSendingObject.AmendmentInvalidationReason = "This is the amendment invalidation reason";
			}
		}
		MessageSendingObject messageSendingObject;
	}
}
