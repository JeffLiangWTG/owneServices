using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class ProcessRelatedNumberCollection : DependentBusinessObjectCollection<ProcessRelatedNumber, JobDeclaration>
	{
		public ProcessRelatedNumberCollection(JobDeclaration master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;

		protected override ZQuery CreateRelationshipFilter() => base.CreateRelationshipFilter().AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.DocumentsRelated);

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			(child as CusEntryNumber).Parent = Master;
		}
	}
}
