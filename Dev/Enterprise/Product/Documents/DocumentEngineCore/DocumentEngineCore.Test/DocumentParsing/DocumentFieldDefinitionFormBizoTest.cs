using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.DocumentParsing.Testing
{
	[TestedType(typeof(DocumentFieldDefinitionFormBizo))]
	public class DocumentFieldDefinitionFormBizoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFields()
		{
			DocumentFieldDefinitionCollection definitions = new DocumentFieldDefinitionCollection();
			definitions.Add(new DocumentFieldDefinition("FieldName", "Description", DocumentFieldDefinition.FieldTypes.Property));

			DocumentFieldDefinitionFormBizo documentFieldBizO = new DocumentFieldDefinitionFormBizo(definitions);

			AssertEquals("Fields should match", definitions, documentFieldBizO.Fields);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			DocumentFieldDefinitionCollection definitions = new DocumentFieldDefinitionCollection();
			return new DocumentFieldDefinitionFormBizo(definitions);
		}

		#endregion

	}
}
