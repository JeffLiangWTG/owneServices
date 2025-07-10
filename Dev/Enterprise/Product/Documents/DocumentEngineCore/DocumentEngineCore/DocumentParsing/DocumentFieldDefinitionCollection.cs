using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.DocumentParsing
{
	public class DocumentFieldDefinitionCollection : NonPersistentBusinessObjectCollection<DocumentFieldDefinition>, IDocumentFieldDefinitionCollection
	{
		public DocumentFieldDefinitionCollection()
			: base(new BusinessObjectFactory())
		{
		}

		#region Field / Description Searching

		public bool ContainsField(string fieldName)
		{
			bool found = false;

			foreach (DocumentFieldDefinition field in this)
			{
				if (field.FieldName.ToUpper() == fieldName.ToUpper())
				{
					found = true;
					break;
				}
			}

			return found;
		}

		public ZString GetDescription(string fieldName)
		{
			string desc = "";

			foreach (DocumentFieldDefinition field in this)
			{
				if (field.FieldName.ToUpper() == fieldName.ToUpper())
				{
					desc = field.FieldDescription;
					break;
				}
			}

			return desc;
		}

		public ZString GetCorrectCasedFieldName(string fieldName)
		{
			string result = "";

			foreach (DocumentFieldDefinition field in this)
			{
				if (field.FieldName.ToUpper() == fieldName.ToUpper())
				{
					result = field.FieldName;
					break;
				}
			}

			return result;
		}

		public DocumentFieldDefinition GetFieldDefinition(string fieldName)
		{
			return this.Cast<DocumentFieldDefinition>().FirstOrDefault(field => field.FieldName.EqualsIgnoringCase(fieldName));
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentFieldDefinition((NoResString)"Test", (NoResString)"Test", DocumentFieldDefinition.FieldTypes.Property);
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#endregion

		#region IDocumentFieldDefinitionCollection Members

		IDocumentField IDocumentFieldDefinitionCollection.this[int i]
		{
			get { return this[i]; }
		}

		#endregion
	}
}
