using System;
using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class AU
		{
			public interface ICertificateManager : IDisposable
			{
				string CompanyCertificatePassword { get; }
				byte[] CompanyCertificateData { get; }
				byte[] CustomsCertificateData { get; }
				byte[] TrustPointCertificateData { get; }
			}

			public interface ICertificateManagerHelper
			{
				void CreateCustomsCertificates();
				void CreateCustomsCertificates(ZDateTime startDate, ZDateTime endDate);
				void CreateCustomsCertificates2004();
				void CreateCustomsCertificates2005();
				void CreateCustomsCertificates2012();
				void CreateCustomsCertificates2021();
				void CreateCustomsCertificates2023();
				void RemoveCertificates();
				void SetupValidCompanyCertificatesForTest();
				void SetupValidCompanyCertificatesForTest(out DateTime startDate, out DateTime endDate);
				void SetupValidCompanyCertificatesForTest(out DateTime startDate, out DateTime endDate, Guid companyPK);
				string AUCCompanyCertificatePasswordForTest { get; }
			}
		}
	}
}
