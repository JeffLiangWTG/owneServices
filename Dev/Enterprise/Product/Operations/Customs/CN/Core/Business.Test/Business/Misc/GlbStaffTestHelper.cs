using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	static class GlbStaffTestHelper
	{
		public static void AssertCusAgentProperties(JobDeclaration dec, string brokerNumber = "", string brokerName = "", string cnoNumber = "", string cnoName = "")
		{
			AssertionWithHtml.CombineAssertions(() =>
			{
				Assertion.AssertEquals("BrokerNumber", brokerNumber, dec.BrokerCertificateNumber);
				Assertion.AssertEquals("BrokerName", brokerName, dec.NameOnBrokerCertificate);
				Assertion.AssertEquals("CNONumber", cnoNumber, dec.OperatorCardID);
				Assertion.AssertEquals("CNOName", cnoName, dec.NameOnOperatorCard);
			});
		}

		public static GenRegCertAccredMaintList AddCert(this GlbStaff staff, string certType, string country, string number)
		{
			var result = staff.Certificates.AddNew();
			result.XZ_Type = certType;
			result.XZ_RN_NKCountryOfIssuance = country;
			result.XZ_RefNumber = number;

			return result;
		}
	}
}
