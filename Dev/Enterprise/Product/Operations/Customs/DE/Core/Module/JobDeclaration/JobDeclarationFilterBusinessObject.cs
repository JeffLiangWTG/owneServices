using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Module
{
	public class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant string")]
		public static class FilterConstants
		{
			public const string PresentationEndDate = "Presentation End Date";
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			var presentationEndDateFilter = filters.AddDateFilter(FilterConstants.PresentationEndDate, GetPresentationEndDateQuery);
			presentationEndDateFilter.Category = FilterCategories.Dates;
			presentationEndDateFilter.MultilingualDescription = ResString.GetMultilingualString("177F3FA9-22B3-45E1-98A4-FF96720956D8", FilterConstants.PresentationEndDate);

			return filters;
		}

		protected override bool SupportsExitControlCore => true;

		ZQuery GetPresentationEndDateQuery(DateComparisonOperator comparisonOperator, ZDateTime fromDate, ZDateTime toDate)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			var queryString = ZString.Empty;
			if (comparisonOperator == DateComparisonOperator.HasNoDateEntered)
			{
				queryString = GetPresentationEndDateQueryString("WHERE JE_PresentationEndDate IS NULL");
			}
			else if (comparisonOperator == DateComparisonOperator.HasDateEntered)
			{
				queryString = GetPresentationEndDateQueryString("WHERE JE_PresentationEndDate IS NOT NULL");
			}
			else
			{
				var fromDateIsValid = fromDate.IsValid;
				var toDateIsValid = toDate.IsValid;
				if (fromDateIsValid || toDateIsValid)
				{
					string whereClause;
					if (fromDateIsValid)
					{
						whereClause = $"WHERE JE_PresentationEndDate >= '{fromDate.SqlFormat}'";
						if (toDateIsValid)
						{
							whereClause += $" AND JE_PresentationEndDate <= '{toDate.SqlFormat}'";
						}
					}
					else
					{
						whereClause = $"WHERE JE_PresentationEndDate <= '{toDate.SqlFormat}'";
					}
					queryString = GetPresentationEndDateQueryString(whereClause);
				}
			}

			if (!queryString.IsEmpty)
			{
				result.AddFilterAndZSQLParameterCollection(queryString, null);
			}
			return result;

			string GetPresentationEndDateQueryString(string whereClause)
			{
				return FormattableString.Invariant($@"JE_PK IN (
				SELECT JE_PK FROM dbo.DEJobDeclaration 
				{whereClause} )");
			}
		}
	}
}
