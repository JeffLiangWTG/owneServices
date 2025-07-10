using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.DocBuilder;
using NUnit.Framework;

namespace Enterprise.Diagnostics.Testing
{
	[TestedType(typeof(EXTemplateSection))]
	public class EXTemplateSectionTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var templateSection = new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + " , My Favourite Martian", 1, 2);

			return new EXTemplateSection(templateSection, false);
		}
	}
}
