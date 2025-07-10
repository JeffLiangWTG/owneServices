using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class SADCertificate10YYWrapperTest : TestCaseWithFactory
{
	[TestDate(2019, 12, 18)]
	public void TestCertificate()
	{
		var sadCertificate10YYWrapper = new SADCertificate10YYWrapper(100);
		CombineAssertions(() =>
		{
			AssertEquals("DocumentType should be", "10YY", sadCertificate10YYWrapper.DocumentType);
			AssertEquals("CountryOfIssue should be", "", sadCertificate10YYWrapper.CountryOfIssue);
			AssertEquals("IssuingYear should be", "", sadCertificate10YYWrapper.IssuingYear);
			AssertEquals("Reference should be", "", sadCertificate10YYWrapper.Reference);
			AssertEquals("Quantity should be", 100m, sadCertificate10YYWrapper.Quantity);
			AssertEquals("UnitOfMeasurement should be", "", sadCertificate10YYWrapper.UnitOfMeasurement);
			AssertEquals("DerogationFlag should be", false, sadCertificate10YYWrapper.DerogationFlag);
			AssertEquals("RetrospectiveDerogationFlag should be", false, sadCertificate10YYWrapper.RetrospectiveDerogationFlag);
		});
	}
}
