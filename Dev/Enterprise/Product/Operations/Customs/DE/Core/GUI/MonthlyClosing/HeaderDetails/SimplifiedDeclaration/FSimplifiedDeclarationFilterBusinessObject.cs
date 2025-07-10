using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using CusReconDeclaration = Enterprise.Customs.DE.Business.CusReconDeclaration;
using CusReconEntry = Enterprise.Customs.DE.Business.CusReconEntry;
using CusReconEntryLine = Enterprise.Customs.DE.Business.CusReconEntryLine;

namespace Enterprise.Customs.DE.GUI
{
	public class FSimplifiedDeclarationFilterBusinessObject : FilterStripBusinessObject
	{
		public static class Schema
		{
			public const string EntryDate = "Local Clearance Date";
			public const string OriginalEntryNumber = "Registration Number";
			public const string DeclarationReference = "Job Number";
			public const string OwnerRef = "Owner Reference";
			public const string LineNumber = "Line Number";
			public const string CustomsStatus = "Customs Status";
		}

		public FSimplifiedDeclarationFilterBusinessObject()
		{
			ResetLineOnlyQuery();
		}

		public FSimplifiedDeclarationFilterBusinessObject(CusReconDeclaration declaration)
			: this()
		{
			this.declaration = declaration;

			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.Customs.EU.DE.SimplifiedDeclaration.Name;
		}

		public CusReconDeclaration declaration;

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			var jobDeclarationSubGroup = new JobDeclarationSubGroup();

			var entryDateDateFilter = result.AddDateFilter(Schema.EntryDate, CusReconEntrySchema.CRE_EntryDate);
			entryDateDateFilter.Category = FilterCategories.Dates;
			entryDateDateFilter.MultilingualDescription = ResString.GetMultilingualString("60A40E63-0C1E-48E9-B33C-9458F0DE65A7", Schema.EntryDate);
			var originalEntryNumberTextFilter = result.AddTextFilter(Schema.OriginalEntryNumber, CusReconEntrySchema.CRE_OriginalEntryNumber);
			originalEntryNumberTextFilter.Category = FilterCategories.NumbersAndReferences;
			originalEntryNumberTextFilter.MultilingualDescription = ResString.GetMultilingualString("0373CEFD-CA2B-4C1E-AA0E-3D7D9C7EB97D", Schema.OriginalEntryNumber);
			var declarationReferenceTextFilter = result.AddTextFilter(Schema.DeclarationReference, JobDeclarationSchema.JE_DeclarationReference);
			declarationReferenceTextFilter.Category = FilterCategories.NumbersAndReferences;
			declarationReferenceTextFilter.MultilingualDescription = ResString.GetMultilingualString("D60509AC-ED15-4FC0-AF45-1DA7BF803671", Schema.DeclarationReference);
			declarationReferenceTextFilter.SubGroup = jobDeclarationSubGroup;
			var ownerRefTextFilter = result.AddTextFilter(Schema.OwnerRef, JobDeclarationSchema.JE_OwnerRef);
			ownerRefTextFilter.Category = FilterCategories.StatusAndFlags;
			ownerRefTextFilter.MultilingualDescription = ResString.GetMultilingualString("C1B6835B-D5EF-49F3-A384-4D14B113033F", Schema.OwnerRef);
			ownerRefTextFilter.SubGroup = jobDeclarationSubGroup;

			AddLineFilters(result);
			return result;
		}

		void AddLineFilters(ModuleFilterCollection filters)
		{
			var entryLineFilterSubGroup = new EntryLineFilterSubGroup(this);

			var lineNumberFilter = filters.AddNumberFilter(
				Schema.LineNumber,
				(comparisonOperator, value) => GetEntryLineNumberQuery(CusReconEntryLineSchema.CRL_LineNumber, comparisonOperator, value));
			lineNumberFilter.MaxLength = 2;
			lineNumberFilter.Category = FilterCategories.NumbersAndReferences;
			lineNumberFilter.MultilingualDescription = ResString.GetMultilingualString("386D03F9-DBC4-4DB7-861D-E9F7F700AAB5", Schema.LineNumber);
			lineNumberFilter.SubGroup = entryLineFilterSubGroup;
			lineNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			lineNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			lineNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			lineNumberFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);

			var lineCustomsStatusFilter = filters.AddTextFilter(Schema.CustomsStatus, CusReconEntryLineSchema.CRL_CustomsStatus, ReconDeclarationLookups.CustomsStatusList)
				.WithMaxLengthOf<ModuleTextFilter>(CusReconEntryLineSchema.CRL_CustomsStatus);
			lineCustomsStatusFilter.Category = FilterCategories.StatusAndFlags;
			lineCustomsStatusFilter.MultilingualDescription = ResString.GetMultilingualString("0A07E3E7-4887-40FE-A115-C052861D447E", Schema.CustomsStatus);
			lineCustomsStatusFilter.SupportsBlankComparisonOperators = false;
			lineCustomsStatusFilter.SubGroup = entryLineFilterSubGroup;
		}

		ZQuery GetEntryLineNumberQuery(SchemaShortColumn column, SQLComparisonOperator comparisonOperator, ZString value) => new (column, comparisonOperator, ZShort.ParseSafe(value, ZShort.Zero));

		public void ResetLineOnlyQuery()
		{
			LineOnlyQuery = new ZDBOnlyQuery(typeof(CusReconEntryLine));
		}

		public ZQuery LineOnlyQuery;

		Business.CusReconDeclarationLookups ReconDeclarationLookups => reconDeclarationLookups ?? (reconDeclarationLookups = Factory.GetNull<CusReconDeclaration>().Lookups);
		Business.CusReconDeclarationLookups reconDeclarationLookups;

		class EntryLineFilterSubGroup : ModuleFilterSubGroup
		{
			public EntryLineFilterSubGroup(FSimplifiedDeclarationFilterBusinessObject filterBizo)
			{
				this.filterBizo = filterBizo;
			}
			readonly FSimplifiedDeclarationFilterBusinessObject filterBizo;

			public override ZQuery GetSubQuery(ZQuery filter)
			{
				filterBizo.LineOnlyQuery = filter;

				var lineQuery = new ZDBOnlySubQuery(typeof(CusReconEntryLine), CusReconEntryLineSchema.CRL_CRE);
				lineQuery.AddToFilter(filter);
				var headerQuery = new ZDBOnlyQuery(typeof(CusReconEntry));
				headerQuery.AddSubQuery(lineQuery, JoinCondition.And);
				return headerQuery;
			}
		}

		class JobDeclarationSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var query = new ZDBOnlyQuery(typeof(CusReconEntry));
				var entryHeaderQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusReconEntrySchema.CRE_CH_OriginalEntry);
				var jobDeclarationQuery = new ZDBOnlySubQuery(typeof(BaseJobDeclaration), CusEntryHeaderSchema.CH_JE);
				jobDeclarationQuery.AddToFilter(filter);
				entryHeaderQuery.AddSubQuery(jobDeclarationQuery, JoinCondition.And);
				query.AddSubQuery(entryHeaderQuery, JoinCondition.And);
				return query;
			}
		}
	}
}
