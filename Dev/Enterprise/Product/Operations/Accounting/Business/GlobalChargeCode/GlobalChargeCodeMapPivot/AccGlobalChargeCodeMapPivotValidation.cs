//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccGlobalChargeCodeMapPivotValidation
//
//    This class should be used for overriding validation in AutoAccGlobalChargeCodeMapPivotValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	using CargoWise.EntityFramework;

	public class AccGlobalChargeCodeMapPivotValidation : AutoAccGlobalChargeCodeMapPivotValidation
	{
		public AccGlobalChargeCodeMapPivotValidation(AutoAccGlobalChargeCodeMapPivot parent) : base(parent)
		{
		}

		protected override void CheckYP_TYPE()
		{
			base.CheckYP_TYPE();
			MandatoryValidation.CheckEntered(Parent.YP_TYPEInfo);
			ListValidation.ErrorIfInvalidCode(Parent.YP_TYPEInfo);
			if (Parent.YP_TYPE == ZArchitecture.Core.LedgerTypes.AccountsPayable && !IsAPLedgerUnique())
			{
				Parent.YP_TYPEInfo.AddError(APLedgerNotUniqueErrorMessage());
			}
		}

		protected virtual string APLedgerNotUniqueErrorMessage()
		{
			return Res.GetString("354E6FC4-A5F5-444a-95FE-04B1021D858F", "You can only create a single AP Charge Code per mapping.");
		}

		protected virtual bool IsAPLedgerUnique()
		{
			return true;
		}

		protected override void CheckYP_AC()
		{
			base.CheckYP_AC();
			if (!Parent.YP_AC.IsEmpty && !Parent.YP_TYPE.IsEmpty && !Parent.YP_YG.IsEmpty && !IsRecordUnique())
			{
				Parent.YP_ACInfo.AddError(ChargeCodeAndTypeAlreadyMappedErrorMessage());
			}
		}

		protected virtual string ChargeCodeAndTypeAlreadyMappedErrorMessage()
		{
			return Res.GetString("ECCC241A-0D71-4140-9AE4-525F51467D4C", "This Charge Code and Type is already mapped to this Global Code");
		}

		protected virtual bool IsRecordUnique()
		{
			return true;
		}
	}
}

