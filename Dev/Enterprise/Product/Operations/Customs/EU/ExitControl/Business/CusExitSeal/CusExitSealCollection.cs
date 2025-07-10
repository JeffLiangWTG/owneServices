using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Business
{
	public class CusExitSealCollection : DependentBusinessObjectCollection<CusExitSeal, CusExitContainer>
	{
		public CusExitSealCollection(CusExitContainer master)
			: base(master)
		{
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusSealSchema.BK_ParentID;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newElement = (CusExitSeal)child;
			newElement.BK_SequenceNumber = Count > ZShort.Zero ? this.Cast<CusExitSeal>().Max(x => x.BK_SequenceNumber) + 1 : (ZShort)1;
		}
	}
}
