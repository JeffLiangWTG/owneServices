using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CertificateOfOriginLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCertificateTypeList()
		{
			var parent = Factory.New<CertificateOfOrigin>();
			var certificateTypeList = parent.Lookups.CertificateTypeList;
			AssertEquals(2, certificateTypeList.Count);
			AssertEquals("CCPTC, CCROM", certificateTypeList.CodesAsString);
		}
	}
}
