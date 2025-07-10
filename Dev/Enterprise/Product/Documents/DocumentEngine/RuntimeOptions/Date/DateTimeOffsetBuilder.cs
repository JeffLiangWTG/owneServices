using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class DateTimeOffsetBuilder : BaseDateBuilder<ZDateTimeOffset>
	{
		public DateTimeOffsetBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
		}

		protected override IReportDocumenter GetDateFilterDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter("DateTimeOffset", Res.GetString("FilterDocumentation|765c2076-6e11-4be3-a917-bb66dff13751", "Generates a date time offset filter. Filters data matching the date."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^date\s?time\s?offset$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new DateTimeOffsetField(fBusinessObjectFactory);
		}

		#region ICustomBuilder Members

		public override void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			base.DoCustomBuilding(fieldTree, newField);

			if (fieldTree.ChildExists(DefaultValue))
			{
				var defaultValueNode = fieldTree.FindChild(DefaultValue);
				var defaultString = defaultValueNode.Child().Value;

				try
				{
					((DateTimeOffsetField)(newField)).Value = GetMacroDateReplacement(defaultString);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException("Unknown date format in default value", defaultValueNode.Child().CellReference);
				}
			}
		}

		protected override void BuildDateFormat(StringTreeNode fieldTree, FilterField newField)
		{
			((IDateFormatSupport)newField).PickerFormat = DocEngineDatePickerFormats.Long;
		}

		#endregion
	}
}
