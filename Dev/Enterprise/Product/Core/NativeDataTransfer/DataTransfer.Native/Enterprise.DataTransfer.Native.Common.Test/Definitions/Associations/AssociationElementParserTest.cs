using System.Xml.Linq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.Associations
{
	public class AssociationElementParserTest : TestCase
	{
		public void TestBuildThroughTable()
		{
			var testElement = new XElement("Test",
				new XAttribute("Through", "ThroughTable"));
			var result = builder.BuildThroughTable(testElement);
			AssertEquals("ThroughTable", result);
		}

		public void TestBuildChildName()
		{
			var testElement = new XElement("Test",
				new XAttribute("From", "ChildEntity"));
			var result = builder.BuildOwnerEntity(testElement);
			AssertEquals("ChildEntity", result);
		}

		public void TestBuildParentName()
		{
			var testElement = new XElement("Test",
				new XAttribute("To", "ParentEntity"));
			var result = builder.BuildReferEntity(testElement);
			AssertEquals("ParentEntity", result);
		}

		public void TestBuildChildKey()
		{
			var testElement = new XElement("Test",
				new XAttribute("ChildFK", "ChildFK"));
			var result = builder.BuildChildKey(testElement);
			AssertEquals("ChildFK", result);
		}

		public void TestBuildParentKey()
		{
			var testElement = new XElement("Test",
				new XAttribute("Key", "ZD1_ParentKey"));
			var result = builder.BuildParentKey(testElement);
			AssertEquals("ZD1_ParentKey", result);
		}

		public void TestBuildCardinality()
		{
			var testElement = new XElement("Test",
				new XAttribute("Cardinality", "1"));
			var result = builder.BuildCardinality(testElement);
			AssertEquals("1", result);

			testElement = new XElement("Test",
				new XAttribute("Cardinality", "*"));
			result = builder.BuildCardinality(testElement);
			AssertEquals("*", result);

			testElement = new XElement("Test",
				new XAttribute("Cardinality", "Hello"));
			result = builder.BuildCardinality(testElement);
			AssertEquals("Hello", result);

			testElement = new XElement("Test");
			result = builder.BuildCardinality(testElement);
			AssertEquals("", result);
		}

		public void TestBuildIsExternal()
		{
			var testElement = new XElement("Test", new XAttribute("External", "TRUE"));
			var result = builder.BuildIsExternal(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("External", "TrUe"));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("External", "true"));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("External", "FALSE"));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("External", "FaLsE"));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("External", "false"));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("External", ""));
			result = builder.BuildIsExternal(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test");
			result = builder.BuildIsExternal(testElement);
			AssertEquals(false, result);
		}

		public void TestBuildIsExternalChild()
		{
			var testElement = new XElement("Test", new XAttribute("ExternalChild", "TRUE"));
			var result = builder.BuildIsExternalChild(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", "TrUe"));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", "true"));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", "FALSE"));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", "FaLsE"));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", "false"));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalChild", ""));
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test");
			result = builder.BuildIsExternalChild(testElement);
			AssertEquals(false, result);
		}

		public void TestBuildIsExternalParent()
		{
			var testElement = new XElement("Test", new XAttribute("ExternalParent", "TRUE"));
			var result = builder.BuildIsExternalParent(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", "TrUe"));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", "true"));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(true, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", "FALSE"));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", "FaLsE"));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", "false"));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test", new XAttribute("ExternalParent", ""));
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(false, result);

			testElement = new XElement("Test");
			result = builder.BuildIsExternalParent(testElement);
			AssertEquals(false, result);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			builder = new AssociationElementParser();
		}

		#endregion

		AssociationElementParser builder;
	}
}
