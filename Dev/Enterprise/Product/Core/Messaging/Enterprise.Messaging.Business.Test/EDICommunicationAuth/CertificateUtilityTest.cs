using System;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Enterprise.Messaging.Business.EDICommunicationAuthInbound;
using NUnit.Framework;

namespace Enterprise.Messaging.Testing
{
	[TestedType(typeof(CertificateUtility))]
	public class CertificateUtilityTest : TestCase
	{
		public void TestCsrValidation()
		{
			Assert(CertificateUtility.IsCsrValid(SampleCsrPem));
			Assert(!CertificateUtility.IsCsrValid(SampleCertificatePem));
			Assert(!CertificateUtility.IsCsrValid("1234"));
			Assert(!CertificateUtility.IsCsrValid(null));
		}

		public void TestCsrValidationForValidCsrWithTrailingContent()
		{
			Assert(CertificateUtility.IsCsrValid(SampleCsrPem + "    \t\r\n    "));
			Assert(!CertificateUtility.IsCsrValid(SampleCsrPem + "\r\n123456756756456"));
			Assert(!CertificateUtility.IsCsrValid(SampleCsrPem + "\r\n" + SampleCsrPem));
			Assert(!CertificateUtility.IsCsrValid(SampleCsrPem + "\r\n" + SampleCertificatePem));
		}

		public void TestConvertCertificateFromPem()
		{
			Assert(CertificateUtility.TryReadCertificate(SampleCertificateBlob, out X509Certificate2 certificate1, out string pem, out Exception exception));
			AssertEquals(SampleCertificatePem, pem);
			AssertEquals("L=Sydney, CN=wisetechglobal eAdaptor ca1 g1, S=NSW, OU=eAdaptor, O=WiseTech Global Limited, C=AU", certificate1.Issuer);
			Assert(!CertificateUtility.TryReadCertificate(Encoding.UTF8.GetBytes("1234"), out X509Certificate2 certificate2, out pem, out exception));
			Assert(!CertificateUtility.TryReadCertificate(null, out X509Certificate2 certificate3, out pem, out exception));
		}

		public void TestGetCertificateCN()
		{
			Assert(CertificateUtility.TryReadCertificate(SampleCertificateBlob, out X509Certificate2 certificate1, out string pem, out Exception exception));
			AssertEquals("UP3", CertificateUtility.GetCertificateSubjectCN(certificate1));
			AssertEquals("wisetechglobal eAdaptor ca1 g1", CertificateUtility.GetCertificateIssuerCN(certificate1));
			Assert(CertificateUtility.TryReadCertificate(SampleCertificateEmptyCNBlob, out X509Certificate2 certificate2, out pem, out exception));
			AssertEquals(string.Empty, CertificateUtility.GetCertificateIssuerCN(certificate2));
		}

		readonly string SampleCsrPem = @"-----BEGIN CERTIFICATE REQUEST-----
MIIE8TCCAtkCAQAwga0xKTAnBgkqhkiG9w0BCQEWGnN1cHBvcnRAd2lzZXRlY2hn
bG9iYWwuY29tMQswCQYDVQQGEwJBVTEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQI
DA9OZXcgU291dGggV2FsZXMxGDAWBgNVBAoMD1dpc2VUZWNoIEdsb2JhbDERMA8G
A1UECwwIRURJL0RBVC8xGzAZBgNVBAMMEndpc2V0ZWNoZ2xvYmFsLmNvbTCCAiIw
DQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAJydrqNcFsF4nx0wYDCW8gce8LCO
CDker2zJMDU2txqRwtttKat2mrL9pUrJ+nPdFp7YJWO4LAqc1T9+rK0bp87Ez+nQ
6HArlQhAvBR3qH4nBbUReOHXyd6aVNqp220/ox6W0uCBRojU68NvzzDeY43FmEbv
Y8RTrVpuZNEVeY6geojF6AiArhcwNSHoMnvTCVWCiGpekapD7uFUXO4BELHddhoT
MEGFqRG8JUmvsj+kKY41MAUsH6NctHAqMxY66/CZTPpH/1r3sKUgLlaUEPea22/4
rPbG7GfDRTGvK2du1BEpbO08hf/AE0h4B06zhyqbP7frOi2sSRb5Px3mT7b+Ob3/
nAotROsijEtHEV6pMr6O28uND4hP3wurxZHPAI0i+0dKNNM01ZmnAtijHI3U1Rdd
DUcehO12zWaMaB80sOuTSNdWzcah+0327uY3+fc7CLt2HxLFoLkCLGBO7Ef/vpK+
uS58AT0oYwF9WMpIgj3ROZ6jAU8G7OL4m6JKWRh2e2W29DunL1nhHL9kJNMFJlMA
FnRMFLwBEZU7GSjsKU1drdFPn7GiJ1Q/7jKKSY75XkUYCXJ0gc2ladVVz3B2RFaA
ZGD5ZsS2imiZQSGjNY5DLLMTq0NtHqTPQNQZ+JbBbzhUS43WNCXNtK3kOMcqapfg
AL7KAX1m119rQ+WZAgMBAAEwDQYJKoZIhvcNAQELBQADggIBACgNhD7hz2RsUhFW
a6lZslUeLORB5709V5/qL989VppufpmmmVFigh6+ftw7HFJ32KmeXTaJHQX+PlS9
0Vy63ObDhDbK7H/g8G/n69Lo43+hhnWYcJhPt55LiR2CmXlijvMHwsbWmsOv7QZh
NsYKZW+7kIcE2Ql9e+ObrCKp9gLgIEdrTTtaVFNMWsGIU2Y3rOr5kKcwsJuHg/TA
b6lGpqp5i8wP00e7JPnY28XQs8pvI91VGS701Q6F62mfnLGwo8gknf+Ic5WIbTuv
FA/HJ4foW9gux5FrMTDdzWAWJijcpYmxiMaHHX8d93V+wR3DUJw0G4AUDP0YTrNR
ShMs9LKlQAgA8Qdg+Lhq3wImaQLIlaEcA2FER38GZYaCfC4DH4vRkOWRXdBzjNcv
982E26x5Ek5LH365heh1PfSveVZIBZPKOwLNCLXrGa4gQtG95wsqRHXOWOeh/9Qa
wx99puxtepatNQEHMDA3rbPSsqpoBfzOlyNcvQV147DRQYTPXYp84gI3tHaiYpJ7
G20jq99TMr7fXNfuskn0YG2cL4p5C1ebQJV45Rzy6b0gaJR+2qiUzDzQGFwJgJq5
zlqZUNc3Gy9U2xVqrRCNR1wVaKdDtog0bp3+znEoBhZBKImGUw7vSIwFjpUV+VCe
2edWf4T63EQOzlShlxeaNt1QOMuc
-----END CERTIFICATE REQUEST-----";

		readonly string SampleCertificatePem = @"-----BEGIN CERTIFICATE-----
MIIFmjCCBIKgAwIBAgIRAO0pHLgIx/PG2Te6WFM2WvswDQYJKoZIhvcNAQELBQAw
gYoxCzAJBgNVBAYTAkFVMSAwHgYDVQQKDBdXaXNlVGVjaCBHbG9iYWwgTGltaXRl
ZDERMA8GA1UECwwIZUFkYXB0b3IxDDAKBgNVBAgMA05TVzEnMCUGA1UEAwwed2lz
ZXRlY2hnbG9iYWwgZUFkYXB0b3IgY2ExIGcxMQ8wDQYDVQQHDAZTeWRuZXkwHhcN
MjQwOTEwMDY1MjM0WhcNMjUwOTEwMDc1MjM0WjCBqDELMAkGA1UEBhMCQVUxGDAW
BgNVBAgMD05ldyBTb3V0aCBXYWxlczEPMA0GA1UEBwwGU3lkbmV5MRgwFgYDVQQK
DA9XaXNlVGVjaCBHbG9iYWwxKDAmBgNVBAsMH1RoaXNJc0FUZXN0Rm9yVGhlQXV0
b21hdGlvblRlYW0xHDAaBgkqhkiG9w0BCQEWDXRlc3RAdGVzdC5jb20xDDAKBgNV
BAMMA1VQMzCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCCAgoCggIBAKtZ/mTyb+/I
nMFtocCRh2c1PdmRSu5piHGDTYgAwtlisUSSJOwzA5DNXBx5tzefabKhvT4swabc
EhU4HPvly2V48g8c40kMR4wt6TYN6oSCWT0y1scY3fVSqrZX/NgC/S0P1xhE+UXo
mPrQfcN3ZRBYFvGu3cKqIBfVDouWQ9/fYejk2nS4N/lmhL0pH0pb80m6G0GE/ah/
VhmOPrzypRW16xLTsMrQseWv4PkOVKuvl9bRL80dKmTwa4lYu7+nYXYaSK8oZQhA
fmVJDQ23n7I+6baOL1L+zz+nAo5WuNTdr4JV75FTgCYVKAaXU73VG3sDswpeAUcB
yF32X3AiVt3EZh+BikIyzLiazhUNswjnmQ8VNsVszsizBfkuc/O/BiFiYlwxt5gJ
X7QkzZ49j9ZJcMShjMGf0m3fxY1tIAGUUnEUyLEWhPCBXGW62UL0drsTXDOKUWfL
pNkko1YwEXplZlKxh0Uh3Af6Om/j4fv1QZEgVeUk0eYga8p4nL9kFMTSe1R5aWOl
rsj1gLjGchBHObEnR+VOish3qXLVgP8YiGFU+gxBMct6wMarR2rj0oMuqOIDRCwT
FfWRtqjJQjeNoxexZiNLm/vqERGseQTBPCesnwQmosteT6pEDelEK77awaKLHCHC
6PYotQedqvIO8k0yv00ZtVx14l4S/cZBAgMBAAGjgdowgdcwCQYDVR0TBAIwADAf
BgNVHSMEGDAWgBTTJFiJFQV7qI6ada9eIbsWoC9mOjAdBgNVHQ4EFgQUJHFVoiYp
5HX+pGhfMi/p+/KZokwwDgYDVR0PAQH/BAQDAgWgMB0GA1UdJQQWMBQGCCsGAQUF
BwMBBggrBgEFBQcDAjBbBgNVHR8EVDBSMFCgTqBMhkpodHRwOi8vY3JsLndpc2V0
ZWNoZ2xvYmFsLmNvbS9jcmwvMGM5ODdhZDgtZThjMS00YzVmLTkwNmUtNmU0YWZk
MmYzOGI5LmNybDANBgkqhkiG9w0BAQsFAAOCAQEAK/dJxgBvsDwIprZ7oWLQtMWv
LIRWhhCLy8FYcrMLgieoerUra/YRW7nfeY02971zo7QnFs5WtMzdkPDZpQhmuhbC
5q7zTL97EJAonCN+9ovjbSJ76tyv7CiiUloVJ9pe2y4pwluGlczg/M9eqhCKy2DM
RhBw6Ie/ymqKR6P1s3bYx8VJPgc99fD6Xt+m91Ap4n3SOzyrNGywTtGp4PacjbMg
IV/8CtCoAGNBzdlSfOOBVySiis76AHP60KvksnxuqDGdMVsS7yoZCB2ZPRMdey3p
9i4ZzrxYFBaWEI38nH+UHjBj7rIOiCknttTE/5utwjdvkGYrYpeGrC8AmCrJMQ==
-----END CERTIFICATE-----";

		readonly string SampleCertificatePemEmptyCN = @"-----BEGIN CERTIFICATE-----
MIIEWjCCA0KgAwIBAgICFBYwDQYJKoZIhvcNAQELBQAwWjELMAkGA1UEBhMCVVMx
EzARBgNVBAgTCldhc2hpbmd0b24xEDAOBgNVBAcTB1NlYXR0bGUxJDAiBgNVBAoT
G2dldGFDZXJ0IC0gd3d3LmdldGFjZXJ0LmNvbTAeFw0yNDA3MjQwMDA5MTRaFw0y
NDA5MjIwMDA5MTRaMDgxCzAJBgNVBAYTAkFVMRgwFgYDVQQIDA9OZXcgU291dGgg
V2FsZXMxDzANBgNVBAcMBlN5ZG5leTCCAiIwDQYJKoZIhvcNAQEBBQADggIPADCC
AgoCggIBAMlNnyxQinkNOWUhx7k1DFMwyAUYT1T/c8Zs8GBd1PieAd/ACcHxYJvA
GxBEAmFiWKzc2g7rwI5+oS2Z96PGQSJLqHCxf2StQ1TBxyXMvnMpu1WMMlzdgKCo
Bwq+skOaYdrtaaK4LkrALcPAzFtW6idGQzhvBi7N3yIT75jTpYN6290o+m719TcB
sPS7okqM+kW4N49yXX4rnvUnXP1AJr+mGSQVHvg8MyoMOA7HJ7rBAPOo48fLYM7V
HMbfhxWeH2OH8gkHG/rHskyG29Dj2kx8VAPb/bf0ovA3KqB6nyrT2GDA8Ju6czde
3wEx2/7hOLTUynuW2yarvgTafzYYxyVhr31wzRL5/P/iv3/PLfwQ8h21M+PMN5Ju
Fov/+gQJZAWqyHd6eg2d3I7leb2Fx8D4OPbyz8fxqaM1E0OooTwDj/vyobu5YrA2
iGEdTGjbThm5UXiUcAjAxRYfi9TkFco8Ajcp5SMUXUEIzIWviEJBAn24kBMFdWda
UHCBAi28kPQQ23tPKRtZydOKmtG5C1U7fkLcRewTc8KVHACt+xRsFXhzZDY4LeSC
tsxJRqM138oOSkF+2Wd8FMCWev2XnKQYzAgSYUN0sCbIEpCKFV36PI3AZveqd9F+
iHcdd126G6Aa9ZUPvVn5BGqyYVBn67ZXnZ6csa1XsXrCovlqC26jAgMBAAGjTDBK
MAkGA1UdEwQCMAAwEQYJYIZIAYb4QgEBBAQDAgTwMAsGA1UdDwQEAwIFoDAdBgNV
HSUEFjAUBggrBgEFBQcDAgYIKwYBBQUHAwEwDQYJKoZIhvcNAQELBQADggEBAJu9
NGWoeH1kndjbSaGJdiEK0+RGo00CFh02Un08MOCrCjLcQkZREpRgzvZ6qZe6Jly0
zGRz5+xuc86WPpW1Zu7jlIeeL0+7u5WL2bJFdq7umPbZe1YJDkj6Kco7XYLNkv5D
Cdf8gUHjxHmtPabnJjvtqLQyv2dZFI98WUoJPwtfL+V+2IMbIPJ/IT742FTtOk8l
dRj9KTkYBVYRGoEOp7vwJtqnnMf/ROdiH8Ebw+GUTxjsN2xJ41HkG+SlLOz6ur0w
jBrRVHgXta5xHIU+miI42KgY7o7C8ZkHiTIoD6Cq5XhyQPOoGkJm5Ah0XgO274hT
q12fz8yKmc0G0aGL0nQ=
-----END CERTIFICATE-----
";

		byte[] SampleCertificateBlob
		{
			get
			{
				return Encoding.UTF8.GetBytes(SampleCertificatePem);
			}
		}

		byte[] SampleCertificateEmptyCNBlob
		{
			get
			{
				return Encoding.UTF8.GetBytes(SampleCertificatePemEmptyCN);
			}
		}
	}
}
