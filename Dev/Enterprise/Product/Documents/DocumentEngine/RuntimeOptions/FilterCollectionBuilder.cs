using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	/// <summary>
	/// Builds IFilters into a FilterCollection by reading from an ExcelInterface
	/// </summary>
	class FilterCollectionBuilder : SheetCollectionBuilder
	{
		public FilterCollectionBuilder(StringCollection parameters, ValidatorPack validatorPack, StringTreeNode root, MatchEvaluator evaluatorForDefaultValues, string templateFileName = "", ReportRunningType reportRunningType = ReportRunningType.Report)
		{
			this.Parameters = parameters;//analyser.DataSourceParameters;
			fValidatorPack = validatorPack;
			this.Root = root;//new StringTreeBuilder(Sheet).GetTree();
			this.EvaluatorForDefaultValues = evaluatorForDefaultValues;
			this.ColumnHeadingManager = new ColumnConfigurationsManager(ZGuid.Empty, false);
			this.templateFileName = templateFileName;
			this.reportRunningType = reportRunningType;
		}

		public FilterCollectionBuilder(StringCollection parameters, ValidatorPack validatorPack, StringTreeNode root, MatchEvaluator evaluatorForDefaultValues, ColumnConfigurationsManager columnHeadingManager, string templateFileName = "", ReportRunningType reportRunningType = ReportRunningType.Report)
			: this(parameters, validatorPack, root, evaluatorForDefaultValues, templateFileName, reportRunningType)
		{
			this.ColumnHeadingManager = columnHeadingManager;
		}

		readonly string templateFileName;
		protected StringCollection Parameters;
		protected ValidatorPack fValidatorPack;
		protected StringTreeNode Root;
		readonly MatchEvaluator EvaluatorForDefaultValues;
		readonly ColumnConfigurationsManager ColumnHeadingManager;
		readonly ReportRunningType reportRunningType;

		protected CollectionOfIFilter fFilters = new CollectionOfIFilter();
		protected List<IReportProcessingError> fErrors = new List<IReportProcessingError>();

		public CollectionOfIFilter IFilterCollection
		{
			get { return HasErrors ? new CollectionOfIFilter() : fFilters; }
		}

		public List<IReportProcessingError> Errors
		{
			get { return fErrors; }
		}

		public bool HasErrors
		{
			get { return Errors.Count != 0; }
		}

		// N.B. Does not return the FilterCollection, because the Errors are just as important.
		// If we returned the FilterCollection here, clients would be more likely to forget to check Errors.
		public void Build(bool addConfigurations, bool isFromRuntime = true)
		{
			Errors.Clear();
			IFilterCollection.Clear();
			try
			{
				BuildFiltersFromTree(isFromRuntime);
				if (addConfigurations && IFilterCollection.Count > 0 && isFromRuntime)
				{
					ColumnConfigurationField configField = new ColumnConfigurationField(Factory);
					configField.DisplayName = (NoResString)"Configurations";
					((IColumnHeadingManagerListener)configField).SetManager(ColumnHeadingManager);
					IFilterCollection.Insert(0, configField);

					if (!IFilterCollection.FilterGroups.ContainsCode(configField.GroupName))
					{
						var pair = new CodeDescriptionPair(configField.GroupName, configField.GroupDescription);
						IFilterCollection.FilterGroups.Insert(0, pair);
					}
				}
			}
			catch (TemplateDefinitionException ex)
			{
				Errors.Add(new ReportProcessingError(Res.GetString("44e2dc9e-5acc-4fc9-95fa-5510b0e847f6", "Error Building Filters: {0}", ex.Message),
																						ex.CellReference, ReportProcessingErrorSeverity.Error));
			}
		}

		public void Build()
		{
			Build(true);
		}

		public void BuildFiltersInTaskBuild()
		{
			Build(true, false);
		}

		protected void BuildFiltersFromTree(bool isFromRuntime = true)
		{
			foreach (StringTreeNode filterDef in Root.Children)
			{
				StringTreeNode typeNode = null;
				try
				{
					typeNode = filterDef.FindChild((NoResString)"type").Child();

					var filterBuilder = FindBuilderForType(typeNode, isFromRuntime);
					filterBuilder.TemplateFileName = templateFileName;

					IFilter newField = filterBuilder.Build(filterDef, Parameters, isFromRuntime);
					IFilterCollection.Add(newField);
				}
				catch (TemplateDefinitionException ex)
				{
					var errorMessage = Res.GetString("95394e3e-dc71-4196-b5f5-cccd57843eca", "Error Building Filters from Tree: {0}", ex.Message);

					Errors.Add(new ReportProcessingError(errorMessage
						, typeNode == null ? ex.CellReference : typeNode.CellReference
						, ReportProcessingErrorSeverity.Error));
				}
			}
			IFilterCollection.ProcessRelationsBetweenFilters();
			BuildFilterGroupsFromFilters();
		}

		void BuildFilterGroupsFromFilters()
		{
			foreach (FilterField field in IFilterCollection)
			{
				if (!IFilterCollection.FilterGroups.ContainsCode(field.GroupName))
				{
					IFilterCollection.FilterGroups.AddPair(field.GroupName, field.GroupDescription);
				}
			}
		}

		ArrayList fFilterBuilders;
		internal ArrayList FilterBuilders
		{
			get
			{
				if (fFilterBuilders == null)
				{
					fFilterBuilders = new ArrayList();
					foreach (var filterType in filterBuilderList)
					{
						fFilterBuilders.Add((FilterBuilder)Activator.CreateInstance(filterType, new object[] { fValidatorPack, Factory, EvaluatorForDefaultValues, reportRunningType }));
					}
					fFilterBuilders.Add(new ColumnConfigurationFilterBuilder(fValidatorPack, Factory, EvaluatorForDefaultValues, ColumnHeadingManager, reportRunningType));
				}
				return fFilterBuilders;
			}
		}

		ArrayList fFilterBuildersInTaskBuild;
		ArrayList FilterBuildersInTaskBuild
		{
			get
			{
				if (fFilterBuildersInTaskBuild == null)
				{
					fFilterBuildersInTaskBuild = new ArrayList();
					foreach (var filterType in filterBuilderList)
					{
						fFilterBuildersInTaskBuild.Add((FilterBuilder)Activator.CreateInstance(filterType, new object[] { fValidatorPack, null, EvaluatorForDefaultValues, reportRunningType }));
					}
				}
				return fFilterBuildersInTaskBuild;
			}
		}

		Type[] filterBuilderList
		{
			get
			{
				return new Type[]
				{
					typeof(AccountingPeriodBuilder),
					typeof(AccountingPeriodsRangeBuilder),
					typeof(DateBasedAccountingPeriodBuilder),
					typeof(PeriodDateRangeBuilder),
					typeof(SingleAccountingPeriodBuilder),
					typeof(SingleAccountingPeriodEndDateBuilder),
					typeof(SingleAccountingPeriodStartDateBuilder),
					typeof(CurrentCompanyBuilder),
					typeof(DateBuilder),
					typeof(DateRangeBuilder),
					typeof(CodeLookupBuilder),
					typeof(LookupBuilder),
					typeof(CodeListMultipleChoiceBuilder),
					typeof(CodeListMultipleSelectionBuilder),
					typeof(MultipleChoiceBuilder),
					typeof(NumberBuilder),
					typeof(NumberNotInRangeBuilder),
					typeof(AccountingNumberRangeBuilder),
					typeof(OptionGroupBuilder),
					typeof(TextBuilder),
					typeof(TextRangeBuilder),
					typeof(MaximumDateFieldBuilder),
					typeof(MultipleSelectionLookupBuilder),
					typeof(MinimumDateFieldBuilder),
					typeof(NumberRangeBuilder),
					typeof(ExactTextBuilder),
					typeof(SecurityFilterBuilder),
					typeof(RegistrationCodeBuilder),
					typeof(DependenceLookupBuilder),
					typeof(SalesTradeLaneChecklistBuilder),
					typeof(PermitTypeChecklistBuilder),
					typeof(DateTimeOffsetBuilder),
					typeof(DateTimeOffsetRangeBuilder),
					typeof(MonthYearPeriodBuilder),
				};
			}
		}

		public FilterBuilder FindBuilderForType(StringTreeNode typeNode, bool isFromRuntime)
		{
			FilterBuilder builder = null;
			var filterBuilders = isFromRuntime ? FilterBuilders : FilterBuildersInTaskBuild;

			foreach (FilterBuilder candidate in filterBuilders)
			{
				if (candidate.CanBuild(typeNode.Value))
				{
					if (builder != null)
					{
						// We found more than one builder for this type. This is a programming error, not a template error.
						// There should only be one builder per type.
						throw new InvalidOperationException("Ambiguous filter name " + typeNode.Value + " - multiple filterBuilders found");
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
				throw new TemplateDefinitionException("Unknown filter type \"" + typeNode.Value + "\"", typeNode.CellReference);
			}
		}

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
		}
		BusinessObjectFactory factory;
	}
}
