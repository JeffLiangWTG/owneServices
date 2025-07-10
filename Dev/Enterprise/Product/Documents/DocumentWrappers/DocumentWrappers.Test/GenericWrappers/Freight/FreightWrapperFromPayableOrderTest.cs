using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPayableOrder))]
	sealed class FreightWrapperFromPayableOrderTest : FreightWrapperTest
	{
		public void TestSupplier()
		{
			var wrapper = SetupWrapperWithTestData();
			var supplier = wrapper.Supplier;
			AssertEquals("Contact Name 2", supplier.ContactName);
			AssertEquals("Phone 2", supplier.ContactPhone);
			AssertEquals("Fax 2", supplier.ContactFax);
			AssertEquals("Email 2", supplier.ContactEmail);
		}

		FreightWrapperFromPayableOrder SetupWrapperWithTestData()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "TEST SUPPLIER FOR ORDER";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "TEST BUYER FOR ORDER";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			var deliveryAddress = buyer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "THE FIRST PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_Address2 = "THE SECOND PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_City = "WHO CARES";
			deliveryAddress.OA_PostCode = "*#&$(*@#";
			deliveryAddress.OA_RN_NKCountryCode = "AU";

			var deliveryContact = buyer.Contacts.AddNew();
			deliveryContact.OC_ContactName = "Contact Name 1";
			deliveryContact.OC_Phone = "Phone 1";
			deliveryContact.OC_Fax = "Fax 1";
			deliveryContact.OC_Email = "Email 1";

			var pickupContact = supplier.Contacts.AddNew();
			pickupContact.OC_ContactName = "Contact Name 2";
			pickupContact.OC_Phone = "Phone 2";
			pickupContact.OC_Fax = "Fax 2";
			pickupContact.OC_Email = "Email 2";

			var order = Factory.New<AccPayableOrderHeader>();
			order.APH_Stage = "REQ";
			order.APH_Disposition = "OIC";
			order.APH_OrderNumber = "PO000001";
			order.APH_OrderNumberSplit = 1;
			order.APH_OA_Buyer = deliveryAddress.PK;
			order.APH_OC_BuyerContact = deliveryContact.PK;
			order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			order.SupplierDocumentaryAddress.ContactPK = pickupContact.PK;
			order.APH_Type = "VOD";
			order.APH_GoodsDescription = "Hello World!";
			order.APH_BookingConfDate = new ZDate(2015, 06, 15);
			order.APH_BookingConfRef = "13579";
			order.APH_InvoiceNumber = "24680";
			order.APH_InvoiceDate = new ZDate(2015, 06, 20);
			order.APH_RX_NKOrderCurrency = "AUD";
			order.APH_GSTInclusive = true;
			order.APH_DueDate = new ZDate(2015, 07, 20);
			order.APH_ReadyForDelivery = new ZDate(2015, 06, 17);
			order.APH_ExpectedDelivery = new ZDate(2015, 06, 19);
			order.APH_GoodsReceivedStatus = "UIV";
			order.APH_FollowupDate = new ZDate(2015, 06, 28);

			return new FreightWrapperFromPayableOrder(order, Factory);
		}

		public override void TestBuyer()
		{
			var wrapper = SetupWrapperWithTestData();
			var buyer = wrapper.Buyer;
			AssertEquals("Contact Name 1", buyer.ContactName);
			AssertEquals("Phone 1", buyer.ContactPhone);
			AssertEquals("Fax 1", buyer.ContactFax);
			AssertEquals("Email 1", buyer.ContactEmail);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var result = Factory.New<AccPayableOrderHeader>();
			return result;
		}

		protected override bool IsCarrierUsed
		{
			get { return false; }
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "GoodsDescription", "Hello World!" },
					{ "JobNumber", "PO000001" },
					{ "JobNumberBarcodeText", "^POD=PO000001;;|" },
					{ "JobNumberBarcodeTextForFont", "È^POD=POÃ¯¯!Ä;;|aÊ" },
					{ "JobNumberBarcodeTextWithoutDocManagerCodes", "ÈPO000001]Ê" },
					{ "SecondaryHeading", "Split" },
					{ "SecondaryNumber", "1" },
					{ "ShippersReference", "13579" },
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
Buyer : TEST BUYER FOR ORDER\nTHE FIRST PART OF THE DELIVERY ADDRESS\nTHE SECOND PART OF THE DELIVERY ADDRESS\nWHO CARES *#&$(*@#\nAUSTRALIA
Supplier : TEST SUPPLIER FOR ORDER\nAUSTRALIA";
			}
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "TEST SUPPLIER FOR ORDER";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "TEST BUYER FOR ORDER";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress deliveryAddress = buyer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "THE FIRST PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_Address2 = "THE SECOND PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_City = "WHO CARES";
			deliveryAddress.OA_PostCode = "*#&$(*@#";
			deliveryAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress pickupAddress = supplier.Addresses.AddNew();
			pickupAddress.OA_Address1 = "THE FIRST PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_Address2 = "THE SECOND PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_City = "SCOTTLAND";
			pickupAddress.OA_PostCode = "666";
			pickupAddress.OA_RN_NKCountryCode = "AU";

			AccPayableOrderHeader order = Factory.New<AccPayableOrderHeader>();
			order.APH_Stage = "REQ";
			order.APH_Disposition = "OIC";
			order.APH_OrderNumber = "PO000001";
			order.APH_OrderNumberSplit = 1;
			order.APH_OA_Buyer = deliveryAddress.PK;
			order.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
			order.APH_Type = "VOD";
			order.APH_GoodsDescription = "Hello World!";
			order.APH_BookingConfDate = new ZDate(2015, 06, 15);
			order.APH_BookingConfRef = "13579";
			order.APH_InvoiceNumber = "24680";
			order.APH_InvoiceDate = new ZDate(2015, 06, 20);
			order.APH_RX_NKOrderCurrency = "AUD";
			order.APH_GSTInclusive = true;
			order.APH_DueDate = new ZDate(2015, 07, 20);
			order.APH_ReadyForDelivery = new ZDate(2015, 06, 17);
			order.APH_ExpectedDelivery = new ZDate(2015, 06, 19);
			order.APH_GoodsReceivedStatus = "UIV";
			order.APH_FollowupDate = new ZDate(2015, 06, 28);

			return new FreightWrapperFromPayableOrder(order, Factory);
		}
	}
}
