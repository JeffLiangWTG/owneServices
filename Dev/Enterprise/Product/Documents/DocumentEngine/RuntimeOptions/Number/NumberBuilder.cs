using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class NumberBuilder : FilterBuilder, ICustomBuilder
	{
		const string DecimalPlaces = FilterBuilderPropertyCodeDescriptionList.Codes.DecimalPlaces;
		const string ShowGroupSeparators = FilterBuilderPropertyCodeDescriptionList.Codes.ShowGroupSeparators;
		const string MinValue = FilterBuilderPropertyCodeDescriptionList.Codes.MinValue;
		const string MaxValue = FilterBuilderPropertyCodeDescriptionList.Codes.MaxValue;

		public NumberBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
			ExpectedProperties.Add(DecimalPlaces);
			ExpectedProperties.Add(ShowGroupSeparators);
			ExpectedProperties.Add(MinValue);
			ExpectedProperties.Add(MaxValue);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[ ]number[ ]", Res.GetString("FilterDocumentation|AFAAEE4F-EA9A-46C9-B597-EEABB0BBE9EA", "Generates a number filter whose value is a decimal. Filters the data matching the number."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^\s*number\s*$", RegexOptions.IgnoreCase);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			NumberField numberField = (NumberField)newField;

			if (fieldTree.ChildExists(DefaultValue))
			{
				var defaultNode = fieldTree.FindChild(DefaultValue);
				string defaultValue = defaultNode.Child().Value.ToLower();
				switch (defaultValue)
				{
					case "<now.year>":
						numberField.Value = Env.Time.CurrentLocalDateTime.Year;
						break;
					default:
						try
						{
							numberField.Value = decimal.Parse(defaultValue, CultureInfo.InvariantCulture);
						}
						catch (FormatException)
						{
							// Ignore the error - default can still be used as a DefaultExpression
						}
						break;
				}
			}

			if (fieldTree.ChildExists(DecimalPlaces))
			{
				var decimalPlacesNode = fieldTree.FindChild(DecimalPlaces);
				string decimalPlacesValue = decimalPlacesNode.Child().Value;
				try
				{
					numberField.DecimalPlaces = int.Parse(decimalPlacesValue);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException($"The DecimalPlaces value for a number field must be a number. \"{decimalPlacesValue}\" is not a number.", decimalPlacesNode.Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(ShowGroupSeparators))
			{
				var showGroupSeparatorsNode = fieldTree.FindChild(ShowGroupSeparators);
				string showGroupSeparatorsValue = showGroupSeparatorsNode.Child().Value;
				try
				{
					numberField.ShowGroupSeparators = new ZBool(showGroupSeparatorsValue);
				}
				catch (ZTypeValueException)
				{
					throw new TemplateDefinitionException($"Invalid value for Show Group Separators for a number field: \"{showGroupSeparatorsValue}\". Value must be either Y or N.", showGroupSeparatorsNode.Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(MinValue))
			{
				var minValueNode = fieldTree.FindChild(MinValue);
				string minValueValue = minValueNode.Child().Value;
				try
				{
					numberField.MinValue = decimal.Parse(minValueValue, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException($"The MinValue value for a number field must be a number. \"{minValueValue}\" is not a number.", minValueNode.Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(MaxValue))
			{
				var maxValueNode = fieldTree.FindChild(MaxValue);
				string maxValueValue = maxValueNode.Child().Value;
				try
				{
					numberField.MaxValue = decimal.Parse(maxValueValue, CultureInfo.InvariantCulture);
				}
				catch (FormatException)
				{
					throw new TemplateDefinitionException($"The MaxValue value for a number field must be a number. \"{maxValueValue}\" is not a number.", maxValueNode.Child().CellReference);
				}
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		protected override FilterField GetFilterField()
		{
			return new NumberField(fBusinessObjectFactory);
		}
	}
}
