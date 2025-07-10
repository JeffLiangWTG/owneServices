using System.Collections;
using System.IO;
using System.Xml;
using Enterprise.eHubMessaging.Business;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests
{
	class XPathReaderTest : TestCase
	{
		public void TestChildAxisEmptyElement()
		{
			string xmlDocument = "<e/>";

			var collection = new XPathCollection();
			var query = new XPathQuery("/e");

			int xpath1 = collection.Add(query);
			int xpath2 = collection.Add("child::node()");
			int xpath3 = collection.Add("e");

			var reader = new XmlTextReader(new StringReader(xmlDocument));
			var xpathReader = new XPathReader(reader, collection);

			while (xpathReader.Read())
			{
				if (xpathReader.NodeType == XmlNodeType.Element || xpathReader.NodeType == XmlNodeType.EndElement)
				{
					Assert(CheckQuery(xpathReader, xpath1));
					Assert(CheckQuery(xpathReader, xpath2));
					Assert(CheckQuery(xpathReader, xpath3));
				}
			}
		}

		public void TestChildAxis()
		{
			string xmlDocument = "<e>test</e>";

			var collection = new XPathCollection();
			var query = new XPathQuery("/e");

			int xpath1 = collection.Add(query);
			int xpath2 = collection.Add("child::node()");
			int xpath3 = collection.Add("e");
			int xpath4 = collection.Add("e/child::text()");

			var reader = new XmlTextReader(new StringReader(xmlDocument));
			var xpathReader = new XPathReader(reader, collection);

			while (xpathReader.Read())
			{
				switch (xpathReader.NodeType)
				{
					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
						Assert(CheckQuery(xpathReader, xpath1));
						Assert(CheckQuery(xpathReader, xpath2));
						Assert(CheckQuery(xpathReader, xpath3));
						break;

					case XmlNodeType.Text:
						Assert(xpathReader.Match(xpath4));
						AssertEquals("test", reader.Value);
						break;

					default:
						break;
				}
			}
		}

		public void TestChildAxisWithPrefixWithoutNamespaceMgr()
		{
			string xmlDocument = "<p:e xmlns:p='foo'>test</p:e>";

			var collection = new XPathCollection();
			var query = new XPathQuery("/p:e");

			int xpath1 = collection.Add(query);
			int xpath2 = collection.Add("child::node()");
			int xpath3 = collection.Add("p:e");
			int xpath4 = collection.Add("p:e/child::text()");

			var reader = new XmlTextReader(new StringReader(xmlDocument));
			var xpathReader = new XPathReader(reader, collection);

			while (xpathReader.Read())
			{
				switch (xpathReader.NodeType)
				{
					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
						Assert(!CheckQuery(xpathReader, xpath1));
						Assert(CheckQuery(xpathReader, xpath2));
						Assert(!CheckQuery(xpathReader, xpath3));
						break;

					case XmlNodeType.Text:
						Assert(!xpathReader.Match(xpath4));
						break;

					default:
						break;
				}
			}
		}

		public void TestChildAxisWithPrefixWithNamespaceMgr()
		{
			string xmlDocument = "<p:e xmlns:p='foo'>test</p:e>";
			var nsManager = new XmlNamespaceManager(new NameTable());
			nsManager.AddNamespace("p", "foo");

			var collection = new XPathCollection(nsManager);
			var query = new XPathQuery("/p:e");

			int xpath1 = collection.Add(query);
			int xpath2 = collection.Add("child::node()");
			int xpath3 = collection.Add("p:e");
			int xpath4 = collection.Add("p:e/child::text()");

			var reader = new XmlTextReader(new StringReader(xmlDocument));
			var xpathReader = new XPathReader(reader, collection);

			while (xpathReader.Read())
			{
				switch (xpathReader.NodeType)
				{
					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
						Assert(CheckQuery(xpathReader, xpath1));
						Assert(CheckQuery(xpathReader, xpath2));
						Assert(CheckQuery(xpathReader, xpath3));
						break;

					case XmlNodeType.Text:
						Assert(xpathReader.Match(xpath4));
						break;

					default:
						break;
				}
			}
		}

		public void TestParentNotSupported()
		{
			AssertExceptionThrown("xpath is not supported!", typeof(XPathReaderException), () => { var xp = new XPathQuery("/Root/e/parent::node()"); });
		}

		public void TestParentShortSyntaxNotSupported()
		{
			AssertExceptionThrown("xpath is not supported!", typeof(XPathReaderException), () => { var xp = new XPathQuery("/Root/e/.."); });
		}

		public void TestLoanBook()
		{
			var reader = new XmlTextReader(new StringReader(@"
               <books>
                  <book publisher='IDG books' on-loan='Sanjay'>
                     <title>XML Bible</title>
                     <author>Elliotte Rusty Harold</author>
                  </book>
                  <book publisher='Addison-Wesley'>
                     <title>The Mythical Man Month</title>
                     <author>Frederick Brooks</author>
                  </book>
                  <book publisher='WROX'>
                     <title>Professional XSLT 2nd Edition</title>
                     <author>Michael Kay</author>
                  </book>
                  <book publisher='APress'>
                     <title>A Programmer's Introduction to C#</title>
                     <author>Eric Gunnerson</author>
                  </book>
                </books>"));

			var collection = new XPathCollection();

			int onloanXpath = collection.Add("/books/book[@on-loan]");
			int titleXpath = collection.Add("/books/book[@on-loan]/title");
			int authorXpath = collection.Add("/books/book[@on-loan]/author");

			var xpr = new XPathReader(reader, collection);
			int counter = 0;
			while (xpr.ReadUntilMatch())
			{
				if (xpr.NodeType == XmlNodeType.Element)
				{
					if (xpr.Match(onloanXpath))
					{
						counter += 1;
					}
					else if (xpr.Match(titleXpath))
					{
						counter += 2;
					}
					else if (xpr.Match(authorXpath))
					{
						counter += 3;
					}
				}
			}
			AssertEquals(6, counter);
		}

		public void TestDifferentNamespaces()
		{
			var collection = new XPathCollection();
			collection.NamespaceManager = new XmlNamespaceManager(new NameTable());
			collection.NamespaceManager.AddNamespace("xe", "http://PropPromotion.XmlEnvelope");
			collection.NamespaceManager.AddNamespace("xb", "http://PropPromotion.XmlBody");

			// 1) /XmlEnvelope/Attributes/@integer
			collection.Add("/xe:XmlEnvelope/xe:Attributes/@integer");
			// 2) /XmlEnvelope/Attributes/@language
			collection.Add("/xe:XmlEnvelope/xe:Attributes/@language");
			// 3) /XmlEnvelope/FieldElements/anyuri
			collection.Add("/xe:XmlEnvelope/xe:FieldElements/@anyuri");
			// 4) /XmlEnvelope/FieldElements/boolean
			collection.Add("/xe:XmlEnvelope/xe:FieldElements/xe:boolean");
			// 5) /XmlEnvelope/Body
			collection.Add("/xe:XmlEnvelope/xe:Body");
			// 6) /XmlEnvelope/Body/Attributes/@integer
			collection.Add("/xe:XmlEnvelope/xe:Body/xb:XmlBody/xb:Attributes/@integer");
			// 7) /XmlEnvelope/Body/Attributes/@language
			collection.Add("/xe:XmlEnvelope/xe:Body/xb:XmlBody/xb:FieldElements/xb:anyuri");

			var reader = new XmlTextReader(new StringReader(@"
              <XmlEnvelope xmlns='http://PropPromotion.XmlEnvelope'>
                 <Attributes integer='57604' language='en-us'/>
                    <FieldElements>
                       <anyuri>ftp://bla.bla</anyuri>
                       <boolean>1</boolean>
                    </FieldElements>
                    <Body>
                       <XmlBody xmlns='http://PropPromotion.XmlBody'>
                          <Attributes integer='-3368' language='fr-ca' />
                          <FieldElements>
                             <anyuri>ftp://yes.yes</anyuri>
                             <boolean>false</boolean>
                          </FieldElements>
                       </XmlBody>
                    </Body>
               </XmlEnvelope>"));

			var xpathReader = new XPathReader(reader, collection);
			var matchList = new ArrayList();
			int counter = 0;
			while (xpathReader.ReadUntilMatch())
			{
				counter++;
				xpathReader.MatchesAny(matchList);
				AssertEquals(1, matchList.Count);
			}

			AssertEquals(9, counter);
		}

		#region Implementation

		bool CheckQuery(XPathReader reader, int index)
		{
			return reader.Match(index);
		}

		#endregion

	}
}
