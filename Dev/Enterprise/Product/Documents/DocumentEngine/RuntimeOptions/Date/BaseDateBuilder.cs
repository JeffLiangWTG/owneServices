using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	#region SuppressResourceStringsCheckRegion
	abstract class BaseDateBuilder<T> : FilterBuilder, ICustomBuilder where T : IZDate, new()
	{
		protected const string DateFormat = FilterBuilderPropertyCodeDescriptionList.Codes.DateFormat;
		protected BaseDateBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected sealed override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			var documentation = GetDateFilterDocumentation(supportedProperties);
			documentation.DefaultOptions = Res.GetString("FilterDocumentation|CF342759-FF31-4785-8EB3-F3C611AE9611", "The date can be defaulted in the template with these options: {0}.", GetDateDefaultedOptionsNameString());

			return documentation;
		}

		protected abstract IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties);

		#region ICustomBuilder Members
		public virtual void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			BuildDateFormat(fieldTree, newField);
		}

		protected virtual void BuildDateFormat(StringTreeNode fieldTree, FilterField newField)
		{
			if (fieldTree.ChildExists(DateFormat))
			{
				var dateFormatNode = fieldTree.FindChild(DateFormat);
				var dateFormatValue = dateFormatNode.Child().Value.ToLower().Trim();
				switch (dateFormatValue)
				{
					case "long":
						((IDateFormatSupport)newField).PickerFormat = DocEngineDatePickerFormats.Long;
						break;
					case "short":
						((IDateFormatSupport)newField).PickerFormat = DocEngineDatePickerFormats.Short;
						break;
					case "yearandmonth":
						((IDateFormatSupport)newField).PickerFormat = DocEngineDatePickerFormats.YearAndMonth;
						break;
					default:
						throw new TemplateDefinitionException("Unknown date format \"" + dateFormatValue + "\", please choose Short or long.", dateFormatNode.CellReference);
				}
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		#endregion

		#region Helper Methods

		protected T GetMacroDateReplacement(string macro)
		{
			var zDate = new T();
			var result = zDate.Default;
			var nonWhitespaceMacro = Regex.Replace(macro.ToLower(), @"[\s*<>]", "");

			if (Enum.TryParse(nonWhitespaceMacro, true, out DateDefaultedOption dataRangeType))
			{
				switch (dataRangeType)
				{
					case DateDefaultedOption.Now:
						result = zDate.Now;
						break;
					case DateDefaultedOption.Today:
						result = zDate.Today;
						break;
					case DateDefaultedOption.LastYear:
						result = zDate.Now.AddYears(-1);
						break;
					case DateDefaultedOption.NextYear:
						result = zDate.Now.AddYears(1);
						break;
					case DateDefaultedOption.LastMonth:
						result = zDate.Now.AddMonths(-1);
						break;
					case DateDefaultedOption.NextMonth:
						result = zDate.Now.AddMonths(1);
						break;
					case DateDefaultedOption.LastThreeMonths:
						result = zDate.Now.AddMonths(-3);
						break;
					case DateDefaultedOption.NextThreeMonths:
						result = zDate.Now.AddMonths(3);
						break;
					case DateDefaultedOption.LastWeek:
						result = zDate.Now.AddDays(-7);
						break;
					case DateDefaultedOption.NextWeek:
						result = zDate.Now.AddDays(7);
						break;
					case DateDefaultedOption.Yesterday:
						result = zDate.Now.AddDays(-1);
						break;
					case DateDefaultedOption.Tomorrow:
						result = zDate.Now.AddDays(1);
						break;
					case DateDefaultedOption.FirstDayOfLastCalendarYear:
						result = zDate.Now.FirstDayOfLastCalendarYear;
						break;
					case DateDefaultedOption.LastDayOfLastCalendarYear:
						result = zDate.Now.LastDayOfLastCalendarYear;
						break;
					default:
						if (zDate.TryParse(nonWhitespaceMacro, out var date))
						{
							result = date;
						}
						break;
				}
			}
			else
			{
				if (zDate.TryParse(nonWhitespaceMacro, out var date))
				{
					result = date;
				}
			}

			return (T)result;
		}

		[SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
		protected string GetDateDefaultedOptionsNameString()
		{
			var dateDefaultedOption = Enum.GetNames(typeof(DateDefaultedOption));

			return string.Join(", ", dateDefaultedOption).ToLowerInvariant();
		}

		#endregion
	}
	#endregion
}
