//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiCustomerUserAccountValidation
//
//    This class should be used for overriding validation in AutoEdiCustomerUserAccountValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using CargoWise.EntityFramework;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public class EdiCustomerUserAccountValidation : AutoEdiCustomerUserAccountValidation
	{
		public EdiCustomerUserAccountValidation(AutoEdiCustomerUserAccount parent) : base(parent)
		{
		}

		protected override void CheckEUA_IsEmailVerificationRequired()
		{
			base.CheckEUA_IsEmailVerificationRequired();
			if (Parent.EUA_IsEmailVerificationRequired && Parent.WebAccessContact != null)
			{
				Parent.EUA_IsEmailVerificationRequiredInfo.AddError(Res.GetString("3c610390-96fd-48ae-aa82-197b3768a46d", "User Accounts with a linked Web Access Contact should not require Email Verification"));
			}
		}

		protected override void CheckEUA_RN_NKCountry()
		{
			base.CheckEUA_RN_NKCountry();
			ListValidation.ErrorIfInvalidCode(Parent.EUA_RN_NKCountryInfo);
		}
	}
}
