using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageRegisterLinesFilterBusinessObject : EU.TemporaryStorage.Module.TempStorageRegisterLinesFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class FilterConstants
		{
			public const string LineOwnerReferenceNumber = "Owner Reference Number";
			public const string LineLimitDate = "Limit Date";
			public const string LineOwnerEORI = "Owner EORI";
			public const string LinePackageMarks = "Package Marks/Vehicles";
			public const string LineLocationOfGoods = "Location of goods";
			public const string LineCustomsStatus = "Line Status";
			public const string LineItemTSDItemNumber = "TSD Item Number";
			public const string LineItemTariffCode = "Tariff Code";
			public const string LineItemDescription = "Description";
			public const string LineTransactionEntryDate = "Entry Date";
			public const string LineTransactionInternalReference = "Internal Reference Number";
			public const string LineTransactionInternalReferenceType = "Internal Reference Type";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			AddLineFilters(result);
			AddLineTransactionsFilters(result);
			AddLineItemFilters(result);
			AddPremisesFilter(result);
			return result;
		}

		void AddLineFilters(ModuleFilterCollection filters)
		{
			var lineOwnerReferenceNumberFilter = filters.AddTextFilter(FilterConstants.LineOwnerReferenceNumber, CusTempStorageRegLineSchema.SRL_OwnerReference).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_OwnerReference);
			lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("DC72DB29-7F27-492E-97C4-7EECC9856B99", FilterConstants.LineOwnerReferenceNumber);

			var lineLimitDateFilter = filters.AddDateFilter(FilterConstants.LineLimitDate, CusTempStorageRegLineSchema.SRL_LimitDate);
			lineLimitDateFilter.Category = FilterCategories.Dates;
			lineLimitDateFilter.MultilingualDescription = ResString.GetMultilingualString("FE135A3A-2696-4B28-83D6-DBF9DD6ECF6F", FilterConstants.LineLimitDate);

			var ownerEori = filters.AddTextFilter(FilterConstants.LineOwnerEORI, CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier);
			ownerEori.Category = FilterCategories.NumbersAndReferences;
			ownerEori.MultilingualDescription = ResString.GetMultilingualString("4F845001-BACB-4A8E-B0BB-5224DEF4464C", FilterConstants.LineOwnerEORI);

			var linePackageMarks = filters.AddTextFilter(FilterConstants.LinePackageMarks, CusTempStorageRegLineSchema.SRL_PackageMarks).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_PackageMarks);
			linePackageMarks.Category = FilterCategories.NumbersAndReferences;
			linePackageMarks.MultilingualDescription = ResString.GetMultilingualString("D6E489C6-618B-454F-B963-9CA733B67CA2", FilterConstants.LinePackageMarks);

			var lineLocationOfGoods = filters.AddTextFilter(FilterConstants.LineLocationOfGoods, CusTempStorageRegLineSchema.SRL_LocationOfGoods).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_LocationOfGoods);
			lineLocationOfGoods.Category = FilterCategories.NumbersAndReferences;
			lineLocationOfGoods.MultilingualDescription = ResString.GetMultilingualString("E4788AAE-12B5-4113-8B08-DFB65DA201F3", FilterConstants.LineLocationOfGoods);

			var lineCustomsStatus = filters.AddTextFilter(FilterConstants.LineCustomsStatus, CusTempStorageRegLineSchema.SRL_CustomsStatus).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustomsStatus);
			lineCustomsStatus.Category = FilterCategories.StatusAndFlags;
			lineCustomsStatus.MultilingualDescription = ResString.GetMultilingualString("88EF9273-F5DB-4886-A2D3-D161A06DE7AB", FilterConstants.LineCustomsStatus);
		}

		void AddLineTransactionsFilters(ModuleFilterCollection filters)
		{
			var regLineTransactionFilterSubGroup = new RegLineTransactionFilterSubGroup();

			var lineTransactionEntryDateFilter = filters.AddDateFilter(
			FilterConstants.LineTransactionEntryDate,
			(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.LineTransactionEntryDate, CusTempStorageRegLineTransactionSchema.SRT_PhysicalInOutDate, comparisonOperator, value1, value2)
			);
			lineTransactionEntryDateFilter.Category = FilterCategories.Dates;
			lineTransactionEntryDateFilter.MultilingualDescription = ResString.GetMultilingualString("E76885BF-0AE7-49A7-9230-FC51E51B02E1", FilterConstants.LineTransactionEntryDate);
			lineTransactionEntryDateFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionInternalReferenceTypeFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionInternalReferenceType,
			(comparisonOperator, value) => new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType, comparisonOperator, value)).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType);
			lineTransactionInternalReferenceTypeFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionInternalReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("6945009A-9CEB-4FEA-88A9-A7D6587F653E", FilterConstants.LineTransactionInternalReferenceType);
			lineTransactionInternalReferenceTypeFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionInternalReferenceFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionInternalReference,
			(comparisonOperator, value) => new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber, comparisonOperator, value)).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber);
			lineTransactionInternalReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionInternalReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("0DE38B95-147C-425A-B878-55DEAA7C1D9B", FilterConstants.LineTransactionInternalReference);
			lineTransactionInternalReferenceFilter.SubGroup = regLineTransactionFilterSubGroup;
		}

		void AddLineItemFilters(ModuleFilterCollection filters)
		{
			var regLineItemFilterSubGroup = new RegLineItemFilterSubGroup();

			var lineItemTSDItemNumber = filters.AddNumberFilter(
				FilterConstants.LineItemTSDItemNumber,
				(comparisonOperator, value) => new ZQuery(CusTempStorageRegLineItemSchema.SRI_GoodsItemNumber, comparisonOperator, ZInt.ParseSafe(value, ZInt.Zero)));
			lineItemTSDItemNumber.Category = FilterCategories.NumbersAndReferences;
			lineItemTSDItemNumber.MultilingualDescription = ResString.GetMultilingualString("C66D50A8-E180-4FA3-A0BF-6C051E1B380A", FilterConstants.LineItemTSDItemNumber);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			lineItemTSDItemNumber.SubGroup = regLineItemFilterSubGroup;

			var lineItemTariffCode = filters.AddTextFilter(
				FilterConstants.LineItemTariffCode,
				(comparisonOperator, value) => new ZQuery(CusTempStorageRegLineItemSchema.SRI_Tariff, comparisonOperator, value)).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineItemSchema.SRI_Tariff);
			lineItemTariffCode.Category = FilterCategories.NumbersAndReferences;
			lineItemTariffCode.MultilingualDescription = ResString.GetMultilingualString("3BD62C7B-5A51-4DB8-B4E6-589941C14D05", FilterConstants.LineItemTariffCode);
			lineItemTariffCode.SubGroup = regLineItemFilterSubGroup;

			var lineItemDescription = filters.AddTextFilter(
				FilterConstants.LineItemDescription,
				(comparisonOperator, value) => new ZQuery(CusTempStorageRegLineItemSchema.SRI_GoodsDescription, comparisonOperator, value)).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineItemSchema.SRI_GoodsDescription);
			lineItemDescription.Category = FilterCategories.NumbersAndReferences;
			lineItemDescription.MultilingualDescription = ResString.GetMultilingualString("A3C2A145-CE68-40B9-92E4-2B37AAC98848", FilterConstants.LineItemDescription);
			lineItemDescription.SubGroup = regLineItemFilterSubGroup;
		}

		ZQuery GetRegLineDateOffsetQuery(ZString filteredField, SchemaDateTimeOffsetColumn column, DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var query = new ZQuery();
			AddDateTimeOffsetRange(query, comparisonOperator, JoinCondition.And, column, value1, value2, false, false);

			var lineQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
			lineQuery.AddToFilter(new ZQuery(CusTempStorageRegLineTransactionSchema.SRT_TransactionType, SQLComparisonOperator.Contains, CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance), JoinCondition.Or);
			query.AddToFilter(lineQuery, JoinCondition.And);

			return query;
		}

		class RegLineTransactionFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
				var lineTransactionQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction), CusTempStorageRegLineTransactionSchema.SRT_SRL);
				lineTransactionQuery.AddToFilter(filter);
				lineQuery.AddSubQuery(lineTransactionQuery, JoinCondition.And);
				return lineQuery;
			}
		}

		class RegLineItemFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
				var lineItemQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItem), CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);

				var linePivotItemQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);
				var linePivotLineQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line);
				var linePivotPkQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.PK);

				lineItemQuery.AddToFilter(filter);

				linePivotPkQuery.AddSubQuery(lineItemQuery, JoinCondition.And);
				linePivotItemQuery.AddSubQuery(linePivotPkQuery, JoinCondition.And);
				linePivotLineQuery.AddSubQuery(linePivotPkQuery, JoinCondition.And);

				lineQuery.AddSubQuery(linePivotLineQuery, JoinCondition.And);
				return lineQuery;
			}
		}

		protected override EU.TemporaryStorage.Business.CusTempStorageRegHeader GetCusTempStorageRegHeaderForLookups() => Factory.GetNull<ES.Business.CusTempStorage.CusTempStorageRegHeader>();
	}
}
