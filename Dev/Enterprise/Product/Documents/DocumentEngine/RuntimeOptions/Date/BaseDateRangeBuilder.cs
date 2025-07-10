using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	abstract class BaseDateRangeBuilder<TBaseType, TZType> : BaseDateBuilder<TZType> where TBaseType : struct, IComparable where TZType : IZDate, new()
	{
		const string SubstituteMaxDateForNullTo = FilterBuilderPropertyCodeDescriptionList.Codes.SubstituteMaxDateForNullTo;
		const string SubstituteMinDateForNullFrom = FilterBuilderPropertyCodeDescriptionList.Codes.SubstituteMinDateForNullFrom;
		const string ConvertToUtc = FilterBuilderPropertyCodeDescriptionList.Codes.ConvertToUtc;
		const string RequireBothFromAndToDates = FilterBuilderPropertyCodeDescriptionList.Codes.RequireBothFromAndToDates;
		const string DateRangeMaxYears = FilterBuilderPropertyCodeDescriptionList.Codes.DateRangeMaxYears;
		const string DateRangeMaxMonths = FilterBuilderPropertyCodeDescriptionList.Codes.DateRangeMaxMonths;
		const string Language = FilterBuilderPropertyCodeDescriptionList.Codes.Language;

		protected BaseDateRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
			ExpectedProperties.Add(SubstituteMinDateForNullFrom);
			ExpectedProperties.Add(SubstituteMaxDateForNullTo);
			ExpectedProperties.Add(ConvertToUtc);
			ExpectedProperties.Add(RequireBothFromAndToDates);
			ExpectedProperties.Add(DateRangeMaxYears);
			ExpectedProperties.Add(DateRangeMaxMonths);
			ExpectedProperties.Add(Language);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^date\s?range$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new DateRangeField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			var dateRangeField = (BaseDateRangeField<TBaseType, TZType>)newField;
			using (dateRangeField.GetValidationSuspender())
			{
				dateRangeField.ValueLow = ExtractFromTree(fieldTree, DefaultFrom);
				dateRangeField.ValueHigh = ExtractFromTree(fieldTree, DefaultTo);
				dateRangeField.SubstituteMinDateForNullFrom = fieldTree.ChildExists(SubstituteMinDateForNullFrom);
				dateRangeField.SubstituteMaxDateForNullTo = fieldTree.ChildExists(SubstituteMaxDateForNullTo);
				dateRangeField.ConvertToUtc = fieldTree.ChildExists(ConvertToUtc);
				dateRangeField.RequireBothFromAndToDates = fieldTree.ChildExists(RequireBothFromAndToDates) || fieldTree.ChildExists(DateRangeMaxYears) || fieldTree.ChildExists(DateRangeMaxMonths);
				dateRangeField.DateRangeMaxYears = ExtractDateRangeMaxYearsFromTree(fieldTree);
				dateRangeField.DateRangeMaxMonths = ExtractDateRangeMaxMonthsFromTree(fieldTree);
				dateRangeField.Language = ExtractLanguageTree(fieldTree);
			}
		}

		TZType ExtractFromTree(StringTreeNode fieldTree, string fromOrTo)
		{
			if (fieldTree.ChildExists(fromOrTo))
			{
				var fromOrToNode = fieldTree.FindChild(fromOrTo);
				var defaultValue = fromOrToNode.Child().Value;

				try
				{
					return GetMacroDateReplacement(defaultValue);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException("Unknown date format in default value", fromOrToNode.Child().CellReference);
				}
			}

			return default;
		}

		ZString ExtractLanguageTree(StringTreeNode fieldTree)
		{
			if (fieldTree.ChildExists(Language))
			{
				var languageNode = fieldTree.FindChild(Language);
				var language = languageNode.Child().Value;

				if (LanguageHelper.ActiveLanguageExistsForOLookUpEditType(language))
				{
					return language;
				}
				else
				{
					throw new TemplateDefinitionException(string.Format("Unknown Language \"{0}\"", language), languageNode.Child().CellReference);
				}
			}

			return ZString.Empty;
		}

		int? ExtractDateRangeMaxYearsFromTree(StringTreeNode fieldTree)
		{
			if (fieldTree.TryFindSingleChild(DateRangeMaxYears, out var node))
			{
				var value = node.Child().Value;
				if (int.TryParse(value, out var dateRangeMaxYears) && dateRangeMaxYears > 0)
				{
					return dateRangeMaxYears;
				}
				else
				{
					throw new TemplateDefinitionException(string.Format("The DateRangeMaxYears value for a Date Range field must be a positive number. \"{0}\" is not a positive number.", value), node.Child().CellReference);
				}
			}

			return null;
		}

		int? ExtractDateRangeMaxMonthsFromTree(StringTreeNode fieldTree)
		{
			if (fieldTree.TryFindSingleChild(DateRangeMaxMonths, out var node))
			{
				var value = node.Child().Value;
				if (int.TryParse(value, out var dateRangeMaxMonths) && dateRangeMaxMonths > 0)
				{
					return dateRangeMaxMonths;
				}
				else
				{
					throw new TemplateDefinitionException(string.Format("The DateRangeMaxMonths value for a Date Range field must be a positive number. \"{0}\" is not a positive number.", value), node.Child().CellReference);
				}
			}

			return null;
		}

		#endregion
	}
}
