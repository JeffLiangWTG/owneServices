using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	internal class PeriodDateRangeBuilder : BaseDateBuilder<ZDateTime>
	{
		public PeriodDateRangeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultFrom);
			ExpectedProperties.Add(DefaultTo);
		}

		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"Period Date Range", Res.GetString("FilterDocumentation|9FAB5850-C1E7-461F-928F-0D16E5390F43", "Generates a date range control with accounting period support. Filters data matching the selected date range."), supportedProperties);
		}

		public override bool CanBuild(string filterType) =>
				filterType.Equals((NoResString)"period date range", StringComparison.OrdinalIgnoreCase);

		protected override FilterField GetFilterField()
		{
			return new PeriodDateRangeField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			using (newField.GetValidationSuspender())
			{
				((PeriodDateRangeField)(newField)).ValueLow = ExtractFromTree(fieldTree, newField, DefaultFrom);
				((PeriodDateRangeField)(newField)).ValueHigh = ExtractFromTree(fieldTree, newField, DefaultTo);
			}
		}

		ZDateTime ExtractFromTree(StringTreeNode fieldTree, FilterField newField, string fromOrTo)
		{
			ZDateTime result = ZDateTime.Empty;
			if (fieldTree.ChildExists(fromOrTo))
			{
				string defaultString = fieldTree.FindChild(fromOrTo).Child().Value;
				if (defaultString.Equals((NoResString)"<now>", StringComparison.OrdinalIgnoreCase))
				{
					result = ZDateTime.Now;
				}
				else
				{
					try
					{
						defaultString = defaultString.Replace("<", "").Replace(">", "");
						DateTime defaultTime = DateTime.Parse(defaultString);
						result = new ZDateTime(defaultTime.Year, defaultTime.Month, defaultTime.Day);
					}
					catch (FormatException)
					{
						throw new TemplateDefinitionException("Unknown date format in default value", CellReference.UnKnown);
					}
				}
			}

			return result;
		}

		#endregion
	}
}
