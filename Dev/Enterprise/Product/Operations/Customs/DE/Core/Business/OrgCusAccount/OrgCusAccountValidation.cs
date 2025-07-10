using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class OrgCusAccountValidation : Enterprise.MasterFiles.Business.OrgCusAccountValidation
	{
		const int AccountLength = 6;

		const int PasswordLength = 25;

		public OrgCusAccountValidation(OrgCusAccount parent)
			: base(parent)
		{
		}

		protected new OrgCusAccount Parent => (OrgCusAccount)base.Parent;
		protected override void CheckCZ_Account()
		{
			base.CheckCZ_Account();
			if (Parent.CZ_Account.Length != AccountLength || !Parent.CZ_Account.IsNumbersOnlyOrEmpty)
			{
				Parent.CZ_AccountInfo.AddMessageError(Res.GetString("F7B4C804-18E0-4B97-B3DB-26444562DEDC", "Entered Account must have {0} numeric digits", AccountLength));
			}
		}

		protected override void CheckDecryptedPassword()
		{
			base.CheckDecryptedPassword();
			if (Parent.DecryptedPassword.Length != PasswordLength || !Parent.DecryptedPassword.IsNumbersOnlyOrEmpty)
			{
				Parent.DecryptedPasswordInfo.AddMessageError(Res.GetString("263ECDC2-BCE8-41CE-8035-8263444C5796", "Entered BIN must have {0} numeric digits", PasswordLength));
			}
		}

		protected override void CheckCZ_Issuer()
		{
			base.CheckCZ_Issuer();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CZ_IssuerInfo);
		}

		protected override void CheckCZ_Code()
		{
			base.CheckCZ_Code();
			ListValidation.MessageErrorIfInvalidCode(Parent.CZ_CodeInfo);
			var orgHeader = Parent.Header;
			var shouldBeChecked = orgHeader != null && (!Parent.IsInDatabase || Parent.CZ_CodeInfo.HasChanges);
			if (shouldBeChecked && orgHeader.DefermentAccountNumberCollection.Cast<OrgCusAccount>().Any(x => !ReferenceEquals(x, Parent) && x.CZ_Code == Parent.CZ_Code))
			{
				Parent.CZ_CodeInfo.AddMessageError(Res.GetString("563192B0-6923-4B10-A4DB-29FC41616B23", "Account is already present"));
			}

			if (orgHeader != null)
			{
				if (orgHeader.CustomsCodes.Cast<OrgCusCode>().All(x => x.OK_CodeType != OrgCusCode.EuropeanUnionSharedCodeTypes.Eori))
				{
					Parent.CZ_CodeInfo.AddMessageError(Res.GetString("6D5AAAC1-CDDD-4E01-A61D-F6FA7BBE4CFD", "A Registration Number/Code of Type '{0}' is required for Deferment Accounts", OrgCusCode.EuropeanUnionSharedCodeTypes.Eori));
				}
			}
		}

		protected override void CheckCZ_Type()
		{
			base.CheckCZ_Type();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CZ_TypeInfo);
		}
	}
}
