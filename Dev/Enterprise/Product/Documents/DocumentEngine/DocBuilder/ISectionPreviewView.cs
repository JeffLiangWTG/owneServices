using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public interface ISectionPreviewView
	{
		void UpdateSectionViews(string documentTitle, IBODocDataProvider[] docDataProviders, ExcelTemplate systemExcelTemplate, ExcelTemplate customizedExcelTemplate, ContactType contactType, DocumentDirection documentDirection);
		void UpdateZoom(int zoom);
	}
}