using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class MultipleSelectionLookupBuilder : LookupBuilderBase, ICustomBuilder
	{
		const string Style = FilterBuilderPropertyCodeDescriptionList.Codes.Style;
		const string SerialisedByPK = FilterBuilderPropertyCodeDescriptionList.Codes.SerialisedByPK;
		const string ColumnsOptions = FilterBuilderPropertyCodeDescriptionList.Codes.ColumnsOptions;
		const string MaxAllowableSelections = FilterBuilderPropertyCodeDescriptionList.Codes.MaxAlowableSelections;
		const string UseCodesForWhereClause = FilterBuilderPropertyCodeDescriptionList.Codes.UseCodesForWhereClause;
		const string DefaultCurrentCountryToCountryMultipleSelectionLookup = FilterBuilderPropertyCodeDescriptionList.Codes.DefaultCurrentCountryToCountryMultipleSelectionLookup;
		const string HideIfMeetCondition = FilterBuilderPropertyCodeDescriptionList.Codes.HideIfMeetCondition;

		public MultipleSelectionLookupBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			matchEvaluator = evaluatorForDefaultValues;

			ExpectedProperties.Add(MaxAllowableSelections);
			ExpectedProperties.Add(Style);
			ExpectedProperties.Add(SerialisedByPK);
			ExpectedProperties.Add(ColumnsOptions);
			ExpectedProperties.Add(UseCodesForWhereClause);
			ExpectedProperties.Add(DefaultCurrentCountryToCountryMultipleSelectionLookup);
			ExpectedProperties.Add(HideIfMeetCondition);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Multiple[ ]Selection[ ]Lookup", Res.GetString("FilterDocumentation|0F64389A-8860-4EFB-AFFC-65D4C34DE61E", "Generates a grid filter allowing multiple-selection. Filters data matching the PKs of the rows attached to the grid."), supportedProperties, true);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType);
		}

		protected override FilterField GetFilterField()
		{
			return new MultipleSelectionLookup(fBusinessObjectFactory);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			MultipleSelectionLookup field = (MultipleSelectionLookup)newField;

			if (fieldTree.ChildExists(SerialisedByPK))
			{
				field.SerialisedByPK = true;
			}

			if (fieldTree.ChildExists(UseCodesForWhereClause))
			{
				field.UseCodesForWhereClause = true;
			}

			if (fieldTree.ChildExists(ColumnsOptions))
			{
				foreach (StringTreeNode columnOptionsNode in fieldTree.FindChild(ColumnsOptions).Children)
				{
					ColumnInfo columnInfo = new ColumnInfo(columnOptionsNode.Value);
					foreach (StringTreeNode columnOptionNode in columnOptionsNode.Children)
					{
						if (columnOptionNode.Value.Equals((NoResString)"columntype", StringComparison.OrdinalIgnoreCase))
						{
							try
							{
								columnInfo.ColumnType = (ColumnTypes)Enum.Parse(typeof(ColumnTypes), columnOptionNode.Child().Value, true);
							}
							catch (Exception)
							{
								throw new TemplateDefinitionException(string.Format("'{0}' is not a valid column type.", columnOptionNode.Value), columnOptionNode.CellReference);
							}
						}
						else if (columnOptionNode.Value.ToLower() == "columnname")
						{
							columnInfo.ColumnName = columnOptionNode.Child().Value;
						}
						else
						{
							columnInfo.AddProperty(columnOptionNode.Value, columnOptionNode.Child().Value);
						}
					}
					field.Columns.Add(columnInfo);
				}
			}

			if (fieldTree.ChildExists(MaxAllowableSelections))
			{
				StringTreeNode maxAllowableSelectionsNode = fieldTree.FindChild(MaxAllowableSelections);
				string maxAllowableSelectionsValue = maxAllowableSelectionsNode.Child().Value;
				try
				{
					((MultipleSelectionLookup)newField).MaxAllowableSelections = int.Parse(maxAllowableSelectionsValue);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException(string.Format("The MaxAllowableSelections value for a Multiple selection lookup field must be a number. \"{0}\" is not a number.", maxAllowableSelectionsValue), maxAllowableSelectionsNode.Child().CellReference);
				}
				if (int.Parse(maxAllowableSelectionsValue) < 0)
				{
					throw new TemplateDefinitionException(string.Format("The MaxAllowableSelections value for a Multiple selection lookup field must be a positive number. \"{0}\" is not a positive number.", maxAllowableSelectionsValue), maxAllowableSelectionsNode.Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(DefaultCurrentCountryToCountryMultipleSelectionLookup))
			{
				field.DefaultCurrentCountryToCountryMultipleSelectionLookup();
			}
		}

		protected override void ProcessFieldTreeBeforeValidation(StringTreeNode fieldTree, FilterField newField)
		{
			var field = (MultipleSelectionLookup)newField;

			if (fieldTree.ChildExists(Style))
			{
				string style = fieldTree.FindChild(Style).Child().Value;
				if (Regex.IsMatch(style, @"^\s*Grid\s*$", RegexOptions.IgnoreCase))
				{
					field.Style = MultipleSelectionLookup.Styles.Grid;
				}
				else
				{
					throw new TemplateDefinitionException("Only Grid style is supported.", fieldTree.FindChild(Style).Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(HideIfMeetCondition))
			{
				var childNode = fieldTree.FindChild(HideIfMeetCondition).Child();
				var conditionExpression = childNode.Value;
				string conditionResult = null;

				try
				{
					conditionResult = matchEvaluator.Invoke(Regex.Match(conditionExpression, @".*")).ToUpperInvariant();
				}
				catch
				{
					throw new TemplateDefinitionException($"The condition expression '{conditionExpression}' is not a proper macro or is not supported.", childNode.CellReference);
				}

				if (conditionResult != "TRUE" && conditionResult != "FALSE")
				{
					throw new TemplateDefinitionException($"The condition expression '{conditionExpression}' should have a result either be 'TRUE' or 'FALSE'.", childNode.CellReference);
				}

				if (conditionResult == "FALSE")
				{
					field.Style = MultipleSelectionLookup.Styles.None;
				}
			}
		}

		public override void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuildingInTaskBuild(fieldTree, newField);

			if (newField is MultipleSelectionLookup multipleSelectionLookup && fieldTree.ChildExists(ColumnsOptions))
			{
				foreach (var columnOptionsNode in fieldTree.FindChild(ColumnsOptions).Children)
				{
					var columnInfo = new ColumnInfo(columnOptionsNode.Value);
					foreach (var columnOptionNode in columnOptionsNode.Children)
					{
						if (columnOptionNode.Value.ToLower() == (NoResString)"columnname")
						{
							columnInfo.ColumnName = columnOptionNode.Child().Value;
						}
					}
					multipleSelectionLookup.Columns.Add(columnInfo);
				}
			}
		}

		protected override string RegularExpressionToMatchFilterType
		{
			get { return regularExpressionToMatchFilterType; }
		}

		readonly MatchEvaluator matchEvaluator;

		internal static readonly string regularExpressionToMatchFilterType = @"Multiple\s*Selection\s*Lookup$";
		internal static readonly Regex Regex = new Regex(regularExpressionToMatchFilterType, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
	}
}
