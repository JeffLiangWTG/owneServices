using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EDISalesInquiry))]
	public class EDISalesInquiryTest : SalesEnquiryTest
	{
		public void TestPopulateContactWorkPhoneAndExtension()
		{
			EDISalesInquiry inquiry = Factory.NewWithValidTestData<EDISalesInquiry>();
			inquiry.O1_Phone = "+61 2 43825283<333>";
			OrgHeader org = inquiry.CreateOrg(Factory);
			OrgContact newContact = org.Contacts[0];
			AssertEquals("+61 2 43825283", newContact.OC_Phone);
			AssertEquals("333", newContact.OC_PhoneExtension);
		}
	}
}
