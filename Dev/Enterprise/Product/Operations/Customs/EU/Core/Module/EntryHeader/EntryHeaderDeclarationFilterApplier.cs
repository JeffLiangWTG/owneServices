using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.EU.Module.EntryHeaderFilterBusinessObject;

namespace Enterprise.Customs.EU.Module
{
	public class EntryHeaderDeclarationFilterApplier
	{
		public EntryHeaderDeclarationFilterApplier(EntryHeaderFilterBusinessObject parentFilter)
		{
			this.parentFilter = Argument.NotNull(parentFilter, nameof(parentFilter));
		}

		readonly EntryHeaderFilterBusinessObject parentFilter;

		public void AddFilters(ModuleFilterCollection filters)
		{
			Argument.NotNull(filters, nameof(filters));

			AddEntryStyleFilter(filters);
			AddCustomsOfficePresentationFilter(filters);
			AddDispatchFilter(filters);
			AddDestinationFilter(filters);
			AddApprovalDeferNoFilter(filters);
		}

		#region Entry Style

		void AddEntryStyleFilter(ModuleFilterCollection filters)
		{
			var entryStyleFilter = filters.AddTextFilter(EUFilterConstants.DeclarationEntryStyleFilter, GetEntryStyleFilterQuery, Lookups.EntryStyleList);
			entryStyleFilter.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_MessageSubType);
			entryStyleFilter.MultilingualDescription = ResString.GetMultilingualString("F31B32E9-370D-4345-81AF-52248C7A8C31", EUFilterConstants.DeclarationEntryStyleFilter);
			entryStyleFilter.Category = DeclarationCategory;

			ZQuery GetEntryStyleFilterQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetFilterQuery(JobDeclarationSchema.JE_MessageSubType, comparisonOperator, value);
		}

		#endregion

		#region Customs Office Presentation

		void AddCustomsOfficePresentationFilter(ModuleFilterCollection filters)
		{
			var customsOfficePresentationFilter = filters.AddNkFilter(EUFilterConstants.DeclarationCustomsOfficeOfPresentation, GetCustomsOfficePresentationQuery, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOfficeList);
			customsOfficePresentationFilter.WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_CustomsOffice);
			customsOfficePresentationFilter.MultilingualDescription = ResString.GetMultilingualString("BD6D797C-D604-4482-9F8C-6BA734CC3644", EUFilterConstants.DeclarationCustomsOfficeOfPresentation);
			customsOfficePresentationFilter.Category = parentFilter.CustomsOfficeCategory;

			ZQuery GetCustomsOfficePresentationQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetFilterQuery(JobDeclarationSchema.JE_CustomsOffice, comparisonOperator, value);
		}

		#endregion

		#region Dispatch

		void AddDispatchFilter(ModuleFilterCollection filters)
		{
			var dispatchFilter = filters.AddTextFilter(EUFilterConstants.DeclarationDispatch, GetDispatchQuery, Lookups.GoodsOriginList);
			dispatchFilter.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_GoodsOrigin);
			dispatchFilter.MultilingualDescription = ResString.GetMultilingualString("B62FEE7D-4DF4-492B-A383-62695B77DD77", EUFilterConstants.DeclarationDispatch);
			dispatchFilter.Category = DeclarationCategory;

			ZQuery GetDispatchQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetFilterQuery(JobDeclarationSchema.JE_GoodsOrigin, comparisonOperator, value);
		}

		#endregion

		#region Destination

		void AddDestinationFilter(ModuleFilterCollection filters)
		{
			var destinationFilter = filters.AddTextFilter(EUFilterConstants.DeclarationDestination, GetDestinationQuery, Lookups.GoodsOriginList);
			destinationFilter.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_GoodsDestination);
			destinationFilter.MultilingualDescription = ResString.GetMultilingualString("9301B56F-5CDB-4652-A39F-C8B734AA3007", EUFilterConstants.DeclarationDestination);
			destinationFilter.Category = DeclarationCategory;

			ZQuery GetDestinationQuery(SQLComparisonOperator comparisonOperator, ZString value) => GetFilterQuery(JobDeclarationSchema.JE_GoodsDestination, comparisonOperator, value);
		}

		#endregion

		#region Approval Defer No

		void AddApprovalDeferNoFilter(ModuleFilterCollection filters)
		{
			var approvalDeferNoFilter = filters.AddTextFilter(EUFilterConstants.DeclarationApprovalDeferNo, GetApprovalDeferNoFilter);
			approvalDeferNoFilter.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_DefermentAccountNumber);
			approvalDeferNoFilter.MultilingualDescription = ResString.GetMultilingualString("3B2C420F-773D-4AE7-9803-8141800E4D00", EUFilterConstants.DeclarationApprovalDeferNo);
			approvalDeferNoFilter.Category = DeclarationCategory;

			ZQuery GetApprovalDeferNoFilter(SQLComparisonOperator comparisonOperator, ZString value) => GetFilterQuery(JobDeclarationSchema.JE_DefermentAccountNumber, comparisonOperator, value);
		}

		#endregion

		ZQuery GetFilterQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = GetEntryHeaderQuery();

			var declarationSubQuery = GetDeclarationSubQuery();
			declarationSubQuery.AddToFilter(schemaColumn, comparisonOperator, value);

			query.AddSubQuery(declarationSubQuery, JoinCondition.And);
			return query;
		}

		EntryHeaderFilterLookups Lookups => parentFilter.Lookups;

		ZDBOnlyQuery GetEntryHeaderQuery() => new ZDBOnlyQuery(typeof(CusEntryHeader));

		ZDBOnlySubQuery GetDeclarationSubQuery() => new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);

		FilterCategory DeclarationCategory => declarationCategory ?? (declarationCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("EntryHeaderFilter|JobDeclaration", "Declaration")));
		FilterCategory declarationCategory;
	}
}
