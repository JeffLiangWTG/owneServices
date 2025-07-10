using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class CodeListMultipleSelectionBuilder : FilterBuilder, ICustomBuilder
	{
		public CodeListMultipleSelectionBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"{code list type} codelistmultipleselect", Res.GetString("FilterDocumentation|2E201F7F-F44A-4EF9-9A7C-EC7128BC5667", "Generates a check box code filter with a lookup list allowing multiple selections. Filters the data matching the selected values."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.EndsWith((NoResString)"codelistmultipleselect", StringComparison.OrdinalIgnoreCase);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			string codeListTypeName = Regex.Match(fieldTree.FindChild((NoResString)"type").Child().Value, @"(.*)\s+codelistmultipleselect$", RegexOptions.IgnoreCase).Groups[1].Value;
			SetupOptionGroupFilterWithCodeList(codeListTypeName, newField);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		protected void SetupOptionGroupFilterWithCodeList(string codeListType, FilterField newField)
		{
			OptionGroup optionGroupField = (OptionGroup)newField;

			optionGroupField.IsRadioButton = false;

			ICodeDescriptionPairListProvider codeDescriptionPairListProvider = CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider(codeListType);
			if (codeDescriptionPairListProvider != null)
			{
				ReadOnlyCodeDescriptionPairList codeDescriptionList = codeDescriptionPairListProvider.GetCodeDescriptionPairList();
				optionGroupField.AddAllOptions(codeDescriptionList);
			}
			else
			{
				throw new TemplateDefinitionException(String.Format(@"Unknown code list type ""{0}""", codeListType), FilterTree.FindChild("type").Child().CellReference);
			}
		}

		protected override FilterField GetFilterField()
		{
			return new OptionGroup(fBusinessObjectFactory);
		}
	}
}
