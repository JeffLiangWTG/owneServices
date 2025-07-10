using System;
using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Xml.Testing
{
	sealed class RootNamespaceRenamingXmlReaderTest : TestCase
	{
		public void TestNamespaceRemoved_PassingInNamespace()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<x xmlns='input_ns'><aha xmlns='nested_input_ns'></aha></x>"));
			RootNamespaceRenamingXmlReader readerWithNewNamespace = new RootNamespaceRenamingXmlReader(reader, "new_ns");

			readerWithNewNamespace.MoveToContent();
			AssertEquals("NamespaceURI", "new_ns", readerWithNewNamespace.NamespaceURI);
			AssertEquals("IsStartElement", true, readerWithNewNamespace.IsStartElement("x", "new_ns"));
			readerWithNewNamespace.ReadStartElement("x", "new_ns");

			readerWithNewNamespace.MoveToContent();
			AssertEquals("IsStartElement", true, readerWithNewNamespace.IsStartElement("aha", "nested_input_ns"));
			AssertEquals("Should return same namespace as it is not the root element", "nested_input_ns", readerWithNewNamespace.NamespaceURI);
			readerWithNewNamespace.ReadStartElement("aha", "nested_input_ns");
		}

		public void TestNamespaceRemoved_NotPassingInNamespace()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<x xmlns='input_ns'><aha xmlns='nested_input_ns'></aha></x>"));
			RootNamespaceRenamingXmlReader readerWithNewNamespace = new RootNamespaceRenamingXmlReader(reader, "new_ns");

			readerWithNewNamespace.MoveToContent();
			AssertEquals("NamespaceURI", "new_ns", readerWithNewNamespace.NamespaceURI);
			AssertEquals("IsStartElement", true, readerWithNewNamespace.IsStartElement("x"));
			readerWithNewNamespace.ReadStartElement("x");

			readerWithNewNamespace.MoveToContent();
			AssertEquals("IsStartElement", true, readerWithNewNamespace.IsStartElement("aha"));
			AssertEquals("Should return same namespace as it is not the root element", "nested_input_ns", readerWithNewNamespace.NamespaceURI);
			readerWithNewNamespace.ReadStartElement("aha");
		}

		[ExpectException(typeof(NotImplementedException))]
		public void TestReadOuterXml_NotImplementedException()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<x xmlns='input_ns'><aha xmlns='nested_input_ns'></aha></x>"));
			RootNamespaceRenamingXmlReader readerWithNewNamespace = new RootNamespaceRenamingXmlReader(reader, "new_ns");
			readerWithNewNamespace.ReadOuterXml();
		}

		public void TestReadOuterXml_SupportedInNestedElement()
		{
			XmlTextReader reader = new XmlTextReader(new StringReader("<x xmlns='input_ns'><aha xmlns='nested_input_ns'></aha></x>"));
			RootNamespaceRenamingXmlReader readerWithNewNamespace = new RootNamespaceRenamingXmlReader(reader, "new_ns");
			readerWithNewNamespace.ReadStartElement("x", "input_ns");
			string outerXml = readerWithNewNamespace.ReadOuterXml();
			AssertNotNull("Should return something in ReadOuterXml", outerXml);
		}
	}
}
