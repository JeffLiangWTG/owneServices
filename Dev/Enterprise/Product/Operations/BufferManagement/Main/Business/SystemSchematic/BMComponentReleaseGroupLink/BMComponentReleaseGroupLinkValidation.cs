using System.Globalization;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BMComponentReleaseGroupLinkValidation : AutoBMComponentReleaseGroupLinkValidation
	{
		public BMComponentReleaseGroupLinkValidation(AutoBMComponentReleaseGroupLink parent)
			: base(parent)
		{
		}

		protected override void CheckFO_GG_ReleaseGroup()
		{
			base.CheckFO_GG_ReleaseGroup();

			// This should be enforced in the DB - remove once constraint added.
			MandatoryValidation.CheckEntered(Parent.FO_GG_ReleaseGroupInfo);
		}

		protected override void CheckFO_ReleaseGateMode()
		{
			base.CheckFO_ReleaseGateMode();
			MandatoryValidation.CheckEntered(Parent.FO_ReleaseGateModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FO_ReleaseGateModeInfo);
		}

		protected override void CheckFO_AutoAssignTasksAgeIsValidZDateTime()
		{
			var value = Parent.FO_AutoAssignTasksAgeInfo.Value;
			if (!value.IsEmpty && !value.IsValid)
			{
				var description = ResString.GetMultilingualString("534315C7-B3C4-4F95-8689-72812AC9F643", "Auto Assign Tasks Age. Correct format should be 000:00");
				var message = string.Format(CultureInfo.InvariantCulture, TypeValidation.InvalidTypeMessage, description);
				Parent.FO_AutoAssignTasksAgeInfo.AddError(message);
			}
		}
	}
}
