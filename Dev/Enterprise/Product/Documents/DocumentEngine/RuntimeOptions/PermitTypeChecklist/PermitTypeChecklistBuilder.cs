using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class PermitTypeChecklistBuilder : FilterBuilder, ICustomBuilder
	{
		const string SubTypeField = FilterBuilderPropertyCodeDescriptionList.Codes.SubTypeField;

		public PermitTypeChecklistBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, factory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(SubTypeField);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter("PermitTypeChecklist", Res.GetString("FilterDocumentation|2DBDD84F-E897-4C74-958B-389274D2D616", "Generates a three level check box list filter for Permit Types and Sub Types, with level 1 being the Type and level 2 the Sub Type which depends on level 1."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.Equals("PermitTypeChecklist", StringComparison.OrdinalIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new PermitTypeChecklistField(fBusinessObjectFactory);
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
			var field = (PermitTypeChecklistField)newField;
			field.SubTypeField = GetRequiredParameter(fieldTree, field, SubTypeField);
		}

		static string GetRequiredParameter(StringTreeNode fieldTree, FilterField field, string parameterName)
		{
			if (fieldTree.ChildExists(parameterName))
			{
				return fieldTree.FindChild(parameterName).Child().Value;
			}
			else
			{
				throw new TemplateDefinitionException(Res.GetString("418FDF20-981A-4F6B-A121-6E18157F6118", "The filter '{0}' requires the parameter '{1}'.", field.DisplayName, parameterName), fieldTree.CellReference);
			}
		}
	}
}
