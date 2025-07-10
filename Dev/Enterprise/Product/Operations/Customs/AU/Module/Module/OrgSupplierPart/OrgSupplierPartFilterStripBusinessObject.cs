using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Module
{
	public class OrgSupplierPartFilterStripBusinessObject : Customs.Module.OrgSupplierPartFilterStripBusinessObject
	{
		protected override void ClassificationFilter(ModuleFilterCollection result)
		{
			result.AddGuidFilter("Import Classification", ModuleIDs.ImportClassification, GetClassificationQuery, ImportClassifications);
			result.AddGuidFilter("Export Classification", ModuleIDs.ExportClassification, GetClassificationQuery, ExportClassifications);
		}

		public BaseClassificationCollection<Classification> ImportClassifications => new ImportClassificationCollection(Factory);

		public BaseClassificationCollection<Classification> ExportClassifications => new ExportClassificationCollection(Factory);
	}
}
