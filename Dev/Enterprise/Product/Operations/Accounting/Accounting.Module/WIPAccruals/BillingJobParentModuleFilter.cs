using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class BillingJobParentModuleFilter : ModuleGuidModuleSpecifiedFilter
	{
		public BillingJobParentModuleFilter(Type typeOfBusinessObjectToQuery, SchemaColumn fieldName, BusinessObjectFactory factory)
			: base(FilterDescription, JobHeaderSchema.JH_ParentID, () => BillingJobParentModules(factory))
		{
			MultilingualDescription = ResString.GetMultilingualString(BillingJobParentModuleFilterCacheKey, FilterDescription);
			Category = FilterCategories.Other;
			TypeOfBusinessObjectToQuery = typeOfBusinessObjectToQuery;
			FieldName = fieldName;
		}
		
		internal const string BillingJobParentModuleFilterCacheKey = "Accounting|Module|BillingJobParentModuleFilter";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "not to be translated")]
		const string FilterDescription = "Billing Job Parent";
		readonly Type TypeOfBusinessObjectToQuery;
		readonly SchemaColumn FieldName;

		public static IEnumerable<CodeDescriptionModuleFilterPair> BillingJobParentModules(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue(BillingJobParentModuleFilterCacheKey, () =>
			{
				var result = new List<CodeDescriptionModuleFilterPair>();

				var jobTypes = JobInvoicingConsumerTypes.NewOnlyJobInvoicingTypes();
				jobTypes.RemoveCode(JobInvoicingConsumerTypes.ForwardingConsol);

				foreach (JobInvoicingConsumerType type in jobTypes)
				{
					var controller = ZControllerFactory.Create(type.ControllerID);
					if (controller != null && controller.ModuleID != null)
					{
						result.Add(new CodeDescriptionModuleFilterPair(type.Code, type.Description, controller.ModuleID));
					}
				}
				return result;
			});
		}

		protected override ZQuery GetQuery()
		{
			var transactionLinesQuery = new ZDBOnlyQuery(TypeOfBusinessObjectToQuery);
			var value = Property;
			var comparisonOperator = SqlComparisonOperator;
			var jobHeaderQuery = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK);
			if (ComparisonOperator == ModuleTextFilter.ComparisonConstants.FiltersMatch)
			{
				jobHeaderQuery.AddToFilter(GetQueryForSelectedFilters());
			}
			else
			{
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_ParentID, comparisonOperator, value);
			}
			transactionLinesQuery.AddSubQuery(FieldName, JobHeaderSchema.PK, jobHeaderQuery, JoinCondition.And);
			return transactionLinesQuery;
		}

		public override IReadOnlyList<string> AllowedComparisonOperators => new[] { ComparisonConstants.Exact, ComparisonConstants.NotEqual, ComparisonConstants.FiltersMatch };

		protected override ModuleFilterValidation GetNewValidation()
		{
			return new BillingJobParentModuleFilterValidation(this);
		}

		class BillingJobParentModuleFilterValidation : ModuleGuidModuleSpecifiedFilterValidation
		{
			public BillingJobParentModuleFilterValidation(BillingJobParentModuleFilter parent)
				: base(parent)
			{
			}

			protected override void CheckProperty()
			{
				base.CheckProperty();
				if (Parent.ComparisonOperator != ModuleTextFilter.ComparisonConstants.FiltersMatch)
				{
					MandatoryValidation.CheckEntered(Parent.PropertyInfo);
				}
			}
		}

		#region For Test
#if DEBUG

		protected override void SetModuleId_ForTestCore(ModuleIdentifier moduleId)
		{
			// This filter uses codes from JobInvoicingConsumerTypes instead of module names, so this is required to select a module:
			var result = BillingJobParentModules(new ReadOnlyBusinessObjectFactory()).Single(x => Equals(x.Module, moduleId));
			SelectedModule = result.Code;
		}

#endif
		#endregion
	}
}
