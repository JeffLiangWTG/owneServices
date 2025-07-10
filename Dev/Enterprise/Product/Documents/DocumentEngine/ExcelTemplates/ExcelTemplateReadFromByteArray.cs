using System.IO;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.ExcelTemplates
{
	public class ExcelTemplateReadFromByteArray : ExcelTemplate
	{
		public ExcelTemplateReadFromByteArray(string templateName, string templateSourceLocation, byte[] templateContents)
			: base(templateName, templateSourceLocation)
		{
			this.templateContents = templateContents;
		}
		readonly byte[] templateContents;

		protected override byte[] GetAsByteArrayInternal()
		{
			return templateContents;
		}

		protected override Stream GetAsTemplateStreamInternal()
		{
			return new MemoryStream(templateContents, 0, templateContents.Length, false);
		}

		public DataContextValue GetDataContextValueFromBlobData()
		{
			SectionRepository sectionRepository = new SectionRepository(this);
			return new DataContextValue(sectionRepository.DataContext);
		}
	}
}
