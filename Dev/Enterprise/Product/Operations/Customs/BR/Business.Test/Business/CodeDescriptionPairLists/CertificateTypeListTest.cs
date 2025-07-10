using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class CertificateTypeListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			AssertEquals("Result empty", ZString.Empty, CertificateTypeList.MapToCustomsCode(""));
			AssertEquals("Result empty", ZString.Empty, CertificateTypeList.MapToCustomsCode("XX"));
			AssertEquals("CCPTC -> 2", "2", CertificateTypeList.MapToCustomsCode(CertificateTypeList.Codes.CCPTC));
			AssertEquals("CCROM -> 3", "3", CertificateTypeList.MapToCustomsCode(CertificateTypeList.Codes.CCROM));
		}
	}
}
