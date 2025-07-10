using CargoWise.Types;
using Enterprise.DocumentEngine.Business;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing
{
	public interface ICustomizeSectionView
	{
		ITemplateEditor GetTemplateEditor(StmTemplateBase template);

		bool ShouldOverrideExistingSection(ZString sectionName, ZString templateName);
		void ShowEditOnlyMessage(ZString sectionName, ZString templateName);
	}
}
