//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoFeatureControlRuleLicenceDatabasePivotValidation
//
//    This class should be used for overriding validation in AutoFeatureControlRuleLicenceDatabasePivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Client.EDI.FeatureControl.Business
{
	public class FeatureControlRuleLicenceDatabasePivotValidation : AutoFeatureControlRuleLicenceDatabasePivotValidation
	{
		public FeatureControlRuleLicenceDatabasePivotValidation(AutoFeatureControlRuleLicenceDatabasePivot parent) : base(parent)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();

			var pivot = (FeatureControlRuleLicenceDatabasePivot)Parent;
			if (!pivot.ControlRule.CanAttachLicenceDatabase(pivot.Database, out var errorMessage))
			{
				Parent.AddRowError(errorMessage);
			}
		}
	}
}
