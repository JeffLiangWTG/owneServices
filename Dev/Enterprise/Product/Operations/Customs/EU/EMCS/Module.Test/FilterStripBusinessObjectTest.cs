using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Module.Testing
{
	[TestedType(typeof(FilterStripBusinessObject))]
	class FilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = (FilterStripBusinessObject)GetNewFilterStripBusinessObject();
			AssertNotNull(filter[FilterStripBusinessObject.Schema.EntryNumber]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.LocalReferenceNumber]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DeferredSubmission]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.RegistrationStatus]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DispatchDate]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.InvoiceDate]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.ReportDate]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DestinationType]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DeclarantType]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.TransportMode]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.GuarantorType]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.OriginType]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.TransportArrangement]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.Consignee]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.Consignor]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.CarrierAgent]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DestinationWarehouse]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.DispatchWarehouse]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.GoodsOwner]);
			AssertNotNull(filter[FilterStripBusinessObject.Schema.Transporter]);
		}

		public void TestFilters_MultilingualDescription()
		{
			var filter = (FilterStripBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				var localReferenceNumberFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.LocalReferenceNumber];
				AssertEquals("LocalReferenceNumber", FilterStripBusinessObject.Schema.LocalReferenceNumber, localReferenceNumberFilter.MultilingualDescription);

				var registrationStatusFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.RegistrationStatus];
				AssertEquals("RegistrationStatus", FilterStripBusinessObject.Schema.RegistrationStatus, registrationStatusFilter.MultilingualDescription);
				var deferredSubmissionFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.DeferredSubmission];
				AssertEquals("DeferredSubmission", FilterStripBusinessObject.Schema.DeferredSubmission, deferredSubmissionFilter.MultilingualDescription);

				var dispatchDateFilter = (ModuleDateFilter)filter[FilterStripBusinessObject.Schema.DispatchDate];
				AssertEquals("DispatchDate", FilterStripBusinessObject.Schema.DispatchDate, dispatchDateFilter.MultilingualDescription);
				var invoiceDateFilter = (ModuleDateFilter)filter[FilterStripBusinessObject.Schema.InvoiceDate];
				AssertEquals("InvoiceDate", FilterStripBusinessObject.Schema.InvoiceDate, invoiceDateFilter.MultilingualDescription);
				var reportDateFilter = (ModuleDateFilter)filter[FilterStripBusinessObject.Schema.ReportDate];
				AssertEquals("ReportDate", FilterStripBusinessObject.Schema.ReportDate, reportDateFilter.MultilingualDescription);

				var carrierAgentFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.CarrierAgent];
				AssertEquals("CarrierAgent", FilterStripBusinessObject.Schema.CarrierAgent, carrierAgentFilter.MultilingualDescription);
				var consigneeFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.Consignee];
				AssertEquals("Consignee", FilterStripBusinessObject.Schema.Consignee, consigneeFilter.MultilingualDescription);
				var consignorFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.Consignor];
				AssertEquals("Consignor", FilterStripBusinessObject.Schema.Consignor, consignorFilter.MultilingualDescription);
				var destinationWarehouseFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.DestinationWarehouse];
				AssertEquals("DestinationWarehouse", FilterStripBusinessObject.Schema.DestinationWarehouse, destinationWarehouseFilter.MultilingualDescription);
				var dispatchWarehouseFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.DispatchWarehouse];
				AssertEquals("DispatchWarehouse", FilterStripBusinessObject.Schema.DispatchWarehouse, dispatchWarehouseFilter.MultilingualDescription);
				var goodsOwnerFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.GoodsOwner];
				AssertEquals("GoodsOwner", FilterStripBusinessObject.Schema.GoodsOwner, goodsOwnerFilter.MultilingualDescription);
				var transporterFilter = (ModuleGuidFilter)filter[FilterStripBusinessObject.Schema.Transporter];
				AssertEquals("Transporter", FilterStripBusinessObject.Schema.Transporter, transporterFilter.MultilingualDescription);

				var declarantTypeFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.DeclarantType];
				AssertEquals("DeclarantType", FilterStripBusinessObject.Schema.DeclarantType, declarantTypeFilter.MultilingualDescription);
				var destinationTypeFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.DestinationType];
				AssertEquals("DestinationType", FilterStripBusinessObject.Schema.DestinationType, destinationTypeFilter.MultilingualDescription);
				var guarantorTypeFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.GuarantorType];
				AssertEquals("GuarantorType", FilterStripBusinessObject.Schema.GuarantorType, guarantorTypeFilter.MultilingualDescription);
				var originTypeFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.OriginType];
				AssertEquals("OriginType", FilterStripBusinessObject.Schema.OriginType, originTypeFilter.MultilingualDescription);
				var transportArrangementFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.TransportArrangement];
				AssertEquals("TransportArrangement", FilterStripBusinessObject.Schema.TransportArrangement, transportArrangementFilter.MultilingualDescription);
				var transportModeFilter = (ModuleTextFilter)filter[FilterStripBusinessObject.Schema.TransportMode];
				AssertEquals("TransportMode", FilterStripBusinessObject.Schema.TransportMode, transportModeFilter.MultilingualDescription);
			});
		}

		public void TestInheritedFiltersExist()
		{
			var filter = (FilterStripBusinessObject)GetNewFilterStripBusinessObject();

			Assert("Expected Audit Information Category", filter.Any(f => f.Category.Description == "Audit Information"));
			Assert("Expected Other Category", filter.Any(f => f.Category.Description == "Other"));
			Assert("Expected Billing Category", filter.Any(f => f.Category.Description == "Billing"));
			Assert("Expected Commercial Invoice Attribute Category", filter.Any(f => f.Category.Description == "Commercial Invoice Attribute Search"));
			Assert("Expected Workflow Milestones Category", filter.Any(f => f.Category.Description == "Workflow Milestones"));
			Assert("Expected Workflow Tasks Category", filter.Any(f => f.Category.Description == "Workflow Tasks"));
		}

		public void TestCurrentCompanyFilter()
		{
			var currentCompanyPK = GlbCompany.CurrentCompany.PK;
			var currentCompanyBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch1.GB_GC = currentCompanyPK;
			var currentCompanyBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			currentCompanyBranch2.GB_GC = currentCompanyPK;

			var company2 = Factory.NewWithValidTestData<GlbCompany>();
			var company2Branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company2Branch1.GB_GC = company2.PK;

			var dec1 = Factory.New<EMCSJobDeclaration>();
			dec1.JE_ApplicationCode = "EMC";
			dec1.JE_GB = currentCompanyBranch1.PK;
			dec1.JE_DeclarationReference = "B0001";

			var dec2 = Factory.New<EMCSJobDeclaration>();
			dec2.JE_ApplicationCode = "EMC";
			dec2.JE_GB = currentCompanyBranch2.PK;
			dec2.JE_DeclarationReference = "B0002";

			var dec3 = Factory.New<EMCSJobDeclaration>();
			dec3.JE_ApplicationCode = "EMC";
			dec3.JE_GB = company2Branch1.PK;
			dec3.JE_DeclarationReference = "B0003";

			Factory.Save();

			var filterObj = GetNewFilterStripBusinessObject();

			var coll = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filterObj.Filter);
			coll.Sort("JE_DeclarationReference");
			AssertEquals(2, coll.Count);
			AssertEquals(dec1.JE_DeclarationReference, coll[0].JE_DeclarationReference);
			AssertEquals(dec2.JE_DeclarationReference, coll[1].JE_DeclarationReference);
		}

		public void TestNumberFilters()
		{
			// Job #
			var dec1 = CreateEMCSDeclaration("B00001002");

			// Invoice #
			var dec2 = CreateEMCSDeclaration("B00001003");
			var invHeader = dec2.Invoices.AddNew();
			invHeader.JZ_InvoiceNumber = "INV001";

			//Local Reference Number
			var dec3 = CreateEMCSDeclaration("B00001004");
			dec3.JE_OwnerRef = "Local Ref";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsInTextFilterResults(dec1, DeclarationFilterConstants.NumberFilterTypes.DeclarationReference, "B00001002", "Should Find Declaration by Job #");
				AssertDeclarationIsInTextFilterResults(dec2, DeclarationFilterConstants.NumberFilterTypes.InvoiceNumber, "INV001", "Should Find Declaration by Invoice #");
				AssertDeclarationIsInTextFilterResults(dec3, FilterStripBusinessObject.Schema.LocalReferenceNumber, "Local Ref", "Should Find Declaration by Local Ref. #");
			});
		}

		public void TestStatusAndFlagFilters()
		{
			// Deferred Submission
			var dec5 = CreateEMCSDeclaration("B00001005");
			dec5.ZG_DeferredSubmission = EMCSDeferredSubmissionList.Codes.Yes;

			// Message Status
			var dec6 = CreateEMCSDeclaration("B00001006");
			dec6.JE_MessageStatus = "XYZ";

			// Registration Status
			var dec7 = CreateEMCSDeclaration("B00001007");
			dec7.JE_EntryStatus = "ABC";

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsInTextFilterResults(dec5, FilterStripBusinessObject.Schema.DeferredSubmission, EMCSDeferredSubmissionList.Codes.Yes, "Should Find Declaration by DeferredSubmission");
				AssertDeclarationIsInTextFilterResults(dec6, "Message Status", "XYZ", "Should Find Declaration by MessageStatus");
				AssertDeclarationIsInTextFilterResults(dec7, FilterStripBusinessObject.Schema.RegistrationStatus, "ABC", "Should Find Declaration by RegistrationStatus");
			});
		}

		public void TestDateFilters()
		{
			// Dispatch Date
			var dec1 = CreateEMCSDeclaration("B00001001");
			dec1.JE_DateAtOrigin = TestDate;

			// Invoice Date
			var dec2 = CreateEMCSDeclaration("B00001002");
			dec2.AllGroupHeaders[0].JZ_InvoiceDate = TestDate;

			// Report Date
			var dec3 = CreateEMCSDeclaration("B00001003");
			dec3.JE_EntryAuthorisationDate = TestDate;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsInDateFilterResults(dec1, FilterStripBusinessObject.Schema.DispatchDate, "Should Find Declaration by DispatchDate");
				AssertDeclarationIsInDateFilterResults(dec2, FilterStripBusinessObject.Schema.InvoiceDate, "Should Find Declaration by InvoiceDate");
				AssertDeclarationIsInDateFilterResults(dec3, FilterStripBusinessObject.Schema.ReportDate, "Should Find Declaration by ReportDate");
			});
		}

		ZDateTime TestDate => ZDateTime.Today.AddDays(2);

		public void TestOrganizationFilters()
		{
			var filter = new FilterStripBusinessObject();
			var organisation = Factory.LoadTop1<OrgHeader>(new ZQuery());

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;

			// Branch
			var dec1 = CreateEMCSDeclaration("B00001001");
			dec1.JE_GB = branch1.PK;
			dec1.JE_OH_Consignee = organisation.PK;
			Factory.Save();

			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = GlbCompany.CurrentCompany.PK;

			// Consignor
			var dec2 = CreateEMCSDeclaration("B00001002");
			dec2.JE_GB = branch2.PK;
			dec2.JE_OH_Supplier = organisation.PK;
			Factory.Save();

			// Consignee
			var dec3 = CreateEMCSDeclaration("B00001003");
			dec3.JE_GB = branch2.PK;
			dec3.JE_OH_Importer = organisation.PK;
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsTheOnlyOrgFilterResult(branch1.PK, "Declaration Branch", dec1, "Should Find Declaration by Branch");
				AssertDeclarationIsTheOnlyOrgFilterResult(organisation.PK, FilterStripBusinessObject.Schema.Consignor, dec2, "Should Find Declaration by Consignor");
				AssertDeclarationIsTheOnlyOrgFilterResult(organisation.PK, FilterStripBusinessObject.Schema.Consignee, dec3, "Should Find Declaration by Consignee");
			});
		}

		public void TestJobDocAddressFilters()
		{
			var dec = CreateEMCSDeclaration("B00001000");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addr = org.Addresses.AddNew();
			addr.OA_Address1 = "xyz";

			var jda = Factory.New<JobDocAddress>();
			jda.E2_ParentID = dec.PK;
			jda.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda.E2_AddressType = "";
			jda.E2_OA_Address = addr.PK;

			var dec1 = CreateEMCSDeclaration("B00001001");

			var carrierAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr1 = carrierAgentOrg.Addresses.AddNew();
			addr1.OA_Address1 = "abc";

			var jda1 = Factory.New<JobDocAddress>();
			jda1.E2_ParentID = dec1.PK;
			jda1.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda1.E2_AddressType = AutoDocAddressTypes.Codes.CarrierAgent;
			jda1.E2_OA_Address = addr1.PK;

			var destinationWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr2 = destinationWarehouseOrg.Addresses.AddNew();
			addr2.OA_Address1 = "abc";

			var jda2 = Factory.New<JobDocAddress>();
			jda2.E2_ParentID = dec1.PK;
			jda2.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda2.E2_AddressType = AutoDocAddressTypes.Codes.DestinationWarehouse;
			jda2.E2_OA_Address = addr2.PK;

			var dispatchWarehouseOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr3 = dispatchWarehouseOrg.Addresses.AddNew();
			addr3.OA_Address1 = "abc";

			var jda3 = Factory.New<JobDocAddress>();
			jda3.E2_ParentID = dec1.PK;
			jda3.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda3.E2_AddressType = AutoDocAddressTypes.Codes.DispatchWarehouse;
			jda3.E2_OA_Address = addr3.PK;

			var goodsOwnerOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr4 = goodsOwnerOrg.Addresses.AddNew();
			addr4.OA_Address1 = "def";

			var jda4 = Factory.New<JobDocAddress>();
			jda4.E2_ParentID = dec1.PK;
			jda4.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda4.E2_AddressType = AutoDocAddressTypes.Codes.GoodsOwner;
			jda4.E2_OA_Address = addr4.PK;

			var transporterOrg = Factory.NewWithValidTestData<OrgHeader>();
			var addr5 = transporterOrg.Addresses.AddNew();
			addr5.OA_Address1 = "def";

			var jda5 = Factory.New<JobDocAddress>();
			jda5.E2_ParentID = dec1.PK;
			jda5.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			jda5.E2_AddressType = AutoDocAddressTypes.Codes.Transporter;
			jda5.E2_OA_Address = addr5.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsTheOnlyOrgFilterResult(carrierAgentOrg.PK, FilterStripBusinessObject.Schema.CarrierAgent, dec1, "Should Find Declaration by CarrierAgent Org");
				AssertDeclarationIsTheOnlyOrgFilterResult(destinationWarehouseOrg.PK, FilterStripBusinessObject.Schema.DestinationWarehouse, dec1, "Should Find Declaration by DestinationWarehouse Org");
				AssertDeclarationIsTheOnlyOrgFilterResult(dispatchWarehouseOrg.PK, FilterStripBusinessObject.Schema.DispatchWarehouse, dec1, "Should Find Declaration by DispatchWarehouse Org");
				AssertDeclarationIsTheOnlyOrgFilterResult(goodsOwnerOrg.PK, FilterStripBusinessObject.Schema.GoodsOwner, dec1, "Should Find Declaration by GoodsOwner Org");
				AssertDeclarationIsTheOnlyOrgFilterResult(transporterOrg.PK, FilterStripBusinessObject.Schema.Transporter, dec1, "Should Find Declaration by Transporter Org");
			});
		}

		public void TestModesAndTypesFilters()
		{
			// Declaration Type
			var dec11 = CreateEMCSDeclaration("B000010011");
			dec11.JE_DeclarantType = EMCSEntryTypeList.Codes.Consignee;

			// Transport Mode
			var dec14 = CreateEMCSDeclaration("B000010014");
			dec14.JE_TransportMode = TransportTypeList.Codes.Road;

			var dec15 = CreateEMCSDeclaration("B000010015");
			dec15.JE_TransportMode = TransportTypeList.Codes.Sea;

			var dec16 = CreateEMCSDeclaration("B000010016");
			dec16.JE_TransportMode = TransportTypeList.Codes.Air;

			// Destination Type
			var dec18 = CreateEMCSDeclaration("B000010018");
			dec18.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;

			var dec19 = CreateEMCSDeclaration("B000010019");
			dec19.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;

			// Guarantor Type
			var dec20 = CreateEMCSDeclaration("B000010020");
			dec20.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignee;

			var dec21 = CreateEMCSDeclaration("B000010021");
			dec21.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;

			// Origin Type
			var dec22 = CreateEMCSDeclaration("B000010022");
			dec22.ZG_OriginType = EMCSOriginTypeList.Codes.Import;

			var dec23 = CreateEMCSDeclaration("B000010023");
			dec23.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;

			//Transport Arrangement
			var dec24 = CreateEMCSDeclaration("B000010024");
			dec24.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.OwnerOfGoods;

			var dec25 = CreateEMCSDeclaration("B000010025");
			dec25.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertDeclarationIsInTextFilterResults(dec11, FilterStripBusinessObject.Schema.DeclarantType, EMCSEntryTypeList.Codes.Consignee, "Should Find Declaration by DeclarantType Consignee");

				AssertDeclarationIsInTextFilterResults(dec14, Customs.Module.DeclarationFilterConstants.TransportMode, TransportTypeList.Codes.Road, "Should Find Declaration by TransportMode Road");
				AssertDeclarationIsInTextFilterResults(dec15, Customs.Module.DeclarationFilterConstants.TransportMode, TransportTypeList.Codes.Sea, "Should Find Declaration by TransportMode Sea");
				AssertDeclarationIsInTextFilterResults(dec16, Customs.Module.DeclarationFilterConstants.TransportMode, TransportTypeList.Codes.Air, "Should Find Declaration by TransportMode Air");

				AssertDeclarationIsInTextFilterResults(dec18, FilterStripBusinessObject.Schema.DestinationType, EMCSDestinationTypeList.Codes.DestinationExport, "Should Find Declaration by DestinationType Export");
				AssertDeclarationIsInTextFilterResults(dec19, FilterStripBusinessObject.Schema.DestinationType, EMCSDestinationTypeList.Codes.DestinationDirectDelivery, "Should Find Declaration by DestinationType Direct Delivery");

				AssertDeclarationIsInTextFilterResults(dec20, FilterStripBusinessObject.Schema.GuarantorType, EMCSGuarantorTypeList.Codes.Consignee, "Should Find Declaration by GuarantorType Consignee");
				AssertDeclarationIsInTextFilterResults(dec21, FilterStripBusinessObject.Schema.GuarantorType, EMCSGuarantorTypeList.Codes.Consignor, "Should Find Declaration by GuarantorType Consignor");

				AssertDeclarationIsInTextFilterResults(dec22, FilterStripBusinessObject.Schema.OriginType, EMCSOriginTypeList.Codes.Import, "Should Find Declaration by Origin Type Import");
				AssertDeclarationIsInTextFilterResults(dec23, FilterStripBusinessObject.Schema.OriginType, EMCSOriginTypeList.Codes.TaxWarehouse, "Should Find Declaration by Origin Type TaxWarehouse");

				AssertDeclarationIsInTextFilterResults(dec24, FilterStripBusinessObject.Schema.TransportArrangement, EMCSTransportArrangementList.Codes.OwnerOfGoods, "Should Find Declaration by Transport Arrangement OwnerOfGoods");
				AssertDeclarationIsInTextFilterResults(dec25, FilterStripBusinessObject.Schema.TransportArrangement, EMCSTransportArrangementList.Codes.Other, "Should Find Declaration by Transport Arrangement Other");
			});
		}

		public void TestLookups()
		{
			var filter = (FilterStripBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				AssertType<DeclarationFilterLookups>("Type", filter.Lookups);
				AssertSame("Cached", filter.Lookups, filter.Lookups);
			});
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheck()
		{
			var list = base.GetFiltersExcludedFromSubgroupCheck();
			list.AddRange(ExcludedFilters);
			return list;
		}

		protected override List<Tuple<string, string>> GetFiltersExcludedFromSubgroupCheckForCommonTables()
		{
			var list = base.GetFiltersExcludedFromSubgroupCheckForCommonTables();
			list.AddRange(ExcludedFilters);
			return list;
		}

		protected override ZArchitecture.Business.FilterStripBusinessObject GetNewFilterStripBusinessObject() => new FilterStripBusinessObject();

		EMCSJobDeclaration CreateEMCSDeclaration(ZString jobNumber)
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_ApplicationCode = "EMC";
			declaration.JE_DeclarationReference = jobNumber;
			declaration.ZG_OriginType = "0";
			declaration.ZG_TransportArrangement = "0";

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			declaration.JE_GB = branch.PK;

			return declaration;
		}

		IEnumerable<Tuple<string, string>> ExcludedFilters
		{
			get
			{
				return new Tuple<string, string>[]
				{
					TableFilter(JobDocAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.CarrierAgent),
					TableFilter(JobDocAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.DestinationWarehouse),
					TableFilter(JobDocAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.DispatchWarehouse),
					TableFilter(JobDocAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.GoodsOwner),
					TableFilter(JobDocAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.Transporter),

					TableFilter(OrgAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.CarrierAgent),
					TableFilter(OrgAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.DestinationWarehouse),
					TableFilter(OrgAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.DispatchWarehouse),
					TableFilter(OrgAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.GoodsOwner),
					TableFilter(OrgAddressSchema.Constants.TableName, FilterStripBusinessObject.Schema.Transporter),

					TableFilter(GenAddOnColumnSchema.Constants.TableName, FilterStripBusinessObject.Schema.DeferredSubmission),
					TableFilter(GenAddOnColumnSchema.Constants.TableName, FilterStripBusinessObject.Schema.GuarantorType),
					TableFilter(GenAddOnColumnSchema.Constants.TableName, FilterStripBusinessObject.Schema.OriginType),
					TableFilter(GenAddOnColumnSchema.Constants.TableName, FilterStripBusinessObject.Schema.TransportArrangement)
				};
			}
		}

		void AssertDeclarationIsInDateFilterResults(EMCSJobDeclaration declaration, string filterName, ZString message)
		{
			var filterStrip = new FilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterStrip[filterName];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.Property1 = TestDate;
			dateFilter.Property2 = TestDate;
			dateFilter.IsActive = true;

			var coll = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var query = filterStrip.Filter;
			coll.Load(query);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, declaration.JE_DeclarationReference, coll[0].JE_DeclarationReference);

			dateFilter.IsActive = false;
		}

		void AssertDeclarationIsTheOnlyOrgFilterResult(ZGuid orgPK, string filterName, EMCSJobDeclaration declaration, ZString message)
		{
			var filter = new FilterStripBusinessObject();
			var orgFilter = (ModuleGuidFilter)filter[filterName];
			orgFilter.Property = orgPK;
			orgFilter.IsActive = true;

			var coll = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, declaration.JE_DeclarationReference, coll[0].JE_DeclarationReference);

			orgFilter.IsActive = false;
		}

		void AssertDeclarationIsInTextFilterResults(EMCSJobDeclaration declaration, string filterName, ZString filterProperty, ZString message)
		{
			var filter = new FilterStripBusinessObject();
			var textFilter = (ModuleTextFilter)filter[filterName];

			AssertNotNull(message, textFilter);

			textFilter.Property = filterProperty;
			textFilter.IsActive = true;

			var coll = new EMCSJobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			coll.Load(filter.Filter);
			AssertEquals(message + " [count]", 1, coll.Count);
			AssertEquals(message, declaration.JE_DeclarationReference, coll[0].JE_DeclarationReference);

			textFilter.IsActive = false;
		}
	}
}
