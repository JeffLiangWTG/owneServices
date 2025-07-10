using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business;

public class ITCustomsNumberViewStmNumsWrapper : CustomsNumberViewStmNumsCompanyWrapper
{
	public ITCustomsNumberViewStmNumsWrapper(CustomsNumberViewStmNums stmNums)
		: base(stmNums)
	{
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1053:StaticHolderTypesShouldNotHaveConstructors")]
	public new class Schema : CustomsNumberViewStmNumsCompanyWrapper.Schema
	{
		public const string AppliesTo = "AppliesTo";
		public const string YearOfApplicability = "YearOfApplicability";

		public const int AppliesToMaxLength = 16;
		public const int YearOfApplicabilityMaxLength = 4;
		public const char AppliesToSeparator = ':';
	}

	#region AppliesTo

	[ReadOnlyMember(nameof(AppliesTo_ReadOnly))]
	[List(nameof(Lookups) + "." + nameof(ITCustomsNumberViewStmNumsWrapperLookups.AppliesToList))]
	[MaxLength(Schema.AppliesToMaxLength)]
	[ResourceStringData("ITCustomsNumberViewStmNums|AppliesTo", Caption = "Applies To")]
	public ZString AppliesTo
	{
		get { return appliesTo; }
		set
		{
			if (SetNonPersistentPropertyValue(AppliesToInfo, ref appliesTo, value))
			{
				UpdateSN_FountainName();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateAppliesTo();
			}
		}
	}
	ZString appliesTo;

	[ReadOnlyMember(nameof(AppliesTo_ReadOnly))]
	public ZPropertyInfo AppliesToInfo
	{
		get { return GetZPropertyInfo(Schema.AppliesTo); }
	}

	bool AppliesTo_ReadOnly => StmNums.IsInDatabase;

	#endregion

	#region YearOfApplicability

	[ReadOnlyMember(nameof(YearOfApplicability_ReadOnly))]
	[MaxLength(Schema.YearOfApplicabilityMaxLength)]
	[ResourceStringData("ITCustomsNumberViewStmNums|YearOfApplicability", Caption = "Year Of Applicability", ShortCaption = "Year")]
	public ZInt YearOfApplicability
	{
		get
		{
			if (!yearOfApplicabilityLoaded)
			{
				yearOfApplicabilityLoaded = true;
			}
			return yearOfApplicability;
		}
		set
		{
			if (SetNonPersistentPropertyValue(YearOfApplicabilityInfo, ref yearOfApplicability, value))
			{
				UpdateSN_FountainName();
			}
			if (!IsValidationSuspended)
			{
				Validation.ValidateYearOfApplicability();
			}
		}
	}
	ZInt yearOfApplicability;
	bool yearOfApplicabilityLoaded;

	[ReadOnlyMember(nameof(YearOfApplicability_ReadOnly))]
	public ZPropertyInfo YearOfApplicabilityInfo
	{
		get { return GetZPropertyInfo(Schema.YearOfApplicability); }
	}

	bool YearOfApplicability_ReadOnly => StmNums.IsInDatabase;

	#endregion

	public static ZString GenerateFountainName(ZInt yearOfApplicability, ZString appliesTo)
	{
		return new ZString(yearOfApplicability.ToString()).Left(4).PadLeft(4) + Schema.AppliesToSeparator + appliesTo;
	}

	public new ITCustomsNumberViewStmNumsWrapperLookups Lookups => (ITCustomsNumberViewStmNumsWrapperLookups)base.Lookups;

	protected override CustomsNumberViewStmNumsWrapperLookups GetNewLookups()
	{
		return new ITCustomsNumberViewStmNumsWrapperLookups(this);
	}

	public new ITCustomsNumberViewStmNumsWrapperValidation Validation => (ITCustomsNumberViewStmNumsWrapperValidation)base.Validation;

	public override CustomsNumberViewStmNumsWrapperValidation GetNewValidation()
	{
		return new ITCustomsNumberViewStmNumsWrapperValidation(this);
	}

	public override bool ReadOnly
	{
		get => base.ReadOnly || (StmNums.IsInDatabase && YearOfApplicability < ZDate.Today.Year);
		set => base.ReadOnly = value;
	}

	#region Implementation

	protected override void InitializeDataCore()
	{
		LoadFromSN_FountainName();
		if (!StmNums.IsInDatabase)
		{
			YearOfApplicability = ZDate.Today.Year;
		}
		StmNums.SN_FountainNameInfo.ValueChanged -= SN_FountainNameInfo_ValueChanged;
		StmNums.SN_FountainNameInfo.ValueChanged += SN_FountainNameInfo_ValueChanged;
	}

	void SN_FountainNameInfo_ValueChanged(object sender, System.EventArgs e)
	{
		var ve = e as ValueChangedEventArgs;
		if (ve != null && ve.OldValue != ve.NewValue)
		{
			LoadFromSN_FountainName();
		}
	}

	void LoadFromSN_FountainName()
	{
		if (!updatingSN_FountainNameInProgress)
		{
			try
			{
				loadingFromSN_FountainNameInProgress = true;
				var year = ZShort.Zero;
				var applies = ZString.Empty;
				ExtraDataFromFountainName(StmNums.SN_FountainName, out year, out applies);
				YearOfApplicability = year;
				AppliesTo = applies;
			}
			finally
			{
				loadingFromSN_FountainNameInProgress = false;
			}
		}
	}
	bool loadingFromSN_FountainNameInProgress;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
	public static void ExtraDataFromFountainName(ZString fountainName, out ZShort year, out ZString appliesTo)
	{
		var appliesToSeparator = fountainName.IndexOf(Schema.AppliesToSeparator);
		if (appliesToSeparator > 1)
		{
			year = ZShort.ParseSafe(fountainName.Left(appliesToSeparator), ZShort.Zero);
			appliesTo = fountainName.SubstringSafe(appliesToSeparator + 1).Left(Schema.AppliesToMaxLength);
		}
		else
		{
			year = ZShort.ParseSafe(fountainName, ZShort.Zero);
			appliesTo = ZString.Empty;
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
			SN_FountainName = GenerateFountainName(YearOfApplicability, AppliesTo);
		}
	}
	#endregion
}
