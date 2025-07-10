using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Module;

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
		get => lookups ??= GetNewSumARegisterBusinessObjectLookups();
	}

	SumARegisterFilterBusinessObjectLookups lookups;

	protected virtual SumARegisterFilterBusinessObjectLookups GetNewSumARegisterBusinessObjectLookups() => new(this);

	protected override ModuleFilterCollection GetModuleFiltersCore()
	{
		var result = new ModuleFilterCollection();

		var atbNumberTextFilter = result.AddTextFilter(Schema.ATBNumber, CusTempStorageRegHeaderSchema.SRH_Reference);
		atbNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
		atbNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("A9DFEE75-41D9-428B-A047-AF77CAB946AB", Schema.ATBNumber);
		var arrivalDateDateFilter = result.AddDateFilter(Schema.ArrivalDate, CusTempStorageRegHeaderSchema.SRH_ArrivalDate);
		arrivalDateDateFilter.Category = FilterCategories.Dates;
		arrivalDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("2B2FA2CA-7618-4337-AAF1-CF0B3A730B26", Schema.ArrivalDate);
		var presentationDateDateFilter = result.AddDateFilter(Schema.PresentationDate, CusTempStorageRegHeaderSchema.SRH_PresentationDate);
		presentationDateDateFilter.Category = FilterCategories.Dates;
		presentationDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("F711A6EF-6CE8-435E-AB37-A126D5CFA88E", Schema.PresentationDate);
		var previousRefNumberTextFilter = result.AddTextFilter(Schema.PreviousRefNumber, CusTempStorageRegHeaderSchema.SRH_PreviousReference);
		previousRefNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
		previousRefNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("65D01A84-372F-4D96-A6DA-5A80EC01D072", Schema.PreviousRefNumber);
		var statusTextFilter = result.AddTextFilter(Schema.Status, CusTempStorageRegHeaderSchema.SRH_Status);
		statusTextFilter.Category = FilterCategories.StatusAndFlags;
		statusTextFilter.MultilingualDescription = ResString.GetMultilingualString("B35BB530-45E2-4FBD-BA08-EAE075D06E56", Schema.Status);
		var customerReferenceTextFilter = result.AddTextFilter(Schema.CustomerReference, CusTempStorageRegHeaderSchema.SRH_InternalReference);
		customerReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
		customerReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("14623BE1-C682-4C74-975B-BE7DDCD4A6FA", Schema.CustomerReference);

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
		lineOwnerReferenceTypeFilter.MultilingualDescription = ResString.GetMultilingualString("5D65640D-81FA-464D-A101-7477F694F2EC", Schema.LineOwnerReferenceType);
		lineOwnerReferenceTypeFilter.SubGroup = regLineFilterSubGroup;

		var lineOwnerReferenceNumberFilter = filters.AddTextFilter(
			Schema.LineOwnerReferenceNumber,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_OwnerReference, comparisonOperator, value)
		).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_OwnerReference);
		lineOwnerReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
		lineOwnerReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AA0049EC-EB0F-4276-BF79-56E98F26755D", Schema.LineOwnerReferenceNumber);
		lineOwnerReferenceNumberFilter.SubGroup = regLineFilterSubGroup;

		var lineLimitDateFilter = filters.AddDateFilter(
			Schema.LineLimitDate,
			(comparisonOperator, value1, value2) => GetRegLineDateQuery(CusTempStorageRegLineSchema.SRL_LimitDate, comparisonOperator, value1, value2)
		);
		lineLimitDateFilter.Category = FilterCategories.Dates;
		lineLimitDateFilter.MultilingualDescription = ResString.GetMultilingualString("854014EB-A86F-48E2-A919-7A33E4CFE32B", Schema.LineLimitDate);
		lineLimitDateFilter.SubGroup = regLineFilterSubGroup;

		var custodianEoriFilter = filters.AddTextFilter(
			Schema.LineCustodianEori,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustodianIdentifier, comparisonOperator, value)
		).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustodianIdentifier);
		custodianEoriFilter.Category = FilterCategories.NumbersAndReferences;
		custodianEoriFilter.MultilingualDescription = ResString.GetMultilingualString("DCCE76F2-FCB6-4EFB-B362-5F1963B9C2E1", Schema.LineCustodianEori);
		custodianEoriFilter.SubGroup = regLineFilterSubGroup;

		var traderEoriFilter = filters.AddTextFilter(
			Schema.LineTraderEori,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier, comparisonOperator, value)
		).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_GoodsOwnerIdentifier);
		traderEoriFilter.Category = FilterCategories.NumbersAndReferences;
		traderEoriFilter.MultilingualDescription = ResString.GetMultilingualString("CCD91769-9F08-45C5-9091-0C450FEB4001", Schema.LineTraderEori);
		traderEoriFilter.SubGroup = regLineFilterSubGroup;

		var lineCustomsStatusFilter = filters.AddTextFilter(
			Schema.LineStatus,
			(comparisonOperator, value) => GetRegLineQuery(CusTempStorageRegLineSchema.SRL_CustomsStatus, comparisonOperator, value),
			() => Lookups.CustomsStatusList
		).WithMaxLengthOf<ModuleTextFilter>(CusTempStorageRegLineSchema.SRL_CustomsStatus);
		lineCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
		lineCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("E18EF64D-35E3-4319-8F2B-1108BE0532B3", Schema.LineStatus);
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
