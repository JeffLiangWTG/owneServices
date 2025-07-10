using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.ExitControl.Module
{
	public class ExitControlReportFilterBusinessObject : FilterStripBusinessObject
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		public static class FilterConstants
		{
			public const string Type = "Report Type";
			public const string EntryConsignment = "Entry/Consignment";
			public const string Status = "Status";
			public const string MessageStatus = "Message Status";
			public const string OfficeOfExit = "Office Of Exit";
			public const string JobNumber = "Job Number";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var moduleFilterCollection = new ModuleFilterCollection();
			AddReportTypeFilter(moduleFilterCollection);
			AddJobNumberFilter(moduleFilterCollection);
			AddExitConsignmentFilter(moduleFilterCollection);
			AddStatusFilter(moduleFilterCollection);
			AddMessageStatusFilter(moduleFilterCollection);
			AddOfficeOfExitFilter(moduleFilterCollection);

			AddBranchFilter(moduleFilterCollection);
			AddCarrierFilter(moduleFilterCollection);
			AddExporterFilter(moduleFilterCollection);

			return moduleFilterCollection;
		}

		void AddReportTypeFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var reportTypeFilter = moduleFilterCollection.AddTextFilter(FilterConstants.Type, CusExitReportSchema.CER_Type, ExitReportLookups.TypeList);
			reportTypeFilter.Category = FilterCategories.ModesAndTypes;
			reportTypeFilter.WithMaxLengthOf<ModuleTextFilter>(CusExitReportSchema.CER_Type);
			reportTypeFilter.MultilingualDescription = ResString.GetMultilingualString("E37F48A7-924E-4D95-8E2A-E2D2D66B93E1", FilterConstants.Type);
			reportTypeFilter.RemoveComparisonOperatorsLeavingOne(ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);
			reportTypeFilter.SupportsBlankComparisonOperators = false;
		}

		void AddJobNumberFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var jobNumberFilter = moduleFilterCollection.AddTextFilter(FilterConstants.JobNumber, FilterBusinessObjectHelper.GetJobNumberQuery);
			jobNumberFilter.Category = FilterCategories.NumbersAndReferences;
			jobNumberFilter.MultilingualDescription = ResString.GetMultilingualString("40EE284C-E94C-442C-B69D-C485D488D79C", FilterConstants.JobNumber);
			jobNumberFilter.MaxLength = CusExitHeaderSchema.CXH_JobReference.MaxLength;
			jobNumberFilter.SupportsBlankComparisonOperators = false;
			jobNumberFilter.SubGroup = ExitHeaderGroup;
		}

		void AddBranchFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var branchFilter = moduleFilterCollection.AddGuidFilter(ExitControlFilterBusinessObject.FilterConstants.Branch, ModuleIDs.GlbBranch, CusExitHeaderSchema.CXH_GB_Branch, new GlbBranchCollection(Factory));
			branchFilter.Category = FilterCategories.Organisations;
			branchFilter.MultilingualDescription = ResString.GetMultilingualString("067B0CA3-C2A5-438C-BDCF-478B18C3DC9F", ExitControlFilterBusinessObject.FilterConstants.Branch);
			branchFilter.Property = GlbBranch.CurrentBranch.PK;
			branchFilter.SubGroup = ExitHeaderGroup;
		}

		void AddCarrierFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var carrierFilter = moduleFilterCollection.AddGuidFilter(ExitControlFilterBusinessObject.FilterConstants.Carrier, ModuleIDs.Organisation, GetExitHeaderCarrierOrgQuery, new OrganisationsFindBoxCollection(Factory));
			carrierFilter.MultilingualDescription = ResString.GetMultilingualString("B210A178-7854-460D-B22A-C0199D23302B", ExitControlFilterBusinessObject.FilterConstants.Carrier);
			carrierFilter.Category = FilterCategories.Organisations;

			carrierFilter.SubGroup = ExitHeaderGroup;
		}

		void AddExporterFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var exporterFilter = moduleFilterCollection.AddGuidFilter(ExitControlFilterBusinessObject.FilterConstants.Exporter, ModuleIDs.Organisation, GetExitHeaderExporterQuery, new OrganisationsFindBoxCollection(Factory));
			exporterFilter.Category = FilterCategories.Organisations;
			exporterFilter.MultilingualDescription = ResString.GetMultilingualString("1A25DB2D-FB19-480A-8730-E81B3E021762", ExitControlFilterBusinessObject.FilterConstants.Exporter);
			exporterFilter.SubGroup = ExitHeaderGroup;
		}

		void AddExitConsignmentFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var consignmentFilter = moduleFilterCollection.AddTextFilter(FilterConstants.EntryConsignment, CusExitConsignmentSchema.CXC_MovementReference);
			consignmentFilter.MultilingualDescription = ResString.GetMultilingualString("5CC24C65-1A29-4495-875B-73FACAE925B4", FilterConstants.EntryConsignment);
			consignmentFilter.Category = FilterCategories.NumbersAndReferences;
			consignmentFilter.SubGroup = ExitConsignmentSubGroup;
		}

		void AddStatusFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var statusFilter = AddNewStatusFilter(moduleFilterCollection);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("3A2CDAD9-0CD8-4BB4-A2DC-2B618B053DA9", FilterConstants.Status);
			statusFilter.Category = FilterCategories.StatusAndFlags;
		}

		protected virtual ModuleFilter AddNewStatusFilter(ModuleFilterCollection moduleFilterCollection)
		{
			return moduleFilterCollection.AddNkFilter(FilterConstants.Status, CusExitReportSchema.CER_Status, ModuleIDs.Customs.Universal.ZZRefCusCodeList, GetStatusList());
		}

		IBusinessObjectCollection GetStatusList() => new ZZRefCusCodeListCombinedCollection(
			Factory,
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus,
			ZDateTime.Today
		);

		void AddMessageStatusFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var statusFilter = moduleFilterCollection.AddTextFilter(FilterConstants.MessageStatus, CusExitReportSchema.CER_MessageStatus, MessageStatusList);
			statusFilter.MultilingualDescription = ResString.GetMultilingualString("5521D2ED-791E-4961-A3EB-1D8714FB62D4", FilterConstants.MessageStatus);
			statusFilter.Category = FilterCategories.StatusAndFlags;
			ClearupStatusList(statusFilter);
		}

		protected virtual IList MessageStatusList => Factory.GetCachedValue<LogicalStatusList>();

		protected void ClearupStatusList(ModuleTextFilter targetFilter)
		{
			targetFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.StartsWith);
			targetFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotStartsWith);
			targetFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.Contains);
			targetFilter.ComparisonOperator_List.RemoveCode(ModuleTextFilter.ComparisonConstants.NotContain);
		}

		void AddOfficeOfExitFilter(ModuleFilterCollection moduleFilterCollection)
		{
			var officeOfExitFilter = moduleFilterCollection.AddNkFilter(FilterConstants.OfficeOfExit, CusExitReportSchema.CER_OfficeOfExit, ModuleIDs.Customs.Universal.ZZRefCusCodeList, ExitReportLookups.OfficeOfExitList)
				.WithMaxLengthOf<ModuleNkFilter>(CusExitReportSchema.CER_OfficeOfExit);
			officeOfExitFilter.MultilingualDescription = ResString.GetMultilingualString("93B01C4B-083F-416F-82ED-1CED1A09F384", FilterConstants.OfficeOfExit);
			officeOfExitFilter.Category = FilterCategories.Locations;
		}

		#region Sub Groups

		ExitHeaderFilterSubGroup exitHeaderGroup;
		ExitHeaderFilterSubGroup ExitHeaderGroup => exitHeaderGroup ?? (exitHeaderGroup = new ExitHeaderFilterSubGroup());

		class ExitHeaderFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery exitHeaderQuery)
			{
				var exitReportQuery = new ZDBOnlyQuery(typeof(CusExitReport));

				var exitHeaderQueryToAdd = new ZDBOnlySubQuery(typeof(CusExitHeader), CusExitReportSchema.CER_CXH_Header);
				exitHeaderQueryToAdd.AddToFilter(exitHeaderQuery);

				exitReportQuery.AddSubQuery(exitHeaderQueryToAdd, JoinCondition.And);
				return exitReportQuery;
			}
		}

		protected ModuleFilterSubGroup ExitConsignmentSubGroup => exitConsignmentSubGroup ?? (exitConsignmentSubGroup = new ExitConsignmentFilterSubGroup());
		ModuleFilterSubGroup exitConsignmentSubGroup;

		class ExitConsignmentFilterSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var result = new ZDBOnlyQuery(typeof(CusExitReport));
				ZDBOnlySubQuery exitConsignmentQuery = new ZDBOnlySubQuery(typeof(CusExitConsignment), CusExitReportSchema.CER_CXC_Consignment);
				exitConsignmentQuery.AddToFilter(filter);
				result.AddSubQuery(exitConsignmentQuery, JoinCondition.And);
				return result;
			}
		}

		#endregion

		static ZQuery GetExitHeaderCarrierOrgQuery(ZGuid carrierPk)
		{
			var orgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgAddress), CusExitHeaderSchema.CXH_OA_Carrier);
			orgAddressSubQuery.AddToFilter(OrgAddressSchema.OA_OH, carrierPk);

			var exitHeaderQuery = new ZDBOnlyQuery(typeof(CusExitHeader));
			exitHeaderQuery.AddSubQuery(orgAddressSubQuery, JoinCondition.And);
			return exitHeaderQuery;
		}

		static ZQuery GetExitHeaderExporterQuery(ZGuid exporterPk)
		{
			return new ZQuery(CusExitHeaderSchema.CXH_OH_Exporter, exporterPk);
		}

		CusExitReportLookups ExitReportLookups => exitReportLookups ?? (exitReportLookups = Factory.GetNull<CusExitReport>().Lookups);
		CusExitReportLookups exitReportLookups;
	}
}
