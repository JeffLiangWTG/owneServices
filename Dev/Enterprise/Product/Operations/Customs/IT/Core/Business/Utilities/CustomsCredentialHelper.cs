using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public static class CustomsCredentialHelper
{
	public static Account GetAccountFromNode(ZString node)
	{
		return GetAccounts().FirstOrDefault(x => x.AccountNode == node);
	}

	public static AccountDetail GetAccountDetailFromInternalCode(ZString internalCode)
	{
		return GetAllAccountDetailsForCompany().FirstOrDefault(x => x.InternalCode == internalCode);
	}

	public static Account GetAccountFromInternalCode(ZString internalCode)
	{
		return GetAccountDetailFromInternalCode(internalCode)?.Account;
	}

	public static IEnumerable<Account> GetAccounts()
	{
		return ITAccountsManagementRegistry.Instance.AccountsManagement.Value.OfType<Account>();
	}

	public static IEnumerable<AccountDetail> GetAccountDetailsForDeclarant(ZString declarantCode)
	{
		return GetAllAccountDetailsForCompany().Where(x => x.DeclarantCode == declarantCode);
	}

	public static IEnumerable<AccountDetail> GetAllAccountDetailsForCompany()
	{
		return GetAccounts().SelectMany(x => x.AccountDetails).Cast<AccountDetail>();
	}

	public static OrgHeader GetDeclarantFromInternalCode(ZString internalCode)
	{
		return GetAllAccountDetailsForCompany().FirstOrDefault(x => x.InternalCode == internalCode)?.Declarant;
	}

	public static ZString GetNodeFromInternalCode(ZString internalCode)
	{
		return GetAllAccountDetailsForCompany().FirstOrDefault(x => x.InternalCode == internalCode)?.Account?.AccountNode ?? ZString.Empty;
	}

	public static ZBool InternalCodeIsValid(ZString internalCode)
	{
		return !internalCode.IsEmpty && GetAllAccountDetailsForCompany().Any(x => x.InternalCode == internalCode);
	}

	public static ZBool NodePresentInCompanyAllowedListWithMAUCertificate(ZString internalCode)
	{
		if (internalCode.IsEmpty)
		{
			return false;
		}

		var certificateProvider = new GlbCertificateProvider();
		return certificateProvider.GetMauCertificatePassword(internalCode) != null;
	}

	public static bool CurrentUserHasFiscalCode()
	{
		var currentUser = GlbStaff.CurrentUser;

#if DEBUG
		if (currentUser.IsSupportUserAndIsNotTestEnviroment())
		{
			return true;
		}
#endif
		var fiscalCode = currentUser.GetItalianRegistrationNumber();
		return !fiscalCode.IsEmpty;
	}
}
