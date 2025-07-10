using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.DE.Business
{
	public class SimplifiedDeclarationFilterStripBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string RegistrationNumber = "Registration Number";
			public const string LocalClearanceDate = "Local Clearance Date";
			public const string EntryType = "Entry Type";
			public const string RepresentationType = "Rep. Type";
			public const string Declarant = "Declarant";
			public const string Importer = "Importer";
			public const string Representative = "Representative";
			public const string RepresentedParty = "Represented Party";
			public const string Branch = "Branch";
			public const string NumberOfRows = "Number Of Rows";
		}

		public override ZQuery Filter
		{
			get
			{
				var filter = base.Filter;
				if (ActiveModuleFilters.SingleOrDefault(f => f.Description == Schema.NumberOfRows) is ModuleTextFilter numberOfRowsFilter && int.TryParse(numberOfRowsFilter.Property, out var maximumRows))
				{
					filter.MaximumRows = maximumRows;
				}
				return filter;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var registrationNumberFilter = result.AddTextFilter(Schema.RegistrationNumber, CusReconEntrySchema.CRE_OriginalEntryNumber);
			registrationNumberFilter.Category = FilterCategories.NumbersAndReferences;
			registrationNumberFilter.MultilingualDescription = ResString.GetMultilingualString("AA74E100-A250-4343-B9B9-73737A6DBC2F", Schema.RegistrationNumber);
			registrationNumberFilter.MaxLength = CusReconEntrySchema.CRE_OriginalEntryNumber.MaxLength;
			registrationNumberFilter.SupportsBlankComparisonOperators = false;

			var localClearanceDateFilter = result.AddDateFilter(Schema.LocalClearanceDate, CusReconEntrySchema.CRE_EntryDate);
			localClearanceDateFilter.Category = FilterCategories.Dates;
			localClearanceDateFilter.Visibility = FilterVisibility.AlwaysVisible;
			localClearanceDateFilter.MultilingualDescription = ResString.GetMultilingualString("A55A1998-991E-4534-AFCC-22D784E26A77", Schema.LocalClearanceDate);

			var entryTypeFilter = result.AddTextFilter(Schema.EntryType, CusReconEntrySchema.CRE_EntryType, ReconEntryLookups.EntryTypeList);
			entryTypeFilter.Category = FilterCategories.ModesAndTypes;
			entryTypeFilter.MultilingualDescription = ResString.GetMultilingualString("70E50D1D-FDCD-45B0-ACE8-2818434A83AA", Schema.EntryType);
			entryTypeFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			var representationTypeFilter = result.AddTextFilter(Schema.RepresentationType, GetRepresentationTypeQuery, RepresentationTypeList);
			representationTypeFilter.Category = FilterCategories.ModesAndTypes;
			representationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("E9CC88D6-99D6-4B06-9264-96B2A3DB04E8", Schema.RepresentationType);
			representationTypeFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

			var declarantFilter = result.AddTextFilter(Schema.Declarant, (comparisonOperator, value) => GetOrgHeaderCodeQuery(CusReconEntrySchema.CRE_OA_DeclarantAddress, comparisonOperator, value));
			declarantFilter.Category = FilterCategories.Organisations;
			declarantFilter.MultilingualDescription = ResString.GetMultilingualString("16C11293-2D4F-4F0B-8A87-C0346A9856C3", Schema.Declarant);
			declarantFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
			declarantFilter.SupportsBlankComparisonOperators = false;

			var importerFilter = result.AddTextFilter(Schema.Importer, (comparisonOperator, value) => GetOrgHeaderCodeQuery(CusReconEntrySchema.CRE_OA_ImporterAddress, comparisonOperator, value));
			importerFilter.Category = FilterCategories.Organisations;
			importerFilter.MultilingualDescription = ResString.GetMultilingualString("3D162D35-0B2F-48BB-A380-6391A5B36F55", Schema.Importer);
			importerFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
			importerFilter.SupportsBlankComparisonOperators = false;

			var representativeFilter = result.AddTextFilter(Schema.Representative, (comparisonOperator, value) => GetOrgHeaderCodeQuery(CusReconEntrySchema.CRE_OA_RepresentativeAddress, comparisonOperator, value));
			representativeFilter.Category = FilterCategories.Organisations;
			representativeFilter.MultilingualDescription = ResString.GetMultilingualString("A65891F4-F5D5-491B-BCDE-2959CD1C4BE0", Schema.Representative);
			representativeFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
			representativeFilter.SupportsBlankComparisonOperators = false;

			var representedPartyFilter = result.AddTextFilter(Schema.RepresentedParty, (comparisonOperator, value) => GetOrgHeaderCodeQuery(CusReconEntrySchema.CRE_OA_BuyingAgentAddress, comparisonOperator, value));
			representedPartyFilter.Category = FilterCategories.Organisations;
			representedPartyFilter.MultilingualDescription = ResString.GetMultilingualString("129ADBDC-82C7-4D82-8FE7-E36C3C512FD5", Schema.RepresentedParty);
			representedPartyFilter.MaxLength = OrgHeaderSchema.OH_Code.MaxLength;
			representedPartyFilter.SupportsBlankComparisonOperators = false;

			var branchFilter = result.AddGuidFilter(Schema.Branch, ModuleIDs.GlbBranch, CusReconEntrySchema.CRE_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("0F98D42E-7E5F-4D73-8E9D-22692789AD7F", Schema.Branch);
			branchFilter.SupportsBlankComparisonOperators = false;

			var numberOfRowsFilter = result.AddTextFilter(Schema.NumberOfRows, (_, _) => new ZDBOnlyQuery(typeof(CusReconEntry)));
			numberOfRowsFilter.Category = FilterCategories.Other;
			numberOfRowsFilter.MultilingualDescription = ResString.GetMultilingualString("7899CF38-0AE7-49F7-97F7-F2FA0E0343F8", Schema.NumberOfRows);

			return result;
		}

		ZQuery GetOrgHeaderCodeQuery(CargoWise.Schema.SchemaColumn column, SQLComparisonOperator comparisonOperator, ZString value)
		{
			var orgHeaderQuery = new ZDBOnlySubQuery(typeof(OrgHeader), OrgHeaderSchema.PK);
			orgHeaderQuery.AddToFilter(OrgHeaderSchema.OH_Code, comparisonOperator, value);
			var orgAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, orgHeaderQuery, JoinCondition.And);
			var result = new ZDBOnlyQuery(typeof(CusReconEntry));
			result.AddSubQuery(column, orgAddressQuery, JoinCondition.And);
			return result;
		}

		ZQuery GetRepresentationTypeQuery(ZString value)
		{
			const string declarantTypeQuery = @"CRE_CH_OriginalEntry IN (" + // filter requires SQL statement
@"(
	SELECT CH_PK 
	FROM dbo.CusEntryHeader INNER JOIN
		dbo.JobDeclaration ON JE_ClusterKey = CH_ClusterKey
	WHERE JE_DeclarantType = @declarantType
))";
			var result = new ZDBOnlyQuery(typeof(CusReconEntry));
			result.AddFilterAndZSQLParameterCollection(declarantTypeQuery,
				new ZSqlParameterCollection(ZSqlParameter.New("@declarantType", value, JobDeclarationSchema.JE_DeclarantType)));
			return result;
		}

		CusReconEntryLookups ReconEntryLookups => reconEntryLookups ?? (reconEntryLookups = Factory.GetNull<CusReconEntry>().Lookups);
		CusReconEntryLookups reconEntryLookups;

		CodeDescriptionPairList RepresentationTypeList => Factory.GetCachedValue<EU.Business.RepresentationTypeList>();
	}
}
