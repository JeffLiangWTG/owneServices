using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusVehicleCollection : DependentBusinessObjectCollection<CusVehicle, JobDeclaration>
	{
		public CusVehicleCollection(JobDeclaration master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusVehicleSchema.CVH_ParentID;

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			((CusVehicle)dependent).CVH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
		}
	}
}
