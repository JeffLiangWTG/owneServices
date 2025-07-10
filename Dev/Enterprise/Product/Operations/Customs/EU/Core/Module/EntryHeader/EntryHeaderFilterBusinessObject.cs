using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Module
{
	public class EntryHeaderFilterBusinessObject : Customs.Module.EntryHeaderFilterBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class EUFilterConstants
		{
			public const string DeclarationEntryStyleFilter = "Entry Style";
			public const string DeclarationCustomsOfficeOfPresentation = "Presentation Office";
			public const string DeclarationDispatch = "Dispatch";
			public const string DeclarationDestination = "Destination";
			public const string DeclarationApprovalDeferNo = "Approval Defer No";
			public const string AddressFromWarehouseFilter = "From Warehouse";
			public const string AddressToWarehouseFilter = "To Warehouse";
			public const string AddressLocalClientCodeFilter = "Local Client Code";
			public const string CustomsQuantity = "Total Customs Quantity";
			public const string NumberOfEntryLines = "No. of Entry Lines";
			public const string ImporterFullName = "Importer (Full Name)";
			public const string SupplierFullName = "Supplier (Full Name)";
			public const string ExitedStatus = "Export Exit Status";
		}

		public new EntryHeaderFilterLookups Lookups => (EntryHeaderFilterLookups)base.Lookups;

		protected override Customs.Module.EntryHeaderFilterLookups GetNewLookups() => new EntryHeaderFilterLookups(this);

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();

			new EntryHeaderDeclarationFilterApplier(this).AddFilters(filters);
			new EntryHeaderAddressesFilterApplier(this).AddFilters(filters);

			var customsQtyFilter = new EntryHeaderTotalCustomsQtyFilterGenerator()
				.Generate(EUFilterConstants.CustomsQuantity, FilterCategories.Other, customQuantityDescription);
			filters.AddFilter(customsQtyFilter);

			var noOfEntryLinesFilter = new EntryHeaderNoOfEntryLinesFilterGenerator()
				.Generate(EUFilterConstants.NumberOfEntryLines, FilterCategories.Other, noOfEntryLinesDescription);
			filters.AddFilter(noOfEntryLinesFilter);

			var importerFullNameFilter = filters.AddTextFilter(EUFilterConstants.ImporterFullName, GetImporterFullNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			importerFullNameFilter.Category = FilterCategories.Organisations;
			importerFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("8EFCDDE7-0763-40AA-949C-E09365039046", EUFilterConstants.ImporterFullName);

			var supplierFullNameFilter = filters.AddTextFilter(EUFilterConstants.SupplierFullName, GetSupplierFullNameQuery)
				.WithMaxLengthOf<ModuleTextFilter>(OrgHeaderSchema.OH_FullName);
			supplierFullNameFilter.Category = FilterCategories.Organisations;
			supplierFullNameFilter.MultilingualDescription = ResString.GetMultilingualString("72EAD5EA-4A01-4585-8F6D-B84463E1B23C", EUFilterConstants.SupplierFullName);

			var exitedStatus = filters.AddTextFilter(EUFilterConstants.ExitedStatus, CusEntryHeaderSchema.CH_ExitedStatus, Lookups.ExportExitStatusList);
			exitedStatus.Category = FilterCategories.StatusAndFlags;
			exitedStatus.MaxLength = CusEntryHeaderSchema.CH_ExitedStatus.MaxLength;
			exitedStatus.MultilingualDescription = ResString.GetMultilingualString("CB9D01E8-88EE-476B-8192-BF8FE6DF07AF", EUFilterConstants.ExitedStatus);

			AddRequestedProcedureFilters(filters);

			return filters;
		}

		public FilterCategory CustomsOfficeCategory => customsOfficeCategory ?? (customsOfficeCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("F853AF04-46CD-4EA9-BF4C-5EEF82EB6FC2", "Customs Offices")));
		FilterCategory customsOfficeCategory;

		MultilingualString customQuantityDescription =>
			ResString.GetMultilingualString("55C30738-E540-4E82-B923-0EC451A7E94C", EUFilterConstants.CustomsQuantity);

		MultilingualString noOfEntryLinesDescription =>
			ResString.GetMultilingualString("ABA24FB4-63E9-4581-AB1B-A50F474BE1AA", EUFilterConstants.NumberOfEntryLines);

		public bool SupportRequestedProcedure => SupportRequestedProcedureCore;
		protected virtual bool SupportRequestedProcedureCore => false;

		void AddRequestedProcedureFilters(ModuleFilterCollection filters)
		{
			if (SupportRequestedProcedure)
			{
				var requestedProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.RequestedProcedure, GetRequestedProcedureQuery, Lookups.RequestedProcedureList);
				requestedProcedureFilter.MaxLength = 2;
				requestedProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				requestedProcedureFilter.MultilingualDescription = DeclarationFilterConstants.RequestedProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(requestedProcedureFilter);

				var previousProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.PreviousProcedure, GetPreviousProcedureQuery, Lookups.PreviousProcedureCodeList);
				previousProcedureFilter.MaxLength = 2;
				previousProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				previousProcedureFilter.MultilingualDescription = DeclarationFilterConstants.PreviousProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(previousProcedureFilter);

				var additionalProcedureFilter = filters.AddTextFilter(DeclarationFilterConstants.AdditionalProcedure, GetAdditionalProcedureQuery, Lookups.AdditionalProcedureCodeList);
				additionalProcedureFilter.MaxLength = 3;
				additionalProcedureFilter.Category = FilterCategories.NumbersAndReferences;
				additionalProcedureFilter.MultilingualDescription = DeclarationFilterConstants.AdditionalProcedureMultilingualDescription;
				RequestedProcedureHelper.ApplyAllowedComparisonOperatorList(additionalProcedureFilter);
			}
		}

		protected override ResourceString GetImporterSupplierFilterMultilingualDescription()
		{
			return ResString.GetMultilingualString("D639B91D-2304-41C2-A783-A5A3C479D91B", "Importer/Supplier (Codes)");
		}

		protected override ResourceString GetEntryNumberFilterMultilingualDescription()
		{
			return ResString.GetMultilingualString("55A58E4D-6E31-40A0-AC4C-565922F07FCD", "MRN");
		}

		ZQuery GetImporterFullNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GeOrganisationFullNameQuery(JobDeclarationSchema.JE_OH_Importer, comparisonOperator, value);
		}

		ZQuery GetSupplierFullNameQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			return GeOrganisationFullNameQuery(JobDeclarationSchema.JE_OH_Supplier, comparisonOperator, value);
		}

		ZQuery GeOrganisationFullNameQuery(SchemaColumn schemaColumn, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);

			var organisationSubQuery = new ZDBOnlySubQuery(typeof(OrgHeader), schemaColumn);
			organisationSubQuery.AddToFilter(OrgHeaderSchema.OH_FullName, comparisonOperator, value);
			declarationSubQuery.AddSubQuery(organisationSubQuery, JoinCondition.Or);

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				declarationSubQuery.AddToFilter(JoinCondition.Or, schemaColumn, SQLComparisonOperator.Equal, null);
			}

			query.AddSubQuery(declarationSubQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetRequestedProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetCusEntryHeaderFromRequestedProcedureQuery(RequestedProcedureHelper.GetRequestedProcedureFilterTextWithWildcards(filterText));

		ZQuery GetPreviousProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetCusEntryHeaderFromRequestedProcedureQuery(RequestedProcedureHelper.GetPreviousProcedureFilterTextWithWildcards(filterText));

		ZQuery GetAdditionalProcedureQuery(SQLComparisonOperator comparisonOperator, ZString filterText) => RequestedProcedureHelper.GetCusEntryHeaderFromRequestedProcedureQuery(RequestedProcedureHelper.GetAdditionalProcedureFilterTextWithWildcards(filterText));
	}
}
