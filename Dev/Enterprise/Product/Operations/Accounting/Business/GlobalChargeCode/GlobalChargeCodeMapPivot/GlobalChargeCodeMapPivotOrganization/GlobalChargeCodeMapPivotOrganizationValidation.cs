namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotOrganizationValidation : AccGlobalChargeCodeMapPivotValidation
	{
		public GlobalChargeCodeMapPivotOrganizationValidation(GlobalChargeCodeMapPivotOrganization parent)
			: base(parent)
		{
		}

		protected override bool IsAPLedgerUnique()
		{
			bool isUnique = true;
			foreach (GlobalChargeCodeMapPivotOrganization pivot in ((GlobalChargeCodeMapPivotOrganization)Parent).ParentCollection)
			{
				if (pivot != Parent && pivot.YP_TYPE == ZArchitecture.Core.LedgerTypes.AccountsPayable && pivot.YP_YG == Parent.YP_YG)
				{
					isUnique = false;
					break;
				}
			}
			return isUnique;
		}

		protected override bool IsRecordUnique()
		{
			bool isUnique = true;
			foreach (GlobalChargeCodeMapPivotOrganization pivot in ((GlobalChargeCodeMapPivotOrganization)Parent).ParentCollection)
			{
				if (pivot != Parent && pivot.YP_AC == Parent.YP_AC && pivot.YP_TYPE == Parent.YP_TYPE && pivot.YP_YG == Parent.YP_YG)
				{
					isUnique = false;
					break;
				}
			}
			return isUnique;
		}
	}
}

