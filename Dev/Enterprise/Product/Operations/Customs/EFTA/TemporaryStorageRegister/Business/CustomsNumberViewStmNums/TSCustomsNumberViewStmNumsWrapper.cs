using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsWrapper
{
	public TSCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
		: base(stmNums)
	{
	}

	public new class Schema : CustomsNumberViewStmNumsWrapper.Schema
	{
		public const string NumberPrefix = "NumberPrefix";
		public const string NumberSuffix = "NumberSuffix";
		public const string NumberPadding = "NumberPadding";
		public const string IsActive = "IsActive";

		public const int NumberPrefixMaxLength = 10;
		public const int NumberSuffixMaxLength = 10;

		public const char FountainNameSeparator = '@';
		public const string NumberConfigSeparator = "@";
	}
	public CusTempStorageRegPremises Premises => premises ??= Factory.Load<CusTempStorageRegPremises>(SN_Owner);
	CusTempStorageRegPremises premises;

	public new TSCustomsNumberViewStmNumsWrapperValidation Validation => (TSCustomsNumberViewStmNumsWrapperValidation)base.Validation;

	public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation() => new TSCustomsNumberViewStmNumsWrapperValidation(this);

	[ResourceStringData("DAA8C5E5-2D2D-42F1-ABF2-1E99AC2DC1AE", Caption = "Next Number", MediumCaption = "Next Number", ShortCaption = "Next Num.", FullDescription = "Next Number for Range")]
	public new ZLong SN_ValueForDisplay => base.SN_ValueForDisplay;

	[ResourceStringData("B1EC22CF-A3E4-4F76-826C-F5662A8F6604", Caption = "Prefix", MediumCaption = "Prefix", ShortCaption = "Prefix", FullDescription = "Prefix for Range")]
	[MaxLength(Schema.NumberPrefixMaxLength)]
	public ZString NumberPrefix
	{
		get => numberPrefix;
		set
		{
			if (SetNonPersistentPropertyValue(NumberPrefixInfo, ref numberPrefix, value))
			{
				UpdateSN_FountainName();
			}
		}
	}
	ZString numberPrefix;

	public ZPropertyInfo NumberPrefixInfo => GetZPropertyInfo(Schema.NumberPrefix);

	[ResourceStringData("CD9E332D-D349-4C8C-A2FF-7CFC4B2CCFB1", Caption = "Padding", MediumCaption = "Padding", ShortCaption = "Padding", FullDescription = "Padding for Range")]
	public ZInt NumberPadding
	{
		get => numberPadding;
		set
		{
			if (SetNonPersistentPropertyValue(NumberPaddingInfo, ref numberPadding, value))
			{
				UpdateSN_FountainName();
			}
		}
	}
	ZInt numberPadding;

	public ZPropertyInfo NumberPaddingInfo => GetZPropertyInfo(Schema.NumberPadding);

	[ResourceStringData("3835A292-E0BA-4F0D-BC1D-0DDD3460061F", Caption = "Suffix", MediumCaption = "Suffix", ShortCaption = "Suffix", FullDescription = "Suffix for Range")]
	[MaxLength(Schema.NumberSuffixMaxLength)]
	public ZString NumberSuffix
	{
		get => numberSuffix;
		set
		{
			if (SetNonPersistentPropertyValue(NumberSuffixInfo, ref numberSuffix, value))
			{
				UpdateSN_FountainName();
			}
		}
	}
	ZString numberSuffix;

	public ZPropertyInfo NumberSuffixInfo => GetZPropertyInfo(Schema.NumberSuffix);

	[ResourceStringData("51DB4AD6-1052-4478-A3B0-EFE5E25098F5", Caption = "Is Active?", MediumCaption = "Is Active?", ShortCaption = "Is Active?", FullDescription = "Is this configuration active?")]
	public virtual ZBool IsActive
	{
		get => isActive;
		set
		{
			if (SetNonPersistentPropertyValue(IsActiveInfo, ref isActive, value))
			{
				UpdateSN_FountainName();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateIsActive();
			}
		}
	}
	ZBool isActive;

	public ZPropertyInfo IsActiveInfo => GetZPropertyInfo(Schema.IsActive);

	#region Implementation

	protected override void InitializeDataCore()
	{
		LoadFromSN_FountainName();

		StmNums.SN_FountainNameInfo.ValueChanged -= SN_FountainNameInfo_ValueChanged;
		StmNums.SN_FountainNameInfo.ValueChanged += SN_FountainNameInfo_ValueChanged;

		if (!StmNums.IsInDatabase)
		{
			StmNums.SN_Type = NumberRangeTypeList.Codes.TemporaryStorage;
			StmNums.SN_Count = 1;
			var provider = StmNums.Provider;
			if (provider != null)
			{
				IsActive = !provider.CustomsNumberWrappers.Cast<TSCustomsNumberViewStmNumsWrapper>().Any(x => x.IsActive);
			}
		}
	}

	void SN_FountainNameInfo_ValueChanged(object sender, EventArgs e)
	{
		if (e is ValueChangedEventArgs valueChangedEventArgs && !ReferenceEquals(valueChangedEventArgs.OldValue, valueChangedEventArgs.NewValue))
		{
			LoadFromSN_FountainName();
		}
	}

	void LoadFromSN_FountainName()
	{
		if (updatingSN_FountainNameInProgress)
		{
			return;
		}

		try
		{
			loadingFromSN_FountainNameInProgress = true;
			ExtraDataFromFountainName(StmNums.SN_FountainName, out var prefix, out var padding, out var suffix, out var isActive);
			NumberPrefix = prefix;
			NumberPadding = padding;
			NumberSuffix = suffix;
			IsActive = isActive;
		}
		finally
		{
			loadingFromSN_FountainNameInProgress = false;
		}
	}
	bool loadingFromSN_FountainNameInProgress;

	public static void ExtraDataFromFountainName(ZString fountainName, out ZString prefix, out ZInt padding, out ZString suffix, out ZBool isActive)
	{
		prefix = ZString.Empty;
		padding = ZInt.Zero;
		suffix = ZString.Empty;
		isActive = ZBool.False;

		var numberConfig = fountainName.Split(Schema.NumberConfigSeparator);
		if (numberConfig.Length == 4)
		{
			prefix = numberConfig[0];
			padding = ZInt.ParseEmptyAsZero(numberConfig[1]);
			suffix = numberConfig[2];
			isActive = new ZBool(numberConfig[3]);
		}
	}

	void UpdateSN_FountainName()
	{
		if (!loadingFromSN_FountainNameInProgress)
		{
			try
			{
				updatingSN_FountainNameInProgress = true;
				UpdateFountainName();
			}
			finally
			{
				updatingSN_FountainNameInProgress = false;
			}
		}
	}
	bool updatingSN_FountainNameInProgress;

	void UpdateFountainName()
	{
		if (!StmNums.IsSetNameAndPrefixInProgress)
		{
			var builder = new ZStringBuilder();
			_ = builder.Append(NumberPrefix);
			_ = builder.Append(NumberPadding.ToString());
			_ = builder.Append(NumberSuffix);
			_ = builder.Append(IsActive.ToString());
			StmNums.SN_FountainName = builder.ToStringWithDelimiterBetweenAppends(Schema.NumberConfigSeparator);
		}
	}

	#endregion
}
