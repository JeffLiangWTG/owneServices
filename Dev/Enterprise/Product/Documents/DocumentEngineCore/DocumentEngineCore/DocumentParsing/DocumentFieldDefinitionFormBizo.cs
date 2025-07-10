using CargoWise.EntityFramework;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	public class DocumentFieldDefinitionFormBizo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DocumentFieldDefinitionFormBizo(DocumentFieldDefinitionCollection fields)
		{
			fFields = fields;
		}

		public DocumentFieldDefinitionCollection Fields
		{
			get { return fFields; }
		}

		readonly DocumentFieldDefinitionCollection fFields;
	}
}
