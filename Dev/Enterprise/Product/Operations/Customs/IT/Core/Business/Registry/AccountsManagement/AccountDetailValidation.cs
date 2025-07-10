using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;

namespace Enterprise.Customs.IT.Registry;

public class AccountDetailValidation : ZValidation
{
	public AccountDetailValidation(AccountDetail parent) : base(parent)
	{
		this.parent = Argument.NotNull(parent, nameof(parent));
	}
	readonly AccountDetail parent;

	public override Type AutoValidationType => typeof(AccountDetailValidation);

	public void ValidateInternalCode()
	{
		var internalCodeInfo = parent.InternalCodeInfo;
		var internalCode = parent.InternalCode;

		internalCodeInfo.ClearAllNotifications();

		new InternalCodeValidation()
			.CheckInternalCode(internalCodeInfo, parent.InternalCode);

		CheckDuplicatedInternalCode(internalCodeInfo, internalCode);
	}

	public void ValidateAuthorizedUser()
	{
		var authorizedUserInfo = parent.AuthorizedUserInfo;
		authorizedUserInfo.ClearAllNotifications();

		MandatoryValidation.CheckEntered(authorizedUserInfo);
		CheckAuthorizedUserFormat(authorizedUserInfo);
		CheckDeclarantCodeAndAuthorizedUserMustBeUnique(authorizedUserInfo);
	}

	public void ValidateDeclarantCode()
	{
		var declarantCodeInfo = parent.DeclarantCodeInfo;
		declarantCodeInfo.ClearAllNotifications();

		MandatoryValidation.CheckEntered(declarantCodeInfo);
		ListValidation.ErrorIfInvalidCode(declarantCodeInfo);
		CheckDeclarantCodeAndAuthorizedUserMustBeUnique(declarantCodeInfo);
	}

	public override void ValidateAll()
	{
		ValidateInternalCode();
		ValidateAuthorizedUser();
		ValidateDeclarantCode();
	}

	#region Implementation

	void CheckDuplicatedInternalCode(ZPropertyInfo internalCodeInfo, ZString internalCode)
	{
		var hasDuplicatedInternalCodeInSameCompany = ParentAccountDetails.Where(x => x.InternalCode == internalCode).Skip(1).Any();
		if (hasDuplicatedInternalCodeInSameCompany)
		{
			internalCodeInfo.AddError(ValidationCaptions.Account.AccountDetailInternalCodeMustBeUnique);
		}

		if (!ParentCompanyPK.IsEmpty)
		{
			var companyWithSameInternalCode = ITAccountsManagementRegistry.Instance.GetOtherCompanyNameMatchingCondition(HasSameInternalCode, ParentCompanyPK, parent.Factory);
			if (!companyWithSameInternalCode.IsEmpty)
			{
				internalCodeInfo.AddError(ValidationCaptions.Account.AccountDetailInternalCodeAlreadyExistsInAnotherCompany(companyWithSameInternalCode));
			}
		}

		bool HasSameInternalCode(Account account) => account.AccountDetails.Cast<AccountDetail>().Any(y => y.InternalCode == internalCode);
	}

	void CheckAuthorizedUserFormat(ZPropertyInfo authorizedUserInfo)
	{
		var authorizedUser = parent.AuthorizedUser;
		if (!authorizedUser.IsEmpty && !AuthorizedUserRegex.IsMatch(authorizedUser))
		{
			authorizedUserInfo.AddWarning(ValidationCaptions.Account.AccountDetailAuthorizedUserCodeFormat);
		}
	}

	void CheckDeclarantCodeAndAuthorizedUserMustBeUnique(ZPropertyInfo propertyInfo)
	{
		var declarantCode = parent.DeclarantCode;
		var authorizedUser = parent.AuthorizedUser;

		if (declarantCode.IsEmpty || authorizedUser.IsEmpty)
		{
			return;
		}

		var hasDuplicatedDeclarantCodeAuthorizedUser = parent.Account
			.AccountDetails
			.Cast<AccountDetail>()
			.Any(x => HasSameDeclarantCodeAndAuthorizedUser(x));

		if (hasDuplicatedDeclarantCodeAuthorizedUser)
		{
			propertyInfo.AddError(ValidationCaptions.Account.AccountDetailDeclarantCodeAndAuthorizedUserMustBeUnique);
		}
	}

	bool HasSameDeclarantCodeAndAuthorizedUser(AccountDetail accountDetailToCompare)
	{
		var declarantCodeToCompare = accountDetailToCompare.DeclarantCode;
		var authorizedUserToCompare = accountDetailToCompare.AuthorizedUser;

		if (accountDetailToCompare == parent || declarantCodeToCompare.IsEmpty || authorizedUserToCompare.IsEmpty)
		{
			return false;
		}

		var keyToCompare = declarantCodeToCompare + authorizedUserToCompare;
		return keyToCompare == parent.DeclarantCode + parent.AuthorizedUser;
	}

	Regex AuthorizedUserRegex => authorizedUserRegex ?? (authorizedUserRegex = new Regex(AuthorizedUserRegexPattern));
	Regex authorizedUserRegex;

	IEnumerable<AccountDetail> ParentAccountDetails => parent.Account
		?.ParentAccountCollection
		?.Cast<Account>()
		.SelectMany(x => x.AccountDetails)
		.Cast<AccountDetail>() ?? Enumerable.Empty<AccountDetail>();

	ZGuid ParentCompanyPK => parent.Account
		.CurrentFallbackLevel?
		.CompanyPK(false) ?? ZGuid.Empty;

	#endregion

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant regex pattern")]
	const string AuthorizedUserRegexPattern = @"^[A-Z0-9]+-\d{3}$";
}
