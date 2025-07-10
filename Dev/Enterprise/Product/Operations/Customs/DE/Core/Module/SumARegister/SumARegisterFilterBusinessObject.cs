using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Module
{
	public class SumARegisterFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string ATBNumber = "Registration Number";
			public const string ArrivalDate = "Arrival Date";
			public const string PresentationDate = "Presentation Date";
			public const string PreviousRefNumber = "Previous Reference Number";
			public const string Status = "Status";
			public const string CustomerReference = "Customer Reference";

			public const string LineOwnerReferenceType = "Owner Reference Type";
			public const string LineOwnerReferenceNumber = "Owner Reference Number";
			public const string LineLimitDate = "Limit Date";
			public const string LineCustodianEori = "Custodian EORI";
			public const string LineTraderEori = "Disp. Ent. Trader EORI";
			public const string LineStatus = "Line Status";
		}

		public SumARegisterFilterBusinessObject()
		{
			ResetLineOnlyQuery();
		}

		public SumARegisterFilterBusinessObjectLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new SumARegisterFilterBusinessObjectLookups(this);
				}

				return lookups;
			}
		}

		SumARegisterFilterBusinessObjectLookups lookups;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var atbNumberTextFilter = result.AddTextFilter(Schema.ATBNumber, CusTempStorageRegHeaderSchema.SRH_Reference);
			atbNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			atbNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("507E37EC-BDB3-46B8-8097-F13C351FAF47", Schema.ATBNumber);
			var arrivalDateDateFilter = result.AddDateFilter(Schema.ArrivalDate, CusTempStorageRegHeaderSchema.SRH_ArrivalDate);
			arrivalDateDateFilter.Category = FilterCategories.Dates;
			arrivalDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("91A70BB0-77F2-4FEC-A586-929D749A5887", Schema.ArrivalDate);
			var presentationDateDateFilter = result.AddDateFilter(Schema.PresentationDate, CusTempStorageRegHeaderSchema.SRH_PresentationDate);
			presentationDateDateFilter.Category = FilterCategories.Dates;
			presentationDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("81AF61D9-7EC0-438F-9D7A-D9B1C345DE12", Schema.PresentationDate);
			var previousRefNumberTextFilter = result.AddTextFilter(Schema.PreviousRefNumber, CusTempStorageRegHeaderSchema.SRH_PreviousReference);
			previousRefNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			previousRefNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("8644C8B6-1174-4D98-9608-F198C63A816B", Schema.PreviousRefNumber);
			var statusTextFilter = result.AddTextFilter(Schema.Status, CusTempStorageRegHeaderSchema.SRH_Status);
			statusTextFilter.Category = FilterCategories.StatusAndFlags;
			statusTextFilter.MultilingualDescription = ResString.GetMultilingualString("0e25a683-ea80-4082-aded-6e064cc43cce", Schema.Status);
			var customerReferenceTextFilter = result.AddTextFilter(Schema.CustomerReference, CusTempStorageRegHeaderSchema.SRH_InternalReference);
			customerReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
			customerReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("46C4CBF3-93DF-441D-9B0A-0DB5896CC2AD", Schema.CustomerReference);

			AddLineFilters(result);
			return result;
		}

		void AddLineFilters(ModuleFilterCollection filters)
		{
			var regLineFilterSubGroup = new RegLineFilterSubGroup(this);
			var lineOwnerReferenceTypeFilter = filters.AddTextFilter(
				Schema.LineOwnerReferenceType,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_OwnerReferenceType, comparisonOperator, value),
				() => Lookups.OwnerReferenceTypeList
				).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_OwnerReferenceType);
			lineOwnerReferenceTypeFilter.Category = FilterCategories.StatusAndFlags;
			lineOwnerReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("4A6D8411-F49F-4589-83CA-E6D94C38AA13", Schema.LineOwnerReferenceType);
			lineOwnerReferenceTypeFilter.SubGroup = regLineFilterSubGroup;

			var lineOwnerReferenceNumberFilter = filters.AddTextFilter(
				Schema.LineOwnerReferenceNumber,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_OwnerReference, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_OwnerReference);
			lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("F58E45A9-F542-4CDB-90D6-E3C3971D3AD0", Schema.LineOwnerReferenceNumber);
			lineOwnerReferenceNumberFilter.SubGroup = regLineFilterSubGroup;

			var lineLimitDateFilter = filters.AddDateFilter(
				Schema.LineLimitDate,
				(comparisonOperator, value1, value2) => GetRegLineDateQuery(CusTempStorageRegLineSchema.SRL_LimitDate, comparisonOperator, value1, value2)
				);
			lineLimitDateFilter.Category = FilterCategories.Dates;
			lineLimitDateFilter.MultilingualDescription = ResString.GetMultilingualString("7656B2BB-957B-485D-B9E6-C018AAACF19F", Schema.LineLimitDate);
			lineLimitDateFilter.SubGroup = regLineFilterSubGroup;

			var custodianEoriFilter = filters.AddTextFilter(
				Schema.LineCustodianEori,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustodianIdentifier, comparisonOperator, value)
				).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustodianIdentifier);
			custodianEoriFilter.Category = FilterCategories.NumbersAndReferences;
			custodianEoriFilter.MultilingualDescription = ResString.GetMultilingualString("8F3F9A59-F2EA-49DF-9904-17EAFF7894DD", Schema.LineCustodianEori);
			custodianEoriFilter.SubGroup = regLineFilterSubGroup;

			var traderEoriFilter = filters.AddTextFilter(
				Schema.LineTraderEori,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier, comparisonOperator, value)
			).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier);
			traderEoriFilter.Category = FilterCategories.NumbersAndReferences;
			traderEoriFilter.MultilingualDescription = ResString.GetMultilingualString("6DAB74B2-1606-469C-8DF4-184C1FA11FBB", Schema.LineTraderEori);
			traderEoriFilter.SubGroup = regLineFilterSubGroup;

			var lineCustomsStatusFilter = filters.AddTextFilter(
				Schema.LineStatus,
				(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustomsStatus, comparisonOperator, value),
				() => Lookups.CustomsStatusList
				).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustomsStatus);
			lineCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			lineCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("60F07555-C39D-4F47-817E-50C0E0CFAB11", Schema.LineStatus);
			lineCustomsStatusFilter.SubGroup = regLineFilterSubGroup;
		}

		ZQuery GetRegLineDateQuery(SchemaDateTimeColumn column, DateComparisonOperator comparisonOperator, ZDateTime value1, ZDateTime value2)
		{
			var query = new ZQuery();
			AddDateTimeRange(query, comparisonOperator, JoinCondition.And, column, value1, value2);
			return query;
		}

		ZQuery GetRegLineQuery(SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			return new ZQuery(column, comparisonOperator, value);
		}

		public void ResetLineOnlyQuery()
		{
			LineOnlyQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegLine));
		}

		public ZQuery LineOnlyQuery;

		class RegLineFilterSubGroup : ModuleFilterSubGroup
		{
			public RegLineFilterSubGroup(SumARegisterFilterBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
			}
			readonly SumARegisterFilterBusinessObject filterBizo;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				filterBizo.LineOnlyQuery = filter;

				var lineQuery = new ZDBOnlySubQuery(typeof(CusTempStorageRegLine), CusTempStorageRegLineSchema.SRL_SRH);
				lineQuery.AddToFilter(filter);
				var headerQuery = new ZDBOnlyQuery(typeof(CusTempStorageRegHeader));
				headerQuery.AddSubQuery(lineQuery, JoinCondition.And);
				return headerQuery;
			}
		}
	}
}
