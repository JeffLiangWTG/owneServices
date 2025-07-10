using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Registry;

public class AccountValidation
{
	const int VatAccountNumberMaxLength = 15;
	const int FiscalCodeAccountNumberMaxLength = 20;

	public AccountValidation(Account parent, BusinessObjectFactory factory) : base()
	{
		this.parent = Argument.NotNull(parent, nameof(parent));
		this.factory = Argument.NotNull(factory, nameof(factory));
	}
	readonly Account parent;
	readonly BusinessObjectFactory factory;

	public void ValidateAccountNumber()
	{
		var accountNumber = parent.AccountNumber;
		var accountNumberInfo = parent.AccountNumberInfo;

		accountNumberInfo.ClearAllNotifications();

		var validVATAccountNumber = (NoResString)@"^[a-zA-Z0-9]{11}-[a-zA-Z0-9]{3}$";
		var validFiscalCodeAccountNumber = (NoResString)@"^[a-zA-Z0-9]{16}-[a-zA-Z0-9]{3}$";

		if (accountNumber.IsEmpty)
		{
			accountNumberInfo.AddError(ValidationCaptions.Account.EnterAccountNumber);
		}
		else if (accountNumber.Length != VatAccountNumberMaxLength && accountNumber.Length != FiscalCodeAccountNumberMaxLength)
		{
			accountNumberInfo.AddError(ValidationCaptions.Account.AccountLengthMustBe15Or20);
		}

		if (!Regex.IsMatch(accountNumber, validVATAccountNumber) && !Regex.IsMatch(accountNumber, validFiscalCodeAccountNumber))
		{
			accountNumberInfo.AddError(ValidationCaptions.Account.AccountFormat);
		}

		ValidateDuplicatedAccountNumber();
	}

	void ValidateDuplicatedAccountNumber()
	{
		var hasDuplicatedAccountNumberInSameCompany = ParentAccountCollection.Where(x => x.AccountNumber == parent.AccountNumber).Skip(1).Any();
		if (hasDuplicatedAccountNumberInSameCompany)
		{
			parent.AccountNumberInfo.AddError(ValidationCaptions.Account.AccountNumberMustBeUnique);
		}

		if (!ParentCompanyPK.IsEmpty)
		{
			var companyWithSameAccountNumber = ITAccountsManagementRegistry.Instance.GetOtherCompanyNameMatchingCondition(x => x.AccountNumber == parent.AccountNumber, ParentCompanyPK, factory);
			if (!companyWithSameAccountNumber.IsEmpty)
			{
				parent.AccountNumberInfo.AddError(ValidationCaptions.Account.AccountNumberAlreadyExistsInAnotherCompany(companyWithSameAccountNumber));
			}
		}
	}

	public void ValidateAccountPassword()
	{
		parent.AccountPasswordInfo.ClearAllNotifications();

		var validPassword = (NoResString)@"^[a-zA-Z0-9]*$";
		if (parent.AccountPassword.IsEmpty)
		{
			parent.AccountPasswordInfo.AddError(ValidationCaptions.Account.EnterPassword);
		}
		else
		{
			if (parent.AccountPassword.Length < 8 || parent.AccountPassword.Length > 15)
			{
				parent.AccountPasswordInfo.AddError(ValidationCaptions.Account.PasswordLengthMustBeBetween8and15);
			}

			if (!Regex.IsMatch(parent.AccountPassword, validPassword))
			{
				parent.AccountPasswordInfo.AddError(ValidationCaptions.Account.PasswordFormat);
			}
		}
	}

	public void ValidateAccountStatus()
	{
		var accountStatusInfo = parent.AccountStatusInfo;
		accountStatusInfo.ClearAllNotifications();

		MandatoryValidation.CheckEntered(accountStatusInfo);
		ListValidation.ErrorIfInvalidCode(accountStatusInfo);
	}

	public void ValidateAccountCertificate()
	{
		parent.AccountCertificateInfo.ClearAllNotifications();

		if (!parent.IsValidationSuspended && !parent.AccountCertificate.IsEmpty)
		{
			X509Certificate2 cert = null;
			try
			{
				cert = new X509Certificate2(parent.AccountCertificate, parent.AccountCertificatePassword, X509KeyStorageFlags.MachineKeySet);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				parent.AccountCertificateInfo.AddError(ValidationCaptions.Account.CertificateOrPasswordInvalid);
			}
			finally
			{
				cert?.Dispose();
			}
		}
	}

	public void ValidateAccountCertificatePassword()
	{
		parent.AccountCertificatePasswordInfo.ClearAllNotifications();

		if (!parent.AccountCertificate.IsEmpty && parent.AccountCertificatePassword.IsEmpty)
		{
			parent.AccountCertificatePasswordInfo.AddError(ValidationCaptions.Account.PasswordRequiredForCertificate);
		}
	}

	public void ValidateNode()
	{
		parent.AccountNodeInfo.ClearAllNotifications();

		var accountNode = parent.AccountNode;
		if (accountNode.Length != 4)
		{
			parent.AccountNodeInfo.AddError(ValidationCaptions.Account.AccountNodeMandatoryAndLengthMustBe4);
		}
		else
		{
			ValidateDuplicatedAccountNode();
		}
	}

	void ValidateDuplicatedAccountNode()
	{
		var hasDuplicatedAccountNodeInSameCompany = ParentAccountCollection.Where(x => x.AccountNode == parent.AccountNode).Skip(1).Any();
		if (hasDuplicatedAccountNodeInSameCompany)
		{
			parent.AccountNodeInfo.AddError(ValidationCaptions.Account.AccountNodeMustBeUnique);
		}

		if (!ParentCompanyPK.IsEmpty)
		{
			var companyWithSameAccountNode = ITAccountsManagementRegistry.Instance.GetOtherCompanyNameMatchingCondition(x => x.AccountNode == parent.AccountNode, ParentCompanyPK, factory);
			if (!companyWithSameAccountNode.IsEmpty)
			{
				parent.AccountNodeInfo.AddError(ValidationCaptions.Account.AccountNodeAlreadyExistsInAnotherCompany(companyWithSameAccountNode));
			}
		}
	}

	public void ValidateAccountRangeStart()
	{
		parent.AccountRangeStartInfo.ClearAllNotifications();

		var accountRangeEnd = parent.AccountRangeEnd;
		var accountRangeStart = parent.AccountRangeStart;
		if (!accountRangeStart.IsEmpty)
		{
			if (!IsAccountRangeNumberValid(accountRangeStart))
			{
				parent.AccountRangeStartInfo.AddError(ValidationCaptions.Account.AccountRangeInvalidFormat);
			}
			else if (!accountRangeEnd.IsEmpty && IsAccountRangeNumberValid(accountRangeEnd) && IsRangeStartGreaterOrEqualThanRangeEnd(parent))
			{
				parent.AccountRangeStartInfo.AddError(ValidationCaptions.Account.AccountRangeStartMustBeLessThanRangeEnd);
			}
		}
	}

	public void ValidateAccountRangeEnd()
	{
		parent.AccountRangeEndInfo.ClearAllNotifications();

		var accountRangeStart = parent.AccountRangeStart;
		var accountRangeEnd = parent.AccountRangeEnd;
		if (!accountRangeEnd.IsEmpty)
		{
			if (!IsAccountRangeNumberValid(accountRangeEnd))
			{
				parent.AccountRangeEndInfo.AddError(ValidationCaptions.Account.AccountRangeInvalidFormat);
			}
			else if (!accountRangeStart.IsEmpty && IsAccountRangeNumberValid(accountRangeStart) && IsRangeStartGreaterOrEqualThanRangeEnd(parent))
			{
				parent.AccountRangeEndInfo.AddError(ValidationCaptions.Account.AccountRangeEndMustBeGreaterThanRangeStart);
			}
		}
	}

	public void ValidateEmcsNotificationEnabled()
	{
		var emcsNotificationEnabledInfo = parent.EmcsNotificationEnabledInfo;
		emcsNotificationEnabledInfo.ClearAllNotifications();
		if (parent.EmcsNotificationEnabled && parent.ExciseNumbers.Count == 0)
		{
			emcsNotificationEnabledInfo.AddError(ValidationCaptions.Account.AccountMustHaveAtLeastOneExciseNumber);
		}
	}

	public void ValidateHasAtLeastOneAccountDetails()
	{
		var parent = this.parent;
		var errorMessage = ValidationCaptions.Account.AccountMustHaveAtLeastOneAccountDetail;

		parent.ClearRowNotificationsContaining(errorMessage);
		if (parent.AccountDetails.Count == 0)
		{
			parent.AddRowError(errorMessage);
		}
	}

	ZGuid ParentCompanyPK => parent.CurrentFallbackLevel?.CompanyPK(false) ?? ZGuid.Empty;
	IEnumerable<Account> ParentAccountCollection => parent?.ParentAccountCollection?.Cast<Account>() ?? Enumerable.Empty<Account>();
	bool IsAccountRangeNumberValid(string accountRangeNumber) => Regex.IsMatch(accountRangeNumber, "^[a-zA-Z0-9]*$");
	bool IsRangeStartGreaterOrEqualThanRangeEnd(Account account) => string.CompareOrdinal(account.AccountRangeStart, account.AccountRangeEnd) > -1;
}
