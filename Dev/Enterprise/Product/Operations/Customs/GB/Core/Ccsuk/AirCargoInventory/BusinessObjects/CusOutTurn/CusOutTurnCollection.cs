using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	public class CusOutTurnCollection : DependentBusinessObjectCollection<CusOutTurn, CusHAWB>
	{
		public CusOutTurnCollection(CusHAWB parentHawb)
			: base(parentHawb)
		{
		}

		public override Type GetTypeOfElementsFromPK(ZGuid pK)
		{
			return typeof(CusOutTurn);
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusOutturnSchema.C5_ParentID; }
		}

		public int TotalDelivered
		{
			get { return (from CusOutTurn outTurn in this where outTurn.IsDelivered select (int)outTurn.C5_PackagesOutturned).Sum(); }
		}
	}
}
