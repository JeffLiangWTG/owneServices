using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public class DefermentAccount : IDefermentAccount
	{
		public DefermentAccount(OrgHeader organisation, ZString account)
		{
			this.organisation = Argument.NotNull(organisation, nameof(organisation));
			this.account = Argument.NotNullOrEmpty(account, nameof(account));
		}
		readonly OrgHeader organisation;
		readonly ZString account;

		public string Type => AccountToUse?.CZ_Code;

		public string ApplicationType => AccountToUse?.CZ_Type;

		public string AccountPrefix => AccountToUse?.CZ_Issuer;

		public string AccountNumber => AccountToUse?.CZ_Account;

		public string AccountHolder => organisation.OH_FullName;

		public string AuthorisationNumber => AccountToUse?.DecryptedPassword;

		public string Applicant => organisation.GetEUEoriDetails();

		internal OrgCusAccount AccountToUse => CachedValueHelper.GetValue(ref accountToUse, () => organisation.GetDefermentAccounts().FirstOrDefault(a => a.CZ_Account == account));
		CachedValue<OrgCusAccount> accountToUse;
	}
}
