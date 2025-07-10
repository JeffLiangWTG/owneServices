using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class SalesTradeLaneChecklistBuilder : FilterBuilder, ICustomBuilder
	{
		const string ModeField = FilterBuilderPropertyCodeDescriptionList.Codes.ModeField;
		const string TypeField = FilterBuilderPropertyCodeDescriptionList.Codes.TypeField;

		public SalesTradeLaneChecklistBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, factory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(ModeField);
			ExpectedProperties.Add(TypeField);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter("SalesTradeLaneChecklist", Res.GetString("FilterDocumentation|3A4C665D-0FC6-430C-B076-E779043793FE", "Generates a three level check box list filter for Sales Trades Lanes, with level 1 being the Product, level 2 the Mode which depends on level 1, and level 3 the Type depending on level 1."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.Equals("SalesTradeLaneChecklist", StringComparison.OrdinalIgnoreCase);
		}

		protected override FilterField GetFilterField()
		{
			return new SalesTradeLaneChecklistField(fBusinessObjectFactory);
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
			var field = (SalesTradeLaneChecklistField)newField;
			field.ModeField = GetRequiredParameter(fieldTree, field, ModeField);
			field.TypeField = GetRequiredParameter(fieldTree, field, TypeField);
		}

		static string GetRequiredParameter(StringTreeNode fieldTree, FilterField field, string parameterName)
		{
			if (fieldTree.ChildExists(parameterName))
			{
				return fieldTree.FindChild(parameterName).Child().Value;
			}
			else
			{
				throw new TemplateDefinitionException(Res.GetString("6fb42bb2-d546-4ddc-915b-158977d3618b", "The filter '{0}' requires the parameter '{1}'.", field.DisplayName, parameterName), fieldTree.CellReference);
			}
		}
	}
}
