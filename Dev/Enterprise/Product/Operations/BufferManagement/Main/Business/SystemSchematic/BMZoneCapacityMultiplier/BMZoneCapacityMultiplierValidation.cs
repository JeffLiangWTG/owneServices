using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMZoneCapacityMultiplierValidation : AutoBMZoneCapacityMultiplierValidation
	{
		public BMZoneCapacityMultiplierValidation(AutoBMZoneCapacityMultiplier parent)
			: base(parent)
		{
		}

		new BMZoneCapacityMultiplier Parent
		{
			get { return (BMZoneCapacityMultiplier)base.Parent; }
		}

		protected override void CheckBZC_GG_ReleaseGroup()
		{
			var releaseGroupInfo = Parent.BZC_GG_ReleaseGroupInfo;
			var component = Parent.Component;

			if (component != null)
			{
				var multipliers = component.ZoneCapacityMultipliers;

				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(releaseGroupInfo, multipliers, false);
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(releaseGroupInfo, multipliers.Where(m => m.BZC_GG_ReleaseGroup.IsEmpty), Res.GetString("d7f4a2dd-e6b3-4d8f-ba7c-b666438a87e1", "Only one Capacity Throttle can be specified with no Release Group"), true);
			}
		}

		protected override void CheckBZC_FC_Component()
		{
			MandatoryValidation.CheckEntered(Parent.BZC_FC_ComponentInfo);

			if (Parent.Component != null && !Parent.Component.IsBuffer)
			{
				Parent.BZC_FC_ComponentInfo.AddError(Res.GetString("662b5289-843b-41bb-b216-872f149a9b76", "Only buffers can have Zone Capacity Multipliers."));
			}
		}

		protected override void CheckBZC_Zone0Multiplier()
		{
			base.CheckBZC_Zone0Multiplier();
			CompareValidation.CheckNumberGreaterThanZero(Parent.BZC_Zone0MultiplierInfo);
		}

		protected override void CheckBZC_Zone1Multiplier()
		{
			base.CheckBZC_Zone1Multiplier();
			CompareValidation.CheckNumberGreaterThanZero(Parent.BZC_Zone1MultiplierInfo);
		}

		protected override void CheckBZC_Zone2Multiplier()
		{
			base.CheckBZC_Zone2Multiplier();
			CompareValidation.CheckNumberGreaterThanZero(Parent.BZC_Zone2MultiplierInfo);
		}

		protected override void CheckBZC_Zone3Multiplier()
		{
			base.CheckBZC_Zone3Multiplier();
			CompareValidation.CheckNumberGreaterThanZero(Parent.BZC_Zone3MultiplierInfo);
		}
	}
}
