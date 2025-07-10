using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.Customs.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.Business
{
	public class DispatchInstructionNumberCollection : DependentBusinessObjectCollection<DispatchInstructionNumber, JobDeclaration>
	{
		public DispatchInstructionNumberCollection(JobDeclaration master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusEntryNumSchema.CE_ParentID;

		protected override ZQuery CreateRelationshipFilter() => base.CreateRelationshipFilter().AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.DispatchInstructionDocument);

		protected override void SetCollectionRelationships(BusinessObject child)
		{
			base.SetCollectionRelationships(child);
			(child as CusEntryNumber).Parent = Master;
		}
	}
}
