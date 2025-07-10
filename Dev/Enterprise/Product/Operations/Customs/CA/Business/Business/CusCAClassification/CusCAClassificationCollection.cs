using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAClassificationCollection : DependentBusinessObjectCollection<CusCAClassification, BusinessObject>
	{
		public CusCAClassificationCollection(BusinessObject parent)
			: base(parent)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusCAClassificationSchema.CCA_ParentID; }
		}
	}
}
