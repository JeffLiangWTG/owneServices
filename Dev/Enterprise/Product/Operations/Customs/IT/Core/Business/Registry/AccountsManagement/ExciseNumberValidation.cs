using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Registry;

public class ExciseNumberValidation : ZValidation
{
	public ExciseNumberValidation(BusinessObject parent) : base(parent)
	{
		Parent = Argument.NotNull(parent as ExciseNumber, nameof(parent));
		parentListInternals = parent;
		zValidationInternals = this;
	}

	ExciseNumber Parent { get; }
	readonly ISingleElementListInternal parentListInternals;
	readonly IValidationInternals zValidationInternals;

	public override Type AutoValidationType => typeof(ExciseNumberValidation);

	public void Add(ExciseNumberValidation validation)
	{
		zValidationInternals.Add(validation);
	}

	public void Remove(ExciseNumberValidation validation)
	{
		zValidationInternals.Remove(validation);
	}

	#region Number

	public void ValidateNumber()
	{
		zValidationInternals.Validate(Parent.NumberInfo, new RunValidationInvoker(NumberValidationInvoker));
	}

	void NumberValidationInvoker()
	{
		CheckNumberIsWesternEuropean();
		CheckNumber();
	}

	protected void CheckNumberIsWesternEuropean()
	{
		EnglishCharactersValidation.ErrorIfNotWesternEuropean(Parent.NumberInfo);
	}

	protected void CheckNumber()
	{
		var targetPropertyInfo = Parent.NumberInfo;
		MandatoryValidation.CheckEntered(targetPropertyInfo);

		var exciseNumber = Parent.Number;
		if (!exciseNumber.IsEmpty)
		{
			if (!IsValidExciseNumber(exciseNumber))
			{
				targetPropertyInfo.AddError(ValidationCaptions.ExciseNumber.ExciseNumberInvalidFormat);
			}
			else if (!IsUniqueExciseNumber(exciseNumber))
			{
				targetPropertyInfo.AddError(ValidationCaptions.ExciseNumber.ExciseNumberMustBeUniqueInRegistry);
			}
		}
	}

	bool IsValidExciseNumber(ZString exciseNumber)
	{
		return ValidExciseNumberLength(exciseNumber)
			&& ValidExciseNumberPrefix(exciseNumber)
			&& ValidExciseNumberState(exciseNumber);
	}

	bool ValidExciseNumberLength(ZString exciseNumber)
	{
		const int exciseNumberLength = 13;
		return exciseNumber.Length == exciseNumberLength;
	}

	bool ValidExciseNumberPrefix(ZString exciseNumber)
	{
		const string italianExciseNumberPrefix = "IT00";
		return exciseNumber.StartsWith(italianExciseNumberPrefix);
	}

	bool ValidExciseNumberState(ZString exciseNumber)
	{
		var italianStates = RefCountry.LoadFromCountryCode(Parent.Factory, Core.Constants.CountryCodes.Italy)?.States?.Cast<RefCountryStates>() ?? Array.Empty<RefCountryStates>();
		var exciseNumberStatePart = exciseNumber.SubstringSafe(4, 2);
		return italianStates.Any(x => x.RW_Code == exciseNumberStatePart);
	}

	bool IsUniqueExciseNumber(ZString exciseNumber)
	{
		var isUnique = true;
		var parentAccount = Parent.Account;

		var hasDuplicatedExciseNumberInSameAccount = parentAccount.ExciseNumbers.Cast<ExciseNumber>().Where(x => x.Number == exciseNumber).Skip(1).Any();
		if (hasDuplicatedExciseNumberInSameAccount)
		{
			isUnique = false;
		}

		if (isUnique)
		{
			var parentAccountCollection = parentAccount.ParentAccountCollection?.Cast<Account>() ?? Enumerable.Empty<Account>();
			var hasDuplicatedExciseNumberInSameCompany = parentAccountCollection.Where(x => x.ExciseNumbers.ContainsExciseNumber(exciseNumber)).Skip(1).Any();
			if (hasDuplicatedExciseNumberInSameCompany)
			{
				isUnique = false;
			}
		}

		if (isUnique)
		{
			var parentCompanyPK = Parent.Account.CurrentFallbackLevel?.CompanyPK(false) ?? ZGuid.Empty;
			if (!parentCompanyPK.IsEmpty)
			{
				var accountCollectionsGroupedByCompany = ITAccountsManagementRegistry.Instance.AccountsManagement.AccountCollectionsGroupedByCompany;
				var hasDuplicatedExciseNumberInOtherCompany = accountCollectionsGroupedByCompany.Any(x => x.CompanyPK != parentCompanyPK && x.Accounts.Cast<Account>().Any(y => y.ExciseNumbers.ContainsExciseNumber(exciseNumber)));

				if (hasDuplicatedExciseNumberInOtherCompany)
				{
					isUnique = false;
				}
			}
		}
		return isUnique;
	}

	#endregion

	public override void ValidateAll()
	{
		using (parentListInternals.SuspendListChanged())
		{
			ValidateNumber();
		}
	}
}
