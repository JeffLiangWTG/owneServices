using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.PAVE.MENT.Shared;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	[CodeProperty(BMComponentAcceptabilityBandSchema.Constants.BAB_Name), DescriptionProperty(BMComponentAcceptabilityBandSchema.Constants.BAB_Name)]
	[DebuggerDisplay("{BAB_Name}: Between {BAB_CautionLowerBound} and {BAB_CautionUpperBound}")]
	public class BMComponentAcceptabilityBand : AutoBMComponentAcceptabilityBand,
		IMENTQueryable,
		ITemplateCopyable,
		IFilterPreviewable,
		IRelatedModuleFilterSupportable,
		IAuditParent
	{
		public BMComponentAcceptabilityBand(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		public IMENTAcceptabilityBandViewModel MENTAcceptabilityBandViewModel { get; set; }

		public override ZString BAB_Name
		{
			get { return base.BAB_Name; }
			set
			{
				base.BAB_Name = value;
				MENTAcceptabilityBandViewModel?.UpdateMENTCode();
			}
		}

		[List("Lookups.Components")]
		[RelatedBusinessObject("Component")]
		public override ZGuid BAB_FC_Component
		{
			get { return base.BAB_FC_Component; }
			set
			{
				base.BAB_FC_Component = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateBAB_FC_Component();
				}
			}
		}

		public override ZInt BAB_CautionLowerBound
		{
			get { return base.BAB_CautionLowerBound; }
			set
			{
				base.BAB_CautionLowerBound = value;
				ValidateAllRangeValues();
			}
		}

		public override ZInt BAB_GoodLowerBound
		{
			get { return base.BAB_GoodLowerBound; }
			set
			{
				base.BAB_GoodLowerBound = value;
				ValidateAllRangeValues();
			}
		}

		public override ZInt BAB_ExcellentLowerBound
		{
			get { return base.BAB_ExcellentLowerBound; }
			set
			{
				base.BAB_ExcellentLowerBound = value;
				ValidateAllRangeValues();
			}
		}

		public override ZInt BAB_ExcellentUpperBound
		{
			get { return base.BAB_ExcellentUpperBound; }
			set
			{
				base.BAB_ExcellentUpperBound = value;
				ValidateAllRangeValues();
			}
		}

		public override ZInt BAB_GoodUpperBound
		{
			get { return base.BAB_GoodUpperBound; }
			set
			{
				base.BAB_GoodUpperBound = value;
				ValidateAllRangeValues();
			}
		}

		public override ZInt BAB_CautionUpperBound
		{
			get { return base.BAB_CautionUpperBound; }
			set
			{
				base.BAB_CautionUpperBound = value;
				ValidateAllRangeValues();
			}
		}

		[ReadOnlyMember(nameof(IsSqlDisabled))]
		public override ZString BAB_SqlText
		{
			get { return base.BAB_SqlText; }
			set { base.BAB_SqlText = value; }
		}

		[List("Lookups.Types")]
		public override ZString BAB_Type
		{
			get { return base.BAB_Type; }
			set
			{
				base.BAB_Type = value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateBAB_FC_Component();
					Validation.ValidateBAB_FiltersByReleaseGroup();
					Validation.ValidateBAB_FiltersBySection();
				}

				BoundaryValuesHintLabelInfo.RefreshBinding();
			}
		}

		void ValidateAllRangeValues()
		{
			if (!IsValidationSuspended)
			{
				Validation.ValidateBAB_CautionLowerBound();
				Validation.ValidateBAB_GoodLowerBound();
				Validation.ValidateBAB_ExcellentLowerBound();
				Validation.ValidateBAB_ExcellentUpperBound();
				Validation.ValidateBAB_GoodUpperBound();
				Validation.ValidateBAB_CautionUpperBound();
			}
		}

		#endregion

		#region New Properties

		public bool IsFilterStripsDisabled
		{
			get { return BAB_Type == AcceptabilityBandTypes.Codes.SQL; }
		}

		public bool IsSqlDisabled
		{
			get
			{
				switch (BAB_Type)
				{
					case AcceptabilityBandTypes.Codes.Count:
					case AcceptabilityBandTypes.Codes.TotalPlannedDuration:
					case AcceptabilityBandTypes.Codes.NumberAsPercentage:
					case AcceptabilityBandTypes.Codes.PlannedDurationPercentage:
						return true;

					default:
						return false;
				}
			}
		}

		public bool IsPercentageBand
		{
			get
			{
				return BAB_Type == AcceptabilityBandTypes.Codes.NumberAsPercentage ||
					BAB_Type == AcceptabilityBandTypes.Codes.PlannedDurationPercentage;
			}
		}

		public ResourceString SQLDescription
		{
			get
			{
				var workflowsStatement = BAB_Type == AcceptabilityBandTypes.Codes.Aggregate ? WorkflowsStatement : string.Empty;

				return ResString.GetMultilingualString("6e2c310b-26ce-4d2b-b530-dcc1e91a980a", "Enter a SELECT statement that generates a result set containing three fields: '{0}', '{1}', and '{2}'. The result which contains the selected Component primary key is the result which will be used. To generate multiple bands based on an aggregated value, include a fourth column '{3}' and group by the values in this column to produce one row per unique value. {4}",
					ValueColumnName, ComponentColumnName, ReleaseGroupColumnName, AdditionalAggregatorColumnName, workflowsStatement);
			}
		}

		string WorkflowsStatement
		{
			get
			{
				var filtersBySectionResStringData = DataBoundResourceStrings.GetDataForProperty(BAB_FiltersBySectionInfo);

				return Res.GetString("4cd22c86-be06-4dd6-bcff-0c3baf90ecae", "The query returned by the Value Calculation Filters must be included in the query and can be accessed via the '{0}' result set. If '{1}' is enabled, rows for workflows in the associated section can be accessed via the '{2}' result set.", FilteredWorkflowsResultSetName, filtersBySectionResStringData.Caption, BoardSectionWorkflowsResultSetName);
			}
		}

		public ResourceString ValueCalculationFilterRuleDescription
		{
			get { return ResString.GetMultilingualString("f0cf9339-56c7-412a-a0cb-ce428e9197a6", "Specify filters that restrict which items from the Job Workflows module match this Acceptability Band."); }
		}

		public ResourceString SupersetFilterRuleDescription
		{
			get { return ResString.GetMultilingualString("ad2445c0-2d34-4dce-b646-c8eacef2af71", "Specify filters that restrict which items from the Job Workflows module are used to calculate the total work. Leave this empty to consider all work in the Component."); }
		}

		public ResourceString VisualizationOptionsDescription => ResString.GetMultilingualString("3477947d-605b-4b8f-808c-1a02fe46717e", "Value calculation options for acceptability bands when shown on a visual board");

		public ZString BoundaryValuesHintLabel
		{
			get
			{
				var result = new StringBuilder(Res.GetString("eaa215e1-b49d-4ed1-834e-1b8384ee3f35", "Specify the values used to calculate the status for this Acceptability Band."));

				if (!string.IsNullOrEmpty(BAB_Type))
				{
					result.Append(" ");
				}

				switch (BAB_Type)
				{
					case AcceptabilityBandTypes.Codes.Aggregate:
					case AcceptabilityBandTypes.Codes.SQL:
						result.Append(Res.GetString("d2e0880f-0688-4e55-90e6-da1af48b27f6", "These values represent the value returned by the SQL statement."));
						break;

					case AcceptabilityBandTypes.Codes.Count:
						result.Append(Res.GetString("4fa241e9-51ca-41dc-9488-710c5918160f", "These values represent the number of items returned by the filter strips."));
						break;

					case AcceptabilityBandTypes.Codes.TotalPlannedDuration:
						result.Append(Res.GetString("3703fd3e-5e3f-4438-955e-d1e2c10a9e54", "These values represent the total number of hours in duration."));
						break;

					case AcceptabilityBandTypes.Codes.NumberAsPercentage:
					case AcceptabilityBandTypes.Codes.PlannedDurationPercentage:
						result.Append(Res.GetString("4AC35017-CB0C-4CC8-8472-6619BEE6B782", "These values represent percentages in whole numbers."));
						break;
				}

				return result.ToString();
			}
		}

		public ZPropertyInfo BoundaryValuesHintLabelInfo
		{
			get { return GetZPropertyInfo(nameof(BoundaryValuesHintLabel)); }
		}

		public AcceptabilityBandBoundaryValues BoundaryValues => new AcceptabilityBandBoundaryValues(BAB_CautionLowerBound, BAB_GoodLowerBound, BAB_ExcellentLowerBound, BAB_ExcellentUpperBound, BAB_GoodUpperBound, BAB_CautionUpperBound);

		public bool UseFilterStripsExclusively => BAB_Type != AcceptabilityBandTypes.Codes.SQL && BAB_Type != AcceptabilityBandTypes.Codes.Aggregate;

		#endregion

		#region Related Business Objects

		public BMComponent Component => Factory.Load<BMComponent>(BAB_FC_Component);

		FilterRuleProvider FilterRuleProvider => filterRuleProvider ??= new BMFilterRuleProvider(this, filterName: "VAL");
		FilterRuleProvider filterRuleProvider;

		public StmModuleFilter FilterRule => FilterRuleProvider.GetOrCreateAndCacheFilter();

		FilterRuleProvider SupersetFilterRuleProvider => supersetFilterRuleProvider ??= new BMFilterRuleProvider(this, filterName: "SUP");
		FilterRuleProvider supersetFilterRuleProvider;

		public StmModuleFilter SupersetItemsFilterRule => SupersetFilterRuleProvider.GetOrCreateAndCacheFilter();

		internal ZQuery GetSupersetFilterQuery()
		{
			return RelatedModuleFiltersHelper.GetFilterQuerySafe(SupersetItemsFilterRule);
		}

		#endregion

		#region Business Object Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("144ee96f-dc7a-45cb-b18c-aedb6a479c00", "Acceptability Band - {0}", BAB_Name); }
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (IsInDatabase && BAB_IsActive != (ZBool)BAB_IsActiveInfo.OriginalValue)
			{
				var boards = GetAllBoardsUsingThisAcceptabilityBand();
				var utcNow = ZDateTime.UtcNow;

				foreach (var board in boards)
				{
					board.MB_SystemLastEditTimeUtc = utcNow;
				}
			}
		}

		public override void Delete()
		{
			DeleteBandFromSectionConfigurationInBoards();
			FilterRuleProvider.DeleteFilter();
			SupersetFilterRuleProvider.DeleteFilter();

			base.Delete();
		}

		void DeleteBandFromSectionConfigurationInBoards()
		{
			foreach (var board in GetAllBoardsUsingThisAcceptabilityBand())
			{
				foreach (var section in board.Sections)
				{
					if (section.MS_SectionType == BMConstants.ComponentSectionType)
					{
						var sectionContainDeletedBand = section.SectionConfiguration.AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().SingleOrDefault(b => b.AcceptabilityBandPK == PK);
						if (sectionContainDeletedBand != null)
						{
							sectionContainDeletedBand.Delete();
							section.SectionConfiguration.AcceptabilityBands.Remove(sectionContainDeletedBand);
						}
					}
				}
			}
		}

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var clone = (BMComponentAcceptabilityBand)base.CloneInternal(args);

			FilterRuleProvider.CopyFilterStrips(() => clone.FilterRule);
			SupersetFilterRuleProvider.CopyFilterStrips(() => clone.SupersetItemsFilterRule);

			BMExtensionMethods.SetCopiedBizoNamePropertyComplyingWithMaxLength(clone.BAB_NameInfo);

			return clone;
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			return this.Clone();
		}

		#endregion

		#region IFilterPreviewable Members

		public ZQuery GetAdditionalPreviewFilter(string moduleId, string dropDownCode)
		{
			var query = new ZQuery();

			if (IsPercentageBand)
			{
				var supersetQuery = GetSupersetFilterQuery();
				if (!supersetQuery.IsNoResultQuery)
				{
					query.AddToFilter(supersetQuery);
				}
			}

			if (!BAB_FC_Component.IsEmpty)
			{
				query.AddToFilter(ProcessHeaderSchema.FH_FC_CurrentComponent, BAB_FC_Component);
			}

			return query;
		}

		#endregion

		#region SQL

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Data set name constant")]
		public const string FilteredWorkflowsResultSetName = "Workflows";
		public const string BoardSectionWorkflowsResultSetName = "SectionWorkflows"; // Data set name constant

		public DbCommand GetAcceptabilityBandSqlCommand(AcceptabilityBandSqlBuilderParameters parameters, DbConnection connection, bool createInsertQuery = false)
		{
			var strategy = GetSqlStrategy(parameters);
			var (sql, commandParameters) = strategy.GetAcceptabilityBandSql(createInsertQuery);
			var command = new ZSqlConnectionInfo(connection, null).GetNewDbCommandForSelect(sql, commandParameters);

			var commandTimeoutValue = Env.Instance.IsWebService
				? BMSRegistry.Instance.AcceptabilityBandCalculationServiceExecutionTimeout.Value
				: BMSRegistry.Instance.AcceptabilityBandLocalCalculationExecutionTimeout.Value;
			if (commandTimeoutValue > 0)
			{
				command.CommandTimeout = commandTimeoutValue;
			}

			return command;
		}

		public IAcceptabilityBandSqlStrategy GetSqlStrategy(AcceptabilityBandSqlBuilderParameters parameters)
		{
			switch (BAB_Type)
			{
				case AcceptabilityBandTypes.Codes.Aggregate:
					return new AggregateAcceptabilityBandSqlStrategy(this, parameters);
				case AcceptabilityBandTypes.Codes.Count:
					return new CountAcceptabilityBandSqlStrategy(this, parameters);
				case AcceptabilityBandTypes.Codes.TotalPlannedDuration:
					return new TotalPlannedDurationAcceptabilityBandSqlStrategy(this, parameters);
				case AcceptabilityBandTypes.Codes.PlannedDurationPercentage:
					return new PlannedDurationPercentageAcceptabilityBandSqlStrategy(this, parameters);
				case AcceptabilityBandTypes.Codes.NumberAsPercentage:
					return new NumberAsPercentageAcceptabilityBandSqlStrategy(this, parameters);
				case AcceptabilityBandTypes.Codes.SQL:
					return new SqlAcceptabilityBandSqlStrategy(this, parameters);
				default:
					throw new InvalidOperationException(FormattableString.Invariant($"{Schema.BAB_Type} '{BAB_Type}' is not known."));
			}
		}

		internal ZNonPersistentDataQuery CachedFilterRuleSql { get; set; }

		public bool UsesFilterStrips =>
			BAB_Type == AcceptabilityBandTypes.Codes.Aggregate ||
			BAB_Type == AcceptabilityBandTypes.Codes.Count ||
			BAB_Type == AcceptabilityBandTypes.Codes.NumberAsPercentage ||
			BAB_Type == AcceptabilityBandTypes.Codes.PlannedDurationPercentage ||
			BAB_Type == AcceptabilityBandTypes.Codes.TotalPlannedDuration;

		public IEnumerable<BMBoard> GetAllBoardsUsingThisAcceptabilityBand()
		{
			var boardQuery = new ZDBOnlyQuery(typeof(BMBoard));
			var sectionQuery = new ZDBOnlySubQuery(typeof(BMBoardSection), BMBoardSectionSchema.MS_MB_Board);

			sectionQuery.AddFilterAndZSQLParameterCollection(FormattableString.Invariant($"1 = [MS_LayoutData].exist('/MS_LayoutData/Configuration/AcceptabilityBands/Enterprise.BufferManagement.Business.BoardSectionAcceptabilityBand/AcceptabilityBandPK[text()=''{PK}'']')"), new ZSqlParameterCollection());
			boardQuery.AddSubQuery(sectionQuery, JoinCondition.And);

			return Factory.Load<BMBoard>(boardQuery);
		}

		public static bool IsAdditionalAggregatorColumnPresent(string sql) =>
			Regex.IsMatch(sql, $@"\bSELECT\b.*?\b{AdditionalAggregatorColumnName}\b(?=.*?\bFROM\b)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql column name")]
		public const string ValueColumnName = "Value";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Sql column name")]
		public const string ComponentColumnName = "Component";
		public const string ReleaseGroupColumnName = "ReleaseGroup"; // Sql column name
		public const string AdditionalAggregatorColumnName = "AdditionalAggregator"; // Sql column name

		#endregion

		#region IMENTQueryable

		DataCollectionStrategy IMENTQueryable.CollectionStrategy => new AcceptabilityBandCollectionStrategy(this);

		static string tempTablePrefix => (NoResString)"##tempmentData"; // temp table name prefix
		internal string TempTableName => FormattableString.Invariant($"{tempTablePrefix}{PK.ToGuid():N}"); // temp table name

		#endregion

		#region IAuditParent Members

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion

		#region AcceptabilityBandCollectionStrategy

		class AcceptabilityBandCollectionStrategy : DataCollectionStrategy
		{
			public AcceptabilityBandCollectionStrategy(BMComponentAcceptabilityBand band)
			{
				this.band = band;
			}

			readonly BMComponentAcceptabilityBand band;

			protected override void PerformPreQueryOperation(DbConnection readerConnect)
			{
				base.PerformPreQueryOperation(readerConnect);

				DropGlobalTempTable(band.TempTableName);
				try
				{
					var parameters = new AcceptabilityBandSqlBuilderParameters(band);
					var cmd = band.GetAcceptabilityBandSqlCommand(parameters, readerConnect, true);

					cmd.ExecuteNonQuery();
				}
				catch (SqlException ex)
				{
					throw new InvalidOperationException("Insert of BAB data into temp table failed", ex);
				}
			}

			protected override void PerformPostQueryOperation(DbConnection readerConnect)
			{
				base.PerformPostQueryOperation(readerConnect);

				DropGlobalTempTable(band.TempTableName);
			}

			protected override string QueryText
			{
				get
				{
					return string.Format(CultureInfo.InvariantCulture, (NoResString)"select Value as {0}, Component as {1}, ReleaseGroup as {2}, '' as {3}, '' as {4} from {5}", // SQL needs no Res String
						MENTConstants.AgedScoreValueColumn,
						MENTConstants.ComponentColumn,
						MENTConstants.ReleaseGroupColumn,
						MENTConstants.AttributeValueColumn,
						MENTConstants.StaffColumn,
						band.TempTableName);
				}
			}

			static void DropGlobalTempTable(string tempTableName)
			{
			var sql = FormattableString.Invariant($@"DECLARE @sql NVARCHAR(MAX) = N'DROP TABLE IF EXISTS {tempTableName}';
 EXEC sp_executesql @sql;");
				Db.Connection.ExecuteNonQuery(sql); // dropping a temptable
			}
		}

		#endregion

		#region Service Functions

		public ZBool GetOverriddenShouldFilterByReleaseGroup(AcceptabilityBandVisualizationOption shouldFilterByReleaseGroupOverride)
		{
			switch (shouldFilterByReleaseGroupOverride)
			{
				case AcceptabilityBandVisualizationOption.Yes:
					return true;
				case AcceptabilityBandVisualizationOption.No:
					return false;
				case AcceptabilityBandVisualizationOption.Default:
					return BAB_FiltersByReleaseGroup;
				default:
					ErrorReporter.ReportOnce("Unknown AcceptabilityBandVisualizationOption specified.");
					return BAB_FiltersByReleaseGroup;
			}
		}

		public ZBool GetOverriddenShouldFilterBySection(AcceptabilityBandVisualizationOption shouldFilterBySectionOverride)
		{
			switch (shouldFilterBySectionOverride)
			{
				case AcceptabilityBandVisualizationOption.Yes:
					return true;
				case AcceptabilityBandVisualizationOption.No:
					return false;
				case AcceptabilityBandVisualizationOption.Default:
					return BAB_FiltersBySection;
				default:
					ErrorReporter.ReportOnce("Unknown AcceptabilityBandVisualizationOption specified.");
					return BAB_FiltersBySection;
			}
		}

		#endregion
	}
}
