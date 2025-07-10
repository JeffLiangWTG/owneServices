using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CertificateReissueHeader))]
	sealed class CertificateReissueHeaderTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCertificateReissueRequests()
		{
			var header = new CertificateReissueHeader(Factory, new CodeDescriptionPairList());
			AssertType<CertificateReissueRequestCollection>(header.CertificateReissueRequests);
			AssertEquals(0, header.CertificateReissueRequests.Count);
			AssertType<CertificateReissueRequest>(header.CertificateReissueRequests.AddNew());
			AssertEquals(1, header.CertificateReissueRequests.Count);
		}

		public void TestIsValid()
		{
			var header = new CertificateReissueHeader(Factory, new CodeDescriptionPairList());
			Assert(!header.IsValid);

			var request = header.CertificateReissueRequests.AddNew();
			request.CertificateNumber = "AU1234567";
			request.ReissueReason = "reason";
			Assert(header.IsValid);

			request.CertificateNumber = "AU1234567";
			request.ReissueReason = "";
			Assert(!header.IsValid);

			request.CertificateNumber = "";
			request.ReissueReason = "reason";
			Assert(!header.IsValid);

			request.CertificateNumber = "";
			request.ReissueReason = "";
			Assert(!header.IsValid);
		}

		protected override BusinessObject GetNewBusinessObject() => new CertificateReissueHeader(Factory, new CodeDescriptionPairList());
	}
}
