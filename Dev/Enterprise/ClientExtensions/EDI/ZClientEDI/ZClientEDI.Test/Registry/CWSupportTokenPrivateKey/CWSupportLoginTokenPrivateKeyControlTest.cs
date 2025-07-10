using System.IO;
using System.Text;
using Enterprise.Client.EDI.Registry.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace ZClientEDI.Test.Registry
{
	sealed class CWSupportLoginTokenPrivateKeyControlTest : TestCase
	{
		public void TestValidPrivateKey()
		{
			using (var control = new CWSupportLoginTokenPrivateKeyControlForTest(ValidPrivateKey))
			{
				AssertEquals(DataLoadState.NoData, control.State);

				var loadButton = control.Controls["LoadButton"] as ZButton;
				loadButton.PerformClick();
				AssertEquals(DataLoadState.DataExists, control.State);
				var content = Encoding.UTF8.GetString(control.FileDataAsBinary);
				AssertEquals(ValidPrivateKey, content);

				var clearButton = control.Controls["ClearButton"] as ZButton;
				clearButton.PerformClick();
				AssertEquals(DataLoadState.DataCleared, control.State);
				AssertNull(control.FileDataAsBinary);
			}
		}

		public void TestValidRsaPrivateKey()
		{
			using (var control = new CWSupportLoginTokenPrivateKeyControlForTest(ValidRsaPrivateKey))
			{
				AssertEquals(DataLoadState.NoData, control.State);

				var loadButton = control.Controls["LoadButton"] as ZButton;
				loadButton.PerformClick();
				AssertEquals(DataLoadState.DataExists, control.State);
				var content = Encoding.UTF8.GetString(control.FileDataAsBinary);
				AssertEquals(ValidRsaPrivateKey, content);

				var clearButton = control.Controls["ClearButton"] as ZButton;
				clearButton.PerformClick();
				AssertEquals(DataLoadState.DataCleared, control.State);
				AssertNull(control.FileDataAsBinary);
			}
		}

		public void TestInvalidPrivateKey()
		{
			using (var control = new CWSupportLoginTokenPrivateKeyControlForTest("invalidprivatekeycontent"))
			{
				AssertEquals(DataLoadState.NoData, control.State);

				var loadButton = control.Controls["LoadButton"] as ZButton;
				loadButton.PerformClick();
				AssertEquals(DataLoadState.NoData, control.State);
				AssertEquals("Invalid Data, please upload a file with PEM data.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(control.FileDataAsBinary);

				var clearButton = control.Controls["ClearButton"] as ZButton;
				clearButton.PerformClick();
				AssertEquals(DataLoadState.NoData, control.State);
			}
		}

		const string ValidPrivateKey = @"
-----BEGIN PRIVATE KEY-----
MIIEvQIBADANBgkqhkiG9w0BAQEFAASCBKcwggSjAgEAAoIBAQCyrpE0VrQNxG8h
zQmpeQfkjRu80jvy7N6pTwbHeocNyV2sAkufUV7xuztamiWjCBUjEVTzdCLeAxcC
lQ7LmhU6nGjVTsNK6DaPQ2Zl67L2yJClMWdvZzuOxRx6JPFNa/XyboRLexIeqesz
n1W98T8mPBTs3rOrZXvyKLx5Jth+LISyw+Mo67yaouSdiw1GiucWBqIuBR49vyda
Mix+Ce3pK/YTIFnglFEzj7Dnxa6GhAe4fPTLUd02nu6+Ut5t8fHJUn0EB799+eAB
UqV0ZatdvlOlISmcJEXWL1/0IoEfGH4VDKD8zOb3sUOsOPLJBgGicpfb9LIfPfkh
5lqL1V0dAgMBAAECggEAK9R+ceRCzo285QGyuQujUAD9KNg5NGG+TLHB6/S2ZD9c
5vC5NB91tr5C1Pqy+MbmyG9b80wtsV/4qP1/X5owUuxDGu/zH9DOcV4LJD0o7ThN
ovf3c3BTP7ZCQgQF3QP6lLlfYlSSIUt1EninQ6yF3Q8n4uLOF+ERAlnTwbQxruE7
S442554sdsjRXzHX7+ql/XCZwZzE8NlSPBJTaLc1Df8GpPAyOvSUW2Dk08JkUNT5
iyqAK+NnGqGAd3qZGQ0AdFALk9v0mppvPhI0ldBjMkk1UjDRYswLDw+heb/53Smp
4pJJsHT2U1h5VhLMrcB/Yq+817cLWt8PI8zxRrIDUQKBgQDsDiEA3pj8m9PS3IKf
Yba5l4TztE62aHwJiQ1gLlVXpPk6EdECDHFZWnMQlre/ZT9e0hecd8P3aaeMs8Vi
zjOrf2J7nbZIlTgWHWeC48K9eUd7l/+UotvsmJqeHmTo3ZIsKyS1LOE7cUodyfCL
tQYfzEm4qrs11vRaciI3Zun8GwKBgQDBx3RoykYzRb9VZogHcLsdP/pmpzdJ3pb0
9viZr+kb8rpWBq3R0qB2iv+0TqV4F4B/XfSMI1eUbTgteafofQ70i0h7ovcypgRz
UUORho6Hrd58Znbxd/Q9CW8KLFiTps+moKaw2S2dwu9CME9dlBOL6/VIz7Sgn4O+
XMRf+yUvJwKBgQCs5YRi4KfpjjFOZtj96FIwCb0Fy3FDxa/kRBAZ/JXhxiIN2HLg
L0Duk4NoCRy5AW2zA+rrXgWZODfSpPHUdvf9iyYVKOUUsMcN26evhSdkJGqpKiG3
Oroex3+ohNaggXnJBCi00xR9t3Lz8q9PhN3heH4e1l6dBr6faK2LKsQDNQKBgDQT
Mclncm4c9Eoy/6NgPCikJNqpXUZQtyilpjFHANIt7L1plhSpEc5JlGYULIuVZUbV
LP7sEIEmyM4Pv3vO/9HgDF6NcPj/fHqxAAN/sZXst7men6BMqCou+tQ1Dqi/T1Zs
Hd+wvX2EAWA8M1fmj0ou4v/qMZRoybLCo1NX3qpJAoGAZ99dvWIPQerXdRpeN+za
rtLOfO5gWjdD6qvmGUZGBn9YPwC7pcDTSwGBuJzqlrKxUdLBYK2RMqQ5jEBPJugC
ZJD/BpVIQlyHFH2rsVBxFJetT2YreniKyt1Onqrps6DwTyGZd0MtzHhUj+K+a7JZ
PGEiRH+A0BGHG1fWlRKD/4A=
-----END PRIVATE KEY-----
";

		const string ValidRsaPrivateKey = @"
-----BEGIN RSA PRIVATE KEY-----
MIIEpQIBAAKCAQEAmuM45SxPDkoVVmAduAQGB3tGJqI2XWdT2yziuttXG1w6hgqU
zF+5y5iUXW/JdK/M98ENsCGIGG6elf/Jk1QKVjlEkMlEcH3hBOEdnUDAimetB9E8
97kM9OTMyGY9xzVs56ZPDdfanczD76NFrN/6sRAw4lrCE+x6726dSNmycE0ZQUGc
4JmEUXqFDOxf8oODsIdKbYcJ7c7f8gAKA0wbnnnUcizZ3GF4LNKz9nOsEOWLeZMv
twEhQL9SJzthdWM9UW6GudmI9pzN+kaaMoo/9BU3TcEAsHVZVLJljg9eV4zyoava
1s/Ih+UY4hKx3mqyqtC6EzlC3MbRiVa4P/2PDwIDAQABAoIBAQCKKhkNranTyFgi
VdkM1mH+eIPKYLb4OPz/rZmPL6wTVwFJotS7PsUBDdmDQ/3EHjJL66VnMXIywTKs
AaFxz+zsn2c/dJqdclywupNtPF7E19js+URWgndBWXwnY7TGKC0+swcgBSYZz5gV
A2Na4+2/1v6UXHt1xGShgt3BJ5jaPTkAKJwrgztzRMNBUcbwlhYBBqgdvtsDVZZK
4d0JNH9d4M4LQ3ibTVFkKpQeYVbBVxXVdjEDolhR0ZtS1Tai4yrNqvMi1TAGYjCA
Yr/v+8Kiw+V9uXFvYzokwFh8fKUcy1OURXcnVtBz2MuEql6idjYlQfPd4RFO1t88
uo8E9eqpAoGBAMfqYKPy+LawZkGHG4hv80BLJNpCn6W3jB6qnni9OWGrR61TUNNA
ohedGe9NuMvt4Gp312q/eOt35bcQCl19WO+SH/I2c7TblAL7lytfstjLBM47DGHL
a1P7FA7SdY2G2kdbQ7oyCHr75ySE2VE8IQ99124q+rLzdTvuXseMYtUlAoGBAMZX
Aese6XEqKZUvpDyAQpPTDIsvJtA6RO/3LeVRv+QUHcuU9oByKj9rSweOnJggMSDU
R4trRYJyYOO/zDNNCqjlin1rYKqU39Y3N4fKFJQ/oG8pWX5Fn6KsjwvckvSf6gEy
aGAQvhui+epfqMpuVyO5ItJolqLLQ9EHq0UHpE8jAoGAd7z81xXPO/TZukPHDOgo
pQic8RFYd7RA+5krw5tqhAJHsxuk3HLo4wXYo8lEdvhqIrrWznwSW857SzMTUj2i
iBNYiQVuNhQSSdsRBwyRnWGz5iXd27Ev/qPytpH9kKAFmxmhriMLi0XXgrsDQrGm
B5ZS8wxRDd/eBuBV6mU+SPUCgYEAtyBSlyI9a80AA8yvGWVbE13DsSbh8VQ7nkxc
xeTLBe1E7Vkml8XRGCkaZ2nXT+Y5NhSJ0kJDNYQhzWajRaKWewck7VQdNqKqYQEu
dmOwyIhOuxQ0mw/xqqMkmFEaJl8YEe7WzAvpW18I3Mth7zYdotAnRJpoqvp+LRul
mODLZn0CgYEAvH5ljMnhdvQzPKOJ2krypMtpZ7kSKEE14mt0fq//+S6VMbo8T82/
AnR4WqUssm53nZCD0wi9hexxaKkpy6/dFxx5X3ACKUoaJahS0TPt3rfdKf+Wx/Zc
Pt71WbnS+Kk1Jri94DkCgNaYcIDx7iHWrGX5ya+2TUHFbsUVm8DEgMc=
-----END RSA PRIVATE KEY-----
";
	}

	sealed class CWSupportLoginTokenPrivateKeyControlForTest : CWSupportLoginTokenPrivateKeyControl
	{
		public CWSupportLoginTokenPrivateKeyControlForTest(string fileContent) : base()
		{
			this.fileContent = fileContent;
		}

		readonly string fileContent;

		protected override Stream LoadFile()
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(fileContent);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}
	}
}
