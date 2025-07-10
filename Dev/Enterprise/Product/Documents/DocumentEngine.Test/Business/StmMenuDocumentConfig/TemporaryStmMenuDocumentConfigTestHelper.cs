using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.DocBuilder;

namespace Enterprise.DocumentEngine.Business.Testing
{
	public static class TemporaryStmMenuDocumentConfigTestHelper
	{
		public static StmMenuDocumentConfig GetNewSource(BusinessObjectFactory factory)
		{
			var resourceRetriever = new EmbeddedResourceRetriever();
			var menu = factory.New<StmMenuItemBase>();
			var template = StmTemplateBase.GetDocBuilderTemplate(factory, DocBuilderTemplateType.System);
			var pivot = menu.Documents.AddNew();
			var result = pivot.DocConfigs.AddNew();

			menu.SU_MenuName = "Menu";
			pivot.SI_DocumentTitle = "Pivot";

			pivot.SI_SU = menu.PK;
			pivot.SI_SO = template.PK;
			template.SO_Template = resourceRetriever.GetBytes("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls");
			template.SO_DataContext = ".DummyBODocSupportable";

			return result;
		}
	}
}
