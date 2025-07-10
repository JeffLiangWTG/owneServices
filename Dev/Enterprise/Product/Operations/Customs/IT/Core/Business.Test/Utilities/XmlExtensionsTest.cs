using System;
using System.Xml;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

public sealed class XmlExtensionsTest : TestCase
{
	public void TestDeserializeEmptyString()
	{
		AssertExceptionThrown<ArgumentException>(() => ZString.Empty.DeserializeToObject<DummyObject>());
	}

	public void TestDeserializeInvalidXml()
	{
		AssertExceptionThrown<InvalidOperationException>(() => new ZString("<bad></xml>").DeserializeToObject<DummyObject>());
	}

	public void TestDeserializeValidXml()
	{
		var xml = new ZString("<DummyObject><DummyProperty>DummyValue</DummyProperty></DummyObject>");
		var deserializedObject = xml.DeserializeToObject<DummyObject>();
		AssertEquals("DummyValue", deserializedObject.DummyProperty);
	}

	public void TestSerializeObject()
	{
		var o = new DummyObject();
		o.DummyProperty = "DummyValue";

		var actualResult = o.SerializeToXml();
#if NETFRAMEWORK
		var expectedResult = "ï»¿<?xml version=\"1.0\" encoding=\"utf-8\"?><DummyObject><DummyProperty>DummyValue</DummyProperty></DummyObject>";
#else
		var expectedResult = "<?xml version=\"1.0\" encoding=\"utf-8\"?><DummyObject><DummyProperty>DummyValue</DummyProperty></DummyObject>";
#endif
		AssertEquals("Xml for Dummy object", expectedResult, RemoveStrangeXmlPrefix(actualResult.ToString()));
	}

	string RemoveStrangeXmlPrefix(string xml)
	{
		return xml.Replace("﻿ï»¿", "").Replace("﻿", "");
	}

	public void TestSerializeObjectWithCustomWriterConfigurationSetting()
	{
		var o = new DummyObject();
		o.DummyProperty = "DummyValue";

		var xmlWriterSettings = new XmlWriterSettings()
		{
			Indent = true,
			IndentChars = "*",
			NewLineChars = "*",
			Encoding = new System.Text.UTF8Encoding(false),
		};

		var result = o.SerializeToXml(xmlWriterSettings);
		var expectedString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>*<DummyObject>**<DummyProperty>DummyValue</DummyProperty>*</DummyObject>";
		AssertEquals("Xml for Dummy object", expectedString, result);
	}

	#region Dummy Object

	public class DummyObject
	{
		public ZString DummyProperty { get; set; }
	}

	#endregion
}
