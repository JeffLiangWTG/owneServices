using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(SystemToSystemTrustRegistryDataType))]
	sealed class SystemToSystemTrustRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<SystemToSystemTrustRegistryDataType>
	{
		protected override SystemToSystemTrustRegistryDataType GetNewDataType()
		{
			return new SystemToSystemTrustRegistryDataType();
		}

		protected override string ExpectedEditorName => "SystemToSystemCertificateRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var first = new SystemToSystemTrustInfo();
			var second = new SystemToSystemTrustInfo()
#pragma warning disable CS0618 // Type or member is obsolete
			{
				PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAvTQ1eKPDlt7Akq2q89lv+TPjL+r/Oxlgs+Lugu/7WyFlQPAf
eevry+q7F9gAjblSEDbeKToWTtDuGkPYZOtU3hK5Kk6i2C7tvOeMf7tqYT9Iq76P
NNhW0FHm90W0hWzdl4WzSFi+/RqD50UaXv8sMHJrpEdCcV8PVw35+E3t2AYNdx5a
nXKiXumBbak7q+Aq4eOadmSDTkZOvrEiqdiy6sBBwKe64XymDv6+cbwIt4OPIvmn
b4oxXyCNpKutCCNdGVYNWW00M9prCsCeEkdEw8GBx+cCu41U5Exb15CJ1cuL/kmK
BsjOfQUN/Q81pDnh6DSvyYYbBa62Evt+94MN7wIDAQABAoIBAQCaGGt0Vex2c/Vj
gQ46jF0mGZxu6nX1LDYWTDz1Z37QOeK9gNQh+IOTERpP0RyiUM0FZ6qI6UZB6ugB
fEBB8WitdZJZYrI+X/pAYFWNspZxnZkSR6Fa6NfL0ujJe3wLLx9KyRm7Uow6l93V
/fH8bNcQniANK/xxWXqk7D0qfk24Ic66/zWXskbw6FegrhZ6LmiT/wsf0G0D7he8
ywcsqv5bfOax25SMhQ5MZUG0vignfH+9177hvfID0817f11rBM2Of5cG3tCcPLMm
LtK1heBgBT4DPI2ssr7lpQoKGZchUl8jKEvsaPmc1Q1Yzcfeq5OMvuH3z3z8dnNg
ibuSDZQBAoGBAOlPwzmqcD9H+rSK4heAnZTHAXzYYkHeRan/4TZaeBrte/VkyIy5
oZyde2KQ0ozFWvn0MIXXuFzcK0h9PrGgwJAXzioHY2/sKo3+EORd1kkm/5NGogNe
9SPGvNEkTXQMT3maT5GFOXxj5UzX642neB9WdaUbFBmknhHRDFKQs7b1AoGBAM+a
ZaOaG7ej57MO87EsxX067GcjZuAJCvKBj8AmATtCDB16P9cYYpUGI4V/6kQ05heq
YCsjzlvetaXFchStg643zHc90+/kK34IiHgmNBnRriR8/4txKI7tMk5Po082QSJn
AqWHcfpIBIpJiWiKoUn5Gfuvb5p9+p5QrpQE2PrTAoGAOsLmDo4Iu0drszEPhI/W
IHSGwWTWSnSq4wgZNtFOUqnhgIqjoB3YwFNBki+bd+z03uNLnUoZmvmwxQ8WTTKJ
jUERobA+sR+wEBcfNgUURVRXJkax41t1Lk+Nmrcj8shu89eTlrkRI20dgV8YapL1
RB9Ifg713weA8EfQbLMZMEkCgYEAx9HskpP6qb9xfL+ff6GAVREqCfvhQTJy/wgF
vQTXmpnv30+Tsw0dliLafdypOA1SiuTKu7szHOo4HN7290ArxryeaQdxvyz3T9AW
ys66xz8zRpupvCWmzCsyiH3OyqeF+f78ZScBZc170T8Gq0EEDZdekwpBeYpYd5lg
78fasVECgYAMKazsFA5W1i/RQQuwPLtRWL9KUrUVxApb/KPHQnYfCngC1yfh9DzI
uXO3wlO8rHBR1PodcbGi/JZMJTAmnVrJO5MFCQ6fixl7Da8C66D2baKhpz22btOO
bl7N795AsX2VYpkKWv9TXd+vJ+n2V2XYlSz92Cg4YtX42SSJRVd09g==
-----END RSA PRIVATE KEY----",
				CertificateSigningRequest = @"-----BEGIN CERTIFICATE REQUEST-----
MIICvzCCAacCAQAwejELMAkGA1UEBhMCQVUxDDAKBgNVBAgMA05TVzEPMA0GA1UE
BwwGU3lkbmV5MRgwFgYDVQQKDA9XaXNlVGVjaCBnbG9iYWwxHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxFDASBgNVBAMMC2V4YW1wbGUuY29tMIIBIjANBgkq
hkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvVIkZbr64CddxSFwXVoRMbHihIBoL+9z
ztwm+/9FwFbg6RppDyGp6BkSrxbn5siU2HcobxNI9ZjunbfFi/2bI/+qp5CyfFHr
8qlOxROoGJyGZYUefCLUVLXI8/6CqgQJim5n7Jmxtb8w9MpFd4jlgqZIeQDnc9nK
17M10MVGvr8FnY3twoXTkHDvzhj4cUcvhk9ulDuaU5Y72GfGKqKowP5lL8+Agbi4
tYTFm/0vQBZaWeCHQ33KsYZtCIekrtpkvkaGrGEHKF4twgl4z9GVyarx/+ZC97zn
qF3qatUxDtM50zp0nyG2xdY2h9DfzjkHlGfW9Ts2c6aj0JZD5RyLjwIDAQABoAAw
DQYJKoZIhvcNAQELBQADggEBAI3cZiCqaWVk76AbK8ZxAaj/QOG6McDY3cU4xuDc
co3V2svM0rF7KaV4Ras3IYR2AK10jMIWDODWZyD4rGNkbTtc5f7/eBewukhSWlvN
Tng55EuTH5rJxAsKdLf8Q0VTESbPVzQdwtV5hyrMARDNga3O8E5LiCusKmX/F6Rc
LnQDiVR74BM+YWvceQzN4kAupzvp6enw3iGSjFW9ONR+jtxxedkPQYXYTAYS1mcm
sQfLgkpisxYeE0TO3h0UBx6IaUZI0QdZO1H+BH1nmSR1ewKgvhxrJ6NZT60GzK1J
3n7bECqg5X2rfuSb7dO6VyiTywmuxGksfa2AvBiuN2o0QlA=
-----END CERTIFICATE REQUEST-----",
				OperationId = Guid.NewGuid().ToString()
			};
			var third = new SystemToSystemTrustInfo()
			{
				Certificate = ZBlob.FromAscii("CERTIFICATE"),
				PrivateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAvTQ1eKPDlt7Akq2q89lv+TPjL+r/Oxlgs+Lugu/7WyFlQPAf
eevry+q7F9gAjblSEDbeKToWTtDuGkPYZOtU3hK5Kk6i2C7tvOeMf7tqYT9Iq76P
NNhW0FHm90W0hWzdl4WzSFi+/RqD50UaXv8sMHJrpEdCcV8PVw35+E3t2AYNdx5a
nXKiXumBbak7q+Aq4eOadmSDTkZOvrEiqdiy6sBBwKe64XymDv6+cbwIt4OPIvmn
b4oxXyCNpKutCCNdGVYNWW00M9prCsCeEkdEw8GBx+cCu41U5Exb15CJ1cuL/kmK
BsjOfQUN/Q81pDnh6DSvyYYbBa62Evt+94MN7wIDAQABAoIBAQCaGGt0Vex2c/Vj
gQ46jF0mGZxu6nX1LDYWTDz1Z37QOeK9gNQh+IOTERpP0RyiUM0FZ6qI6UZB6ugB
fEBB8WitdZJZYrI+X/pAYFWNspZxnZkSR6Fa6NfL0ujJe3wLLx9KyRm7Uow6l93V
/fH8bNcQniANK/xxWXqk7D0qfk24Ic66/zWXskbw6FegrhZ6LmiT/wsf0G0D7he8
ywcsqv5bfOax25SMhQ5MZUG0vignfH+9177hvfID0817f11rBM2Of5cG3tCcPLMm
LtK1heBgBT4DPI2ssr7lpQoKGZchUl8jKEvsaPmc1Q1Yzcfeq5OMvuH3z3z8dnNg
ibuSDZQBAoGBAOlPwzmqcD9H+rSK4heAnZTHAXzYYkHeRan/4TZaeBrte/VkyIy5
oZyde2KQ0ozFWvn0MIXXuFzcK0h9PrGgwJAXzioHY2/sKo3+EORd1kkm/5NGogNe
9SPGvNEkTXQMT3maT5GFOXxj5UzX642neB9WdaUbFBmknhHRDFKQs7b1AoGBAM+a
ZaOaG7ej57MO87EsxX067GcjZuAJCvKBj8AmATtCDB16P9cYYpUGI4V/6kQ05heq
YCsjzlvetaXFchStg643zHc90+/kK34IiHgmNBnRriR8/4txKI7tMk5Po082QSJn
AqWHcfpIBIpJiWiKoUn5Gfuvb5p9+p5QrpQE2PrTAoGAOsLmDo4Iu0drszEPhI/W
IHSGwWTWSnSq4wgZNtFOUqnhgIqjoB3YwFNBki+bd+z03uNLnUoZmvmwxQ8WTTKJ
jUERobA+sR+wEBcfNgUURVRXJkax41t1Lk+Nmrcj8shu89eTlrkRI20dgV8YapL1
RB9Ifg713weA8EfQbLMZMEkCgYEAx9HskpP6qb9xfL+ff6GAVREqCfvhQTJy/wgF
vQTXmpnv30+Tsw0dliLafdypOA1SiuTKu7szHOo4HN7290ArxryeaQdxvyz3T9AW
ys66xz8zRpupvCWmzCsyiH3OyqeF+f78ZScBZc170T8Gq0EEDZdekwpBeYpYd5lg
78fasVECgYAMKazsFA5W1i/RQQuwPLtRWL9KUrUVxApb/KPHQnYfCngC1yfh9DzI
uXO3wlO8rHBR1PodcbGi/JZMJTAmnVrJO5MFCQ6fixl7Da8C66D2baKhpz22btOO
bl7N795AsX2VYpkKWv9TXd+vJ+n2V2XYlSz92Cg4YtX42SSJRVd09g==
-----END RSA PRIVATE KEY----",
				ClientId = Guid.NewGuid().ToString(),
				TenantId = Guid.NewGuid().ToString()
			};
#pragma warning restore CS0618 // To be replaced with S2ST library once WI00771920 is implemented

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(first, new SystemToSystemTrustRegistryDataType().Serialise(first)),
				new ValidSampleAndBinaryValueInDB(second, new SystemToSystemTrustRegistryDataType().Serialise(second)),
				new ValidSampleAndBinaryValueInDB(third, new SystemToSystemTrustRegistryDataType().Serialise(third)),
			};
		}
	}
}
