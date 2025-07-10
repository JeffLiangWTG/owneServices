using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Customs.Module;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Module
{
	public class CusStatementFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string StatementNumber = "Statement Number";
			public const string StatementType = "Statement Type";
			public const string CustomsOffice = "Customs Office";
			public const string ImporterCustomsID = "Payer Business No.";
			public const string PaymentParty = "Payment Party";
			public const string DueDate = "Due Date";
			public const string ProcessDate = "Process Date";
			public const string StatementAmount = "Total Amount";
			public const string Payer = "Payer";
			public const string StatementPeriod = "Period From (Month/Year)";
			public const string Company = "Company";
			public const string PaymentDate = "Payment Date";
			public const string PayerCompanyName = "Payer Company Name";
			public const string PaymentStatus = "Payment Status";
			public const string EntryNumber = "Entry Number";
			public const string BillType = "Bill Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var importCustomsIDFilter = result.AddNumberFilter(Schema.ImporterCustomsID, CusStatementHeaderSchema.B2_ImporterCustomsID);
			importCustomsIDFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|ImporterCustomsID", Schema.ImporterCustomsID);

			var statementAmountFilter = result.AddNumberRangeFilter(Schema.StatementAmount, CusStatementHeaderSchema.B2_StatementAmount);
			statementAmountFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|StatementAmount", Schema.StatementAmount);
			statementAmountFilter.Decimals = 0;

			#region Statement Number
			var statementNumberFilter = result.AddNumberFilter(Schema.StatementNumber, GetStatementNumberQuery);
			statementNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|StatementNumber", Schema.StatementNumber);
			statementNumberFilter.PropertyValidation = StatementNumberFilterValidation;

			statementNumberFilter.ComparisonOperator_List.Clear();
			statementNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			statementNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Default));
			statementNumberFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			statementNumberFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;
			#endregion

			#region Payer Company Name
			var importerNameFilter = result.AddTextFilter(Schema.PayerCompanyName, GetPayerCompanyNameMappingQuery);
			importerNameFilter.Category = FilterCategories.TextSearch;
			importerNameFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|PayerName", Schema.PayerCompanyName);
			importerNameFilter.MaxLength = 100;

			importerNameFilter.ComparisonOperator_List.Clear();
			importerNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			importerNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			importerNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			importerNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			importerNameFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			importerNameFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Contains;
			#endregion

			var dueDateFilter = result.AddDateFilter(Schema.DueDate, CusStatementHeaderSchema.B2_DueDate);
			dueDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|DueDate", Schema.DueDate);

			var processDateFilter = result.AddDateFilter(Schema.ProcessDate, CusStatementHeaderSchema.B2_ProcessDate);
			processDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|ProcessDate", Schema.ProcessDate);

			var paymentDateFilter = result.AddDateFilter(Schema.PaymentDate, CusStatementHeaderSchema.B2_PaymentAuthorizationDate);
			paymentDateFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|PaymentDate", Schema.PaymentDate);

			var statementPeriodFilter = new PeriodFilter(Schema.StatementPeriod, GetPeriodQuery);
			statementPeriodFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|StatementPeriod", Schema.StatementPeriod);
			statementPeriodFilter.Category = FilterCategories.Dates;
			result.AddCustomFilter(statementPeriodFilter);

			#region Customs Office
			var customsOfficeFilter = result.AddNkFilter(Schema.CustomsOffice, CusStatementHeaderSchema.B2_ProcessPort, ModuleIDs.Customs.Universal.ZZRefCusCodeList, CustomsOffices);
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|CustomsOffice", Schema.CustomsOffice);
			customsOfficeFilter.Category = FilterCategories.Locations;

			customsOfficeFilter.ComparisonOperator_List.Clear();
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			customsOfficeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.FiltersMatch));
			customsOfficeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			#endregion

			#region Payment Party
			var paymentPartyFilter = result.AddTextFilter(Schema.PaymentParty, CusStatementHeaderSchema.B2_PaymentParty, Factory.GetCachedValue<PaymentPartyList>());
			paymentPartyFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|PaymentParty", Schema.PaymentParty);
			paymentPartyFilter.Category = FilterCategories.ModesAndTypes;

			paymentPartyFilter.ComparisonOperator_List.Clear();
			paymentPartyFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			paymentPartyFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			paymentPartyFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank));
			paymentPartyFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsNotBlank));
			paymentPartyFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			#endregion

			#region Statement Type
			var statementypeFilter = result.AddTextFilter(Schema.StatementType, CusStatementHeaderSchema.B2_StatementType, Factory.GetCachedValue<StatementHeaderTypeList>());
			statementypeFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|StatementType", Schema.StatementType);
			statementypeFilter.Category = FilterCategories.ModesAndTypes;

			statementypeFilter.ComparisonOperator_List.Clear();
			statementypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			statementypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			statementypeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			#endregion

			var importerFilter = result.AddGuidFilter(Schema.Payer, ModuleIDs.Organisation, CusStatementHeaderSchema.B2_OH_Importer, new ConsigneeCollection(Factory));
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|Payer", Schema.Payer);
			importerFilter.Category = FilterCategories.Organisations;

			var currentCompanyFilter = result.AddGuidFilter(Schema.Company, ModuleIDs.GlbCompany, GetCompanyQuery, new GlbCompanyCollection(Factory));
			currentCompanyFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;

			#region Payment Status
			var paymentstatusFilter = result.AddTextFilter(Schema.PaymentStatus, CusStatementHeaderSchema.B2_PaymentStatus, Factory.GetCachedValue<StatementHeaderPaymentStatusList>());
			paymentstatusFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|PaymentStatus", Schema.PaymentStatus);
			paymentstatusFilter.Category = FilterCategories.StatusAndFlags;

			paymentstatusFilter.ComparisonOperator_List.Clear();
			paymentstatusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			paymentstatusFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			paymentstatusFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			#endregion

			#region Entry Number
			var entrynumFilter = result.AddNumberFilter(Schema.EntryNumber, GetEntryNumberQuery);
			entrynumFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|EntryNumber", Schema.EntryNumber);
			entrynumFilter.Category = FilterCategories.NumbersAndReferences;

			entrynumFilter.ComparisonOperator_List.Clear();
			entrynumFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			entrynumFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith));
			entrynumFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Contains));
			entrynumFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.StartsWith;
			#endregion

			#region Bill Type
			var billtypeFilter = result.AddTextFilter(Schema.BillType, CusStatementHeaderSchema.B2_Status, Factory.GetCachedValue<StatementHeaderStatusList>());
			billtypeFilter.MultilingualDescription = ResString.GetMultilingualString("CusStatementFilterStripBusinessObject|BillType", Schema.BillType);
			billtypeFilter.Category = FilterCategories.ModesAndTypes;

			billtypeFilter.ComparisonOperator_List.Clear();
			billtypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact));
			billtypeFilter.ComparisonOperator_List.Add(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.GetComparisonOperatorPair(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual));
			billtypeFilter.ComparisonOperator_List.DefaultCode = ModuleTextFilter.ComparisonConstants.Exact;
			#endregion
			return result;
		}

		ZQuery GetPayerCompanyNameMappingQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var cusStatementQuery = new ZDBOnlyQuery(typeof(CusStatementHeader));
			if (comparisonOperator == SQLComparisonOperator.IsBlank)
			{
				cusStatementQuery.AddToFilter(CusStatementHeaderSchema.B2_OH_Importer, null);
			}
			else
			{
				var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
				orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
				cusStatementQuery.AddSubQuery(CusStatementHeaderSchema.B2_OH_Importer, OrgHeaderSchema.PK, orgHeaderQuery, JoinCondition.And);
			}
			return cusStatementQuery;
		}

		ZZRefCusCodeListCombinedCollection CustomsOffices => new ZZRefCusCodeListCombinedCollection(Factory, Core.Constants.CountryCodes.KoreaSouth, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Now);

		static ZQuery GetPeriodQuery(SQLComparisonOperator comparisonOperator, SchemaDateTimeColumn filterColumn, ZInt year, ZInt month)
		{
			var query = new ZQuery();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				query.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.NotEqual, null);
			}
			else if (year != ZInt.Zero && month != ZInt.Zero)
			{
				var startDate = new ZDateTime(year, month, 1);
				var endDate = new ZDateTime(year, month, DateTime.DaysInMonth(year, month));
				if (startDate.IsValid)
				{
					if (comparisonOperator == SQLComparisonOperator.Equal)
					{
						query.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.GreaterThanOrEqualTo, startDate);
						query.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.LessThanOrEqualTo, endDate);
					}
					else if (comparisonOperator == SQLComparisonOperator.NotEqual)
					{
						query.AddToFilter(CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.LessThan, startDate);
						query.AddToFilter(JoinCondition.Or, CusStatementHeaderSchema.B2_PeriodStartDate, SQLComparisonOperator.GreaterThan, endDate);
						query.AddToFilter(JoinCondition.Or, CusStatementHeaderSchema.B2_PeriodStartDate, null);
					}
				}
			}

			return query;
		}

		ZQuery GetCompanyQuery(ZGuid value)
		{
			return new ZQuery(CusStatementHeaderSchema.B2_GC, GlbCompany.CurrentCompany.PK);
		}

		ZQuery GetEntryNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var statementHeaderQuery = new ZDBOnlyQuery(typeof(CusStatementHeader));

			var statementLineQuery = new ZDBOnlySubQuery(typeof(CusStatementLine), CusStatementLineSchema.PK);
			statementLineQuery.AddToFilter(CusStatementLineSchema.B3_EntryNum, comparisonOperator, value.KeepAlphanumericCharacters());
			statementHeaderQuery.AddSubQuery(CusStatementHeaderSchema.PK, CusStatementLineSchema.B3_B2, statementLineQuery, JoinCondition.And);

			return statementHeaderQuery;
		}

		void StatementNumberFilterValidation(ZPropertyInfo info)
		{
			var errorWarning = Res.GetString("CF9F3E13-1B48-45E2-9E86-F961D3C49B7F", "The search will be done without formatting characters like '-'.");
			var value = (ZString)info.Value;
			if (!value.IsLettersAndNumbersOnlyOrEmpty)
			{
				info.AddWarning(errorWarning);
			}
		}

		ZQuery GetStatementNumberQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(CusStatementHeaderSchema.B2_StatementNumber, comparisonOperator, value.KeepAlphanumericCharacters());
		}
	}
}
