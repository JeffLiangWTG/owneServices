using System;
using System.Net;
using System.Reflection;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class SecureQueryStringTest : TestCase
	{
		public void TestEncodeDecode()
		{
			QueryString qs = new QueryString();
			qs.Add("Name1", "Value1");
			qs.Add("Name2", "Value2");
			qs.Add("Nam&+=e3", "Val&+=ue3");
			QueryString deserializedQS = new QueryString(qs.ToString());
			AssertEquals("Value1", deserializedQS["Name1"]);
			AssertEquals("Value2", deserializedQS["Name2"]);
			AssertEquals("Val&+=ue3", deserializedQS["Nam&+=e3"]);
		}

		public void TestEncodeDecode_WithEmptyValue()
		{
			QueryString qs = new QueryString();
			qs.Add("EmptyValue", "");
			QueryString deserializedQS = new QueryString(qs.ToString());
			AssertEquals("", deserializedQS["EmptyValue"]);
		}

		public void TestUrlEncodeNameAndValue()
		{
			QueryString qs = new QueryString();
			qs.Add("Name1", "Value1");
			qs.Add("Nam&=e2", "Val&=ue2");
			qs.UrlEncodeNameAndValue = true;
			AssertEquals("Serializing when UrlDecodeNameAndValue=true", "Name1=Value1&Nam%26%3de2=Val%26%3due2", qs.ToString());
			qs.Deserialize(qs.ToString());
			AssertEquals("Deserializing when UrlDecodeNameAndValue=true", "Value1", qs["Name1"]);
			AssertEquals("Deserializing when UrlDecodeNameAndValue=true", "Val&=ue2", qs["Nam&=e2"]);
			qs.UrlEncodeNameAndValue = false;
			qs.EscapeAmpersandAndEquals = true;
			AssertEquals("Serializing when UrlDecodeNameAndValue=false", "Name1=Value1&Nam&&==e2=Val&&==ue2", qs.ToString());
			qs.Deserialize(qs.ToString());
			AssertEquals("Deserializing when UrlDecodeNameAndValue=false", "Value1", qs["Name1"]);
			AssertEquals("Deserializing when UrlDecodeNameAndValue=false", "Val&=ue2", qs["Nam&=e2"]);
		}

		public void TestEscapeAmperstandAndEquals()
		{
			QueryString qs = new QueryString();
			qs.Add("Name1&=", "Value1=&");
			qs.Add("&=Name2", "=&Value2");
			qs.EscapeAmpersandAndEquals = true;
			AssertEquals("Serializing when EscapeAmperstandAndEquals=true", "Name1&&===Value1==&&&&&==Name2===&&Value2", WebUtility.UrlDecode(qs.ToString()));
			qs.Deserialize(qs.ToString());
			AssertEquals("Deserializing when EscapeAmperstandAndEquals=true", "Value1=&", qs["Name1&="]);
			AssertEquals("Deserializing when EscapeAmperstandAndEquals=true", "=&Value2", qs["&=Name2"]);
			qs.EscapeAmpersandAndEquals = false;
			AssertEquals("Serializing when EscapeAmperstandAndEquals=false", "Name1%26%3d=Value1%3d%26&%26%3dName2=%3d%26Value2", qs.ToString());
			qs.Deserialize(qs.ToString());
			AssertEquals("Deserializing when EscapeAmperstandAndEquals=false", "Value1=&", qs["Name1&="]);
			AssertEquals("Deserializing when EscapeAmperstandAndEquals=false", "=&Value2", qs["&=Name2"]);
		}

		public virtual void TestDecode_InvalidQueryStringFormat()
		{
			TestDecode_InvalidQueryStringFormat("askldfj asdf%#$%!*(@#$*iouwer jks");
		}

		void TestDecode_InvalidQueryStringFormat(string encodedQueryString)
		{
			if (Encoded)
			{
				try
				{
					QueryString qs = new SecureQueryString(encodedQueryString);
					Fail("InvalidQueryStringException should be thrown");
				}
				catch (InvalidQueryStringException)
				{
				}
			}

			Assert(true);
		}

		public void TestQueryStringEmptyString()
		{
			QueryString qs = new SecureQueryString("");
			AssertNotNull(qs);
			AssertEquals("Count", 0, qs.Count);
			AssertNull(qs["SomeName"]);
		}

		public void TestQueryStringNullString()
		{
			QueryString qs = new SecureQueryString(null);
			AssertNotNull(qs);
			AssertEquals("Count", 0, qs.Count);
			AssertNull(qs["SomeName"]);
		}

		public void TestSerializeEmptyQueryString()
		{
			QueryString qs = new SecureQueryString();
			AssertEquals("Empty query string should be empty", "", qs.SerializeCore());
		}

		protected bool Encoded
		{
			get
			{
				return new SecureQueryString().GetType().GetMethod("Encode", BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType != typeof(QueryString);
			}
		}

		[ExpectException(typeof(ExpiredQueryStringException))]
		public void TestQueryStringThrowsExpiredQueryStringException()
		{
			SecureQueryString qs = new SecureQueryString();
			qs.Add("test1", "value1");
			qs.Add("test2", "value2");
			qs.ExpireTime = TimeSpan.FromMinutes(-1);
			string encodedString = qs.ToString();
			SecureQueryString unused = new SecureQueryString(encodedString);
		}

		public void TestEncryptionDecryptionWorksWithSymbols()
		{
			SecureQueryString qs = new SecureQueryString();
			string value1 = "value1+!@#$%^&*()";
			string value2 = "value2+!@#$%^&*()";
			qs.Add("test1", value1);
			qs.Add("test2", value2);
			string encodedString = qs.ToString();
			SecureQueryString decodedData = new SecureQueryString(encodedString);
			AssertEquals(decodedData["test1"], value1);
			AssertEquals(decodedData["test2"], value2);
		}
	}
}
