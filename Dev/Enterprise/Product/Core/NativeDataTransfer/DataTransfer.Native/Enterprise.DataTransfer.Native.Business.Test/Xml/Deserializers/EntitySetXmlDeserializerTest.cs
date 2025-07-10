using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers
{
	public class EntitySetXmlDeserializerTest : TransactionedTestCase
	{
		public void TestParseEntitySetElement()
		{
			var pk = TestUtil.PrepareDummyBizoData();
			var childPk = TestUtil.PrepareDummyDependentBizoData(pk);
			var childPk2 = TestUtil.PrepareDummyDependentBizoData(pk);

			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement(TagName.PrimaryKey, pk),
				new XElement("DummyDependentBizo",
					new XElement(TagName.PrimaryKey, childPk)),
				new XElement("DummyDependentBizo_SuffixCollection",
					new XElement(TagName.PrimaryKey, childPk2)),
				new XElement("DummyPivotCollection",
					new XElement("DummyPivot")));

			var entity = parser.ParseEntitySetElement(input, definition, new AncillaryImportServices());

			AssertEquals(EntityAction.INSERT, entity.Action);
			AssertEquals(pk, entity.InternalPK);
			AssertEquals(3, entity.Children.Count());

			foreach (var child in entity.Children)
			{
				AssertEquals(entity, child.Parent);
			}
		}

		public void TestParseEntitySetWithoutAction()
		{
			var pk = TestUtil.PrepareDummyBizoData();
			var childPk = TestUtil.PrepareDummyDependentBizoData(pk);
			var childPk2 = TestUtil.PrepareDummyDependentBizoData(pk);

			var input = new XElement("DummyBizo",
				new XElement(TagName.PrimaryKey, pk),
				new XElement("DummyDependentBizo",
					new XElement(TagName.PrimaryKey, childPk)),
				new XElement("DummyDependentBizo_SuffixCollection",
					new XElement(TagName.PrimaryKey, childPk2)),
				new XElement("DummyPivotCollection",
					new XElement("DummyPivot")));

			var entity = parser.ParseEntitySetElement(input, definition, new AncillaryImportServices());
			AssertEquals(false, parser.FoundAtLeastOneAction);
		}

		public void TestParseEntitySetWithAction()
		{
			var pk = TestUtil.PrepareDummyBizoData();
			var childPk = TestUtil.PrepareDummyDependentBizoData(pk);
			var childPk2 = TestUtil.PrepareDummyDependentBizoData(pk);

			var input = new XElement("DummyBizo",
				new XElement(TagName.PrimaryKey, pk),
				new XElement("DummyDependentBizo",
					new XElement(TagName.PrimaryKey, childPk)),
				new XElement("DummyDependentBizo_SuffixCollection",
					new XElement(TagName.PrimaryKey, childPk2)),
				new XElement("DummyPivotCollection",
					new XElement("DummyPivot",
					new XAttribute(TagName.Action, "UPDATE"))));

			var entity = parser.ParseEntitySetElement(input, definition, new AncillaryImportServices());
			AssertEquals(true, parser.FoundAtLeastOneAction);
		}

		public void TestDeserializeThrowsXmlException()
		{
			var deserializer = new EntitySetXmlDeserializerForTest();
			var element = new XElement("Rate");
			var expectedMessage = "XML cannot be processed as it does not adhere to the native XML format. There should only be one element within the 'Rate' section but 0 elements were found.";

			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), expectedMessage, () => deserializer.AssertOnlyOneChildElement_Exposed(element));

			element = new XElement("Rate", new XElement("Version", "2.0"), new XElement("RatingHeaders"));
			expectedMessage = "XML cannot be processed as it does not adhere to the native XML format. There should only be one element within the 'Rate' section but 2 elements were found.";

			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), expectedMessage, () => deserializer.AssertOnlyOneChildElement_Exposed(element));

			element = new XElement("Rate", new XElement("RatingHeaders"));

			AssertNoExceptionThrown("Rate has 1 child element, no exception expected", () => deserializer.AssertOnlyOneChildElement_Exposed(element));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			definition = TestUtil.GetEntitySetDefinition("Dummy").Root;
			parser = new EntitySetXmlDeserializer();
		}

		EntitySetXmlDeserializer parser;
		IEntityDefinition definition;

		class EntitySetXmlDeserializerForTest : EntitySetXmlDeserializer
		{
			public void AssertOnlyOneChildElement_Exposed(XElement element)
			{
				AssertOnlyOneChildElement(element);
			}
		}

		#endregion
	}
}
