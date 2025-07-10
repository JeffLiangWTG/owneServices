using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	[TestedType(typeof(DocumentFieldDefinitionCollection))]
	public class DocumentFieldDefinitionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocumentFieldDefinitionCollection>
	{
		public void TestContainsCode()
		{
			Assert("Should find Apple field", Definitions.ContainsField("Apple"));
			Assert("Should find aPPle field using different casing", Definitions.ContainsField("aPPle"));
			Assert("Should NOT find Apples field", !Definitions.ContainsField("Apples"));
		}

		public void TestGetDescription()
		{
			AssertEquals("Should find description for Apple field", "Red is best", Definitions.GetDescription("Apple"));
			AssertEquals("Should find description for aPPle field using different casing", "Red is best", Definitions.GetDescription("Apple"));
			AssertEquals("Should NOT find description for Apples", ZString.Empty, Definitions.GetDescription("Apples"));
		}

		public void TestGetCorrectCasedFieldName()
		{
			AssertEquals("Should find Apple with correct case", "Apple", Definitions.GetCorrectCasedFieldName("Apple"));
			AssertEquals("Should find aPPLE with correct case", "Apple", Definitions.GetCorrectCasedFieldName("aPPLE"));
			AssertEquals("Should NOT find Apples with correct case", ZString.Empty, Definitions.GetCorrectCasedFieldName("Apples"));
		}

		public void TestAllowNew()
		{
			Assert("Collection should not allow new", !Definitions.AllowNew);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Definitions = new DocumentFieldDefinitionCollection();
			Definitions.Add(new DocumentFieldDefinition("Apple", "Red is best", DocumentFieldDefinition.FieldTypes.Property));
			Definitions.Add(new DocumentFieldDefinition("Lemon", "Yellow fruit", DocumentFieldDefinition.FieldTypes.Property));
		}

		DocumentFieldDefinitionCollection Definitions;

		protected override DocumentFieldDefinitionCollection GetCollectionToTest()
		{
			return new DocumentFieldDefinitionCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			DocumentFieldDefinition definition = new DocumentFieldDefinition("FieldName", "FieldDescription", DocumentFieldDefinition.FieldTypes.Property);
			return definition;
		}

		#endregion

	}
}
