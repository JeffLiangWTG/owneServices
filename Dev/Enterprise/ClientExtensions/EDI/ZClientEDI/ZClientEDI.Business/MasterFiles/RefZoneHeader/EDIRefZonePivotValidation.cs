using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EDIRefZonePivotValidation : RefZonePivotValidation
	{
		public EDIRefZonePivotValidation(AutoRefZonePivot parent) : base(parent) { }

		protected override void CheckF2_ParentID()
		{
			base.CheckF2_ParentID();
			if (Parent.ZoneHeader != null && Parent.Location != null && Parent.ZoneHeader.FZ_ZoneType == EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training)
			{
				ValidateOtherZoneContainsLocation(Parent.Location, Parent.ZoneHeader);
			}
		}
	}
}

