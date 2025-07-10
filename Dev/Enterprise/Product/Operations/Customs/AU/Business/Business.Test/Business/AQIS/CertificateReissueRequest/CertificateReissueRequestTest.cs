using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CertificateReissueRequest))]
	sealed class CertificateReissueRequestTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCertificates()
		{
			var certificates = new CodeDescriptionPairList();
			certificates.AddPair("AU11111");
			certificates.AddPair("AU22222");
			certificates.AddPair("AU33333");
			var header = new CertificateReissueHeader(Factory, certificates);

			var request = header.CertificateReissueRequests.AddNew();
			AssertEquals("AU11111, AU22222, AU33333", request.Certificates.CodesAsString);
		}

		public void TestCertificateNumber()
		{
			var certificates = new CodeDescriptionPairList();
			certificates.AddPair("AU123456");

			var request = new CertificateReissueRequest(Factory, certificates);
			request.CertificateNumber = "";
			AssertHasError(request.CertificateNumberInfo, "Please enter a Certificate Number.");

			request.CertificateNumber = "AU-1234567";
			AssertNoError(request.CertificateNumberInfo, "Please enter a Certificate Number.");
			AssertHasError(request.CertificateNumberInfo, "Certificate Number contains invalid character(s). Use only A-Z and 0-9, no spaces.");

			request.CertificateNumber = "AU 1234567";
			AssertHasError(request.CertificateNumberInfo, "Certificate Number contains invalid character(s). Use only A-Z and 0-9, no spaces.");

			request.CertificateNumber = "AU1234567";
			AssertNoError(request.CertificateNumberInfo, "Certificate Number contains invalid character(s). Use only A-Z and 0-9, no spaces.");
			AssertHasWarning(request.CertificateNumberInfo, "A Certificate matching this Number could not be found.");

			request.CertificateNumber = "AU123456";
			AssertNoWarning(request.CertificateNumberInfo, "A Certificate matching this Number could not be found.");
		}

		public void TestCertificateNumberIsUnique()
		{
			var header = new CertificateReissueHeader(Factory, new CodeDescriptionPairList());

			var request1 = header.CertificateReissueRequests.AddNew();
			request1.CertificateNumber = "AU11111";
			var request2 = header.CertificateReissueRequests.AddNew();
			request2.CertificateNumber = "AU11111";
			AssertHasError(request2.CertificateNumberInfo, "Only one of each Certificate can be selected.");

			request2.CertificateNumber = "AU22222";
			AssertNoError(request2.CertificateNumberInfo, "Only one of each Certificate can be selected.");
		}

		public void TestReissueReason()
		{
			var request = new CertificateReissueRequest(Factory, new CodeDescriptionPairList());
			request.ReissueReason = "";
			AssertHasError(request.ReissueReasonInfo, "Please enter a Reason.");

			request.ReissueReason = "0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789ABCDEFG";
			AssertNoError(request.ReissueReasonInfo, "Please enter a Reason.");
			AssertEquals("Truncated to 500 characters",
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
				"0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789", request.ReissueReason);
		}

		public void TestIsValid()
		{
			var request = new CertificateReissueRequest(Factory, new CodeDescriptionPairList());
			request.CertificateNumber = "";
			request.ReissueReason = "";
			Assert(!request.IsValid);

			request.CertificateNumber = "AU1234567";
			request.ReissueReason = "";
			Assert(!request.IsValid);

			request.CertificateNumber = "";
			request.ReissueReason = "reason";
			Assert(!request.IsValid);

			request.CertificateNumber = "AU1234567";
			request.ReissueReason = "reason";
			Assert(request.IsValid);
		}

		protected override BusinessObject GetNewBusinessObject() => new CertificateReissueRequest(Factory, new CodeDescriptionPairList());
	}
}
