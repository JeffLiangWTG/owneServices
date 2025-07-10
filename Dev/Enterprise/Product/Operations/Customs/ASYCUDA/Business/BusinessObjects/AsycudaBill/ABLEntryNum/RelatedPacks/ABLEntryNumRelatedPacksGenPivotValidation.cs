using System.Linq;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ABLEntryNumRelatedPacksGenPivotValidation : GenPivotValidation
	{
		public ABLEntryNumRelatedPacksGenPivotValidation(ABLEntryNumRelatedPacksGenPivot parent)
				: base(parent)
		{
		}

		protected new ABLEntryNumRelatedPacksGenPivot Parent
		{
			get => (ABLEntryNumRelatedPacksGenPivot)base.Parent;
		}

		protected override void CheckXX_Relation2ID()
		{
			base.CheckXX_Relation2ID();
			var parent = Parent;
			var parentPK = parent.PK;
			var parentRelation2ID = parent.XX_Relation2ID;
			if (parent.Relation1Object?.PackPivots.OfType<ABLEntryNumRelatedPacksGenPivot>().Any(pivot => pivot.PK != parentPK && pivot.XX_Relation2ID == parentRelation2ID) ?? false)
			{
				parent.XX_Relation2IDInfo.AddError(DuplicatePack);
			}

			if ((!parent.Relation2Object?.APA_VINNumber.IsEmpty ?? false) &&
				parent.Relation1Object?.Bill?.CustomsEntryNumbers?
					.Cast<ABLEntryNum>()
					.SelectMany(num => num.PackPivots.Where(x => x.XX_Relation2ID == parent.XX_Relation2ID))
					.Take(2).Count() > 1)
			{
				parent.XX_Relation2IDInfo.AddMessageError(Only1AssociatedLRNAllowedFor1Pack);
			}
		}

		public string DuplicatePack => Res.GetString("412E9FC4-F0AC-403A-8367-42E6E2386DF4", "This Pack has already been associated with customs number {0}.", Parent.Relation1Object.HumanReadableName);

		public string Only1AssociatedLRNAllowedFor1Pack => Res.GetString("40165BDF-F216-48C8-996F-56BE51FD3650", "A Pack that has a VIN number captured can only be linked to one LRN number.");
	}
}
