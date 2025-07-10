//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoEdiIdentityTenantValidation
//
//    This class should be used for overriding validation in AutoEdiIdentityTenantValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Text.RegularExpressions;

namespace Enterprise.Client.EDI.IdentityTenant.Business
{
	using Res = ZClientEDI.Business.Res;

	public class EdiIdentityTenantValidation : AutoEdiIdentityTenantValidation
	{
		public EdiIdentityTenantValidation(AutoEdiIdentityTenant parent) : base(parent)
		{
		}

		protected override void CheckIDT_AuthorityUrl()
		{
			base.CheckIDT_AuthorityUrl();
			if (!Parent.IDT_AuthorityUrl.IsEmpty)
			{
				ValidateUrl();
			}
		}

		public void ValidateUrl()
		{
			var pattern = @"^(https?|http)://([\w-]+\.)+[\w-]+(/[\w-./?%&=]*)?$";
			var regex = new Regex(pattern, RegexOptions.IgnoreCase);

			if (!regex.IsMatch(Parent.IDT_AuthorityUrl))
			{
				Parent.IDT_AuthorityUrlInfo.AddError(Res.GetString("2ECE2CAD-6D26-4911-B722-D5675FD84DB6", "Please enter the correct URL."));
			}
		}
	}
}
