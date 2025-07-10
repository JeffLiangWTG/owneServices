using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		#region SuppressResourceStringsCheckRegion
		public static class JobDeclarationFilterConstants
		{
			public const string ContinuousGuarantee = "Continuous Guarantee"; // Filter types
			public const string LinkedGuarantee = "Linked Guarantee"; // Filter types
			public const string GuaranteeAmount = "Guaranteed Amount"; // Filter types
			public const string GuaranteeActivity = "Guarantee Activity"; // Filter types
			public const string GuaranteeStatus = "Guarantee Status"; // Filter types
			public const string Continuous = "Continuous"; // Filter constants
		}
		#endregion

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddGuaranteeFilters(filters);
			return filters;
		}

		protected override bool ShowManifestNumberFilter() => true;

		#region Guarantee Filters

		void AddGuaranteeFilters(ModuleFilterCollection filters)
		{
			var guaranteeSubGroup = new GuaranteeSubGroup();
			var guaranteeCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("Customs|JobDeclarationFilter|Guarantee", "Guarantee"));

			var guaranteeAmountFilter = filters.AddNumberRangeFilter(JobDeclarationFilterConstants.GuaranteeAmount, CusBondDetailSchema.PW_BondAmount);
			guaranteeAmountFilter.Decimals = 2;
			guaranteeAmountFilter.SubGroup = guaranteeSubGroup;
			guaranteeAmountFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|GuaranteedAmount", JobDeclarationFilterConstants.GuaranteeAmount);
			guaranteeAmountFilter.Category = guaranteeCategory;

			var guaranteeActivityFilter = filters.AddTextFilter(JobDeclarationFilterConstants.GuaranteeActivity, new GetTextQueryWithOperator((comparisonOperator, value) => GetJobDeclarationQueryOfSchemaColumn(comparisonOperator, value, CusBondDetailSchema.PW_ActivityCode)), Lookups.GuaranteeActivityList)
													.WithMaxLengthOf<ModuleTextFilter>(CusBondDetailSchema.PW_ActivityCode);
			guaranteeActivityFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|GuaranteeActivity", JobDeclarationFilterConstants.GuaranteeActivity);
			guaranteeActivityFilter.Category = guaranteeCategory;
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			guaranteeActivityFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var guaranteeStatusFilter = filters.AddTextFilter(JobDeclarationFilterConstants.GuaranteeStatus, new GetTextQueryWithOperator((comparisonOperator, value) => GetJobDeclarationQueryOfSchemaColumn(comparisonOperator, value, CusBondDetailSchema.PW_Status)), Lookups.GuaranteeStatusList)
													.WithMaxLengthOf<ModuleTextFilter>(CusBondDetailSchema.PW_Status);
			guaranteeStatusFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|GuaranteeStatus", JobDeclarationFilterConstants.GuaranteeStatus);
			guaranteeStatusFilter.Category = guaranteeCategory;
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			guaranteeStatusFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var continuousGuaranteeFilter = filters.AddFlagsFilter(JobDeclarationFilterConstants.ContinuousGuarantee, new string[] { JobDeclarationFilterConstants.Continuous }, new GetFlagsQuery[] { GetContinuousGuaranteeQuery });
			continuousGuaranteeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|ContinuousGuarantee", JobDeclarationFilterConstants.ContinuousGuarantee);
			continuousGuaranteeFilter.Category = guaranteeCategory;

			var linkedGuaranteeFilter = filters.AddGuidFilter(JobDeclarationFilterConstants.LinkedGuarantee, ModuleIDs.Customs.Guarantees, (comparisonOperator, value) => GetLinkedGuaranteeQuery(comparisonOperator, value), Lookups.GuaranteeHeaders);
			linkedGuaranteeFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|DeclarationFilter|LinkedGuarantee", JobDeclarationFilterConstants.LinkedGuarantee);
			linkedGuaranteeFilter.Category = guaranteeCategory;
			linkedGuaranteeFilter.IsPublishedOnWeb = false;
			linkedGuaranteeFilter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsBlank);
			linkedGuaranteeFilter.ComparisonOperator_List.RemoveCode(ModuleGuidFilter.ComparisonConstants.IsNotBlank);
		}

		ZQuery GetJobDeclarationQueryOfSchemaColumn(SQLComparisonOperator comparisonOperator, ZString value, SchemaColumn schemaColumn)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);

			var resultComparisonOperator = comparisonOperator.GetNegatingSQLOperatorIfNotInSubquery();
			if (resultComparisonOperator == SpecialComparisonOperator.IsBlank || resultComparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				guaranteeSubQuery.AddToFilter(schemaColumn, SQLComparisonOperator.NotEqual, string.Empty);
			}
			else
			{
				guaranteeSubQuery.AddToFilter(schemaColumn, resultComparisonOperator, value);
			}

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryInstructionSchema.CEI_JE, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetContinuousGuaranteeQuery(ZBool value)
		{
			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_BondType, SQLComparisonOperator.Equal, MasterFiles.Business.GuaranteeBondTypeList.Codes.Continuous);

			var isNegative = value ? ZBool.False : ZBool.True;
			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryInstructionSchema.CEI_JE, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			return result;
		}

		ZQuery GetLinkedGuaranteeQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var isNegative = comparisonOperator.IsNegativeSQLOperator();
			if (comparisonOperator == SpecialComparisonOperator.IsBlank || comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				isNegative = comparisonOperator == SpecialComparisonOperator.IsBlank;
			}

			var linkedGuaranteeQuery = new ZDBOnlySubQuery(typeof(BaseCusGuaranteeHeader), CusBondDetailSchema.PW_CPH_Guarantee);
			linkedGuaranteeQuery.AddToFilter(CusPermitHeaderSchema.PK, SQLComparisonOperator.Equal, value);

			var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
			guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
			guaranteeSubQuery.AddSubQuery(linkedGuaranteeQuery, JoinCondition.And);

			var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryInstructionSchema.CEI_JE, isNegative);
			entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(JobDeclaration));
			result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);

			return result;
		}

		class GuaranteeSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var guaranteeSubQuery = new ZDBOnlySubQuery(typeof(CusBondDetail), CusBondDetailSchema.PW_ParentID, CusEntryInstructionSchema.PK);
				guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);
				guaranteeSubQuery.AddToFilter(CusBondDetailSchema.PW_ApplicationCode, ZString.Empty);
				guaranteeSubQuery.AddToFilter(filter);

				var entryInstructionSubQuery = new ZDBOnlySubQuery(typeof(Business.CusEntryInstruction), CusEntryInstructionSchema.CEI_JE);
				entryInstructionSubQuery.AddSubQuery(guaranteeSubQuery, JoinCondition.And);

				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				result.AddSubQuery(entryInstructionSubQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		#region Lookups

		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

		#endregion
	}
}
