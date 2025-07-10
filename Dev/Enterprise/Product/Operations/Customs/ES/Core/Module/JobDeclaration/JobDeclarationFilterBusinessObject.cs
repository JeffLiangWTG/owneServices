using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Module
{
	public class JobDeclarationFilterBusinessObject : EU.Module.JobDeclarationFilterBusinessObject
	{
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			AddOriginStateIslandFilters(filters);
			AddParallelFilter(filters);
			AddVehicleBrandFilters(filters);
			AddVehicleModelFilters(filters);

			return filters;
		}

		protected override bool SupportsExitControlCore => true;

		protected override bool SupportsMultipleVehiclesCore => true;

		void AddOriginStateIslandFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter
				(
					DeclarationFilterConstants.OriginStateIsland,
					(comparisonOperator, value) =>
					{
						var invoiceLineSubQuery = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey);
						invoiceLineSubQuery.AddToFilter(JobComInvoiceLineSchema.JI_StateOrRegionOfOrigin, comparisonOperator, value);

						var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
						jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.JE_ClusterKey, invoiceLineSubQuery, JoinCondition.And);
						return jobDeclarationQuery;
					},
					() => Lookups.StateIslandCodesList
				)
				.WithMaxLengthOf<ModuleTextFilter>(JobComInvoiceLineSchema.JI_StateOrRegionOfOrigin);

			filter.Category = FilterCategories.Locations;
			filter.MultilingualDescription = ResString.GetMultilingualString("83EFB6C9-44F2-4561-823D-EE47CCA29587", DeclarationFilterConstants.OriginStateIsland);
		}

			#region Parallel

			void AddParallelFilter(ModuleFilterCollection filters)
		{
			var addInfoFilter = new AddInfoModuleBooleanFilter(DeclarationFilterConstants.Parallel, GetParallelQuery, Lookups.ParallelList);
			addInfoFilter.Category = FilterCategories.StatusAndFlags;
			addInfoFilter.MultilingualDescription = ResString.GetMultilingualString("ECCEC6A1-6D6A-4C56-93E9-0488C5644B27", DeclarationFilterConstants.Parallel);

			filters.AddFilter(addInfoFilter);
		}

		ZQuery GetParallelQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entryHeaderQuery = new ZDBOnlySubQuery(typeof(Business.Declaration.CusEntryHeader), CusEntryHeaderSchema.CH_ClusterKey, JobDeclarationSchema.JE_ClusterKey);

			comparisonOperator = SQLComparisonOperator.Contains;
			var parallelColumn = EUAddInfoSchema.ZG_Parallel.Name.Substring(3);
			var queryValue = parallelColumn + "=" + value;

			if (value == YesNoList.Codes.No)
			{
				comparisonOperator = SQLComparisonOperator.NotContains;
				queryValue = parallelColumn + "=";
			}

			entryHeaderQuery.AddToFilter(JoinCondition.Or, CusEntryHeaderSchema.CH_AddInfo, comparisonOperator, queryValue);
			result.AddSubQuery(entryHeaderQuery, JoinCondition.And);
			return result;
		}

		#endregion

		protected override ZQuery GetIndirectExportQuery(ZBool value)
		{
			var exportCustomsOfficeType = new ZString[] { EuOfficeCodesTypes.Codes.OfficeOfExit, EuOfficeCodesTypes.Codes.OfficeOfExport };
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));

			if (value)
			{
				var cusEntryHeaderSubQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryHeader), CusEntryHeaderSchema.CH_JE);

				var entryIndirectExportSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
				entryIndirectExportSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, ZBool.True.ToString());
				entryIndirectExportSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, Business.Declaration.CusEntryHeader.GenAddOnColumnConstants.IndirectExportColumnName);
				entryIndirectExportSubQuery.AddToFilter(GenAddOnColumnSchema.XA_ParentTableCode, "CH");
				cusEntryHeaderSubQuery.AddSubQuery(entryIndirectExportSubQuery, JoinCondition.Or);

				var entryPrintProcedureSubQuery = new ZDBOnlySubQuery(typeof(EU.Business.Declaration.CusEUEntryHeader), CusEUEntryHeaderSchema.EUH_CH);
				entryPrintProcedureSubQuery.AddToFilter(JoinCondition.And, CusEUEntryHeaderSchema.EUH_EADPrintProcedure, SQLComparisonOperator.NotEqual, "0");
				entryPrintProcedureSubQuery.AddToFilter(JoinCondition.And, CusEUEntryHeaderSchema.EUH_EADPrintProcedure, SQLComparisonOperator.IsNotBlank, ZString.Empty);
				cusEntryHeaderSubQuery.AddSubQuery(entryPrintProcedureSubQuery, JoinCondition.Or);

				var decCustomsOfficeSubQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
				decCustomsOfficeSubQuery.AddFilterAndZSQLParameterCollection(GetExitOfficeVsGoodsLocationSqlFilterExpression(JobDeclarationSchema.Constants.JE_CustomsOffice), new ZSqlParameterCollection());
				var noCusCodeDataExitOfficeSubQueryInDec = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID, true);
				noCusCodeDataExitOfficeSubQueryInDec.AddToFilter(CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, exportCustomsOfficeType);
				decCustomsOfficeSubQuery.AddSubQuery(noCusCodeDataExitOfficeSubQueryInDec, JoinCondition.And);

				var cusCodeDataExitOfficeSubQuery = new ZDBOnlySubQuery(typeof(CusCodeData), CusCodeDataSchema.CY_ParentID);
				cusCodeDataExitOfficeSubQuery.AddToFilter(CusCodeDataSchema.CY_Code, SQLComparisonOperator.Equal, exportCustomsOfficeType);
				cusCodeDataExitOfficeSubQuery.AddFilterAndZSQLParameterCollection(GetExitOfficeVsGoodsLocationSqlFilterExpression(CusCodeDataSchema.Constants.CY_Data), new ZSqlParameterCollection());

				var groupFilterQueries = new ZDBOnlyQuery(typeof(JobDeclaration));
				groupFilterQueries.AddSubQuery(cusEntryHeaderSubQuery, JoinCondition.Or);
				groupFilterQueries.AddToFilter(decCustomsOfficeSubQuery, JoinCondition.Or);
				groupFilterQueries.AddSubQuery(cusCodeDataExitOfficeSubQuery, JoinCondition.Or);

				query.AddToFilter(JobDeclarationSchema.JE_MessageType, Common.Shared.SharedJobMessageTypeList.Codes.Export);
				query.AddToFilter(groupFilterQueries, JoinCondition.And);
			}

			return query;
		}

		void AddVehicleModelFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(EU.Module.DeclarationFilterConstants.NumberFilterTypes.VehicleModel, CusVehicleSchema.CVH_ModelName)
				.WithMaxLengthOf<ModuleTextFilter>(CusVehicleSchema.CVH_ModelName);
			filter.MultilingualDescription = ResString.GetMultilingualString("E33E50B7-6177-4842-8C66-93F1074E5F3D", EU.Module.DeclarationFilterConstants.NumberFilterTypes.VehicleModel);
			filter.SubGroup = CusVehicleSubGroup;
		}

		void AddVehicleBrandFilters(ModuleFilterCollection filters)
		{
			var filter = filters.AddTextFilter(EU.Module.DeclarationFilterConstants.NumberFilterTypes.VehicleBrand, CusVehicleSchema.CVH_BrandName)
				.WithMaxLengthOf<ModuleTextFilter>(CusVehicleSchema.CVH_BrandName);
			filter.MultilingualDescription = ResString.GetMultilingualString("D0091A08-E906-45EC-A42F-2B47E263F60F", EU.Module.DeclarationFilterConstants.NumberFilterTypes.VehicleBrand);
			filter.SubGroup = CusVehicleSubGroup;
		}

		ZString GetExitOfficeVsGoodsLocationSqlFilterExpression(ZString exitOfficeFieldToCompare) => string.Format(CultureInfo.CurrentCulture, (NoResString)"{0} <> '' AND {1} <> '' AND {0} <> LEFT({1}, 8) AND {0} <> CONCAT('ES00', LEFT({1}, 4))", exitOfficeFieldToCompare, JobDeclarationSchema.Constants.JE_LocationOfGoods);

		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);
	}
}
