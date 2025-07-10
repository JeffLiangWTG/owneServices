using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Module
{
	abstract class StatementFilterStripBusinessObject : FilterStripBusinessObject
	{
		public class Schema
		{
			public const string PaymentDueDate = "Payment Due Date";
			public const string StatementDate = "Statement Date";
			public const string AccountingDate = "Accounting Date";
			public const string StatementType = "Statement Type";
			public const string ImporterBusinessNumber = "Importer Business Number";

			public static MultilingualString PaymentDueDateMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|PaymentDueDate", PaymentDueDate);
			public static MultilingualString StatementDateMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|StatementDate", StatementDate);
			public static MultilingualString AccountingDateMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|AccountingDate", AccountingDate);
			public static MultilingualString StatementTypeMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|StatementType", StatementType);
			public static MultilingualString ImporterBusinessNumberMultilingualDescription => ResString.GetMultilingualString("CA|StatementFilterStripBusinessObject|ImporterBusinessNumber", ImporterBusinessNumber);
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var currentCompanyFilter = result.AddGuidFilter("Company", ModuleIDs.GlbCompany, GetCompanyAndMessageQuery, new GlbCompanyCollection(Factory));
			currentCompanyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			var lineLevelImporterBusinessNumberFilter = result.AddNumberFilter(Schema.ImporterBusinessNumber, GetLineLevelImportBusinessNumberQuery);
			lineLevelImporterBusinessNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineLevelImporterBusinessNumberFilter.MaxLength = CusStatementLineGroupSchema.B10_ImporterCustomsID.MaxLength;
			lineLevelImporterBusinessNumberFilter.MultilingualDescription = Schema.ImporterBusinessNumberMultilingualDescription;

			var statementTypeFilter = result.AddTextFilter(Schema.StatementType, CusStatementHeaderSchema.B2_StatementType, new CusStatementHeaderTypes());
			statementTypeFilter.Category = FilterCategories.ModesAndTypes;
			statementTypeFilter.MultilingualDescription = Schema.StatementTypeMultilingualDescription;

			var importerFilter = result.AddGuidFilter(ImporterFilterName, ModuleIDs.Organisation, GetGroupImporter, ImporterCollection);
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ImporterFilterNameMultilingualDescription;

			AddCommonFilters(result);

			return result;
		}

		protected void AddCommonFilters(ModuleFilterCollection result)
		{
			var lineLevelImporterBusinessNumberFilter = result.AddTextFilter(HeaderBusinessNubmerFilterName, CusStatementHeaderSchema.B2_ImporterCustomsID);
			lineLevelImporterBusinessNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineLevelImporterBusinessNumberFilter.MaxLength = CusStatementHeaderSchema.B2_ImporterCustomsID.MaxLength;
			lineLevelImporterBusinessNumberFilter.MultilingualDescription = HeaderBusinessNubmerFilterNameMultilingualDescription;

			var statementDateFilter = result.AddDateFilter(Schema.StatementDate, CusStatementHeaderSchema.B2_PrintDate);
			statementDateFilter.Category = FilterCategories.Dates;
			statementDateFilter.MultilingualDescription = Schema.StatementDateMultilingualDescription;
		}

		ZQuery GetCompanyAndMessageQuery(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));
			result.AddToFilter(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
			result.AddToFilter(CusStatementHeaderSchema.B2_StatementNumber, SQLComparisonOperator.NotEqual, ZString.Empty);

			return result;
		}

		protected ZQuery GetLineLevelImportBusinessNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));
			var subQuery = GetCusStatementLineGroupQuery(CusStatementLineGroupSchema.B10_ImporterCustomsID, comparisonOperator, value);
			result.AddSubQuery(subQuery, JoinCondition.And);

			return result;
		}

		protected ZQuery GetGroupImporter(ZGuid value)
		{
			var result = new ZDBOnlyQuery(typeof(CusStatementHeader));
			result.AddToFilter(CusStatementHeaderSchema.B2_OH_Importer, value);

			var subQuery = GetCusStatementLineGroupQuery(CusStatementLineGroupSchema.B10_OH_Importer, SQLComparisonOperator.Equal, value);
			result.AddSubQuery(subQuery, JoinCondition.Or);

			return result;
		}

		ZDBOnlySubQuery GetCusStatementLineGroupQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, IZType value)
		{
			var result = new ZDBOnlySubQuery(typeof(CusStatementLineGroup), CusStatementLineGroupSchema.B10_B2);
			result.AddToFilter_PossiblyCommaSeparated(JoinCondition.And, column, comparisonOperator, value);

			return result;
		}

		protected abstract ZString ImporterFilterName { get; }
		protected abstract MultilingualString ImporterFilterNameMultilingualDescription { get; }

		protected abstract ZString HeaderBusinessNubmerFilterName { get; }
		protected abstract MultilingualString HeaderBusinessNubmerFilterNameMultilingualDescription { get; }

		protected OrgHeaderCollection ImporterCollection => new OrgHeaderCollection(Factory);
	}
}
