using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(CusPermitEnquiryMessage))]
	public class CusPermitEnquiryMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestHasLinkedMessage()
		{
			var permit = Factory.New<CusPermitHeader>();
			var bo1 = (CusPermitEnquiryMessage)GetNewBusinessObject();
			AssertEquals("No linked message", false, bo1.HasLinkedMessage);
			bo1.EM_LinkedObject = permit;
			AssertEquals("Has linked message", true, bo1.HasLinkedMessage);
		}

		public void TestLinkedMessage()
		{
			var permit = Factory.New<CusPermitHeader>();
			var bo1 = (CusPermitEnquiryMessage)GetNewBusinessObject();
			var bo2 = (CusPermitEnquiryMessage)GetNewBusinessObject();
			bo1.EM_LinkedObject = permit;
			AssertEquals(null, bo1.LinkedMessage);
			bo2.EM_LinkedObject = bo1;
			AssertEquals(bo2, bo1.LinkedMessage);
		}
	}
}
