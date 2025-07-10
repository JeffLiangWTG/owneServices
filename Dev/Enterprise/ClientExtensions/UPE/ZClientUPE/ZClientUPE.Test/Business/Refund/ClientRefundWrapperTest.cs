using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(ClientRefundWrapper))]
	public class ClientRefundWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			var wrapper = new ClientRefundWrapper(Factory);
			AssertEquals("wrapper.Contact", "", wrapper.Contact);
			AssertEquals("wrapper.PhoneNumber", "", wrapper.PhoneNumber);
			AssertEquals("wrapper.EnquiryDetails", "", wrapper.EnquiryDetails);
			AssertEquals("wrapper.EnquiryRaisedBy", "", wrapper.EnquiryRaisedBy);
			wrapper.Contact = "Contact";
			wrapper.PhoneNumber = "123";
			wrapper.EnquiryDetails = "EnquiryDetails";
			wrapper.EnquiryRaisedBy = wrapper.RaisedByList[0].Code;
			wrapper.RunPreSaveValidation();
			Assert("wrapper.HasErrors", !wrapper.HasErrors);
			AssertEquals("wrapper.Contact", "Contact", wrapper.Contact);
			AssertEquals("wrapper.PhoneNumber", "123", wrapper.PhoneNumber);
			AssertEquals("wrapper.EnquiryDetails", "EnquiryDetails", wrapper.EnquiryDetails);
			AssertEquals("wrapper.EnquiryRaisedBy", wrapper.RaisedByList[0].Code, wrapper.EnquiryRaisedBy);
			wrapper.PhoneNumber = "123aaa";
			wrapper.RunPreSaveValidation();
			Assert("wrapper.HasErrors", wrapper.HasErrors);
			wrapper.PhoneNumber = "123";
			wrapper.EnquiryRaisedBy = "AAAA";
			wrapper.RunPreSaveValidation();
			Assert("wrapper.HasErrors", wrapper.HasErrors);
		}

		public void TestSyncronise()
		{
			var refund = Factory.NewWithValidTestData<ClientRefund>();
			var wrapper = new ClientRefundWrapper(Factory);
			AssertEquals("refund.T10_EnquiryContact", "", refund.T10_EnquiryContact);
			AssertEquals("refund.T10_EnquiryPhoneNumber", "", refund.T10_EnquiryPhoneNumber);
			AssertEquals("refund.T10_EnquiryDetails", "", refund.T10_EnquiryDetails);
			AssertEquals("refund.T10_EnquiryRaisedBy", "", refund.T10_EnquiryRaisedBy);
			wrapper.Contact = "Contact";
			wrapper.PhoneNumber = "123";
			wrapper.EnquiryDetails = "EnquiryDetails";
			ZString raisedByCode = wrapper.RaisedByList[0].Code;
			wrapper.EnquiryRaisedBy = raisedByCode;
			wrapper.Syncronise(refund);
			AssertEquals("refund.T10_EnquiryContact", "Contact", refund.T10_EnquiryContact);
			AssertEquals("refund.T10_EnquiryPhoneNumber", "123", refund.T10_EnquiryPhoneNumber);
			AssertEquals("refund.T10_EnquiryDetails", "EnquiryDetails", refund.T10_EnquiryDetails);
			AssertEquals("refund.T10_EnquiryRaisedBy", raisedByCode, refund.T10_EnquiryRaisedBy);
		}
	}
}
