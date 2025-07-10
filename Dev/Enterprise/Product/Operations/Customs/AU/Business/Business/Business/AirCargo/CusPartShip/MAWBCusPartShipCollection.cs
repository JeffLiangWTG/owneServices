
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MAWBCusPartShipCollection : DependentBusinessObjectCollection<CusPartShip, CusMAWBBase>
	{
		public MAWBCusPartShipCollection(CusMAWBBase parent)
			: base(parent)
		{
		}

		#region Implementation

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusPartShipSchema.CG_CM_LinkToPartMaster; }
		}

		#endregion
	}
}
