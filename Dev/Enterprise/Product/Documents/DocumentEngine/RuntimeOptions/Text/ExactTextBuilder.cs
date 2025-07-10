using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class ExactTextBuilder : FilterBuilder, ICustomBuilder
	{
		#region Constructors

		public ExactTextBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
		}

		#endregion

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"exacttext", Res.GetString("FilterDocumentation|6C737AF3-2F01-46B8-B574-CABEBECD25A5", "Generates a text filter for matching a text from the default value, using an exact match. This filter is not visible in the form."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.Equals("ExactText", StringComparison.InvariantCultureIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new ExactTextField(fBusinessObjectFactory);
		}

		public void DoCustomBuilding(StringTreeNode tree, FilterField newField)
		{
			var textField = (ExactTextField)newField;

			if (tree.ChildExists(FilterBuilderPropertyCodeDescriptionList.Codes.TextDefaultValue))
			{
				var node = tree.FindChild(FilterBuilderPropertyCodeDescriptionList.Codes.DefaultValue).Child();
				textField.Value = node.Value;
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}
	}
}
