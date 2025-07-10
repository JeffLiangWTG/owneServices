using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ChiefRelatedConsolCollection : ActiveBusinessObjectCollection<ForwardingConsol>
	{
		public ChiefRelatedConsolCollection(ForwardingConsol masterRelation)
			: base(masterRelation, typeof(GenPivot), new ZQuery(), GenPivotSchema.XX_Relation1ID, GenPivotSchema.XX_Relation2ID)
		{
		}

		protected override bool AllowNew
		{
			get { return false; }
		}
	}
}
