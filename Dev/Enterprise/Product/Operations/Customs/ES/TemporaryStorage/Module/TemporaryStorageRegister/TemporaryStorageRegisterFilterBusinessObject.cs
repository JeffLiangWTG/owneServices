using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using CusTempStorageRegHeader = Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageRegHeader;

namespace Enterprise.Customs.ES.TemporaryStorage.Module
{
	public class TemporaryStorageRegisterFilterBusinessObject : EU.TemporaryStorage.Module.TempStorageRegisterFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class FilterConstants
		{
			public const string CustomerReference = "Customer Reference";

			public const string LineOwnerReferenceNumber = "Owner Reference Number";
			public const string LineLimitDate = "Limit Date";
			public const string LineCustodianEori = "Custodian EORI";
			public const string LineOwnerEORI = "Owner EORI";

			public const string LinePackageMarks = "Package Marks/Vehicles";
			public const string LineLocationOfGoods = "Location of goods";
			public const string LineCustomsStatus = "Line Status";

			public const string LineItemTSDItemNumber = "TSD Item Number";
			public const string LineItemTariffCode = "Tariff Code";
			public const string LineItemDescription = "Description";

			public const string LineTransactionExitDate = "Exit Date";
			public const string LineTransactionEntryDate = "Entry Date";
			public const string LineTransactionDeclarationDate = "Declaration Date";
			public const string LineTransactionReferenceType = "Outbound Declaration Type";
			public const string LineTransactionReference = "Outbound Declaration Number";
			public const string LineTransactionInternalReference = "Internal Reference Number";
			public const string LineTransactionInternalReferenceType = "Internal Reference Type";
		}

		public TemporaryStorageRegisterFilterBusinessObject()
		{
			ResetLineOnlyQuery();
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = base.GetModuleFiltersCore();
			var customerReferenceTextFilter = result.AddTextFilter(FilterConstants.CustomerReference, CusTempStorageRegHeaderSchema.SRH_InternalReference);
			customerReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
			customerReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("164F1D6F-A384-4692-A1F9-008341F87CB7", FilterConstants.CustomerReference);

			AddLineFilters(result);
			AddLineTransactionsFilters(result);
			AddLineItemFilters(result);
			AddPremisesFilter(result);
			return result;
		}

		void AddLineTransactionsFilters(ModuleFilterCollection filters)
		{
			var regLineTransactionFilterSubGroup = new RegLineTransactionFilterSubGroup(this);

			var lineTransactionExitDateFilter = filters.AddDateFilter(
				FilterConstants.LineTransactionExitDate,
				(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.LineTransactionExitDate, CusTempStorageRegLineTransactionSchema.SRT_PhysicalInOutDate, comparisonOperator, value1, value2)
				);
			lineTransactionExitDateFilter.Category = FilterCategories.Dates;
			lineTransactionExitDateFilter.MultilingualDescription = ResString.GetMultilingualString("1D4ACA42-F4D3-40BC-A4FF-D830C9F53DCC", FilterConstants.LineTransactionExitDate);
			lineTransactionExitDateFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionEntryDateFilter = filters.AddDateFilter(
			FilterConstants.LineTransactionEntryDate,
			(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.LineTransactionEntryDate, CusTempStorageRegLineTransactionSchema.SRT_PhysicalInOutDate, comparisonOperator, value1, value2)
			);
			lineTransactionEntryDateFilter.Category = FilterCategories.Dates;
			lineTransactionEntryDateFilter.MultilingualDescription = ResString.GetMultilingualString("8A0CA749-E038-4F11-BAF8-E9D4974ACC5B", FilterConstants.LineTransactionEntryDate);
			lineTransactionEntryDateFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionDeclarationDateFilter = filters.AddDateFilter(
			FilterConstants.LineTransactionDeclarationDate,
			(comparisonOperator, value1, value2) => GetRegLineDateOffsetQuery(FilterConstants.LineTransactionDeclarationDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionDate, comparisonOperator, value1, value2)
			);
			lineTransactionDeclarationDateFilter.Category = FilterCategories.Dates;
			lineTransactionDeclarationDateFilter.MultilingualDescription = ResString.GetMultilingualString("F174712D-A640-493B-862F-0C010AFDF30F", FilterConstants.LineTransactionDeclarationDate);
			lineTransactionDeclarationDateFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionReferenceTypeFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionReferenceType,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineTransactionSchema.SRT_ReferenceType, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_ReferenceType);
			lineTransactionReferenceTypeFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("3355EE6A-8A80-4A8B-B6E1-DFECE4A61AD4", FilterConstants.LineTransactionReferenceType);
			lineTransactionReferenceTypeFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionReferenceFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionReference,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineTransactionSchema.SRT_Reference, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_Reference);
			lineTransactionReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("4EFD1B07-67C3-4EE6-9A22-98FEE04FAC8E", FilterConstants.LineTransactionReference);
			lineTransactionReferenceFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionInternalReferenceTypeFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionInternalReferenceType,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceType);
			lineTransactionInternalReferenceTypeFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionInternalReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("9EF105D0-4E2B-4ED3-A94E-6A17197F3155", FilterConstants.LineTransactionInternalReferenceType);
			lineTransactionInternalReferenceTypeFilter.SubGroup = regLineTransactionFilterSubGroup;

			var lineTransactionInternalReferenceFilter = filters.AddTextFilter(
			FilterConstants.LineTransactionInternalReference,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineTransactionSchema.SRT_InternalReferenceNumber);
			lineTransactionInternalReferenceFilter.Category = FilterCategories.NumbersAndReferences;
			lineTransactionInternalReferenceFilter.MultilingualDescription = ResString.GetMultilingualString("2C53A983-0D0A-4EF2-A766-8EB506C09A6F", FilterConstants.LineTransactionInternalReference);
			lineTransactionInternalReferenceFilter.SubGroup = regLineTransactionFilterSubGroup;
		}

		void AddLineItemFilters(ModuleFilterCollection filters)
		{
			var regLineItemFilterSubGroup = new RegLineItemFilterSubGroup(this);

			var lineItemTSDItemNumber = filters.AddNumberFilter(
				FilterConstants.LineItemTSDItemNumber,
				(comparisonOperator, value) => GetRegLineNumberQuery(CusTempStorageRegLineItemSchema.SRI_GoodsItemNumber, comparisonOperator, value));
			lineItemTSDItemNumber.Category = FilterCategories.NumbersAndReferences;
			lineItemTSDItemNumber.MultilingualDescription = ResString.GetMultilingualString("C66D50A8-E180-4FA3-A0BF-6C051E1B380A", FilterConstants.LineItemTSDItemNumber);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			lineItemTSDItemNumber.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
			lineItemTSDItemNumber.SubGroup = regLineItemFilterSubGroup;

			var lineItemTariffCode = filters.AddTextFilter(
				FilterConstants.LineItemTariffCode,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineItemSchema.SRI_Tariff, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineItemSchema.SRI_Tariff);
			lineItemTariffCode.Category = FilterCategories.NumbersAndReferences;
			lineItemTariffCode.MultilingualDescription = ResString.GetMultilingualString("3BD62C7B-5A51-4DB8-B4E6-589941C14D05", FilterConstants.LineItemTariffCode);
			lineItemTariffCode.SubGroup = regLineItemFilterSubGroup;

			var lineItemDescription = filters.AddTextFilter(
				FilterConstants.LineItemDescription,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineItemSchema.SRI_GoodsDescription, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineItemSchema.SRI_GoodsDescription);
			lineItemDescription.Category = FilterCategories.NumbersAndReferences;
			lineItemDescription.MultilingualDescription = ResString.GetMultilingualString("A3C2A145-CE68-40B9-92E4-2B37AAC98848", FilterConstants.LineItemDescription);
			lineItemDescription.SubGroup = regLineItemFilterSubGroup;
		}

		void AddLineFilters(ModuleFilterCollection filters)
		{
			var regLineFilterSubGroup = new RegLineFilterSubGroup(this);

			var lineOwnerReferenceNumberFilter = filters.AddTextFilter(
				FilterConstants.LineOwnerReferenceNumber,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_OwnerReference, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_OwnerReference);
			lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("59CAF52D-69FF-40F7-8720-B20178E7515B", FilterConstants.LineOwnerReferenceNumber);
			lineOwnerReferenceNumberFilter.SubGroup = regLineFilterSubGroup;

			var lineLimitDateFilter = filters.AddDateFilter(
				FilterConstants.LineLimitDate,
				(comparisonOperator, value1, value2) => GetRegLineDateQuery(CusTempStorageRegLineSchema.SRL_LimitDate, comparisonOperator, value1, value2)
				);
			lineLimitDateFilter.Category = FilterCategories.Dates;
			lineLimitDateFilter.MultilingualDescription = ResString.GetMultilingualString("2854DAC1-30F9-433C-A812-202F5D8154A2", FilterConstants.LineLimitDate);
			lineLimitDateFilter.SubGroup = regLineFilterSubGroup;

			var custodianEoriFilter = filters.AddTextFilter(
				FilterConstants.LineCustodianEori,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustodianIdentifier, comparisonOperator, value)
				).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustodianIdentifier);
			custodianEoriFilter.Category = FilterCategories.NumbersAndReferences;
			custodianEoriFilter.MultilingualDescription = ResString.GetMultilingualString("C27F4352-A78E-473F-86FB-971239C5F5FA", FilterConstants.LineCustodianEori);
			custodianEoriFilter.SubGroup = regLineFilterSubGroup;

			var ownerEori = filters.AddTextFilter(
				FilterConstants.LineOwnerEORI,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier);
			ownerEori.Category = FilterCategories.NumbersAndReferences;
			ownerEori.MultilingualDescription = ResString.GetMultilingualString("57B0934D-E6EC-441A-BAF8-D8856EA7503C", FilterConstants.LineOwnerEORI);
			ownerEori.SubGroup = regLineFilterSubGroup;

			var linePackageMarks = filters.AddTextFilter(
				FilterConstants.LinePackageMarks,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_PackageMarks, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_PackageMarks);
			linePackageMarks.Category = FilterCategories.NumbersAndReferences;
			linePackageMarks.MultilingualDescription = ResString.GetMultilingualString("A8AC4C77-214C-43C8-A064-719844822E8C", FilterConstants.LinePackageMarks);
			linePackageMarks.SubGroup = regLineFilterSubGroup;

			var lineLocationOfGoods = filters.AddTextFilter(
				FilterConstants.LineLocationOfGoods,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_LocationOfGoods, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_LocationOfGoods);
			lineLocationOfGoods.Category = FilterCategories.NumbersAndReferences;
			lineLocationOfGoods.MultilingualDescription = ResString.GetMultilingualString("661B3EDC-98B6-4381-BF05-F64912043C59", FilterConstants.LineLocationOfGoods);
			lineLocationOfGoods.SubGroup = regLineFilterSubGroup;

			var lineCustomsStatus = filters.AddTextFilter(
				FilterConstants.LineCustomsStatus,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustomsStatus, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustomsStatus);
			lineCustomsStatus.Category = FilterCategories.StatusAndFlags;
			lineCustomsStatus.MultilingualDescription = ResString.GetMultilingualString("749F33F6-0E7E-4308-8F64-ABA463A7B491", FilterConstants.LineCustomsStatus);
			lineCustomsStatus.SubGroup = regLineFilterSubGroup;
		}

		ZQuery GetRegLineDateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZQuery();
			AddDateTimeRange(query, comparisonOperator, JoinCondition.And, column, value1, value2);
			return query;
		}

		ZQuery GetRegLineDateOffsetQuery(ZString filteredField, SchemaDateTimeOffsetColumn column, DateComparisonOperator comparisonOperator, ZDateTimeOffset value1, ZDateTimeOffset value2)
		{
			var query = new ZQuery();
			AddDateTimeOffsetRange(query, comparisonOperator, JoinCondition.And, column, value1, value2, false, false);

			return filteredField.ToString() switch
			{
				FilterConstants.LineTransactionExitDate => GetSecondQuery(query, typesOfTransactionForExitDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, SQLComparisonOperator.Contains, JoinCondition.Or),
				FilterConstants.LineTransactionEntryDate => GetSecondQuery(query, typesOfTransactionForEntryDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionType, SQLComparisonOperator.Contains, JoinCondition.Or),
				FilterConstants.LineTransactionDeclarationDate => GetSecondQuery(query, statusOfTransactionForDeclarationDate, CusTempStorageRegLineTransactionSchema.SRT_TransactionStatus, SQLComparisonOperator.NotEqual, JoinCondition.And),
				_ => query,
			};
		}

		ZQuery GetSecondQuery(ZQuery query, IEnumerable<string> typesControl, SchemaColumn colum, SQLComparisonOperator comparation, JoinCondition join)
		{
			var lineOnlyQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
			typesControl.ForEach(x => lineOnlyQuery.AddToFilter(new ZQuery(colum, comparation, x), join));
			query.AddToFilter(lineOnlyQuery, JoinCondition.And);
			return query;
		}

		ZQuery GetRegLineQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(column, comparisonOperator, value);
		}

		ZQuery GetRegLineNumberQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value) => new (column, comparisonOperator, ZInt.ParseSafe(value, ZInt.Zero));

		public void ResetLineOnlyQuery()
		{
			LineOnlyQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
		}

		public ZQuery LineOnlyQuery;

		readonly IEnumerable<string> typesOfTransactionForExitDate = new[] { CusTempStorageRegLineTransactionTypeList.Codes.Adjustment, CusTempStorageRegLineTransactionTypeList.Codes.Transaction };

		readonly IEnumerable<string> typesOfTransactionForEntryDate = new[] { CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance };

		readonly IEnumerable<string> statusOfTransactionForDeclarationDate = new[] { CusTempStorageRegLineTransactionStatusList.Codes.Deleted };

		class RegLineFilterSubGroup : ModuleFilterSubGroup
		{
			public RegLineFilterSubGroup(TemporaryStorageRegisterFilterBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
			}
			readonly TemporaryStorageRegisterFilterBusinessObject filterBizo;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineOnlyQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
				lineOnlyQuery.AddToFilter(filter);
				filterBizo.LineOnlyQuery = lineOnlyQuery;

				var lineQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);
				lineQuery.AddToFilter(filter);
				var headerQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
				headerQuery.AddSubQuery(lineQuery, JoinCondition.And);
				return headerQuery;
			}
		}

		class RegLineTransactionFilterSubGroup : ModuleFilterSubGroup
		{
			public RegLineTransactionFilterSubGroup(TemporaryStorageRegisterFilterBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
			}
			readonly TemporaryStorageRegisterFilterBusinessObject filterBizo;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineOnlyQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));
				var lineTransactionQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineTransaction), CusTempStorageRegLineTransactionSchema.SRT_SRL);
				lineTransactionQuery.AddToFilter(filter);

				lineOnlyQuery.AddSubQuery(lineTransactionQuery, JoinCondition.And);
				filterBizo.LineOnlyQuery = lineOnlyQuery;

				var lineQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);
				var headerQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
				lineQuery.AddSubQuery(lineTransactionQuery, JoinCondition.And);
				headerQuery.AddSubQuery(lineQuery, JoinCondition.And);
				return headerQuery;
			}
		}

		class RegLineItemFilterSubGroup : ModuleFilterSubGroup
		{
			public RegLineItemFilterSubGroup(TemporaryStorageRegisterFilterBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
			}
			readonly TemporaryStorageRegisterFilterBusinessObject filterBizo;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var lineOnlyQuery = new ZDBOnlyQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine));

				var lineItemQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItem), CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);
				var linePivotItemQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRI_Item);
				var linePivotLineQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.SRV_SRL_Line);
				var linePivotPkQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLineItemPivot), CusTempStorageRegLineItemPivotSchema.PK);
				lineItemQuery.AddToFilter(filter);

				linePivotPkQuery.AddSubQuery(lineItemQuery, JoinCondition.And);
				linePivotItemQuery.AddSubQuery(linePivotPkQuery, JoinCondition.And);
				linePivotLineQuery.AddSubQuery(linePivotPkQuery, JoinCondition.And);

				lineOnlyQuery.AddSubQuery(linePivotLineQuery, JoinCondition.And);
				filterBizo.LineOnlyQuery = lineOnlyQuery;

				var lineQuery = new ZDBOnlySubQuery(typeof(EU.TemporaryStorage.Business.CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);

				var headerQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
				lineQuery.AddSubQuery(linePivotLineQuery, JoinCondition.And);
				headerQuery.AddSubQuery(lineQuery, JoinCondition.And);
				return headerQuery;
			}
		}

		protected override EU.TemporaryStorage.Business.CusTempStorageRegHeader GetCusTempStorageRegHeaderForLookups() => Factory.GetNull<CusTempStorageRegHeader>();
	}
}
