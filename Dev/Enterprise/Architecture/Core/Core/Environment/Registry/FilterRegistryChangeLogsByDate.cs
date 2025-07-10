using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Schema;
using Res = Enterprise.ZArchitecture.Core.SourceGenerated.Res;

namespace Enterprise.ZArchitecture.Core.Environment.Registry;

public class FilterRegistryChangeLogsByDate : NonPersistentBusinessObject, IRegistryChangeLogFilterParameters
{
	#region Properties

	#region FromDate

	ZDateTime fromDate;

	[ResourceStringData("Enterprise.ZArchitecture.Core.Environment.Registry.FilterRegistryChangeLogsByDate|FromDate", Caption = "Filter From Date", ShortCaption = "From Date", FullDescription = "Filter out registry items that do not have change log events occurring on or after this date.")]
	public ZDateTime FromDate
	{
		get { return this.fromDate; }
		set
		{
			if (fromDate != value)
			{
				SetNonPersistentPropertyValue(FromDateInfo, ref fromDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFromDate();
				}
			}
		}
	}

	public ZPropertyInfo FromDateInfo
	{
		get { return GetZPropertyInfo(nameof(FromDate)); }
	}

	#endregion

	#region ToDate

	ZDateTime toDate;

	[ResourceStringData("Enterprise.ZArchitecture.Core.Environment.Registry.FilterRegistryChangeLogsByDate|ToDate", Caption = "Filter To Date", ShortCaption = "To Date", FullDescription = "Filter out registry items that do not have change log events occurring on or before this date.")]
	public ZDateTime ToDate
	{
		get { return this.toDate; }
		set
		{
			if (toDate != value)
			{
				SetNonPersistentPropertyValue(ToDateInfo, ref toDate, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateToDate();
				}
			}
		}
	}

	public ZPropertyInfo ToDateInfo
	{
		get { return GetZPropertyInfo(nameof(ToDate)); }
	}

	#endregion

	#region Validation

	public FilterRegistryChangeLogsByDateValidation Validation => new FilterRegistryChangeLogsByDateValidation(this);

	protected override void RunPreSaveValidationCore()
	{
		Validation.ValidateAll();
		base.RunPreSaveValidationCore();
	}

	#endregion

	#endregion

	public ZDBOnlySubQuery GenerateSubQueryFilterFromFilterParameters(ZDBOnlySubQuery subQuery)
	{
		if (!FromDate.IsEmpty)
		{
			subQuery.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, FromDate);
		}

		if (!ToDate.IsEmpty)
		{
			subQuery.AddToFilter(StmALogSchema.SL_EventTime, SQLComparisonOperator.LessThanOrEqualTo,
				ToDate.AddDays(1));
		}

		subQuery.AddToFilter(StmALogSchema.SL_Table, SQLComparisonOperator.Equal, StmDataSchema.Constants.TableName);
		return subQuery;
	}
}

public class FilterRegistryChangeLogsByDateValidation : ZValidation
{
	public FilterRegistryChangeLogsByDateValidation(FilterRegistryChangeLogsByDate parent) : base(parent)
	{
		this.parent = parent;
	}

	public void Add(FilterRegistryChangeLogsByDateValidation validation)
	{
		ZValidationInternals.Add(validation);
	}

	public void Remove(FilterRegistryChangeLogsByDateValidation validation)
	{
		ZValidationInternals.Remove(validation);
	}

	#region ValidateAll

	public override void ValidateAll()
	{
		using (ParentListInternals.SuspendListChanged())
		{
			ValidateAllCore();
		}
	}

	protected void ValidateAllCore()
	{
		ValidateFromDate();
		ValidateToDate();
	}

	#endregion

	#region FromDate

	public void ValidateFromDate()
	{
		ZValidationInternals.Validate(Parent.FromDateInfo, CheckFromDate);
	}

	void CheckFromDate()
	{
		if (Parent.FromDate == ZDateTime.Empty)
		{
			Parent.FromDateInfo.AddError(Res.GetString("91A9D81B-2B44-4A2E-AE7D-0B3F37D8AD01", "From date cannot be empty."));
		}
		if (Parent.FromDate > ZDateTime.Today)
		{
			Parent.FromDateInfo.AddError(Res.GetString("F1B96456-3FD2-4C68-9BE6-13D1FF6E485E",
				"From date cannot be later than today."));
		}
		if (Parent.ToDate != ZDateTime.Empty && Parent.ToDate < Parent.FromDate)
		{
			Parent.FromDateInfo.AddError(Res.GetString("161F5702-37C8-45AC-9151-397ACA20B8DA",
				"From date cannot be later than To date."));
		}
	}

	#endregion

	#region ToDate

	public void ValidateToDate()
	{
		ZValidationInternals.Validate(Parent.ToDateInfo, CheckToDate);
	}

	void CheckToDate()
	{
		if (Parent.ToDate == ZDateTime.Empty)
		{
			Parent.ToDateInfo.AddError(Res.GetString("7151A2F9-F33B-4727-8998-EDD4ADF2043F", "To date cannot be empty."));
		}
		if (Parent.ToDate > ZDateTime.Today)
		{
			Parent.ToDateInfo.AddError(Res.GetString("BE4B5FE1-6238-4E68-A914-F7EF7F58D96E",
				"To date cannot be later than today."));
		}
		if (Parent.FromDate != ZDateTime.Empty && Parent.ToDate < Parent.FromDate)
		{
			Parent.ToDateInfo.AddError(Res.GetString("C9B45DFC-CF6D-49A8-8BC5-CE03E4DF10D3",
				"From date cannot be later than To date."));
		}
	}

	#endregion

	public override Type AutoValidationType => typeof(FilterRegistryChangeLogsByDateValidation);

	public FilterRegistryChangeLogsByDate Parent
	{
		[System.Diagnostics.DebuggerStepThrough]
		get => parent;
	}

	readonly FilterRegistryChangeLogsByDate parent;

	IValidationInternals ZValidationInternals => this;
	ISingleElementListInternal ParentListInternals => Parent;
}
