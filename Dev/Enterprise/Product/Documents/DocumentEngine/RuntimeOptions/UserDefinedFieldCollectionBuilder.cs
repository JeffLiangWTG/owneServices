using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class UserDefinedFieldCollectionBuilder
	{
		public UserDefinedFieldCollectionBuilder(StringTreeNode rootNode, string dataContext, ValidatorPack validatorPack, MatchEvaluator evaluatorForDefaultValues)
		{
			this.rootNode = rootNode;
			this.dataContext = dataContext;
			this.validatorPack = validatorPack;
			this.EvaluatorForDefaultValues = evaluatorForDefaultValues;
		}

		public UserDefinedFieldCollectionBuilder(ExcelWorkSheet uDFSheet, ValidatorPack validatorPack, MatchEvaluator evaluatorForDefaultValues)
		{
			Sheet = uDFSheet;
			this.validatorPack = validatorPack;
			this.EvaluatorForDefaultValues = evaluatorForDefaultValues;
		}

		public void Build()
		{
			Errors.Clear();

			try
			{
				BuildFieldsFromTree(RootNode);
			}
			catch (TemplateDefinitionException ex)
			{
				Errors.Add(new ReportProcessingError(Res.GetString("0295c36b-5c7e-4586-b68a-8cf8b51d3534", "Error Building UDF's: {0}", ex.Message), ex.CellReference, ReportProcessingErrorSeverity.Error));
			}
		}

		public readonly UserControlProviderList UserDefinedFields = new UserControlProviderList();
		public List<IReportProcessingError> Errors
		{
			get { return errors; }
		}

		public StringTreeNode RootNode
		{
			get
			{
				if (rootNode == null)
				{
					rootNode = new StringTreeBuilder(Sheet).GetTree();
				}
				return rootNode;
			}
		}

		#region Implementation

		protected StringTreeNode rootNode;
		protected string dataContext;
		readonly ValidatorPack validatorPack;
		protected ExcelWorkSheet Sheet;
		readonly MatchEvaluator EvaluatorForDefaultValues;
		protected List<IReportProcessingError> errors = new List<IReportProcessingError>();

		protected BusinessObjectFactory Factory
		{
			get
			{
				if (factory == null)
				{
					factory = new BusinessObjectFactory() { NameForDebugging = "User Defined Field Collection Builder" };
				}
				return factory;
			}
		}
		protected BusinessObjectFactory factory;

		protected void BuildFieldsFromTree(StringTreeNode root)
		{
			foreach (StringTreeNode fieldDef in root.Children)
			{
				try
				{
					if (!UserDefinedFields.ContainsName(fieldDef.Value))
					{
						StringTreeNode typeNode = fieldDef.FindChild((NoResString)"type").Child();
						FilterField newField = FindBuilderForType(typeNode).Build(fieldDef, dataContext);
						UserDefinedFields.Add(newField);
					}
				}
				catch (TemplateDefinitionException ex)
				{
					Errors.Add(new ReportProcessingError(Res.GetString("1873bf55-3a65-42e7-a4e1-34d4f6e1a831", "Error Building UDF's from Tree: {0}", ex.Message), ex.CellReference, ReportProcessingErrorSeverity.Error));
				}
			}
		}

		ArrayList userDefinedFieldBuilders;
		internal ArrayList UserDefinedFieldBuilders
		{
			get
			{
				if (userDefinedFieldBuilders == null)
				{
					userDefinedFieldBuilders = new ArrayList();
					foreach (var fieldType in userDefinedFieldBuilderList)
					{
						userDefinedFieldBuilders.Add((UserDefinedFieldBuilder)Activator.CreateInstance(fieldType, new object[] { validatorPack, Factory, EvaluatorForDefaultValues }));
					}
				}
				return userDefinedFieldBuilders;
			}
		}

		Type[] userDefinedFieldBuilderList
		{
			get
			{
				return new Type[]
				{
					typeof(CheckBoxFieldBuilder),
					typeof(DateFieldBuilder),
					typeof(LookupFieldBuilder),
					typeof(MultilineTextFieldBuilder),
					typeof(MultipleChoiceFieldBuilder),
					typeof(NumberFieldBuilder),
					typeof(SingleLineTextFieldBuilder)
				};
			}
		}

		public UserDefinedFieldBuilder FindBuilderForType(StringTreeNode typeNode)
		{
			UserDefinedFieldBuilder builder = null;

			foreach (UserDefinedFieldBuilder candidate in UserDefinedFieldBuilders)
			{
				if (candidate.CanBuild(typeNode.Value))
				{
					if (builder != null)
					{
						// We found more than one builder for this type. This is a programming error, not a template error.
						// There should only be one builder per type.
						throw new InvalidOperationException("Ambiguous user defined field builder name " + typeNode.Value + " - multiple user defined field buiders found");
					}
					else
					{
						builder = candidate;
					}
				}
			}

			if (builder != null)
			{
				return builder;
			}
			else
			{
				throw new TemplateDefinitionException("Unknown user defined field type \"" + typeNode.Value + "\"", typeNode.CellReference);
			}
		}

		#endregion
	}
}
