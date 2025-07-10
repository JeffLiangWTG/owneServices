using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Exceptions;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	abstract class FilterBuilder
	{
		protected const string ReadOnlyIf = FilterBuilderPropertyCodeDescriptionList.Codes.ReadOnlyIf;
		protected const string Required = FilterBuilderPropertyCodeDescriptionList.Codes.Required;
		protected const string OnlyOneOfGroup = FilterBuilderPropertyCodeDescriptionList.Codes.OnlyOneOfGroup;
		protected const string AnyOneOfGroup = FilterBuilderPropertyCodeDescriptionList.Codes.AnyOneOfGroup;
		protected const string AtLeastOneOfGroup = FilterBuilderPropertyCodeDescriptionList.Codes.AtLeastOneOfGroup;
		protected const string FilterGroup = FilterBuilderPropertyCodeDescriptionList.Codes.FilterGroup;
		protected const string DefaultValue = FilterBuilderPropertyCodeDescriptionList.Codes.DefaultValue;
		protected const string DefaultValueBackwardCompatibility = FilterBuilderPropertyCodeDescriptionList.Codes.DefaultValueBackwardCompatibility;
		protected const string DefaultFrom = FilterBuilderPropertyCodeDescriptionList.Codes.DefaultFrom;
		protected const string DefaultTo = FilterBuilderPropertyCodeDescriptionList.Codes.DefaultTo;
		protected const string OnlyCurrentPeriodIfPayByWebService = FilterBuilderPropertyCodeDescriptionList.Codes.OnlyCurrentPeriodIfPayByWebService;
		protected const string OnlyCurrentCompanyIfSetInCommissionRegistry = FilterBuilderPropertyCodeDescriptionList.Codes.OnlyCurrentCompanyIfSetInCommissionRegistry;
		protected const string DependentFilter = FilterBuilderPropertyCodeDescriptionList.Codes.DependentFilter;
		protected const string DependentFilterReadonlyIfValue = FilterBuilderPropertyCodeDescriptionList.Codes.DependentFilterReadonlyIfValue;
		protected const string Field = FilterBuilderPropertyCodeDescriptionList.Codes.Field;
		protected const string ExcludeFilterValues = FilterBuilderPropertyCodeDescriptionList.Codes.ExcludeFilterValues;
		protected const string Type = FilterBuilderPropertyCodeDescriptionList.Codes.Type;
		protected const string DisableReadOnlyIfDependency = FilterBuilderPropertyCodeDescriptionList.Codes.DisableReadOnlyIfDependency;

		protected ValidatorPack fValidators;
		protected BusinessObjectFactory fBusinessObjectFactory;
		protected ReportRunningType reportRunningType;

		public string TemplateFileName { get; set; }

		protected FilterBuilder(ValidatorPack validators, BusinessObjectFactory factory, MatchEvaluator evaluatorForDefaultValues, ReportRunningType reportRunningType)
		{
			ExpectedProperties.Add(Field);
			ExpectedProperties.Add(FilterGroup);
			ExpectedProperties.Add(Required);
			ExpectedProperties.Add(OnlyOneOfGroup);
			ExpectedProperties.Add(AnyOneOfGroup);
			ExpectedProperties.Add(AtLeastOneOfGroup);
			ExpectedProperties.Add(ReadOnlyIf);
			ExpectedProperties.Add(Type);
			ExpectedProperties.Add(OnlyCurrentPeriodIfPayByWebService);
			ExpectedProperties.Add(OnlyCurrentCompanyIfSetInCommissionRegistry);
			ExpectedProperties.Add(DependentFilter);
			ExpectedProperties.Add(DependentFilterReadonlyIfValue);
			ExpectedProperties.Add(DisableReadOnlyIfDependency);

			fValidators = validators;
			fBusinessObjectFactory = factory;
			this.reportRunningType = reportRunningType;
		}

		#region Documentation

		IReportDocumenter documentation;
		public IReportDocumenter Documentation
		{
			get
			{
				if (documentation == null)
				{
					documentation = GetDocumentation(ExpectedProperties);
					documentation.ValueProviderDocumenters = NewField().ValueProviderDocumenters;
				}
				return documentation;
			}
		}
		protected abstract IReportDocumenter GetDocumentation(List<string> supportedProperties);

		#endregion

		public virtual IFilter Build(StringTreeNode filterTree, StringCollection expectedDataSourceParameters, bool isInRuntime = true)
		{
			FilterTree = filterTree;

			foreach (var child in filterTree.Children)
			{
				if (!IsValidProperty(child.Value))
				{
					throw new TemplateDefinitionException(string.Format(CultureInfo.InvariantCulture, @"In the ""{0}"" filter, the option ""{1}"" is not allowed in this type of filter", filterTree.Value, child.Value), child.CellReference);
				}
			}

			var newFilterField = NewField();
			newFilterField.DisplayName = FilterTree.Value;

			if (FilterTree.ChildExists(Field))
			{
				var fieldNode = FilterTree.FindChild(Field).Child();
				newFilterField.FieldName = fieldNode.Value;
				newFilterField.IsFilterValueExcluded = fieldNode.ChildExists(ExcludeFilterValues);
			}

			if (FilterTree.ChildExists(FilterGroup))
			{
				foreach (var optionNode in FilterTree.FindChild(FilterGroup).Children)
				{
					newFilterField.GroupName = optionNode.Value;
					newFilterField.GroupDescription = (optionNode.Children.Count == 1 ? optionNode.Child().Value : "");
				}
			}

			if (FilterTree.ChildExists(Required))
			{
				newFilterField.Validators.Add(fValidators.RequiredFilter);
				fValidators.RequiredFilter.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(AtLeastOneOfGroup))
			{
				string groupName = FilterTree.FindChild(AtLeastOneOfGroup).Child().Value;
				var validator = fValidators.GetAtLeastOneFilterNotEmptyValidatorForGroup(groupName);
				newFilterField.Validators.Add(validator);
				validator.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(OnlyOneOfGroup))
			{
				string groupName = FilterTree.FindChild(OnlyOneOfGroup).Child().Value;
				var validator = fValidators.GetOnlyOneFilterNotEmptyValidatorForGroup(groupName);
				newFilterField.Validators.Add(validator);
				validator.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(AnyOneOfGroup))
			{
				string groupName = FilterTree.FindChild(AnyOneOfGroup).Child().Value;
				var validator = fValidators.GetAnyOneFilterNotEmptyValidatorForGroup(groupName);
				newFilterField.Validators.Add(validator);
				validator.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(OnlyCurrentPeriodIfPayByWebService))
			{
				newFilterField.Validators.Add(fValidators.OnlyCurrentPeriodIfPayByWebService);
				fValidators.OnlyCurrentPeriodIfPayByWebService.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(OnlyCurrentCompanyIfSetInCommissionRegistry))
			{
				newFilterField.Validators.Add(fValidators.OnlyCurrentCompanyIfSetInCommissionRegistry);
				fValidators.OnlyCurrentCompanyIfSetInCommissionRegistry.Filters.Add(newFilterField);
			}

			if (FilterTree.ChildExists(ReadOnlyIf))
			{
				newFilterField.ReadOnlyIfFilter = FilterTree.FindChild(ReadOnlyIf).Child().Value;
			}

			if (FilterTree.ChildExists(DependentFilter))
			{
				newFilterField.DependentFilter = FilterTree.FindChild(DependentFilter).Child().Value;
			}

			if (FilterTree.ChildExists(DependentFilterReadonlyIfValue))
			{
				newFilterField.DependentFilterReadonlyIfValue = FilterTree.FindChild(DependentFilterReadonlyIfValue).Child().Value;
			}

			if (FilterTree.ChildExists(DisableReadOnlyIfDependency))
			{
				newFilterField.DisableReadOnlyIfDependency = true;
			}

			var customBuilder = this as ICustomBuilder;
			if (customBuilder != null)
			{
				if (isInRuntime)
				{
					customBuilder.DoCustomBuilding(FilterTree, newFilterField);
				}
				else
				{
					customBuilder.DoCustomBuildingInTaskBuild(FilterTree, newFilterField);
				}
			}

			FilterTree = null;

			return newFilterField;
		}

		/// <summary>
		/// Determines whether the current FilterBuilder can build filters of the given filter type.
		/// </summary>
		/// <param name="filterType">The filter type, as read from the template</param>
		public abstract bool CanBuild(string filterType);

		protected internal StringTreeNode FilterTree;

		/// <summary>
		/// Determines whether a property is valid for this type of filter.
		/// </summary>
		/// <param name="propertyName">The name of the property, as it appears in the template</param>
		protected bool IsValidProperty(string propertyName)
		{
			return ExpectedProperties.Any((expectedProperty) => { return expectedProperty.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase); });
		}

		internal List<string> ExpectedProperties = new List<string>();

		/// <summary>
		/// Creates a new FilterField of the concrete type appropriate for this FilterBuilder.
		/// </summary>
		/// <returns>A new FilterField</returns>
		public FilterField NewField()
		{
			return GetFilterField();
		}

		protected abstract FilterField GetFilterField();

		protected BusinessObjectFactory Factory
		{
			get { return fBusinessObjectFactory; }
		}

#if DEBUG
		internal BusinessObjectFactory GetFactoryForTest()
		{
			return this.Factory;
		}
#endif
	}
}
