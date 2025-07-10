using System.Xml.Linq;
using Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions.Builders;
using Enterprise.DataTransfer.Native.Utils;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common.Definitions.EntityDefinitions
{
	public class XmlEntityDefinitionBuilderTest : TestCase
	{
		public void TestBuildIncludedColumns()
		{
			var testElement = new XElement("Test",
								 new XElement("IncludedProperty", new XAttribute("Name", "Z0_includedColumn_1")),
								 new XElement("IncludedProperty", new XAttribute("Name", "Z0_includedColumn_2")),
								 new XElement("IncludedProperty", new XAttribute("Name", "Z0_includedColumn_3")),
								 new XElement("ExcludedProperty", new XAttribute("Name", "Z0_excludedColumn_1")),
								 new XElement("ExcludedProperty", new XAttribute("Name", "Z0_excludedColumn_2")),
								 new XElement("ExcludedProperty", new XAttribute("Name", "Z0_excludedColumn_3")));

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals(true, entityDefinition.PropertyIsIncluded("Z0_includedColumn_1"));
			AssertEquals(true, entityDefinition.PropertyIsIncluded("Z0_includedColumn_2"));
			AssertEquals(true, entityDefinition.PropertyIsIncluded("Z0_includedColumn_3"));

			AssertEquals(true, entityDefinition.PropertyIsExcluded("Z0_excludedColumn_1"));
			AssertEquals(true, entityDefinition.PropertyIsExcluded("Z0_excludedColumn_2"));
			AssertEquals(true, entityDefinition.PropertyIsExcluded("Z0_excludedColumn_3"));
		}

		public void TestBuildIsUpdateOrInsert()
		{
			var testElement = new XElement("Test",
											 new XAttribute("UpdateOrInsertBehaviour", "True"));

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals("entityDefinition.IsUpdateOrInsert", true, entityDefinition.IsUpdateOrInsert);

			testElement = new XElement("Test",
										 new XAttribute("UpdateOrInsertBehaviour", "False"));

			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);

			AssertEquals("entityDefinition.IsUpdateOrInsert", false, entityDefinition.IsUpdateOrInsert);
		}

		public void TestBuildIsUpdateOrInsert_AttributeIsEmptyOrInvalid()
		{
			var testElement = new XElement("Test");

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals(false, entityDefinition.IsUpdateOrInsert);

			testElement = new XElement("Test",
										 new XAttribute("UpdateOrInsertBehaviour", "whatever"));

			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);

			AssertEquals(false, entityDefinition.IsUpdateOrInsert);
		}

		public void TestBuildHasCustomFields()
		{
			var testElement = new XElement("Test",
											 new XAttribute("HasCustomFields", "True"));

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals(true, entityDefinition.HasCustomColumns);

			testElement = new XElement("Test",
										 new XAttribute("HasCustomFields", "False"));

			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);

			AssertEquals(false, entityDefinition.HasCustomColumns);
		}

		public void TestBuildHasCustomFields_AttributeIsEmptyOrInValid()
		{
			var testElement = new XElement("Test");

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals(false, entityDefinition.HasCustomColumns);

			testElement = new XElement("Test",
										 new XAttribute("HasCustomFields", "whatever"));

			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);

			AssertEquals(false, entityDefinition.HasCustomColumns);
		}

		public void TestBuildEntityName()
		{
			var testElement = new XElement("Test",
											 new XAttribute("EntityName", "EntityName"));

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals("EntityName", entityDefinition.EntityName);

			testElement = new XElement("Test");
			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);

			AssertEquals(true, entityDefinition.EntityName.IsEmpty());
		}

		public void TestBuildIsExternal()
		{
			var testElement = new XElement("Test",
											 new XAttribute(TagName.IsExternal, "True"));

			var reader = new XmlEntityDefinitionBuilder(testElement);
			var entityDefinition = reader.Construct(null);

			AssertEquals(true, entityDefinition.IsExternal);

			testElement = new XElement("Test");
			reader = new XmlEntityDefinitionBuilder(testElement);
			entityDefinition = reader.Construct(null);
			AssertEquals(false, entityDefinition.IsExternal);
		}

		#region Implementation

		#endregion
	}
}
