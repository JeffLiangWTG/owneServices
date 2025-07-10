using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(LVXFilterStripBusinessObject))]
	sealed class LVXFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIsReadyForConsolidation()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var lvxInvoice = dec1.LVXInvoiceHeader;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.Invoices.AddNew();
			dec2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			dec2.Invoices.AddNew();
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(lvxInvoice, dec2);

			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dec4 = Factory.New<JobDeclaration>();
			dec4.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = dec4.Invoices.AddNew();
			Factory.Save();
			invoice.CA_ReadyForConsolidation = true;
			var dec5 = Factory.New<JobDeclaration>();
			dec5.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleFlagsFilter)filterBO[LVXFilterStripBusinessObject.Constants.IsReadyForConsolidation]);
			filter.IsActive = true;
			filter.Property0 = true;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains(dec4, declarationCollection);
			AssertCollectionNotContains(dec1, declarationCollection);

			filter.Property0 = false;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains(dec1, declarationCollection);
			AssertCollectionNotContains(dec4, declarationCollection);
		}

		public void TestOnlyLVXJobsAppears()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var filterObj = new LVXFilterStripBusinessObject();

			Assert(dec1.MatchesFilter(filterObj.Filter));
			Assert(!dec2.MatchesFilter(filterObj.Filter));
			Assert(!dec3.MatchesFilter(filterObj.Filter));
		}

		public void TestJobStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.LVXInvoiceHeader.JZ_InvoiceNumber = "111";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_InvoiceNumber = "222";

			var job1 = new JobHeader.Loader(declaration1).TryLoadOrCreate();
			var job2 = new JobHeader.Loader(declaration2).TryLoadOrCreate();
			AssertNotNull(job1);
			AssertNotNull(job2);
			AssertEquals(JobHeaderStatus.Working.Code, job1.JH_Status);
			job2.JH_Status = JobHeaderStatus.Closed.Code;

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleTextBaseFilter)filterBO["Job Status"]);

			filter.Property = JobHeaderStatus.Working.Code;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains(declaration1, declarationCollection);
			AssertCollectionNotContains(declaration2, declarationCollection);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionNotContains(declaration1, declarationCollection);
			AssertCollectionContains(declaration2, declarationCollection);
		}

		public void TestInvoicedChargesFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration.LVXInvoiceHeader.JZ_InvoiceNumber = "111";

			var job1 = new JobHeader.Loader(declaration).TryLoadOrCreate();
			AssertNotNull(job1);

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleFlagsFilter)filterBO["Invoiced / Charges"]);
			filter.IsActive = true;
			filter["No Charges"] = true;
			var declarationCollection = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertCollectionContains(declaration, declarationCollection);

			var code = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "CCLR"));
			var charge = Factory.New<JobCharge>();
			charge.JR_JH = job1.PK;
			charge.JR_AC = code.PK;
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			charge.JR_LocalSellAmt = 10m;

			Factory.Save();

			declarationCollection = Factory.Load<JobDeclaration>(filterBO.Filter);
			AssertCollectionNotContains(declaration, declarationCollection);
		}

		public void TestPeriodFitler()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			dec1.JE_EntryAuthorisationDate = new ZDateTime(2015, 1, 1);
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			dec2.JE_EntryAuthorisationDate = new ZDateTime(2015, 2, 1);
			var dec3 = Factory.New<JobDeclaration>();
			dec3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			dec3.JE_EntryAuthorisationDate = ZDateTime.Empty;

			Factory.Save();

			var filterObj = new LVXFilterStripBusinessObject();
			var periodFilter = (PeriodFilter)filterObj.ModuleFilters[DeclarationFilterConstants.NumberFilterTypes.Period];
			periodFilter.IsActive = true;
			periodFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			AssertNotNull("a PeriodFilter should be added", periodFilter);

			var results = Factory.Load<JobDeclaration>(periodFilter.Query);
			AssertCollectionContains(dec1, results);
			AssertCollectionContains(dec2, results);
			AssertCollectionContains(dec3, results);

			periodFilter.PeriodYear = 2015;
			periodFilter.PeriodMonth = 1;
			results = Factory.Load<JobDeclaration>(periodFilter.Query);
			AssertCollectionContains(dec1, results);
			AssertCollectionNotContains(dec2, results);
			AssertCollectionNotContains(dec3, results);

			periodFilter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			results = Factory.Load<JobDeclaration>(periodFilter.Query);
			AssertCollectionNotContains(dec1, results);
			AssertCollectionContains(dec2, results);
			AssertCollectionContains(dec3, results);

			periodFilter.SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			results = Factory.Load<JobDeclaration>(periodFilter.Query);
			AssertCollectionNotContains(dec1, results);
			AssertCollectionNotContains(dec2, results);
			AssertCollectionContains(dec3, results);

			periodFilter.SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			results = Factory.Load<JobDeclaration>(periodFilter.Query);
			AssertCollectionContains(dec1, results);
			AssertCollectionContains(dec2, results);
			AssertCollectionNotContains(dec3, results);
		}

		public void TestLVSIDFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.LVXInvoiceHeader.JZ_InvoiceNumber = "111";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_InvoiceNumber = "222";

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleTextBaseFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.LVSID]);
			filter.Property = "111";
			filter.IsActive = true;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("LVS ID", declaration1, declarationCollection);
			AssertCollectionNotContains("LVS ID", declaration2, declarationCollection);
		}

		public void TestImporterVendorFilter()
		{
			var org1 = OrgHeader.New(Factory);
			org1.OH_Code = "AAA";
			var org2 = OrgHeader.New(Factory);
			org2.OH_Code = "BBB";
			var org3 = OrgHeader.New(Factory);
			org3.OH_Code = "CCC";
			var org4 = OrgHeader.New(Factory);
			org4.OH_Code = "DDD";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_OH_Importer = org1.PK;
			declaration1.JE_OH_Supplier = org2.PK;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.JE_OH_Importer = org3.PK;
			declaration2.JE_OH_Supplier = org4.PK;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration3.JE_OH_Importer = org1.PK;
			declaration3.JE_OH_Supplier = org4.PK;
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration4.JE_OH_Importer = org3.PK;
			declaration4.JE_OH_Supplier = org2.PK;
			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var invoice = declaration5.Invoices.AddNew();
			invoice.JZ_OH_Buyer = org1.PK;

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleGuidsFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterVendor]);
			filter.IsActive = true;
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("ImporterVendor", declaration1, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration2, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration3, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration4, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration5, declarationCollection);

			filter.Property1 = org1.PK;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("ImporterVendor", declaration1, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration2, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration3, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration4, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration5, declarationCollection);

			filter.Property2 = org2.PK;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("ImporterVendor", declaration1, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration2, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration3, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration4, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration5, declarationCollection);

			filter.Property1 = ZGuid.Empty;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("ImporterVendor", declaration1, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration2, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration3, declarationCollection);
			AssertCollectionContains("ImporterVendor", declaration4, declarationCollection);
			AssertCollectionNotContains("ImporterVendor", declaration5, declarationCollection);
		}

		public void TestDirectShipmentDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.LVXInvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 6, 20);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_ValuationDateOverride = new ZDateTime(2015, 6, 25);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration3.LVXInvoiceHeader.JZ_ValuationDateOverride = ZDateTime.Empty;

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.DirectShipmentDate]);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2015, 6, 17);
			filter.Property2 = new ZDateTime(2015, 6, 22);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("Direct Shipment Date", declaration1, declarationCollection);
			AssertCollectionNotContains("Direct Shipment Date", declaration2, declarationCollection);
			AssertCollectionNotContains("Direct Shipment Date", declaration3, declarationCollection);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionNotContains("Direct Shipment Date", declaration1, declarationCollection);
			AssertCollectionNotContains("Direct Shipment Date", declaration2, declarationCollection);
			AssertCollectionContains("Direct Shipment Date", declaration3, declarationCollection);
		}

		public void TestReleaseDateFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_EntryAuthorisationDate = new ZDateTime(2022, 6, 20);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.JE_EntryAuthorisationDate = new ZDateTime(2022, 6, 25);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration3.JE_EntryAuthorisationDate = ZDateTime.Empty;

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.ReleaseDate]);
			filter.IsActive = true;
			filter.Property1 = new ZDateTime(2022, 6, 17);
			filter.Property2 = new ZDateTime(2022, 6, 22);
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("Release Date", declaration1, declarationCollection);
			AssertCollectionNotContains("Release Date", declaration2, declarationCollection);
			AssertCollectionNotContains("Release Date", declaration3, declarationCollection);

			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionNotContains("Release Date", declaration1, declarationCollection);
			AssertCollectionNotContains("Release Date", declaration2, declarationCollection);
			AssertCollectionContains("Release Date", declaration3, declarationCollection);
		}

		public void TestPortOfClearanceFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.JE_CustomsOffice = "1111";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.JE_CustomsOffice = "2222";

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleNkFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.PortOfClearance]);
			filter.IsActive = true;
			filter.Property = "1111";

			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, filter.ModuleId);

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("Port Of Clearance", declaration1, declarationCollection);
			AssertCollectionNotContains("Port Of Clearance", declaration2, declarationCollection);
		}

		public void TestCountryofOriginFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration1.LVXInvoiceHeader.JZ_RN_NKDefaultOrigin = "US";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration2.LVXInvoiceHeader.JZ_RN_NKDefaultOrigin = "CN";

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleNkFilter)filterBO[DeclarationFilterConstants.CountryofOrigin]);
			filter.IsActive = true;
			filter.Property = "US";

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);

			AssertCollectionContains("Country/Region of Origin", declaration1, declarationCollection);
			AssertCollectionNotContains("Country/Region of Origin", declaration2, declarationCollection);
		}

		public void TestBillingFilter()
		{
			var filterBO = new LVXFilterStripBusinessObject();
			AssertNotNull(filterBO["AP Invoice #"]);
			AssertNotNull(filterBO["AR Transaction #"]);
			AssertNull(filterBO["Supplier Cost Reference"]);
		}

		public void TestInvoiceLineFilter()
		{
			var filterBO = new LVXFilterStripBusinessObject();
			AssertNotNull(filterBO["Tariff - Invoice Line"]);
			AssertNotNull(filterBO["Description - Invoice Line"]);

			var lvs1 = Factory.New<JobDeclaration>();
			var lvs2 = Factory.New<JobDeclaration>();
			var lvs3 = Factory.New<JobDeclaration>();
			var dec = Factory.New<JobDeclaration>();

			lvs1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvs2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			lvs3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			var line1 = lvs1.LVXInvoiceHeader.JobComInvoiceLines.AddNew();
			var line2 = lvs2.LVXInvoiceHeader.JobComInvoiceLines.AddNew();
			var line3 = lvs3.LVXInvoiceHeader.JobComInvoiceLines.AddNew();
			var line4 = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();

			line1.JI_Tariff = "3333333333";
			line2.JI_Description = "TEST DESC";
			line3.JI_Tariff = "3333333333";
			line3.JI_Description = "TEST DESC";
			line4.JI_Tariff = "3333333333";
			line4.JI_Description = "TEST DESC";
			Factory.Save();

			var filter1 = ((ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.TariffInvLine]);
			filter1.Property = "3333333333";
			filter1.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter1.IsActive = true;
			var declarationCollection1 = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection1.Load(filterBO.Filter);
			AssertEquals(2, declarationCollection1.Count);
			AssertEquals(true, declarationCollection1.Contains(lvs1));
			AssertEquals(true, declarationCollection1.Contains(lvs3));

			filter1.IsActive = false;
			var filter2 = ((ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.DescriptionInvLine]);
			filter2.Property = "TEST DESC";
			filter2.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Contains;
			filter2.IsActive = true;
			var declarationCollection2 = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection2.Load(filterBO.Filter);
			AssertEquals(2, declarationCollection2.Count);
			AssertEquals(true, declarationCollection2.Contains(lvs2));
			AssertEquals(true, declarationCollection2.Contains(lvs3));

			filter1 = ((ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.TariffInvLine]);
			filter1.Property = "3333333333";
			filter1.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Exact;
			filter1.IsActive = true;
			var declarationCollection3 = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection3.Load(filterBO.Filter);
			AssertEquals(1, declarationCollection3.Count);
			AssertEquals(true, declarationCollection3.Contains(lvs3));
		}

		public void TestTransactionNumberFilter()
		{
			var lvsJob1 = Factory.New<JobDeclaration>();
			lvsJob1.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsJob1.TransactionNumber.AccountSecurityCode = "12345";
			lvsJob1.TransactionNumber.SequentialNumber = "1";
			var lvsJob2 = Factory.New<JobDeclaration>();
			lvsJob2.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsJob2.TransactionNumber.AccountSecurityCode = "67890";
			lvsJob2.TransactionNumber.SequentialNumber = "1";
			var lvsJob3 = Factory.New<JobDeclaration>();
			lvsJob3.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration1.LVXInvoiceHeader, lvsJob1);
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration2.LVXInvoiceHeader, lvsJob2);
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			LVXJobsConsolidateHelper.AttachToConsolidatedLVSDeclaration(declaration3.LVXInvoiceHeader, lvsJob3);
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			declaration4.Invoices.AddNew();

			Factory.Save();

			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.TransactionNumber]);
			filter.Property = "1234";
			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.Contains;
			filter.IsActive = true;
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);
			AssertCollectionContains("Transaction Number", declaration1, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration2, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration3, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration4, declarationCollection);

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.NotContain;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);
			AssertCollectionNotContains("Transaction Number", declaration1, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration2, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration3, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration4, declarationCollection);

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.IsBlank;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);
			AssertCollectionNotContains("Transaction Number", declaration1, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration2, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration3, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration4, declarationCollection);

			filter.ComparisonOperator = ModuleNumberFilter.ComparisonConstants.IsNotBlank;
			declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filterBO.Filter);
			AssertCollectionContains("Transaction Number", declaration1, declarationCollection);
			AssertCollectionContains("Transaction Number", declaration2, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration3, declarationCollection);
			AssertCollectionNotContains("Transaction Number", declaration4, declarationCollection);

			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength));
		}

		public void TestInvoiceLineProductCodeFilterMaxLength()
		{
			var filterBO = new LVXFilterStripBusinessObject();
			var filter = ((ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.InvoiceLineProductCode]);
			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(JobComInvoiceLineSchema.JI_PartNo.MaxLength));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new LVXFilterStripBusinessObject();
	}
}
