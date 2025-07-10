using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.Diagnostics
{
	public class EXTemplateSection : TemplateSection
	{
		public EXTemplateSection(TemplateSection originalSection, ZBool isOverridden)
			: base(originalSection, originalSection.StartingRowNumber, originalSection.RowCount)
		{
			IsOverridden = isOverridden;
		}

		public ZBool IsOverridden { get; set; }

		public DocumentCollection Documents
		{
			get
			{
				if (documents == null)
				{
					documents = new DocumentCollection(SectionName);
					documents.Load();
				}
				return documents;
			}
		}
		DocumentCollection documents;
	}
}
