using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class CusSealCollection : ActiveBusinessObjectCollection<CusSeal>
	{
		public CusSealCollection(BusinessObject master) : base(master.Factory, master, new ZQuery(CusSealSchema.BK_ParentTableCode, master.TablePrefix), CusSealSchema.BK_ParentID)
		{
			this.EnableMaxCountValidationWithMessageError(MaximumAdditionalSeals, warnAtHalfway: false, ValidationCaptions.CusSeal.TheMaximumNumberOfAdditionalSealsExceeded(MaximumAdditionalSeals));
		}

		protected override bool AllowNew => (Relationship.Master as ICusSealCollectionSupporter)?.AllowNewCusSeal ?? false;

		const int MaximumAdditionalSeals = 999;
	}
}
