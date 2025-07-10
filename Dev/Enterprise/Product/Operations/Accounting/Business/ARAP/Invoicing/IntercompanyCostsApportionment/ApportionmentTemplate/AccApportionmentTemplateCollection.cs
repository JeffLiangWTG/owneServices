using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	[ModuleID(ModuleId.AccApportionmentTemplate)]
	public class AccApportionmentTemplateCollection : BusinessObjectCollection<AccApportionmentTemplate>
	{
		public AccApportionmentTemplateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			ZQuery additionalFilter = base.CreateAdditionalFilter();
			additionalFilter.AddToFilter(AccApportionmentTemplateSchema.A0_GC, SQLComparisonOperator.Equal, GlbCompany.CurrentCompany.PK);
			return additionalFilter;
		}
	}
}
