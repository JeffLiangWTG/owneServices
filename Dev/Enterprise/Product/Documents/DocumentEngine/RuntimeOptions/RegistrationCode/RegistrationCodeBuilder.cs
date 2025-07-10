using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class RegistrationCodeBuilder : FilterBuilder, ICustomBuilder
	{
		const string FieldNameCodeCountry = FilterBuilderPropertyCodeDescriptionList.Codes.FieldNameCodeCountry;
		const string FieldNameCustomType = FilterBuilderPropertyCodeDescriptionList.Codes.FieldNameCustomType;

		public RegistrationCodeBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(FieldNameCodeCountry);
			ExpectedProperties.Add(FieldNameCustomType);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter("RegistrationCode", Res.GetString("FilterDocumentation|85B320B1-D238-4074-910E-2432557F02C7", "Generates a filter with a Country/Region and an Organization Customs Code field. Filters data for the selected country/region code and customs type code."), supportedProperties);
		}

		protected override FilterField GetFilterField()
		{
			return new RegistrationCodeField(fBusinessObjectFactory);
		}

		public override bool CanBuild(string filterType)
		{
			return Regex.IsMatch(filterType, @"^\s*RegistrationCode\s*$", RegexOptions.IgnoreCase);
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
			var registrationCodeField = (RegistrationCodeField)newField;

			if (fieldTree.ChildExists(FieldNameCodeCountry))
			{
				registrationCodeField.FieldNameCodeCountry = fieldTree.FindChild(FieldNameCodeCountry).Child().Value;
			}
			else
			{
				throw new TemplateDefinitionException(Res.GetString("f144fd0e-6eca-4c23-b3c3-faf74628b7d0", "The filter '{0}' requires the parameter '{1}'.", registrationCodeField.DisplayName, FieldNameCodeCountry), fieldTree.CellReference);
			}

			if (fieldTree.ChildExists(FieldNameCustomType))
			{
				registrationCodeField.FieldNameCustomType = fieldTree.FindChild(FieldNameCustomType).Child().Value;
			}
			else
			{
				throw new TemplateDefinitionException(Res.GetString("02bb4871-c073-4fb8-ad2a-804b4d8fda10", "The filter '{0}' requires the parameter '{1}'.", registrationCodeField.DisplayName, FieldNameCustomType), fieldTree.CellReference);
			}
		}
	}
}
