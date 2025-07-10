using System;
using System.Text;
using System.Windows.Forms;
using Enterprise.Client.EDI.Registry.GUI;
using Enterprise.Integration;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ZClientEDI.Business.Registry;

namespace ZClientEDI.Test.Registry
{
	[TestedType(typeof(CWSupportLoginTokenPrivateKeyRegistryEditor))]
	sealed class CWSupportLoginTokenPrivateKeyRegistryEditorTest : RegistryItemEditorTestCase
	{
		protected override RegistryItemEditor GetEditor()
		{
			return new CWSupportLoginTokenPrivateKeyRegistryEditor(new BinaryRegistryDataType());
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((CWSupportLoginTokenPrivateKeyControl)editorPane).ReadOnly;
		}

		protected override bool CanNotHaveReferenceEquality => false;

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CWSupportLoginTokenPrivateKeyControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			var result = new BinaryRegistryItem("", null, null, null, RegistryStorageFlags.System);
			result.EditorInfo = new CWSupportLoginTokenPrivateKeyEditorInfo();
			return result;
		}

		protected override object[] GetValidRegistryValues()
		{
			return new object[]
			{
				Encoding.UTF8.GetBytes(ValidPrivateKey)
			};
		}

		const string ValidPrivateKey = @"-----BEGIN PRIVATE KEY-----
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
	}
}
