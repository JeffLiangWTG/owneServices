using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class SealContainerCollection : DependentBusinessObjectCollection<SealContainer, EnRouteSeal>
	{
		public SealContainerCollection(EnRouteSeal master)
			: base(master)
		{ }

		protected override CargoWise.Schema.SchemaGuidColumn FKSchemaColumnInDependent => CusInBondContainerSchema.BC_ParentID;

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(CusInBondContainerSchema.BC_TypeOfService, ContainerTypeOfServiceList.Codes.Seal);
			return query;
		}
	}
}
