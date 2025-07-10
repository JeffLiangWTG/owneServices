using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Registry;

public class AccountDetail : NonPersistentBusinessObject<AccountDetailValidation>, IObsoleteValidation
{
	public AccountDetail(Account account, BusinessObjectFactory factory)
		: base(factory)
	{
		Account = Argument.NotNull(account, nameof(account));
		Argument.NotNull(factory, nameof(factory));
	}

	public Account Account { get; }

	public static class Schema
	{
		public const string InternalCode = "InternalCode";
		public const int InternalCodeMaxLength = 20;
		public const string AuthorizedUser = "AuthorizedUser";
		public const int AuthorizedUserMaxLength = 20;
		public const string DeclarantCode = "DeclarantCode";
	}

	[MaxLength(Schema.InternalCodeMaxLength)]
	[ResourceStringData("F1D323F2-68AB-4FBB-B034-CC7E798A12CF", Caption = "Internal Code", ShortCaption = "Int. Code")]
	public ZString InternalCode
	{
		get => internalCode;
		set
		{
			var oldValue = InternalCode;
			SetNonPersistentPropertyValue(InternalCodeInfo, ref internalCode, value);
			if (oldValue != value && !IsValidationSuspended)
			{
				Validation.ValidateInternalCode();
			}
		}
	}
	ZString internalCode;

	public ZPropertyInfo InternalCodeInfo => GetZPropertyInfo(Schema.InternalCode);

	[MaxLength(Schema.AuthorizedUserMaxLength)]
	[ResourceStringData("BA09E076-A15D-4FF9-970B-864DA703C95F", Caption = "Authorized User", ShortCaption = "Auth. User")]
	public ZString AuthorizedUser
	{
		get => authorizedUser;
		set
		{
			var oldValue = AuthorizedUser;
			SetNonPersistentPropertyValue(AuthorizedUserInfo, ref authorizedUser, value);
			if (oldValue != value && !IsValidationSuspended)
			{
				ResetDeclarantTaxNumberAndWorkstationSequentialNumber();
				Validation.ValidateAuthorizedUser();
			}
		}
	}
	ZString authorizedUser;

	public ZPropertyInfo AuthorizedUserInfo => GetZPropertyInfo(Schema.AuthorizedUser);

	[ResourceStringData("570BA435-7D36-4077-8D0A-ECD22BBA736E", Caption = "Declarant", FullDescription = "Declarant Organization Code")]
	[List(nameof(Lookups) + "." + nameof(AccountDetailLookups.Declarants))]
	[RelatedBusinessObject("Declarant")]
	public ZString DeclarantCode
	{
		get => declarantCode;
		set
		{
			var oldValue = DeclarantCode;
			SetNonPersistentPropertyValue(DeclarantCodeInfo, ref declarantCode, value);
			if (oldValue != value && !IsValidationSuspended)
			{
				Validation.ValidateDeclarantCode();
			}
		}
	}
	ZString declarantCode;

	public ZPropertyInfo DeclarantCodeInfo => GetZPropertyInfo(Schema.DeclarantCode);

	public OrgHeader Declarant => Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, DeclarantCode);

	#region AccountNumber Parts

	public ZString DeclarantTaxNumber => GetCachedSplittedValue(x => x.DeclarantTaxNumber);

	public ZInt WorkstationSequentialNumber => GetCachedSplittedValue(x => x.WorkstationSequentialNumber);

	void ResetDeclarantTaxNumberAndWorkstationSequentialNumber()
	{
		cachedAccountNumberSplitted = null;
	}

	#endregion

	public AccountDetailLookups Lookups => lookups ?? (lookups = new AccountDetailLookups(Factory));
	AccountDetailLookups lookups;

	protected override void RunPreSaveValidationCore()
	{
		base.RunPreSaveValidationCore();

		Validation.ValidateAll();
	}

	public override AccountDetailValidation GetNewValidation()
	{
		return new AccountDetailValidation(this);
	}

	T GetCachedSplittedValue<T>(Func<(ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber), T> func)
	{
		if (!cachedAccountNumberSplitted.HasValue)
		{
			cachedAccountNumberSplitted = AccountHelper.SplitCodeBySeparator(AuthorizedUser);
		}

		return func(cachedAccountNumberSplitted.Value);
	}
	(ZString DeclarantTaxNumber, ZInt WorkstationSequentialNumber)? cachedAccountNumberSplitted;
}
