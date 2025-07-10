using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class AccApportionmentTemplateFilterBusinessObject : AccountingFilterStripBusinessObject
	{
		public override ZQuery Filter
		{
			get
			{
				return new ZQuery(base.Filter, new ZQuery(AccApportionmentTemplateSchema.A0_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK));
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			SetActiveStatusFilter(null, false);

			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Description", AccApportionmentTemplateSchema.A0_Description).MultilingualDescription = ResString.GetMultilingualString("02617717-20f5-4a0c-9b1e-272c85d33c15", "Description");
			filters.AddTextFilter("Notes", AccApportionmentTemplateSchema.A0_Notes).MultilingualDescription = ResString.GetMultilingualString("cbf5f9db-36c9-493a-8de9-5dcf6445fca3", "Notes");

			return filters;
		}
	}
}
