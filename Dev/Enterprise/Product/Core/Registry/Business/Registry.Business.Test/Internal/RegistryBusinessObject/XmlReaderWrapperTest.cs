using System;
using System.Xml;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	sealed class XmlReaderWrapperTest : TestCase
	{
		[ExpectExceptionMessage(typeof(ArgumentNullException), @"Value cannot be null.
Parameter name: Reader")]
		public void TestConstructor()
		{
			new XmlReaderWrapper(null);
		}

		public void TestCasting()
		{
			using (System.IO.StringReader sr = new System.IO.StringReader(@"<hello>Hi</hello>"))
			{
				XmlReader reader = new XmlTextReader(sr);
				XmlReaderWrapper wrapper = reader;
				AssertEquals(reader, wrapper.Reader);

				XmlReader reader2 = wrapper;
				AssertEquals(reader, reader2);
			}
		}

		public void TestReadElementStringAsZBool()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("Y");
			AssertEquals(ZBool.True, wrapper.ReadElementStringAsZBool("hello"));
			AssertEquals(ZBool.False, wrapper.ReadElementStringAsZBool("hello2"));
		}

		public void TestReadElementStringAsZDecimal()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("45.34");
			AssertEquals(45.34m, wrapper.ReadElementStringAsZDecimal("hello"));
			AssertEquals(ZDecimal.Zero, wrapper.ReadElementStringAsZDecimal("hello2"));
		}

		public void TestReadElementStringAsZInt()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("45");
			AssertEquals(45, wrapper.ReadElementStringAsZInt("hello"));
			AssertEquals(ZInt.Zero, wrapper.ReadElementStringAsZInt("hello2"));
		}

		public void TestReadElementStringAsZShort()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("45");
			AssertEquals((ZShort)45, wrapper.ReadElementStringAsZShort("hello"));
			AssertEquals(ZShort.Zero, wrapper.ReadElementStringAsZShort("hello2"));
		}

		public void TestReadElementStringAsZDateTime()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("20070522");
			AssertEquals(new ZDateTime(2007, 05, 22), wrapper.ReadElementStringAsZDateTime("hello", "yyyyMMdd"));
			AssertEquals(ZDateTime.Empty, wrapper.ReadElementStringAsZDateTime("hello2", "yyyyMMdd"));
		}

		public void TestReadElementString()
		{
			XmlReaderWrapper wrapper = GetNewXmlReaderWrapper("Hi");
			AssertEquals("Hi", wrapper.ReadElementString("hello"));
			AssertEquals("", wrapper.ReadElementString("hello2"));
		}

		XmlReaderWrapper GetNewXmlReaderWrapper(string value)
		{
			return new XmlReaderWrapper(new XmlTextReader(new System.IO.StringReader("<hello>" + value + "</hello>")));
		}
	}
}
