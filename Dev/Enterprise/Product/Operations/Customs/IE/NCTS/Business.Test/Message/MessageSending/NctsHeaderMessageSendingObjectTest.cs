using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObject))]
	sealed class NctsHeaderMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTCI11DeliveryDate_Readonly()
		{
			SendingObject.MessageType = "013";
			AssertEquals("TC11 Delivery Date should be readonly", true, SendingObject.TC11DeliveryDateInfo.ReadOnly);
			SendingObject.MessageType = "141";
			AssertEquals("TC11 Delivery Date should not be readonly", false, SendingObject.TC11DeliveryDateInfo.ReadOnly);
		}

		public void TestEnquiryText_Readonly()
		{
			SendingObject.MessageType = "013";
			AssertEquals("Enquiry Text should be readonly", true, SendingObject.EnquiryTextInfo.ReadOnly);
			SendingObject.MessageType = "141";
			AssertEquals("Enquiry Text should not be readonly", false, SendingObject.EnquiryTextInfo.ReadOnly);
		}

		public void TestConsignee_Readonly()
		{
			SendingObject.MessageType = "013";
			AssertEquals($"Consignee should be readonly for {SendingObject.MessageType}", true, SendingObject.ConsigneeInfo.ReadOnly);
			SendingObject.MessageType = "141";
			AssertEquals($"Consignee should not be readonly for {SendingObject.MessageType}", false, SendingObject.ConsigneeInfo.ReadOnly);
		}

		public void TestDestinationCustomsOfficeCode_Readonly()
		{
			SendingObject.MessageType = "013";
			AssertEquals($"Destination Customs Office Code should be readonly for {SendingObject.MessageType}", true, SendingObject.DestinationCustomsOfficeCodeInfo.ReadOnly);
			SendingObject.MessageType = "141";
			AssertEquals($"Destination Customs Office Code should not be readonly for {SendingObject.MessageType}", false, SendingObject.DestinationCustomsOfficeCodeInfo.ReadOnly);
		}

		public void TestValidation()
		{
			var validation = SendingObject.Validation;
			AssertType<NctsHeaderMessageSendingObjectValidation>(validation);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new NctsHeaderMessageSendingObject(NctsHeader);
		}

		NctsHeader NctsHeader => nctsHeader ?? (nctsHeader = CreateNctsHeader());
		NctsHeader nctsHeader;
		NctsHeader CreateNctsHeader()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			return nctsHeader;
		}

		NctsHeaderMessageSendingObject SendingObject => sendingObject ?? (sendingObject = new NctsHeaderMessageSendingObject(NctsHeader));
		NctsHeaderMessageSendingObject sendingObject;
	}
}
