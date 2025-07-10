using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class CusPersonCollection : DependentBusinessObjectCollection<CusPerson, JobDeclaration>
	{
		public CusPersonCollection(JobDeclaration master) : base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusPersonSchema.CPN_ParentID;
	}
}
