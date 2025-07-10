using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Module
{
	public class RefDocTypeFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			AddTextFilters(filters);
			return filters;
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter("Document Code", RefDocTypeSchema.RT_DocType).MultilingualDescription = ResString.GetMultilingualString("DocumentScanning|RefDocTypeFilter|DocumentCode", "Document Code");
			filters.AddFiltersForTranslatableText("Description", RefDocTypeSchema.RT_Desc, typeof(RefDocType), ResString.GetMultilingualString("DocumentScanning|RefDocTypeFilter|Description", "Description"));
			filters.AddTextFilter("Category", RefDocTypeSchema.RT_ReferenceType).MultilingualDescription = ResString.GetMultilingualString("DocumentScanning|RefDocTypeFilter|Category", "Category");
			if (EDocsParsingHelper.IsDocumentParsingEnabled())
			{
				filters.AddTextFilter("Parse Type", RefDocTypeSchema.RT_ParseType).MultilingualDescription = ResString.GetMultilingualString("DocumentScanning|RefDocTypeFilter|ParseType", "Parse Type");
			}
		}
	}
}
