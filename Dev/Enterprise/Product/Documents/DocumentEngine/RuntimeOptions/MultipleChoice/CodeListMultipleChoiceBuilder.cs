using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class CodeListMultipleChoiceBuilder : FilterBuilder, ICustomBuilder
	{
		const string SqlDataSource = FilterBuilderPropertyCodeDescriptionList.Codes.SqlDataSource;
		const string AllowInvalidCode = FilterBuilderPropertyCodeDescriptionList.Codes.AllowInvalidCode;

		public CodeListMultipleChoiceBuilder(ValidatorPack validators, BusinessObjectFactory businessObjectFactory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType runtimeReportStyle)
			: base(validators, businessObjectFactory, evaluatorForDefaultValues, runtimeReportStyle)
		{
			ExpectedProperties.Add(DefaultValue);
			ExpectedProperties.Add(SqlDataSource);
			ExpectedProperties.Add(AllowInvalidCode);
		}

		protected override IReportDocumenter GetDocumentation(List<string> supportedProperties)
		{
			return new FilterBuilderDocumenter((NoResString)"{code list type} codelist", Res.GetString("FilterDocumentation|913BA749-C58B-41C1-BF24-4C84411CAA9F", "Generates a drop down code filter with a lookup list allowing a single value to be selected. Filters the data matching the selected value."), supportedProperties);
		}

		public override bool CanBuild(string filterType)
		{
			return filterType.ToLower().EndsWith((NoResString)"codelist");
		}

		public void DoCustomBuilding(StringTreeNode fieldTree, FilterField newField)
		{
			var codeListTypeName = Regex.Match(fieldTree.FindChild((NoResString)"type").Child().Value, @"(.*)\s+codelist$", RegexOptions.IgnoreCase).Groups[1].Value;
			var codeListField = newField as CodeListMultipleChoice ?? throw new ArgumentException("FilterField newField must be of type CodeListMultipleChoice to be used with this class.");

			if (fieldTree.ChildExists(SqlDataSource))
			{
				codeListField.SqlDataSource = fieldTree.FindChild(SqlDataSource).Child().Value;
			}

			if (fieldTree.ChildExists(AllowInvalidCode))
			{
				((CodeListMultipleChoice)newField).AllowInvalidCode = true;
			}

			SetupCodeListMultichoiceFilter(codeListTypeName, codeListField);

			if (fieldTree.ChildExists(DefaultValue))
			{
				codeListField.Value = codeListField.DefaultExpression = fieldTree.FindChild(DefaultValue).Child().Value;
			}
		}

		public void DoCustomBuildingInTaskBuild(StringTreeNode fieldTree, FilterField newField)
		{
		}

		void SetupCodeListMultichoiceFilter(string codeListType, CodeListMultipleChoice newField)
		{
			var listProvider = CodeDescriptionPairListProviderFactory.GetStaticCodeDescriptionPairListProvider(codeListType);
			if (listProvider != null)
			{
				newField.SetPairList(listProvider.GetCodeDescriptionPairList());
				newField.DependenceListProvider = listProvider as IDependenceCodeDescriptionPairListProvider;
			}
			else if (!newField.SqlDataSource.IsEmpty)
			{
				var sqlDataSourceWithReaderFlag = newField.SqlDataSource + "\r\n\r\n" + DbCommand.ExecuteAsReaderFlagComments;
				var codeDescriptionPairList = new CodeDescriptionPairList();

				try
				{
					using (var command = Db.Connection.Command(sqlDataSourceWithReaderFlag))
					{
						using (var dr = command.ExecuteReader())
						{
							while (dr.Read())
							{
								var code = dr[0].ToString().Trim();
								var descriptionLocalized = dr.FieldCount > 1 ? DocBuilder.DocBuilderResourceStrings.GetReportString(TemplateFileName, dr[1].ToString()) : code;
								codeDescriptionPairList.AddPair(code, descriptionLocalized);
							}
						}
					}
				}
				catch (SqlException ex)
				{
					throw new TemplateDefinitionException(String.Format(CultureInfo.InvariantCulture, @"Could not get code list from SqlDataSource [{0}]: {1}", newField.SqlDataSource, ex.Message), FilterTree.FindChild("type").Child().CellReference);
				}

				newField.SetPairList(codeDescriptionPairList);
			}
			else
			{
				throw new TemplateDefinitionException(String.Format(@"Unknown code list type ""{0}""", codeListType), FilterTree.FindChild("type").Child().CellReference);
			}
		}

		protected override FilterField GetFilterField()
		{
			return new CodeListMultipleChoice(fBusinessObjectFactory);
		}
	}
}
