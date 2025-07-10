using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class MultipleChoiceBuilder : FilterBuilder, ICustomBuilder
	{
		const string Style = FilterBuilderPropertyCodeDescriptionList.Codes.Style;
		const string Option = FilterBuilderPropertyCodeDescriptionList.Codes.Option;
		const string AllowInvalidCode = FilterBuilderPropertyCodeDescriptionList.Codes.AllowInvalidCode;

		public MultipleChoiceBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValueBackwardCompatibility);
			ExpectedProperties.Add(DefaultValue);
			ExpectedProperties.Add(Style);
			ExpectedProperties.Add(Option);
			ExpectedProperties.Add(AllowInvalidCode);
			this.EvaluatorForDefaultValues = evaluatorForDefaultValues;
		}

		readonly MatchEvaluator EvaluatorForDefaultValues;

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"[ ]Multiple[ ]Choice", Res.GetString("FilterDocumentation|C83370AC-F47D-4C96-8D08-FC012E6856CB", "Generates a check box filter with a lookup list allowing multiple selections. Filters the data matching the selected values."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^\s*Multiple\s*Choice\s*$", RegexOptions.IgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new MultipleChoice(fBusinessObjectFactory);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			if (fieldTree.ChildExists(Style))
			{
				var style = fieldTree.FindChild(Style).Child().Value;
				if (Regex.IsMatch(style, @"^\s*Drop\s*Down\s*$", RegexOptions.IgnoreCase))
				{
					((MultipleChoice)newField).Style = MultipleChoice.Styles.DropDown;
				}
				else
				{
					throw new TemplateDefinitionException("Only DropDown style is supported.", fieldTree.FindChild(Style).Child().CellReference);
				}
			}

			if (fieldTree.ChildExists(AllowInvalidCode))
			{
				((MultipleChoice)newField).AllowInvalidCode = true;
			}

			if (fieldTree.ChildExists(DefaultValueBackwardCompatibility) || fieldTree.ChildExists(DefaultValue))
			{
				var validator = newField.Validators.Find(x => x is OnlyCurrentPeriodIfPayByWebServiceValidator) as OnlyCurrentPeriodIfPayByWebServiceValidator;
				if (validator != null && validator.IsPaymentWebServiceEnabled)
				{
					newField.DefaultExpression = OnlyCurrentPeriodIfPayByWebServiceValidator.NoAgeing;
				}
				else
				{
					newField.DefaultExpression = fieldTree.ChildExists(DefaultValueBackwardCompatibility) ?
						fieldTree.FindChild(DefaultValueBackwardCompatibility).Child().Value : fieldTree.FindChild(DefaultValue).Child().Value;
				}
			}

			doBuilding(fieldTree, newField);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		void doBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var multipleChoiceField = newField as MultipleChoice;
			var value = RegexProvider.InnermostMacrosRegex.Replace(multipleChoiceField.DefaultExpression, EvaluatorForDefaultValues);

			if (fieldTree.ChildExists(Option))
			{
				foreach (var optionNode in fieldTree.FindChild(Option).Children)
				{
					var code = optionNode.Value;
					var description = (optionNode.Children.Count == 1 ? optionNode.Child().Value : "");
					var descriptionLocalized = DocBuilderResourceStrings.GetReportString(TemplateFileName, description);
					multipleChoiceField.List.Add(new CodeDescriptionPair(code, descriptionLocalized));
				}
			}

			if (multipleChoiceField.List.ContainsCode(value))
			{
				multipleChoiceField.Value = value;
			}
		}
	}
}
