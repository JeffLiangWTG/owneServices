using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	class CSARevenueSummaryFormFilterStripBusinessObject : StatementFilterStripBusinessObject
	{
		public CSARevenueSummaryFormFilterStripBusinessObject()
			: base()
		{
			QueryObjectType = typeof(CusStatementHeader);
		}

		public new class Schema : StatementFilterStripBusinessObject.Schema
		{
			public const string RSFStatementNumber = "RSF Statement Number";
			public const string RSFImporter = "Importer Organization Code";
			public const string RSFPeriod = "RSF Period";
			public const string RSFStatus = "RSF Status";

			public static MultilingualString RSFStatementNumberMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|RSFStatementNumber", RSFStatementNumber);
			public static MultilingualString RSFImporterMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|RSFImporter", RSFImporter);
			public static MultilingualString RSFPeriodMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|RSFPeriod", RSFPeriod);
			public static MultilingualString RSFStatusMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|RSFStatus", RSFStatus);
		}

		protected override ZString ImporterFilterName => Schema.RSFImporter;
		protected override MultilingualString ImporterFilterNameMultilingualDescription => Schema.RSFImporterMultilingualDescription;

		protected override ZString HeaderBusinessNubmerFilterName => Schema.ImporterBusinessNumber;
		protected override MultilingualString HeaderBusinessNubmerFilterNameMultilingualDescription => Schema.ImporterBusinessNumberMultilingualDescription;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var statementNumberFilter = result.AddNumberFilter(Schema.RSFStatementNumber, CusStatementHeaderSchema.B2_StatementNumber);
			statementNumberFilter.Category = FilterCategories.NumbersAndReferences;
			statementNumberFilter.MaxLength = CusStatementHeaderSchema.B2_StatementNumber.MaxLength;
			statementNumberFilter.MultilingualDescription = Schema.RSFStatementNumberMultilingualDescription;

			var importerFilter = result.AddGuidFilter(ImporterFilterName, ModuleIDs.Organisation, CusStatementHeaderSchema.B2_OH_Importer, ImporterCollection);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ImporterFilterNameMultilingualDescription;

			var periodFilter = new PeriodFilter(Schema.RSFPeriod, GetPeriodQuery);
			periodFilter.Category = FilterCategories.Dates;
			periodFilter.MultilingualDescription = Schema.RSFPeriodMultilingualDescription;
			result.AddCustomFilter(periodFilter);

			var statusFilter = result.AddTextFilter(Schema.RSFStatus, CusStatementHeaderSchema.B2_Status, new MessageStatusList());
			statusFilter.Category = FilterCategories.StatusAndFlags;
			statusFilter.MultilingualDescription = Schema.RSFStatusMultilingualDescription;

			AddCommonFilters(result);

			return result;
		}

		static ZQuery GetPeriodQuery(SQLComparisonOperator comparisonOperator, SchemaDateTimeColumn filterColumn, ZInt year, ZInt month)
		{
			ZQuery result = new ZQuery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.NotEqual, null);
			}
			else if (year != ZInt.Zero && month != ZInt.Zero)
			{
				ZDateTime startDate = new ZDateTime(year, month, 1);
				ZDateTime endDate = new ZDateTime(year, month, DateTime.DaysInMonth(year, month));
				if (endDate.IsValid)
				{
					if (comparisonOperator == SQLComparisonOperator.Equal)
					{
						result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.LessThanOrEqualTo, endDate);
						result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
					}
					else if (comparisonOperator == SQLComparisonOperator.NotEqual)
					{
						result.AddToFilter(CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.GreaterThan, endDate);
						result.AddToFilter(JoinCondition.Or, CusStatementHeaderSchema.B2_PeriodEndDate, SQLComparisonOperator.LessThan, startDate);
						result.AddToFilter(JoinCondition.Or, CusStatementHeaderSchema.B2_PeriodEndDate, null);
					}
				}
			}

			return result;
		}

		protected override List<IFilterStripsHelper> GetCustomFilterStripsHelpersCore()
		{
			var helpers = base.GetCustomFilterStripsHelpersCore();
			var workflowHelper = new WorkflowFilterStripsHelper(typeof(CusStatementHeader), WorkflowDescriptors.CusStatementHeaderWorkflowDescriptorCode, Factory);
			workflowHelper.SetShouldAddWorkflowCustomFieldsFilters(true);
			helpers.Add(workflowHelper);
			return helpers;
		}
	}
}
