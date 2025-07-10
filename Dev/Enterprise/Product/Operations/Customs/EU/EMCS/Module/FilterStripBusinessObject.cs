using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.EMCS.Module
{
	public class FilterStripBusinessObject : JobDeclarationFilterBusinessObject, Integration.Customs.EU.IJobDeclarationFilterBusinessObject
	{
		public static class Schema
		{
			// Numbers
			public const string EntryNumber = "EAD Number";
			public const string LocalReferenceNumber = "Local Reference Number";

			// Status and Flags
			public const string DeferredSubmission = "Deferred Submission";
			public const string RegistrationStatus = "Registration Status";

			// Dates
			public const string DispatchDate = "Dispatch Date";
			public const string InvoiceDate = "Invoice Date";
			public const string ReportDate = "Report Date";

			// Organisations / Staff
			public const string CarrierAgent = "Carrier Agent";
			public const string Consignee = "Consignee";
			public const string Consignor = "Consignor";

			public const string GoodsOwner = "Goods Owner";

			public const string Transporter = "Transporter";
			public const string DispatchWarehouse = "Dispatch Warehouse";
			public const string DestinationWarehouse = "Destination Warehouse";

			// Modes and Types
			public const string DestinationType = "Destination Type";
			public const string DeclarantType = "Declaration Type";
			public const string GuarantorType = "Guarantor Type";
			public const string OriginType = "Origin Type";
			public const string TransportArrangement = "Transport Arrangement";
			public const string TransportMode = "Transport Mode";
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var filters = new ModuleFilterCollection();

			// -- Numbers --
			AddInvoiceNumberFilter(filters);

			var entryNumberFilter = filters.AddNumberFilter(Schema.EntryNumber, CusEntryNumSchema.CE_EntryNum);
			entryNumberFilter.SubGroup = new EntryNumberSubGroup();
			entryNumberFilter.MultilingualDescription = ResString.GetMultilingualString("CFABF8B8-D762-4C4D-BD40-4BB6980558B4", "EAD Number");

			var localReferenceNumberFilter = filters.AddTextFilter(Schema.LocalReferenceNumber, JobDeclarationSchema.JE_OwnerRef);
			localReferenceNumberFilter.Category = FilterCategories.NumbersAndReferences;
			localReferenceNumberFilter.MultilingualDescription = ResString.GetMultilingualString("F071345C-4A2F-46E9-ACDA-C09BC9F8E455", Schema.LocalReferenceNumber);
			// ***  DeclarationReference is inherited through GetModuleFilterThatOverridesAllOtherFilters()  ***

			// -- Status --
			base.AddMessageStatusFilter(filters);

			var registrationStatusFilter = filters.AddTextFilter(Schema.RegistrationStatus, GetEntryStatusQuery, Lookups.EntryStatusList);
			registrationStatusFilter.WithMaxLengthOf<ModuleTextFilter>(JobDeclarationSchema.JE_EntryStatus);
			registrationStatusFilter.Category = FilterCategories.StatusAndFlags;
			registrationStatusFilter.MultilingualDescription = ResString.GetMultilingualString("7ABA9020-D799-435E-99F7-E9ED1411C317", Schema.RegistrationStatus);

			var deferredSubmissionFilter = filters.AddTextFilter(Schema.DeferredSubmission, DeferredSubmissionQuery, Lookups.DeferredStatus);
			deferredSubmissionFilter.DefaultProperty = EMCSDeferredSubmissionList.Codes.No;
			deferredSubmissionFilter.Category = FilterCategories.StatusAndFlags;
			deferredSubmissionFilter.MultilingualDescription = ResString.GetMultilingualString("550A55A0-B976-49D8-B5F1-8F6CE8F59BF0", Schema.DeferredSubmission);

			// -- Dates --
			var dispatchDateFilter = filters.AddDateFilter(Schema.DispatchDate, JobDeclarationSchema.JE_DateAtOrigin);
			dispatchDateFilter.MultilingualDescription = ResString.GetMultilingualString("57A6C8CB-8A88-4E06-9ED2-A42FE8DAB394", Schema.DispatchDate);

			var invoiceDateFilter = filters.AddDateFilter(Schema.InvoiceDate, GetInvoiceDateQuery);
			invoiceDateFilter.MultilingualDescription = ResString.GetMultilingualString("9E3C0E96-AA8A-449D-9501-9101373F9852", Schema.InvoiceDate);

			var reportDateFilter = filters.AddDateFilter(Schema.ReportDate, JobDeclarationSchema.JE_EntryAuthorisationDate);
			reportDateFilter.MultilingualDescription = ResString.GetMultilingualString("73B6AEC0-A231-47F6-B423-56BEB008A494", Schema.ReportDate);

			// -- Modes --
			var transportModeFilter = filters.AddTextFilter(Schema.TransportMode, TransportModeQuery, Lookups.TransportModes);
			transportModeFilter.Category = FilterCategories.ModesAndTypes;
			transportModeFilter.MultilingualDescription = ResString.GetMultilingualString("9D43B7BA-6F2E-4E71-8FAA-6C9869A45713", Schema.TransportMode);

			var declarantTypeFilter = filters.AddTextFilter(Schema.DeclarantType, DeclarantTypeQuery, Lookups.DeclarantTypes);
			declarantTypeFilter.Category = FilterCategories.ModesAndTypes;
			declarantTypeFilter.MultilingualDescription = ResString.GetMultilingualString("2D80015D-A1BB-4332-9FDC-9140EFA38ADA", Schema.DeclarantType);

			var destinationTypeFilter = filters.AddTextFilter(Schema.DestinationType, DestinationTypeQuery, Lookups.DestinationTypes);
			destinationTypeFilter.Category = FilterCategories.ModesAndTypes;
			destinationTypeFilter.MultilingualDescription = ResString.GetMultilingualString("E5E9BA5B-A19C-4888-89DF-0926638C0541", Schema.DestinationType);

			var guarantorTypeFilter = filters.AddTextFilter(Schema.GuarantorType, GuarantorTypeQuery, Lookups.GuarantorTypes);
			guarantorTypeFilter.Category = FilterCategories.ModesAndTypes;
			guarantorTypeFilter.MultilingualDescription = ResString.GetMultilingualString("7AE68B2A-EB9F-473F-992B-EAE4AFC75D7D", Schema.GuarantorType);

			var originTypeFilter = filters.AddTextFilter(Schema.OriginType, OriginTypeQuery, Lookups.OriginTypes);
			originTypeFilter.Category = FilterCategories.ModesAndTypes;
			originTypeFilter.MultilingualDescription = ResString.GetMultilingualString("B4E12AFF-8FFA-4350-AD44-D8A07204682D", Schema.OriginType);

			var transportArrangementFilter = filters.AddTextFilter(Schema.TransportArrangement, TransportArrangementQuery, Lookups.TransportArrangements);
			transportArrangementFilter.Category = FilterCategories.ModesAndTypes;
			transportArrangementFilter.MultilingualDescription = ResString.GetMultilingualString("3E74740B-7D13-4B7B-8D3B-DAECD61A0BD8", Schema.TransportArrangement);

			// -- Orgs --
			AddBranchAndBrokerFilters(filters);

			var consignorFilter = filters.AddGuidFilter(Schema.Consignor, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_Supplier, Lookups.Consignors);
			consignorFilter.Category = FilterCategories.Organisations;
			consignorFilter.MultilingualDescription = ResString.GetMultilingualString("96DB4D52-07C4-4CF0-AF1F-B450D458D54C", Schema.Consignor);

			var consigneeFilter = filters.AddGuidFilter(Schema.Consignee, ModuleIDs.Organisation, JobDeclarationSchema.JE_OH_Importer, Lookups.Consignees);
			consigneeFilter.Category = FilterCategories.Organisations;
			consigneeFilter.MultilingualDescription = ResString.GetMultilingualString("FC905E6B-E961-4601-AAE3-DDAC6E343D24", Schema.Consignee);

			var carrierAgentFilter = filters.AddGuidFilter(Schema.CarrierAgent, ModuleIDs.Organisation, CarrierAgentFilter, Lookups.AllOrganisations);
			carrierAgentFilter.Category = FilterCategories.Organisations;
			carrierAgentFilter.MultilingualDescription = ResString.GetMultilingualString("4526760A-4831-40C7-B517-FD7B448C8487", Schema.CarrierAgent);

			var destinationWarehouseFilter = filters.AddGuidFilter(Schema.DestinationWarehouse, ModuleIDs.Organisation, DestinationWarehouseFilter, Lookups.AllOrganisations);
			destinationWarehouseFilter.Category = FilterCategories.Organisations;
			destinationWarehouseFilter.MultilingualDescription = ResString.GetMultilingualString("1111E450-F11B-45A0-A49B-3D29851FF4F2", Schema.DestinationWarehouse);

			var dispatchWarehouseFilter = filters.AddGuidFilter(Schema.DispatchWarehouse, ModuleIDs.Organisation, DispatchWarehouseFilter, Lookups.AllOrganisations);
			dispatchWarehouseFilter.Category = FilterCategories.Organisations;
			dispatchWarehouseFilter.MultilingualDescription = ResString.GetMultilingualString("9DD0F68C-F560-4041-A147-AE325DC5C020", Schema.DispatchWarehouse);

			var goodsOwnerFilter = filters.AddGuidFilter(Schema.GoodsOwner, ModuleIDs.Organisation, GoodsOwnerFilter, Lookups.AllOrganisations);
			goodsOwnerFilter.Category = FilterCategories.Organisations;
			goodsOwnerFilter.MultilingualDescription = ResString.GetMultilingualString("283D399A-6B66-4B0C-9512-B8BC6631E975", Schema.GoodsOwner);

			var transporterFilter = filters.AddGuidFilter(Schema.Transporter, ModuleIDs.Organisation, TransporterFilter, Lookups.AllOrganisations);
			transporterFilter.Category = FilterCategories.Organisations;
			transporterFilter.MultilingualDescription = ResString.GetMultilingualString("FF7B8F26-B4CD-4E1F-95A3-ABE4A6FF0E1D", Schema.Transporter);

			// -- Other Required Groups
			AddAuditFilters(filters);
			AddBillingFilters(filters);
			AddCommercialInvoiceAttributeFilter(filters);

			// ***  Workflow Filters are inherited through GetCustomFilterStripsHelpersCore()  ***

			// -- Current Company --
			AddCurrentCompanyFilter(filters);

			return filters;
		}

		#region filter queries

		ZQuery GetInvoiceDateQuery(DateComparisonOperator comparisonOperator, ZDateTime startDate, ZDateTime endDate)
		{
			var jobDeclarationQuery = new ZDBOnlyQuery(typeof(EMCSJobDeclaration));
			var invoiceHeaderSubQuery = new ZDBOnlySubQuery(typeof(BaseJobComInvoiceHeader), JobComInvoiceHeaderSchema.JZ_JE);
			AddDateRange(invoiceHeaderSubQuery, comparisonOperator, JoinCondition.And, JobComInvoiceHeaderSchema.JZ_InvoiceDate, startDate.Date, endDate.Date);
			jobDeclarationQuery.AddSubQuery(invoiceHeaderSubQuery, JoinCondition.And);
			return jobDeclarationQuery;
		}

		ZQuery CarrierAgentFilter(ZGuid value)
		{
			return GenerateJobDocAddressFilter(AutoDocAddressTypes.Codes.CarrierAgent, value);
		}

		ZQuery DestinationWarehouseFilter(ZGuid value)
		{
			return GenerateJobDocAddressFilter(AutoDocAddressTypes.Codes.DestinationWarehouse, value);
		}

		ZQuery DispatchWarehouseFilter(ZGuid value)
		{
			return GenerateJobDocAddressFilter(AutoDocAddressTypes.Codes.DispatchWarehouse, value);
		}

		ZQuery GoodsOwnerFilter(ZGuid value)
		{
			return GenerateJobDocAddressFilter(AutoDocAddressTypes.Codes.GoodsOwner, value);
		}

		ZQuery TransporterFilter(ZGuid value)
		{
			return GenerateJobDocAddressFilter(AutoDocAddressTypes.Codes.Transporter, value);
		}

		ZQuery GenerateJobDocAddressFilter(string addressType, ZGuid value)
		{
			// it is not feasable to convert this to a SubQuery because of the Join between the filters.

			var orgAddressesQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			orgAddressesQuery.AddToFilter(OrgAddressSchema.OA_OH, value);

			ZDBOnlySubQuery jobDocAddressesSubQuery = new ZDBOnlySubQuery(typeof(JobDocAddress), JobDocAddressSchema.E2_ParentID);
			jobDocAddressesSubQuery.AddToFilter(JobDocAddressSchema.E2_AddressType, addressType);
			jobDocAddressesSubQuery.AddSubQuery(JobDocAddressSchema.E2_OA_Address, orgAddressesQuery, JoinCondition.And);

			ZDBOnlyQuery jobDeclarationQuery = new ZDBOnlyQuery(typeof(EMCSJobDeclaration));
			jobDeclarationQuery.AddSubQuery(jobDocAddressesSubQuery, JoinCondition.And);

			return jobDeclarationQuery;
		}

		#endregion

		#region hidden-operator queries

		ZQuery DeferredSubmissionQuery(ZString value)
		{
			ZQuery query;
			switch (value)
			{
				case DeclarationFilterLookups.DeferredCodes.All:
					query = new ZQuery();
					break;
				default:
					query = GetAddOnColumnQuery(EUEMCSAddInfoSchema.Constants.ZG_DeferredSubmission, SQLComparisonOperator.Equal, value);
					break;
			}
			return query;
		}

		ZQuery TransportModeQuery(ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_TransportMode, value);
		}

		ZQuery DeclarantTypeQuery(ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_DeclarantType, value);
		}

		ZQuery DestinationTypeQuery(ZString value)
		{
			return new ZQuery(JobDeclarationSchema.JE_MessageSubType, value);
		}

		ZQuery GuarantorTypeQuery(ZString value)
		{
			return GetAddOnColumnQuery(EUEMCSAddInfoSchema.Constants.ZG_GuarantorType, SQLComparisonOperator.Equal, value);
		}

		ZQuery OriginTypeQuery(ZString value)
		{
			return GetAddOnColumnQuery(EUEMCSAddInfoSchema.Constants.ZG_OriginType, SQLComparisonOperator.Equal, value);
		}

		ZQuery TransportArrangementQuery(ZString value)
		{
			return GetAddOnColumnQuery(EUEMCSAddInfoSchema.Constants.ZG_TransportArrangement, SQLComparisonOperator.Equal, value);
		}

		ZQuery GetAddOnColumnQuery(string columnName, SQLComparisonOperator op, ZString value)
		{
			// it is not feasable to convert this to a SubQuery.
			return AddOnColumnHelper.SimpleQueryHelper.GetQueryHandlingBlanks(columnName, op, value);
		}

		GenAddOnColumnHelper AddOnColumnHelper
		{
			get { return addonColumnHelper ?? (addonColumnHelper = new GenAddOnColumnHelper()); }
		}
		GenAddOnColumnHelper addonColumnHelper;

		#endregion

		#region ModuleFilterSubGroups

		class EntryNumberSubGroup : ModuleFilterSubGroup
		{
			public override ZQuery GetSubQuery(ZQuery filter)
			{
				var entryNumberSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
				entryNumberSubQuery.AddToFilter(filter);
				entryNumberSubQuery.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);

				var entryHeaderSubQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
				entryHeaderSubQuery.AddSubQuery(entryNumberSubQuery, JoinCondition.And);

				var jobDeclarationQuery = new ZDBOnlyQuery(typeof(EMCSJobDeclaration));
				jobDeclarationQuery.AddSubQuery(entryHeaderSubQuery, JoinCondition.And);

				return jobDeclarationQuery;
			}
		}

		#endregion

		public new DeclarationFilterLookups Lookups => (DeclarationFilterLookups)base.Lookups;
		protected override JobDeclarationFilterLookups GetNewLookups() => new DeclarationFilterLookups(this);
	}
}
