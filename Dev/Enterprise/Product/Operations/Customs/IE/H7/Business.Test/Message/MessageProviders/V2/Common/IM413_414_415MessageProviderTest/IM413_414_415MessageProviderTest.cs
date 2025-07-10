using System;
using System.Linq;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	public abstract class IM413_414_415MessageProviderTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : IM413_414_415HeaderProvider
	{
		public void TestDeclarationType()
		{
			AssertEquals("H7", Provider.DeclarationType);
		}

		public void TestAuthorisations()
		{
			AssertEquals(0, Provider.Authorisations.Count);
		}

		public void TestAuthorisation8F()
		{
			AssertNull(Provider.Authorisation8F);
		}

		public void TestCustomsOfficeOfPresentation()
		{
			AssertNull(Provider.CustomsOfficeOfPresentation);
		}

		public void TestSupervisingCustomsOffice()
		{
			AssertNull(Provider.SupervisingCustomsOffice);
		}

		public void TestCustomsOfficeLodgement()
		{
			AssertEquals("TestOffice", Provider.CustomsOfficeLodgement);
		}

		public void TestImporter()
		{
			CombineAssertions("Importer", () =>
			{
				AssertEquals("Id", "NR", Provider.Importer.Id);
				AssertEquals("Name", "Consignee", Provider.Importer.Name);
				AssertEquals("StreetAndNumber", "ConsigneeStreet1 ConsigneeStreet2", Provider.Importer.Address.StreetAndNumber);
				AssertEquals("Postcode", "CPostcode", Provider.Importer.Address.Postcode);
				AssertEquals("City", "ConsigneeCity", Provider.Importer.Address.City);
				AssertEquals("Country", "US", Provider.Importer.Address.Country);
			});
		}

		public void TestDeclarant()
		{
			CombineAssertions("Declarant", () =>
			{
				AssertEquals("Name", "declarant", Provider.Declarant.Name);
				AssertEquals("StreetAndNumber", "declarantAddress1, declarantAddress2", Provider.Declarant.Address.StreetAndNumber);
				AssertEquals("Postcode", "233333", Provider.Declarant.Address.Postcode);
				AssertEquals("City", "declarantCity", Provider.Declarant.Address.City);
				AssertEquals("Country", "AU", Provider.Declarant.Address.Country);
				AssertEquals("Contact Person Name", "declarantContactName", Provider.Declarant.ContactPerson.Name);
				AssertEquals("Contact Person Phone Number", "1234567", Provider.Declarant.ContactPerson.PhoneNumber);
				AssertEquals("Contact Person Email Address", "123@test.com", Provider.Declarant.ContactPerson.EmailAddress);
			});
		}

		public void TestPersonProvidingAGuaranteeID()
		{
			AssertNull(Provider.PersonProvidingAGuaranteeID);
		}

		public void TestPersonPayingCustomsDutyID()
		{
			AssertNull(Provider.PersonPayingCustomsDutyID);
		}

		public void TestRepresentative()
		{
			CombineAssertions("Representative", () =>
			{
				AssertEquals("Status", "2", Provider.Representative.Status);
				AssertEquals("Contact Person Name", "declarantContactName", Provider.Representative.ContactPerson.Name);
				AssertEquals("Contact Person Phone Number", "1234567", Provider.Representative.ContactPerson.PhoneNumber);
				AssertEquals("Contact Person Email Address", "123@test.com", Provider.Representative.ContactPerson.EmailAddress);
			});
		}

		public void TestGuarantees()
		{
			AssertEquals(0, Provider.Guarantees.Count);
		}

		public void TestCurrencyExchange()
		{
			AssertNull(Provider.CurrencyExchange);
		}

		public void TestDeferredPayments()
		{
			CombineAssertions(() =>
			{
				AssertEquals(1, Provider.DeferredPayments.Count);
				AssertEquals("12345", Provider.DeferredPayments.Single().DeferredPayment);
			});
		}

		public void TestGoodsShipments()
		{
			AssertNotNull(Provider.GoodsShipments);
			AssertEquals(1, Provider.GoodsShipments.Count);
			var goodsShipment = Provider.GoodsShipments.Single();
			AssertNotNull(goodsShipment);
			AssertNotNull(goodsShipment.GoodsShipmentItems);
			AssertEquals(2, goodsShipment.GoodsShipmentItems.Count);
			var goodsShipmentsItem1 = goodsShipment.GoodsShipmentItems.ElementAtOrDefault(0);
			AssertNotNull(goodsShipmentsItem1);
			AssertNotNull(goodsShipmentsItem1.Commodity.InvoiceLine);
			AssertEquals("Amount", 45M, goodsShipmentsItem1.Commodity.InvoiceLine.Amount);
			AssertEquals("Currency", "USD", goodsShipmentsItem1.Commodity.InvoiceLine.Currency);
			var goodsShipmentsItem2 = goodsShipment.GoodsShipmentItems.ElementAtOrDefault(1);
			AssertNotNull(goodsShipmentsItem2);
			AssertNotNull(goodsShipmentsItem2.Commodity.InvoiceLine);
			AssertEquals("Amount", 55M, goodsShipmentsItem2.Commodity.InvoiceLine.Amount);
			AssertEquals("Currency", "USD", goodsShipmentsItem2.Commodity.InvoiceLine.Currency);
		}

		public void TestFallbackProcedure()
		{
			AssertNull(Provider.FallbackProcedure);
		}

		public void TestPreparationDateAndTime()
		{
			AssertEquals(DateTime.MinValue, Provider.PreparationDateAndTime);
		}

		protected override void SetUp()
		{
			base.SetUp();
			messageSendingObject = MessageDataProviderTestHelper.SetUpMessageSendingObject(Factory);
			messageSendingObject.SubStyle = "A";
		}

		protected MessageSendingObject messageSendingObject;
	}
}
