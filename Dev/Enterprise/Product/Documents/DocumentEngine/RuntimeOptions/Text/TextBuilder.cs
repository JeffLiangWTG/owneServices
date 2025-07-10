using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	sealed class TextBuilder : FilterBuilder, ICustomBuilder
	{
		public TextBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, factory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(FilterBuilderPropertyCodeDescriptionList.Codes.TextDefaultValue);
			ExpectedProperties.Add(FilterBuilderPropertyCodeDescriptionList.Codes.FilterMethod);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"text", Res.GetString("FilterDocumentation|B757B2DD-85DA-4952-BD88-477248441AE5", "Generates a text filter for matching a text, using a \"contains\" match."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.Equals((NoResString)"Text", StringComparison.InvariantCultureIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new TextField(fBusinessObjectFactory);
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
			doBuilding(fieldTree, newField);
		}

		void doBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var textField = (TextField)newField;

			if (fieldTree.ChildExists(FilterBuilderPropertyCodeDescriptionList.Codes.TextDefaultValue))
			{
				var node = fieldTree.FindChild(FilterBuilderPropertyCodeDescriptionList.Codes.TextDefaultValue).Child();
				textField.Value = node.Value;
			}

			if (fieldTree.ChildExists(FilterBuilderPropertyCodeDescriptionList.Codes.FilterMethod))
			{
				var node = fieldTree.FindChild(FilterBuilderPropertyCodeDescriptionList.Codes.FilterMethod).Child();
				textField.FilterMethod = node.Value;
			}
		}
	}
}
