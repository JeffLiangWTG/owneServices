using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.H7.Business.Test;
using NUnit.Framework;

namespace Enterprise.Customs.IE.H7.Business.Messaging.V1.Test
{
	sealed class IM414DeclarationProviderTest : DataProviderTestCase<IM414DeclarationProvider>
	{
		public void TestMRN()
		{
			AssertEquals("MRN", "TestMRN", Provider.MRN);
		}

		[TestDate(2024, 2, 20)]
		public void TestDateOfInvalidationRequest()
		{
			var a = Provider.DateOfInvalidationRequest;
			AssertEquals("DateOfInvalidationRequest", new ZDateTime(2024, 2, 20).ToDateTime().ToString("yyyyMMdd"), Provider.DateOfInvalidationRequest);
		}

		public void TestInvalidationReason()
		{
			AssertEquals("InvalidationReason", "TestInvalidReason", Provider.InvalidationReason);
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

		protected override IM414DeclarationProvider GetProvider()
		{
			return new IM414DeclarationProvider(messageSendingObject);
		}

		MessageSendingObject messageSendingObject;
	}
}
