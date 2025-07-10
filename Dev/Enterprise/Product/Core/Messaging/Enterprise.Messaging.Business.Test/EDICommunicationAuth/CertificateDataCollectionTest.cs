using System;
using System.Collections.Generic;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using Moq;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedAsNonPersistentBusinessObject]
	[TestedType(typeof(CertificateDataCollection))]
	public class CertificateDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CertificateDataCollection>
	{
		protected override CertificateDataCollection GetCollectionToTest() => new CertificateDataCollection("c1");

		protected override BusinessObject GetNewElementToAddToTheCollection() => new CertificateData();

		public void TestGetCertificateData()
		{
			var mockDescriptors = new Mock<ICertificateManager>();
			mockDescriptors.Setup(d => d.DownloadCertificates("c1")).Returns(new List<byte[]> { SampleCertificateBlob });

			using (ObjectFactory.Substitute(mockDescriptors.Object))
			{
				var obj = new CertificateDataCollection("c1");
				obj.LoadCollection(out Exception exception);
				AssertEquals(1, obj.Count);
				AssertEquals(SampleCertificatePem, obj[0].CertificatePem);
			}
		}

		readonly string SampleCertificatePem = @"-----BEGIN CERTIFICATE-----
MIIEOjCCAyKgAwIBAgIIX4MTnZS2hhcwDQYJKoZIhvcNAQELBQAwgboxKTAnBgkq
hkiG9w0BCQEWGnN1cHBvcnRAd2lzZXRlY2hnbG9iYWwuY29tMQswCQYDVQQGEwJB
VTEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQIDA9OZXcgU291dGggV2FsZXMxGDAW
BgNVBAoMD1dpc2VUZWNoIEdsb2JhbDEeMBwGA1UECwwVRURJL0RBVC9FRElDbGll
bnROYW1lMRswGQYDVQQDDBJ3aXNldGVjaGdsb2JhbC5jb20wHhcNMjQwNzA1MDcw
MzM3WhcNMjUwNzA1MDcwMzM3WjCBujEpMCcGCSqGSIb3DQEJARYac3VwcG9ydEB3
aXNldGVjaGdsb2JhbC5jb20xCzAJBgNVBAYTAkFVMQ8wDQYDVQQHDAZTeWRuZXkx
GDAWBgNVBAgMD05ldyBTb3V0aCBXYWxlczEYMBYGA1UECgwPV2lzZVRlY2ggR2xv
YmFsMR4wHAYDVQQLDBVFREkvREFUL0VESUNsaWVudE5hbWUxGzAZBgNVBAMMEndp
c2V0ZWNoZ2xvYmFsLmNvbTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB
AJh2uI1Z4DUCLis/ESVZ7qwAagTfoyWEF2O/aljFAmrjDj6DeETAq2IkIPnqnI7Q
EpfbE6uEquMlFAckoQ3z2x9kYIRqyGbKSHhFKJaa1R02VPtmDndGMQJDBstJYFO4
MSVrFMy0hsjymm0LT/3hsKDoCu+/jIZcI6n/of5eGXKbpiyDuV9bNnAym0Aek8CM
T8Zx54JIRfWPepCdz+ydy18vY+adnVtvR/HI8IUNQQGgSPcOrYalpUYEFmsnTtSY
3s53mI49uwtxkOtHRQGMiWuLp9ZPAPnDhoYS3+WZV3WACDcfeF87+NvKzcVYrzNp
uAD1LdFEtW62zkXIYrTnTRcCAwEAAaNCMEAwHwYDVR0jBBgwFoAUXcqGD5uGCnG5
v5A2eg/dn6gqhNEwHQYDVR0OBBYEFF3Khg+bhgpxub+QNnoP3Z+oKoTRMA0GCSqG
SIb3DQEBCwUAA4IBAQB5OTgdKXza9vi7dqwUF+pEzWspyskI4/WJYJV2Euqav8aU
Fw52dmFHntNndYMHtaVAuqrFNOr345E5rg5moggOjvs77ZV+Y/WpnRT7K8NzV/A/
fMVW0Om+fF7DVwJ3u1OyKZNa6Zg4lXch2D3w147A6SHvqNfqFw4XFCBnSBO8vmb9
APVcwWvHyMLFp+29sAvUKQSSyp8gJJz7oyCywHT7llGrgG229GzfB6LoK98385V8
6UscEGYFZHa5U0pJcgwlRRTiM1mYH9KXRccoKtwvGV5pkzr059Tju6nZ9diBy9Lm
Hs2Qrf72nFOdINRlsSdbyoUvvWJvm7k+ANMQXL0v
-----END CERTIFICATE-----";

		byte[] SampleCertificateBlob
		{
			get
			{
				return Encoding.UTF8.GetBytes(SampleCertificatePem);
			}
		}
	}
}
