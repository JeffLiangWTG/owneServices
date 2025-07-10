using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	[TestedType(typeof(DocumentFieldDefinition))]
	public class DocumentFieldDefinitionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDocumentFieldDefinition()
		{
			DocumentFieldDefinition definition = new DocumentFieldDefinition("Lemons", "Yellow fruit", DocumentFieldDefinition.FieldTypes.Property);
			AssertEquals("Field Name should be", "Lemons", definition.FieldName);
			AssertEquals("Description should be", "Yellow fruit", definition.FieldDescription);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocumentFieldDefinition("FieldName", "FieldDescription", DocumentFieldDefinition.FieldTypes.Property);
		}

		#endregion
	}
}
