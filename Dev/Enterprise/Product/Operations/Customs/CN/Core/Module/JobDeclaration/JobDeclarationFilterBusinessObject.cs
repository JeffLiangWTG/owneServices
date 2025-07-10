using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.Module
{
	public class JobDeclarationFilterBusinessObject : Customs.Module.JobDeclarationFilterBusinessObject, Integration.Customs.CN.IJobDeclarationFilterBusinessObject
	{
		#region Declaration Filter
		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = base.GetModuleFiltersCore();
			AddJobDeclarationFilter(filters);
			AddNumberFilter(filters);
			AddEntryInstructionFilter(filters);
			AddEntryHeaderFilter(filters);
			return filters;
		}

		protected void AddJobDeclarationFilter(ModuleFilterCollection filters)
		{
			var manufacturerBuyerFilter = filters.AddGuidFilter(DeclarationFilterConstants.ManufacturerBuyer, ModuleIDs.Organisation, GetManufacturerBuyerQuery, Lookups.Manufacturer, Lookups.Buyer);
			manufacturerBuyerFilter.SetItemDescriptions(Res.GetData("60B03477-3EE0-4D98-A9EF-695E8E0417BF", "Manufacturer"), Res.GetData("A7A7E1E3-DD01-4AC3-91FA-836C195280FA", "Buyer"));
			manufacturerBuyerFilter.Category = FilterCategories.Organisations;
			manufacturerBuyerFilter.MultilingualDescription = ResString.GetMultilingualString("1E0BD1F8-3601-4354-BBEB-8D658B67C1FD", DeclarationFilterConstants.ManufacturerBuyer);

			var customsOfficeFilter = filters.AddNkFilter(DeclarationFilterConstants.CustomsOffice, JobDeclarationSchema.JE_CustomsOffice, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOfficeList).WithMaxLengthOf<ModuleNkFilter>(JobDeclarationSchema.JE_CustomsOffice);
			customsOfficeFilter.Category = FilterCategories.TextSearch;
			customsOfficeFilter.MultilingualDescription = ResString.GetMultilingualString("F492FB71-4D6E-4418-B417-6D75C8BAFC49", DeclarationFilterConstants.CustomsOffice);

			var officeOfEntryExitFilter = filters.AddNkFilter(DeclarationFilterConstants.OfficeOfEntryExit, GetOfficeOfEntryExitFilter, ModuleIDs.Customs.Universal.ZZRefCusCodeList, Lookups.CustomsOfficeList)
				.WithMaxLengthOf<ModuleNkFilter>(CNJobDeclarationSchema.JE_OfficeOfEntryExit);
			officeOfEntryExitFilter.Category = FilterCategories.TextSearch;
			officeOfEntryExitFilter.MultilingualDescription = ResString.GetMultilingualString("C625E2F0-0FC4-403B-B200-6B7D18DBC146", DeclarationFilterConstants.OfficeOfEntryExit);

			var totalWeightFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.TotalWeight, JobDeclarationSchema.JE_TotalWeight);
			totalWeightFilter.Decimals = JobDeclarationSchema.JE_TotalWeight.Scale;
			totalWeightFilter.Category = FilterCategories.Other;
			totalWeightFilter.MultilingualDescription = ResString.GetMultilingualString("059547F6-89BE-4714-842D-5C2009A903B3", DeclarationFilterConstants.TotalWeight);

			var noPackagesFilter = filters.AddNumberRangeFilter(DeclarationFilterConstants.NoPackages, JobDeclarationSchema.JE_TotalNoOfPacks);
			noPackagesFilter.Category = FilterCategories.Other;
			noPackagesFilter.MultilingualDescription = ResString.GetMultilingualString("21F564F5-D460-4E5F-B2B9-9947B3A5C9DF", DeclarationFilterConstants.NoPackages);
		}

		ZQuery GetOfficeOfEntryExitFilter(ZString value)
		{
			return ModelViewColumnHelper.GetModuleFilterQuery(JobDeclaration.Schema.PK, ModelViewPK, ModelView, CNJobDeclarationSchema.Constants.JE_OfficeOfEntryExit, SQLComparisonOperator.Equal, value);
		}

		protected ZQuery GetManufacturerBuyerQuery(ZGuid manufacturer, ZGuid buyer)
		{
			ZQuery result = new ZQuery();
			if (!manufacturer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Manufacturer, manufacturer);
			}

			if (!buyer.IsEmpty)
			{
				result.AddToFilter(JobDeclarationSchema.JE_OH_Buyer, buyer);
			}

			return result;
		}

		protected override void AddShipmentSubTypeFilter(ModuleFilterCollection filters)
		{
			ModuleFilter shipSubTypeFilter = filters.AddTextFilter(DeclarationFilterConstants.DeclarationType, JobDeclarationSchema.JE_MessageSubType, Lookups.MessageSubTypeList).WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_MessageSubType);
			shipSubTypeFilter.Category = FilterCategories.ModesAndTypes;
			shipSubTypeFilter.MultilingualDescription =
				ResString.GetMultilingualString("D22106A1-619A-4840-A459-113726962A4F",
					DeclarationFilterConstants.DeclarationType);
		}
		#endregion

		#region Entry Instruction Filter

		protected void AddEntryInstructionFilter(ModuleFilterCollection filters)
		{
			var entryInstructionCPCFilter = filters.AddTextFilter(DeclarationFilterConstants.EntryInstructionFilterTypes.CPC, GetEntryInstructionCPCQuery, Lookups.EntryInstructionCPCList).WithMaxLengthOf<ModuleTextFilter>(CusEntryInstructionSchema.CEI_Style);
			entryInstructionCPCFilter.Category = FilterCategories.TextSearch;
			entryInstructionCPCFilter.MultilingualDescription =
				ResString.GetMultilingualString("D1E645E1-5521-4C78-97E0-8E20D978A421",
					DeclarationFilterConstants.EntryInstructionFilterTypes.CPC);

			var entryInstructionBLNoFilter = filters.AddTextFilter(DeclarationFilterConstants.EntryInstructionFilterTypes.BLNo, GetEntryInstructionBLNoQuery).WithMaxLengthOf<ModuleTextFilter>(CusEntryNumSchema.CE_EntryNum);
			entryInstructionBLNoFilter.Category = FilterCategories.NumbersAndReferences;
			entryInstructionBLNoFilter.MultilingualDescription =
				ResString.GetMultilingualString("DD1A535B-35AA-40DE-B8CF-7E83CC592CC5",
					DeclarationFilterConstants.EntryInstructionFilterTypes.BLNo);

			var entryInstructionRelatedEntryNoFilster = new AddInfoModuleTextFilter(DeclarationFilterConstants.EntryInstructionFilterTypes.RelatedEntryNo, GetEntryInstructionRelatedEntryNoQuery);
			entryInstructionRelatedEntryNoFilster.Category = FilterCategories.NumbersAndReferences;
			filters.AddFilter(entryInstructionRelatedEntryNoFilster);
			entryInstructionRelatedEntryNoFilster.MultilingualDescription =
				ResString.GetMultilingualString("E4D7B438-87BD-4142-A38E-A8EADAA1EC42",
					DeclarationFilterConstants.EntryInstructionFilterTypes.RelatedEntryNo);
		}

		ZQuery GetEntryInstructionCPCQuery(ZString value)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entryInstructionQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryInstruction), CusEntryInstructionSchema.PK);
			entryInstructionQuery.AddToFilter(CusEntryInstructionSchema.CEI_Style, value);

			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE, entryInstructionQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		ZQuery GetEntryInstructionBLNoQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.China.BillOfLading);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, filterOperator, value);

			var entryInstructionQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryInstruction), CusEntryInstructionSchema.PK);
			entryInstructionQuery.AddSubQuery(cusEntryNumQuery, JoinCondition.And);

			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE, entryInstructionQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		ZQuery GetEntryInstructionRelatedEntryNoQuery(SQLComparisonOperator filterOperator, ZString value)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));

			var entryInstructionQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryInstruction), CusEntryInstructionSchema.PK);
			entryInstructionQuery.AddToFilter(AddInfoFilterRepository.GetAddInfoQuery(filterOperator, value, CusEntryInstructionSchema.CEI_AddInfo, AutoCNCusEntryInstruction.Schema.CEI_RelatedMRN.Substring(4)));

			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.PK, CusEntryInstructionSchema.CEI_JE, entryInstructionQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}
		#endregion

		#region Number Filter
		protected void AddNumberFilter(ModuleFilterCollection filters)
		{
			var pslNumberFilter = filters.AddTextFilter(DeclarationFilterConstants.CusEntryNumberFilterTypes.PSLNumber, (SQLComparisonOperator filterOperator, ZString value) => { return GetEntryNumberQuery(filterOperator, value, AdditionalReferenceNumberTypes.Codes.ProposalNo); });
			pslNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			pslNumberFilter.Category = FilterCategories.NumbersAndReferences;
			pslNumberFilter.MultilingualDescription = ResString.GetMultilingualString("B4E75281-44CF-4CE4-A681-67F3D6AC0B15", DeclarationFilterConstants.CusEntryNumberFilterTypes.PSLNumber);

			var wcqNumberFilter = filters.AddTextFilter(DeclarationFilterConstants.CusEntryNumberFilterTypes.WGQNumber, (SQLComparisonOperator filterOperator, ZString value) => { return GetEntryNumberQuery(filterOperator, value, AdditionalReferenceNumberTypes.Codes.WGQWarehouseNumber); });
			wcqNumberFilter.MaxLength = CusEntryNumSchema.CE_EntryNum.MaxLength;
			wcqNumberFilter.Category = FilterCategories.NumbersAndReferences;
			wcqNumberFilter.MultilingualDescription = ResString.GetMultilingualString("C94DD7D1-B6C7-4EF3-8F90-A2F801F38405", DeclarationFilterConstants.CusEntryNumberFilterTypes.WGQNumber);
		}

		ZQuery GetEntryNumberQuery(SQLComparisonOperator filterOperator, ZString value, ZString entryType)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryNumQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.PK);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
			cusEntryNumQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, filterOperator, value);
			jobDeclarationQuery.AddSubQuery(JobDeclarationSchema.PK, CusEntryNumSchema.CE_ParentID, cusEntryNumQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		#endregion

		#region EntryHeader Filter

		protected void AddEntryHeaderFilter(ModuleFilterCollection filters)
		{
			var readyForCompleteStatusfilter = filters.AddTextFilter(EntryHeaderFilterConstants.ReadyForCompleteDeclaration, GetReadyForCompleteDeclarationQuery, () => ReadyForCompleteDeclarationFilterHelper.OptionsList);
			readyForCompleteStatusfilter.Category = FilterCategories.StatusAndFlags;
			readyForCompleteStatusfilter.MultilingualDescription = ResString.GetMultilingualString("F8EFA2A9-4D34-49C0-B3CE-13CC59E083A9", EntryHeaderFilterConstants.ReadyForCompleteDeclaration);
		}

		ZQuery GetReadyForCompleteDeclarationQuery(ZString status)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var cusEntryHeaderQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryHeader), CusEntryHeaderSchema.CH_JE);

			status = status.Trim().ToUpper();
			if (status == ReadyForCompleteDeclarationFilterHelper.Options.Ready.ToUpper())
			{
				cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.Equal, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration);
			}
			else if (status == ReadyForCompleteDeclarationFilterHelper.Options.NotReady.ToUpper())
			{
				cusEntryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_Status, SQLComparisonOperator.NotEqual, JobMessageStatusList.Codes.ClearedPreliminaryDeclaration);
			}

			jobDeclarationQuery.AddSubQuery(cusEntryHeaderQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		#endregion

		#region Model View

		ModelViewColumnQueryHelper<JobDeclaration> ModelViewColumnHelper => modelViewColumnHelper ??= new();
		ModelViewColumnQueryHelper<JobDeclaration> modelViewColumnHelper;

		string ModelView => CNJobDeclarationSchema.Constants.TableName;
		string ModelViewPK => CNJobDeclarationSchema.Constants.PK;

		#endregion

		#region Lookups

		public new JobDeclarationFilterLookups Lookups
		{
			get { return (JobDeclarationFilterLookups)base.Lookups; }
		}

		protected override Customs.Module.JobDeclarationFilterLookups GetNewLookups()
		{
			return new JobDeclarationFilterLookups(this);
		}

		#endregion
	}
}
