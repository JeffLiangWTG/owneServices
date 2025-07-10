using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class OptionGroupBuilder : FilterBuilder, ICustomBuilder
	{
		const string Options = FilterBuilderPropertyCodeDescriptionList.Codes.Options;
		const string RadioButton = FilterBuilderPropertyCodeDescriptionList.Codes.RadioButton;
		const string DataList = FilterBuilderPropertyCodeDescriptionList.Codes.DataList;

		public OptionGroupBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(Options);
			ExpectedProperties.Add(RadioButton);
			ExpectedProperties.Add(DataList);
			ExpectedProperties.Add(DefaultValue);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"checkbox", Res.GetString("FilterDocumentation|9A368F88-915B-4B45-8DF3-503D02D0E57B", "Generates a check box filter from the provided options allowing multiple selections. Filters the data matching the selected values."), supportedProperties);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var optionGroupField = (OptionGroup)newField;
			var options = new List<OptionGroup.Item>();

			if (fieldTree != null && fieldTree.ChildExists(DataList))
			{
				var codeListTypeName = fieldTree.FindChild(DataList).Child().Value;
				var dataListOptions = SetupCodeListMultichoiceFilter(codeListTypeName, optionGroupField);
				foreach (CodeDescriptionPair multipleChoicePair in dataListOptions)
				{
					options.Add(new OptionGroup.Item() { Code = multipleChoicePair.Code, Desc = multipleChoicePair.Description, Value = false });
				}
			}
			DoBuilding(fieldTree, newField, options);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			var options = new List<OptionGroup.Item>();
			DoBuilding(fieldTree, newField, options, false);
		}

		void DoBuilding(StringTreeNode fieldTree, FilterField newField, List<OptionGroup.Item> options, bool isInRuntime = true)
		{
			var optionGroupField = (OptionGroup)newField;

			optionGroupField.IsRadioButton = fieldTree.ChildExists(RadioButton);

			var defaultList = new ArrayList();

			if (fieldTree.ChildExists(DefaultValue))
			{
				var defaultString = fieldTree.FindChild(DefaultValue).Child().Value;
				var defaultValues = defaultString.Split(new char[] { ',' });
				foreach (var value in defaultValues)
				{
					defaultList.Add(value.Trim());
				}
			}

			if (fieldTree.ChildExists(Options))
			{
				foreach (var optionNode in fieldTree.FindChild(Options).Children)
				{
					var selectedValue = optionNode.Value;
					var displayName = optionNode.Child().Value;
					var displayNameLocalized = DocBuilder.DocBuilderResourceStrings.GetReportString(TemplateFileName, displayName);
					if (selectedValue.IndexOf(",", StringComparison.OrdinalIgnoreCase) > -1)
					{
						throw new TemplateDefinitionException("',' is not allowed in the code part of the option, you can use it in Display name however.", fieldTree.CellReference);
					}
					options.Add(new OptionGroup.Item() { Code = selectedValue, Desc = displayNameLocalized, Value = defaultList.Contains(selectedValue.Trim()) });
				}
			}

			if (options.Count > 0)
			{
				optionGroupField.AddAllOptions(options);
			}

			if (optionGroupField.DescriptionCodePairList.Count == 0 && isInRuntime)
			{
				throw new TemplateDefinitionException(String.Format(CultureInfo.InvariantCulture, @"At least one option is required for the checkbox group ""{0}""", newField.DisplayName), fieldTree.CellReference);
			}

			if (FilterTree != null && FilterTree.ChildExists(Required))
			{
				throw new TemplateDefinitionException(RequiredErrorMessage, FilterTree.FindChild(Required).CellReference);
			}
		}

		ReadOnlyCodeDescriptionPairList SetupCodeListMultichoiceFilter(string codeListType, OptionGroup newField)
		{
			var codeDescriptionPairListProvider = CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider(codeListType) ?? throw new TemplateDefinitionException(String.Format(@"Unknown code list type ""{0}""", codeListType), FilterTree.FindChild("type").Child().CellReference);
			return codeDescriptionPairListProvider.GetCodeDescriptionPairList();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer only string")]
		public const string RequiredErrorMessage = "Checkbox groups cannot be a required field. If the user leaves all the boxes unchecked, this filter will be ignored.";

		public override bool CanBuild(string filterType) =>
				filterType.Equals((NoResString)"checkbox", StringComparison.OrdinalIgnoreCase);

		protected override FilterField GetFilterField()
		{
			return new OptionGroup(fBusinessObjectFactory);
		}
	}
}
