using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ResourceStrings.Module
{
	public class ResourceStringsFilterBusinessObject : FilterStripBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();

			filters.AddTextFilter("Key", HelpDataStringSchema.HD_Code);
			filters.AddTextFilter("Caption or Description", new GetTextQueryWithOperator(QueryCaptionOrDescription))
				.MaxLength = HelpDataStringSchema.HD_ShortCaption.MaxLength;
			filters.AddTextFilter("Short Caption", HelpDataStringSchema.HD_ShortCaption, ComparisonOptions.NationalLanguage);
			filters.AddTextFilter("Medium Caption", HelpDataStringSchema.HD_MidCaption, ComparisonOptions.NationalLanguage);
			filters.AddTextFilter("Full Caption", HelpDataStringSchema.HD_Caption, ComparisonOptions.NationalLanguage);
			filters.AddTextFilter("Description", HelpDataStringSchema.HD_FullDescription, ComparisonOptions.NationalLanguage);
			filters.AddTextFilter("Language", HelpDataStringSchema.HD_Language, Languages);
			filters.AddTextFilter("Context Class Name", HelpDataStringSchema.HD_ContextClassName);
			filters.AddTextFilter("Context Source File", HelpDataStringSchema.HD_ContextSourceFile);

			FilterCategory docStripSearchCategory = new FilterCategory((NoResString)"Search by DocStrip Usage");
			var docStripContentFilter = filters.AddTextFilter("DocStrip Cell Content", new SchemaStringColumn(HelpDataStringSchema.Instance, "DocStripCellContent", 0, System.Data.SqlDbType.NVarChar, "", false, int.MaxValue), ComparisonOptions.NationalLanguage);
			docStripContentFilter.Category = docStripSearchCategory;

			FilterCategory sourceControlCategory = new FilterCategory((NoResString)"Source Control");
			ModuleTextFilter checkoutFilter = filters.AddTextFilter("Check Out Status", QueryCheckOutStatus, CheckOutStatusList);
			checkoutFilter.Category = sourceControlCategory;
			checkoutFilter.Visibility = FilterVisibility.AlwaysVisible;
			var editReasonFilter = filters.AddTextFilter("Edit Reason", HelpDataStringSchema.HD_EditReason, new EditReasons());
			editReasonFilter.Category = sourceControlCategory;

			var testFailuresCategory = new FilterCategory((NoResString)"String Content Test");
			var testFailuresFilter = filters.AddFlagsFilter("String Content Test Failures", new string[] { "Failed Last String Content Unit Test" }, new GetFlagsQuery[] { GetLastFailuresQuery });
			testFailuresFilter.Category = testFailuresCategory;

			return filters;
		}

		protected override bool ShouldAddCustomSqlFilter => false;

		protected override bool ShouldAddUserDefinedFiltersCore => false;

		#region Queries

		static ZQuery QueryCaptionOrDescription(SQLComparisonOperator comparisonOperator, ZString value)
		{
			ZQuery query = new ZQuery();
			query.DefaultJoinCondition = JoinCondition.Or;
			query.AddToFilter(HelpDataStringSchema.HD_ShortCaption, comparisonOperator, value, ComparisonOptions.NationalLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_MidCaption, comparisonOperator, value, ComparisonOptions.NationalLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_Caption, comparisonOperator, value, ComparisonOptions.NationalLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_FullDescription, comparisonOperator, value, ComparisonOptions.NationalLanguage);
			return query;
		}

		ZQuery QueryCheckOutStatus(ZString value)
		{
			ZQuery query = new ZQuery();
			if (value == CheckOutStatus.CheckedOut)
			{
				query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, SQLComparisonOperator.Equal, ZBool.True);
			}
			else if (value == CheckOutStatus.NotCheckedOut)
			{
				query.AddToFilter(HelpDataStringSchema.HD_IsCheckedOut, SQLComparisonOperator.Equal, ZBool.False);
			}

			return query;
		}

		ZQuery GetLastFailuresQuery(ZBool value)
		{
			return new ZQuery(new SchemaBoolColumn(HelpDataStringSchema.Instance, "InLastFailures", 0, false, false, false), value);
		}

		#endregion

		#region Lookups

		CodeDescriptionPairList CheckOutStatusList
		{
			get
			{
				if (checkOutStatusList == null)
				{
					checkOutStatusList = new CodeDescriptionPairList();
					checkOutStatusList.AddPair(CheckOutStatus.AllFiles, "All");
					checkOutStatusList.AddPair(CheckOutStatus.CheckedOut, "Checked Out");
					checkOutStatusList.AddPair(CheckOutStatus.NotCheckedOut, "Not Checked Out");
				}
				return checkOutStatusList;
			}
		}
		CodeDescriptionPairList checkOutStatusList;

		internal static class CheckOutStatus
		{
			internal const string CheckedOut = "CHK";
			internal const string NotCheckedOut = "NOT";
			internal const string AllFiles = "ALL";
		}

		internal CodeDescriptionPairList Languages
		{
			get { return languages ?? (languages = new CodeDescriptionPairList(OLookUpEditType.Language)); }
		}
		CodeDescriptionPairList languages;

		#endregion

		#region IsExpensiveQuery

		public override bool IsExpensiveQuery
		{
			get
			{
				return false;
			}
		}

		#endregion
	}
}
