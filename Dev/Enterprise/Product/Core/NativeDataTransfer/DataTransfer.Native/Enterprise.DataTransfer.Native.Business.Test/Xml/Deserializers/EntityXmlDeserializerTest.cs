using System;
using System.Linq;
using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions;
using Enterprise.DataTransfer.Native.Common.EntityBuilders;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers.Testing
{
	public class EntityXmlDeserializerTest : TransactionedTestCase
	{
		public void TestParseInternalPK_ReturnsGuid()
		{
			var pk = Guid.NewGuid();
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement(TagName.PrimaryKey, pk)
				);

			var xmlDeserializer = new EntityXmlDeserializer(input, definition);

			var result = xmlDeserializer.ParseInternalPK();

			AssertEquals(pk, result);
		}

		public void TestParseInternalPK_WithElements_ThrowsException_PrimaryKeyIsNull()
		{
			String pk = null;
			TestParseInternalPK_WithElements_ThrowsException_Core(pk);
		}

		public void TestParseInternalPK_WithElements_ThrowsException_InvalidPK()
		{
			var pk = "ZAWARDO!";
			TestParseInternalPK_WithElements_ThrowsException_Core(pk);
		}

		void TestParseInternalPK_WithElements_ThrowsException_Core(string pk)
		{
			var input = new XElement("DummyBizo",
							new XAttribute(TagName.Action, "Insert"),
							new XElement(TagName.PrimaryKey, pk)
							);
			var xmlDeserializer = new EntityXmlDeserializer(input, definition);

			AssertExceptionThrown<NativeXMLUserVisibleException>("Fail",
				$"Error Parsing InternalPK: Guid should contain 32 digits with 4 dashes (xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx), but was '{pk}'.",
				() => xmlDeserializer.ParseInternalPK());
		}

		public void TestParseInternalPK_ReturnPK()
		{
			var pk = Guid.NewGuid();
			var input = new XElement(TagName.PrimaryKey, pk);
			var xmlDeserializer = new EntityXmlDeserializer(input, definition);

			var result = xmlDeserializer.ParseInternalPK();

			AssertEquals(pk, result);
		}

		public void TestParseInternalPK_NoElements_ReturnGuidEmpty_NullValue()
		{
			TestParseInternalPK_NoElements_ReturnGuidEmpty_Core(null);
		}
		public void TestParseInternalPK_NoElements_ReturnGuidEmpty_EmptyValue()
		{
			TestParseInternalPK_NoElements_ReturnGuidEmpty_Core(string.Empty);
		}
		public void TestParseInternalPK_NoElements_ReturnGuidEmpty_BadValue()
		{
			TestParseInternalPK_NoElements_ReturnGuidEmpty_Core("This GUID is over 9000.");
		}

		void TestParseInternalPK_NoElements_ReturnGuidEmpty_Core(string value)
		{
			var input = new XElement(TagName.PrimaryKey, value);
			var xmlDeserializer = new EntityXmlDeserializer(input, definition);

			var result = xmlDeserializer.ParseInternalPK();

			AssertEquals(Guid.Empty, result);
		}

		public void TestParseEntityElement()
		{
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement("Code", "TST", new XAttribute("Relationship", "IZN")));

			var entity = BuildEntity(input, definition);

			AssertEquals(definition, entity.Definition);
			AssertEquals(EntityAction.INSERT, entity.Action);
			AssertEquals(true, entity.HasProperty("Code"));
			AssertEquals("IZN", entity.Properties.First().GetAttributeValue("Relationship"));
		}

		public void TestParseEntityElement_WithPrimaryKey()
		{
			var pk = Guid.NewGuid();
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement(TagName.PrimaryKey, pk)
				);

			var entity = BuildEntity(input, definition);

			AssertEquals(pk, entity.InternalPK);
		}

		public void TestParseEntityElement_WithInvalidProperty()
		{
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement("Invalid", "TST"));

			var entity = BuildEntity(input, definition);
			AssertEquals(false, entity.HasProperty("Invalid"));
		}

		public void TestParseEntityElement_WithExcludedProperty()
		{
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Insert"),
				new XElement("Z0_excludedColumn", "TST"));

			var entity = BuildEntity(input, definition);
			AssertEquals(false, entity.HasProperty("Z0_excludedColumn"));
		}

		public void TestParseActionElement()
		{
			var input = new XElement("DummyBizo",
				new XAttribute(TagName.Action, "Create"));

			var entity = BuildEntity(input, definition);
			AssertEquals(EntityAction.INSERT, entity.Action);

			input = new XElement("DummyBizo");
			entity = BuildEntity(input, definition);
			AssertEquals(EntityAction.EMPTY, entity.Action);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			definition = TestUtil.FindEntityDefinition("Dummy", "DummyBizo");
		}

		static Entity BuildEntity(XElement element, IEntityDefinition definition)
		{
			var builder = new XmlEntityBuilder(element, definition, new AncillaryImportServices());
			return EntityBuilder.Construct(builder);
		}

		#endregion

		IEntityDefinition definition;
	}
}
