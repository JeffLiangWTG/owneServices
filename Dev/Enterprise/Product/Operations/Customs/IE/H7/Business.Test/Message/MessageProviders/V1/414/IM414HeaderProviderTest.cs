using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.H7.Business.Test;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM414HeaderProviderTest : DataProviderTestCase<IM414HeaderProvider>
	{
		public void TestDeclaration()
		{
			CombineAssertions("Declaration", () =>
			{
				AssertEquals("MRN", "TestMRN", Provider.Declaration.MRN);
				AssertEquals("CustomsOfficeLodgement", "TestOffice", Provider.Declaration.CustomsOffices.CustomsOfficeLodgement);
			});
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("CustomsOfficeLodgement", "TestOffice", Provider.CustomsOffices.CustomsOfficeLodgement);
		}

		public void TestPresentationCustomsOffice()
		{
			AssertEquals("PresentationCustomsOffice", "Office", Provider.CustomsOffices.PresentationCustomsOffice);
		}

		public void TestCustomsOffices()
		{
			CombineAssertions("CustomsOffices", () =>
			{
				AssertEquals("CustomsOfficeLodgement", "TestOffice", Provider.CustomsOffices.CustomsOfficeLodgement);
				AssertEquals("PresentationCustomsOffice", "Office", Provider.CustomsOffices.PresentationCustomsOffice);
			});
		}

		public void TestParties()
		{
			CombineAssertions("Declarant", () =>
			{
				AssertEquals("Name", "declarant", Provider.Parties.Declarant.Name);
				AssertEquals("StreetAndNumber", "declarantAddress1, declarantAddress2", Provider.Parties.Declarant.Address.StreetAndNumber);
				AssertEquals("Postcode", "233333", Provider.Parties.Declarant.Address.Postcode);
				AssertEquals("City", "declarantCity", Provider.Parties.Declarant.Address.City);
				AssertEquals("Country", "AU", Provider.Parties.Declarant.Address.Country);
			});

			CombineAssertions("Reprentative", () =>
			{
				AssertEquals("Status", "2", Provider.Parties.Representative.Status);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageSendingObject = MessageDataProviderTestHelper.SetUpMessageSendingObject(Factory);
			messageSendingObject.AmendmentInvalidationReason = "TestInvalidReason";
		}

		protected override IM414HeaderProvider GetProvider()
		{
			return new IM414HeaderProvider(messageSendingObject);
		}

		MessageSendingObject messageSendingObject;
	}
}
