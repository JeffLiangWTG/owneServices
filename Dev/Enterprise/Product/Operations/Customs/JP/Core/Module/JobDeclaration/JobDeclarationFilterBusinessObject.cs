using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.JP.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject
	{
		public new JobDeclarationFilterLookups Lookups => (JobDeclarationFilterLookups)base.Lookups;

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups() => new JobDeclarationFilterLookups(this);

		protected override void AddModeFilters(ModuleFilterCollection filters)
		{
			base.AddModeFilters(filters);
			AddCusEntryInstructionModeFilters(filters);
			UpdateImporterSupplierFilter(filters);
		}

		void AddCusEntryInstructionModeFilters(ModuleFilterCollection filters)
		{
			var listFilters = new List<ModuleTextFilter>
			{
				new(nameof(NullInstruction.CEI_TradeType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_TradeTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_TradeTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_TradeTypeInfo) },
				new(nameof(NullInstruction.CEI_PreInspectedCargoType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_PreInspectedCargoTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_PreInspectedCargoTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_PreInspectedCargoTypeInfo) },
				new(nameof(NullInstruction.CEI_ApprovalCertificateCategory), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_ApprovalCertificateCategoryInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_ApprovalCertificateCategoryInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_ApprovalCertificateCategoryInfo) },
				new(nameof(NullInstruction.CEI_CommercialValueType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_CommercialValueTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_CommercialValueTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_CommercialValueTypeInfo) },
				new(nameof(NullInstruction.CEI_FoodHygieneCertificateType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_FoodHygieneCertificateTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_FoodHygieneCertificateTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_FoodHygieneCertificateTypeInfo) },
				new(nameof(NullInstruction.CEI_PlantProtectionCertificateType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_PlantProtectionCertificateTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_PlantProtectionCertificateTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_PlantProtectionCertificateTypeInfo) },
				new(nameof(NullInstruction.CEI_AnimalQuarantineCertificateType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_AnimalQuarantineCertificateTypeInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_AnimalQuarantineCertificateTypeInfo)) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_AnimalQuarantineCertificateTypeInfo) },
				new(nameof(NullInstruction.CEI_DeclarationCargoType), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_DeclarationCargoTypeInfo, @operator, value), () => Factory.GetCachedValue<DeclarationCargoTypeList>()) { MultilingualDescription = GetMultilingualString(NullInstruction.CEI_DeclarationCargoTypeInfo) }
			};

			var subGroup = new EntryInstructionSubGroup();
			listFilters.ForEach(filter =>
			{
				filter.Category = FilterCategories.ModesAndTypes;
				filter.SubGroup = subGroup;
				filter.ComparisonOperator_List.Clear();
				filter.ComparisonOperator_List.AddPair(ModuleTextFilter.ComparisonConstants.Exact);
				filters.AddCustomFilter(filter);
			});
		}

		void UpdateImporterSupplierFilter(ModuleFilterCollection filters)
		{
			var moduleGuidsFilter = filters["Importer/Supplier"] as ModuleGuidsFilter;
			moduleGuidsFilter.MultilingualDescription = ResString.GetMultilingualString("Customs|JPDeclarationFilter|ImporterSupplier", "Importer(Consignee)/Shipper(Exporter)");
		}

		protected override void AddAdditionalDateFilters(ModuleFilterCollection filters)
		{
			base.AddAdditionalDateFilters(filters);
			var bondedDateFilter = filters.AddSingleDateFilter(nameof(JobComInvoiceLine.JI_BondedDate), bondedDate => GetBondedDateQuery(bondedDate));
			bondedDateFilter.MultilingualDescription = GetMultilingualString(NullInvoiceLine.JI_BondedDateInfo);
			bondedDateFilter.SubGroup = new InvoiceLineSpecificFieldSubGroup();
		}

		protected override void AddNumberFilters(ModuleFilterCollection filters)
		{
			base.AddNumberFilters(filters);
			var customsNotesFilter = filters.AddTranslatableTextFilter(nameof(CusEntryInstruction.JP_CustomsNotes), (@operator, value) => GetNotesQuery(@operator, value, Constants.StmNoteDescriptions.CustomsNotesDescription), GetMultilingualString(NullInstruction.JP_CustomsNotesInfo));
			var brokerNotesFilter = filters.AddTranslatableTextFilter(nameof(CusEntryInstruction.JP_BrokersNotes), (@operator, value) => GetNotesQuery(@operator, value, Constants.StmNoteDescriptions.BrokersNotesDescription), GetMultilingualString(NullInstruction.JP_BrokersNotesInfo));
			var ownerNotesFilter = filters.AddTranslatableTextFilter(nameof(CusEntryInstruction.JP_OwnersNotes), (@operator, value) => GetNotesQuery(@operator, value, Constants.StmNoteDescriptions.OwnersNotesDescription), GetMultilingualString(NullInstruction.JP_OwnersNotesInfo));
			var marksAndNumbersFilter = filters.AddTranslatableTextFilter(nameof(CusEntryInstruction.JP_MarksAndNumbers), (@operator, value) => GetNotesQuery(@operator, value, Constants.StmNoteDescriptions.MarksAndNumbersDescription), GetMultilingualString(NullInstruction.JP_MarksAndNumbersInfo));
			var contentInspectionResultFilter = filters.AddTextFilter(nameof(CusEntryInstruction.CEI_ContentInspectionResult), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_ContentInspectionResultInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_ContentInspectionResultInfo));
			contentInspectionResultFilter.MultilingualDescription = GetMultilingualString(NullInstruction.CEI_ContentInspectionResultInfo);
			contentInspectionResultFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			var beforePermitApplicationFilter = filters.AddTextFilter(nameof(CusEntryInstruction.CEI_BeforePermitApplicationReason), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_BeforePermitApplicationReasonInfo, @operator, value), () => GetLookupsList(NullInstruction.CEI_BeforePermitApplicationReasonInfo));
			beforePermitApplicationFilter.MultilingualDescription = GetMultilingualString(NullInstruction.CEI_BeforePermitApplicationReasonInfo);
			beforePermitApplicationFilter.RemoveComparisonOperatorsLeavingOne(ModuleTextFilter.ComparisonConstants.Exact);
			var commonControlNumberFilter = filters.AddTranslatableTextFilter(nameof(CusEntryInstruction.CEI_CommonControlNumber), (@operator, value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_CommonControlNumberInfo, @operator, value), GetMultilingualString(NullInstruction.CEI_CommonControlNumberInfo));
			commonControlNumberFilter.MaxLength = NullInstruction.CEI_CommonControlNumberInfo.MaxLength;
			var loadingConfirmationIsRequiredFilter = filters.AddFlagsFilter(nameof(CusEntryInstruction.CEI_LoadingConfirmationIsRequired), new[] { Customs.Business.YesNoList.Descriptions.Yes }, new GetFlagsQuery[] { (value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_LoadingConfirmationIsRequiredInfo, SQLComparisonOperator.Equal, value) });
			loadingConfirmationIsRequiredFilter.MultilingualDescription = GetMultilingualString(NullInstruction.CEI_LoadingConfirmationIsRequiredInfo);
			var dutyDrawBackFilter = filters.AddFlagsFilter(nameof(CusEntryInstruction.CEI_DutyDrawback), new[] { Customs.Business.YesNoList.Descriptions.Yes }, new GetFlagsQuery[] { (value) => GetEntryInstructionAddInfoQuery(NullInstruction.CEI_DutyDrawbackInfo, SQLComparisonOperator.Equal, value) });
			dutyDrawBackFilter.MultilingualDescription = GetMultilingualString(NullInstruction.CEI_DutyDrawbackInfo);

			var subGroup = new EntryInstructionSubGroup();
			new ModuleFilter[]
			{
				customsNotesFilter,
				brokerNotesFilter,
				ownerNotesFilter,
				marksAndNumbersFilter,
				contentInspectionResultFilter,
				beforePermitApplicationFilter,
				commonControlNumberFilter,
				loadingConfirmationIsRequiredFilter,
				dutyDrawBackFilter,
			}.ForEach(filter =>
			{
				filter.Category = FilterCategories.NumbersAndReferences;
				filter.SubGroup = subGroup;
			});
		}

		protected override void AddOrganisationFilters(ModuleFilterCollection filters)
		{
			base.AddOrganisationFilters(filters);
			var inspectionWitnessFilter = filters.AddGuidFilter(nameof(JobDeclaration.InspectionWitness), ModuleIDs.Organisation, (guid) => GetDocAddressQueryJoiningDeclaration(guid, DocAddressType.InspectionWitness), () => new OrgHeaderCollection(Factory));
			inspectionWitnessFilter.MultilingualDescription = GetMultilingualString(ZCustomTypeDescriptor.GetProperties(typeof(JobDeclaration))[nameof(JobDeclaration.InspectionWitness)]);

			inspectionWitnessFilter.Category = FilterCategories.Organisations;
		}

		MultilingualString GetMultilingualString(PropertyDescriptor propertyInfo)
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(propertyInfo, null);
			return ResString._GetMultilingualString(0, resourceStringData.Key, resourceStringData.Caption);
		}

		IList GetLookupsList(ZPropertyInfo propertyInfo)
		{
			return MetaData.GetListDataSource(propertyInfo.BizObj, propertyInfo.PropertyDescriptor) as IList;
		}

		ZQuery GetEntryInstructionAddInfoQuery(PropertyDescriptor propertyInfo, SQLComparisonOperator @operator, IZType value)
		{
			var propertyDBName = (NullInstruction as Customs.Business.IAddInfoManager).AddInfo.GetKey(propertyInfo.Name);
			if (value is ZDateTime dtValue)
			{
				value = dtValue.Date;
				@operator = SQLComparisonOperator.StartsWith;
			}

			if (value is ZBool bValue && bValue == false)
			{
				return new ZQuery(CusEntryInstructionSchema.CEI_AddInfo, SQLComparisonOperator.NotContains, propertyDBName + "=" + ZBool.True.GetStringRepresentation());
			}
			else
			{
				return Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(@operator, value.GetStringRepresentation(), CusEntryInstructionSchema.CEI_AddInfo, propertyDBName);
			}
		}

		ZQuery GetDocAddressQueryJoiningDeclaration(ZGuid orgPK, DocAddressType addressType)
		{
			var result = new ZDBOnlyQuery(typeof(JobDeclaration));

			var docAddressSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), JobDocAddressSchema.E2_OA_Address);

			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, orgPK);
			docAddressSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, DocAddressTypes.GetCode(Factory, addressType));
			docAddressSubQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);

			result.AddSubQuery(docAddressSubQuery, JoinCondition.And);

			return result;
		}

		ZDBOnlyQuery GetNotesQuery(SQLComparisonOperator comparisonOperator, ZString value, string description)
		{
			var query = new ZDBOnlyQuery(typeof(CusEntryInstruction));

			var subQuery = new ZDBOnlySubQuery(typeof(StmNote), StmNoteSchema.ST_ParentID);
			subQuery.AddToFilter(StmNoteSchema.ST_NoteText, comparisonOperator, value);
			subQuery.AddToFilter(StmNoteSchema.ST_Table, CusEntryInstructionSchema.Constants.TableName);
			subQuery.AddToFilter(StmNoteSchema.ST_Description, SQLComparisonOperator.Equal, description);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		ZQuery GetBondedDateQuery(ZDateTime value)
		{
			if (!value.IsEmpty)
			{
				var result = new ZDBOnlyQuery(typeof(JobDeclaration));
				var filterInvoiceLine = new ZDBOnlySubQuery(typeof(JobComInvoiceLine), JobComInvoiceLineSchema.JI_ClusterKey, JobDeclarationSchema.JE_ClusterKey);
				filterInvoiceLine.AddToFilter(Customs.Business.AddInfoFilterRepository.GetAddInfoQuery(SQLComparisonOperator.StartsWith, value.Date.GetStringRepresentation(), JobComInvoiceLineSchema.JI_AddInfo, JPJobComInvoiceLineSchema.JI_BondedDate.Name.Substring(3)));
				result.AddSubQuery(filterInvoiceLine, JoinCondition.And);
				return result;
			}
			else
			{
				return new ZQuery();
			}
		}

		CusEntryInstruction NullInstruction => nullInstruction ??= Factory.GetNull<CusEntryInstruction>();
		CusEntryInstruction nullInstruction;

		JobComInvoiceLine NullInvoiceLine => nullInvoiceLine ??= Factory.GetNull<JobComInvoiceLine>();
		JobComInvoiceLine nullInvoiceLine;
	}
}
