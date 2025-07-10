using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class BaseJobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportRequestedProcedure()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(!filterBizObj.SupportRequestedProcedure);
		}

		public void TestRequestedProcedureFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryLine1 = declaration1.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.SetFirst2CharactersOfJI_Procedure("01");

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryLine2 = declaration2.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.SetFirst2CharactersOfJI_Procedure("02");

			Factory.Save();

			var filterObj = (JobDeclarationFilterBusinessObjectForSupportingTest)GetNewFilterStripBusinessObjectForSupportingTest();
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.RequestedProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "01";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				Assert("find first 2 characters of JI_Procedure", declaration1.MatchesFilter(filterQuery));
				Assert("find first 2 characters of JI_Procedure", !declaration2.MatchesFilter(filterQuery));
			});
		}

		public void TestPreviousProcedureCodeFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			var entryLine1 = declaration1.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "0001001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = "EXP";
			var entryLine2 = declaration2.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "0002001";
			Factory.Save();

			var filterObj = (JobDeclarationFilterBusinessObjectForSupportingTest)GetNewFilterStripBusinessObjectForSupportingTest();
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.PreviousProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "01";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("01 matchs", true, declaration1.MatchesFilter(filterQuery));
				AssertEquals("02 should not match", false, declaration2.MatchesFilter(filterQuery));
			});
		}

		public void TestAdditionalProcedureFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			var entryLine1 = declaration1.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine1 = declaration1.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Procedure = "0001011";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			declaration2.JE_MessageType = "EXP";
			var entryLine2 = declaration2.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
			var invoiceLine2 = declaration2.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = "0001022";
			Factory.Save();

			var filterObj = (JobDeclarationFilterBusinessObjectForSupportingTest)GetNewFilterStripBusinessObjectForSupportingTest();
			var filter = (ModuleTextFilter)filterObj[DeclarationFilterConstants.AdditionalProcedure];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "011";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("01 matchs", true, declaration1.MatchesFilter(filterQuery));
				AssertEquals("02 should not match", false, declaration2.MatchesFilter(filterQuery));
			});
		}

		public void TestSupportsExitControl()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(!filterBizObj.SupportsExitControl);
		}

		public void TestExitPresentationStatusFilter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "IE";
			company.GC_Code = "DJC";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "IEDUB";
			branch.GB_Code = "DJC";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000", "IE");
			branch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration2.JE_MessageType = MessageTypeList.Codes.Export;
				declaration2.JE_CustomsOffice = "IEDUB100";
				var (_, _, exitReports) = ExitControlTestHelper.CreateCusExitReportWithStatus(declaration2, new ZString[] { "EXR" });
				exitReports[0].CER_Type = "PRE";

				var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration3.JE_MessageType = MessageTypeList.Codes.Export;
				declaration3.JE_CustomsOffice = "IEDUB100";
				(_, _, exitReports) = ExitControlTestHelper.CreateCusExitReportWithStatus(declaration3, new ZString[] { "REQ", "REQ" });
				exitReports[0].CER_Type = "PRE";
				exitReports[1].CER_Type = "PRE";

				var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration4.JE_MessageType = MessageTypeList.Codes.Export;
				declaration4.JE_CustomsOffice = "IEDUB100";
				(_, _, exitReports) = ExitControlTestHelper.CreateCusExitReportWithStatus(declaration4, new ZString[] { "COX", "REQ" });
				exitReports[0].CER_Type = "PRE";
				exitReports[1].CER_Type = "PRE";

				var nonXitDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				nonXitDeclaration.JE_MessageType = MessageTypeList.Codes.Export;
				nonXitDeclaration.JE_CustomsOffice = "IEDUB100";
				(var exitHeader, _, exitReports) = ExitControlTestHelper.CreateCusExitReportWithStatus(nonXitDeclaration, new ZString[] { "COX", "REQ" });
				exitReports[0].CER_Type = "PRE";
				exitReports[1].CER_Type = "PRE";
				exitHeader.CXH_ApplicationCode = "DAC";

				var declaration5 = Factory.NewWithValidTestData<JobDeclaration>();
				declaration5.JE_MessageType = MessageTypeList.Codes.Export;
				declaration5.JE_CustomsOffice = "IEDUB100";
				(_, _, exitReports) = ExitControlTestHelper.CreateCusExitReportWithStatus(declaration5, new ZString[] { "", "COX", "COX" });
				exitReports[0].CER_Type = "EXT";
				exitReports[1].CER_Type = "PRE";
				exitReports[2].CER_Type = "PRE";

				Factory.Save();

				var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				var stripBO = (JobDeclarationFilterBusinessObjectForSupportingTest)GetNewFilterStripBusinessObjectForSupportingTest();
				declarationCollection.Load(stripBO.Filter);

				CombineAssertions(() =>
				{
					AssertEquals("Total Declarations", 6, declarationCollection.Count);

					LoadDeclarationCollectionTextFilter(DeclarationFilterConstants.ExitPresentationStatus, "REQ", declarationCollection, stripBO);
					AssertEquals("Total declarations match the filter REQ", 2, declarationCollection.Count);
					AssertEquals("Declaration 3 is in the filter", true, declarationCollection.Contains(declaration3));
					AssertEquals("Declaration 4 is in the filter", true, declarationCollection.Contains(declaration4));

					LoadDeclarationCollectionTextFilter(DeclarationFilterConstants.ExitPresentationStatus, "REQ", declarationCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual);
					AssertEquals("Total declarations that does not match the filter REQ", 3, declarationCollection.Count);
					AssertEquals("Declaration 2 is in the filter", true, declarationCollection.Contains(declaration2));
					AssertEquals("Declaration 4 is in the filter", true, declarationCollection.Contains(declaration4));
					AssertEquals("Declaration 5 is in the filter", true, declarationCollection.Contains(declaration5));

					LoadDeclarationCollectionTextFilter(DeclarationFilterConstants.ExitPresentationStatus, "EXR", declarationCollection, stripBO);
					AssertEquals("Total declarations match the filter EXR", 1, declarationCollection.Count);
					AssertEquals("Declaration 2 is in the filter", true, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionTextFilter(DeclarationFilterConstants.ExitPresentationStatus, "MLT", declarationCollection, stripBO);
					AssertEquals("Total declarations match the filter MLT", 1, declarationCollection.Count);
					AssertEquals("Declaration 4 is in the filter", true, declarationCollection.Contains(declaration4));

					LoadDeclarationCollectionTextFilter(DeclarationFilterConstants.ExitPresentationStatus, "COX", declarationCollection, stripBO);
					AssertEquals("Total declarations match the filter COX", 2, declarationCollection.Count);
					AssertEquals("Declaration 4 is in the filter", true, declarationCollection.Contains(declaration4));
					AssertEquals("Declaration 5 is in the filter", true, declarationCollection.Contains(declaration5));
				});
			}
		}

		public void TestMultipleCPCFiltersInGroup()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.CustomsProcedureCode];
			filter1.Property = "500";
			filter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter1.IsActive = true;
			var filter2 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter2.Property = "50011";
			filter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			var filter3 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter3.Property = "50012,50013";
			filter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var query = stripBO.ModuleFilters.GetFilterQuery(new[] { filter1, filter2, filter3 });
			AssertEquals("Group the same filters",
				"JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_Procedure like '500%' and JI_Procedure <> '50011' and (JI_Procedure not in ('50012', '50013')))",
				query.LiteralTextADO);
		}

		public void TestMultipleVehicleModelFiltersInGroup()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.VehicleModel];
			filter1.Property = "AAA";
			filter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter1.IsActive = true;
			var filter2 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter2.Property = "AAA1";
			filter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			var filter3 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter3.Property = "AAA2,AAA3";
			filter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var query = stripBO.ModuleFilters.GetFilterQuery(new[] { filter1, filter2, filter3 });
			AssertEquals("Group the same filters",
				"JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_Model like 'AAA%' and JI_Model <> 'AAA1' and (JI_Model not in ('AAA2', 'AAA3')))",
				query.LiteralTextADO);
		}

		public void TestMultipleVehicleBrandFiltersInGroup()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.VehicleBrand];
			filter1.Property = "AAA";
			filter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter1.IsActive = true;
			var filter2 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter2.Property = "AAA1";
			filter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			var filter3 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter3.Property = "AAA2,AAA3";
			filter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var query = stripBO.ModuleFilters.GetFilterQuery(new[] { filter1, filter2, filter3 });
			AssertEquals("Group the same filters",
				"JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_BrandName like 'AAA%' and JI_BrandName <> 'AAA1' and (JI_BrandName not in ('AAA2', 'AAA3')))",
				query.LiteralTextADO);
		}

		public void TestSupportsMultipleVehiclesEnabled() => CombineAssertions(() =>
		{
			var stripBO = (JobDeclarationFilterBusinessObjectForSupportingTest)GetNewFilterStripBusinessObjectForSupportingTest();
			var brandFilter = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.VehicleBrand];
			var modelFilter = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.VehicleModel];
			AssertNull("When SupportsMultipleVehicles is true, Vehicle Brand Filter is not present", brandFilter);
			AssertNull("When SupportsMultipleVehicles is true, Vehicle Model Filter is not present", modelFilter);
		});

		public void TestMultiplePreferenceFiltersInGroup()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)stripBO[DeclarationFilterConstants.NumberFilterTypes.Preference];
			filter1.Property = "500";
			filter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter1.IsActive = true;
			var filter2 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter2.Property = "50011";
			filter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			var filter3 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter3.Property = "50012,50013";
			filter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var query = stripBO.ModuleFilters.GetFilterQuery(new[] { filter1, filter2, filter3 });
			AssertEquals("Group the same filters",
				"JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_PrimaryPreference like '500%' and JI_PrimaryPreference <> '50011' and (JI_PrimaryPreference not in ('50012', '50013')))",
				query.LiteralTextADO);
		}

		public void TestMultipleOriginFiltersInGroup()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter1 = (ModuleTextFilter)stripBO[DeclarationFilterConstants.OriginInvoiceLine];
			filter1.Property = "US";
			filter1.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter1.IsActive = true;
			var filter2 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter2.Property = "AU";
			filter2.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter2.IsActive = true;
			var filter3 = (ModuleTextFilter)stripBO.ModuleFilters.AddNewDuplicateFilter(filter1);
			filter3.Property = "NZ";
			filter3.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.NotEqual;
			filter3.IsActive = true;

			var query = stripBO.ModuleFilters.GetFilterQuery(new[] { filter1, filter2, filter3 });
			AssertEquals("Group the same filters",
				"JE_ClusterKey IN (SELECT JI_ClusterKey FROM dbo.JobComInvoiceLine WHERE JI_CountryOfOrigin <> 'US' and JI_CountryOfOrigin <> 'AU' and JI_CountryOfOrigin <> 'NZ')",
				query.LiteralTextADO);
		}

		public void TestLookups()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestJobDeclarationFilter()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "LV";
			company.GC_Code = "DJC";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "LVRIX";
			branch.GB_Code = "DJC";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branch.GB_OH_OrgProxy = orgProxy.PK;

			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declarationDummy = Factory.New<JobDeclaration>();
				var declaration = Factory.New<JobDeclaration>();
				var cei = declaration.CustomsEntryInstructions.AddNew();
				var invoice = declaration.Invoices.AddNew();
				var line1 = invoice.InvoiceLines.AddNew();
				line1.JI_Procedure = "4000000";
				line1.JI_BrandName = "BRAND";
				line1.JI_Model = "MODEL";

				var vehicle = new CusVehicleCollection<Business.CusVehicle, BaseJobComInvoiceLine>(line1).AddNew();
				vehicle.CVH_VehicleIdentificationNumber = "4444";

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				cei.CEI_SubStyle = "A";
				declaration.JE_EntryStyle = "IM";
				declaration.JE_LocationOfGoods = "goods";
				cei.CEI_Style = "IFD";
				declaration.ZG_ShipmentType = ShipmentTypeList.Codes.BackToBack;
				declaration.ZG_CTStatusID = ImportCommunityTransitStatusList.Codes.T2;
				declaration.CustomsEntryHeaders.AddNew().CH_Status = "CLP";

				var instruction = declaration.CustomsEntryInstructions.AddNew();
				var goodsLocation = instruction.GoodsLocation;
				goodsLocation.Address.AuthorisationNumber = "N123";

				var testDate = new ZDateTime(1998, 8, 9);
				var log = declaration.Logs.AddNew(Events.CustomsCleared, testDate.ToOffset());
				Factory.Save();

				CombineAssertions(() =>
				{
					CheckTextFilter("Location of Goods", "goods", declaration, true, false);
					CheckTextFilter("Location of Goods", "N123", declaration, true, false);
					CheckTextFilter("Declaration Type", "IFD", declaration, isExpensive: false);
					CheckTextFilter("Shipment Type", ShipmentTypeList.Codes.BackToBack, declaration);
					CheckTextFilter("CT Status", ImportCommunityTransitStatusList.Codes.T2, declaration);
					CheckTextFilter("Entry Style", "IM", declaration, isExpensive: false);
					CheckTextFilter("Entry Sub-style", "A", declaration, isExpensive: false);
					CheckDateFilter("Clearance Date", testDate, declaration);
					CheckTextFilter("Customs Procedure Code (CPC)", "400", declaration, true, isExpensive: false);
					CheckTextFilter("Customs Procedure Code (CPC)", "500", declaration, false, isExpensive: false);
					CheckTextFilter("Vehicle Brand", "BRAND", declaration, true, isExpensive: false);
					CheckTextFilter("Vehicle Model", "MODEL", declaration, true, isExpensive: false);
					CheckTextFilter("VIN", "4444", declaration, true, isExpensive: false);

					line1.JI_Procedure = "XYZ";
					Factory.Save();
					CheckTextFilter("Customs Procedure Code (CPC)", "500", declaration, false, isExpensive: false);
					CheckTextFilter("Customs Procedure Code (CPC)", "X", declaration, true, isExpensive: false);

					declaration.Delete();
					var declaration2 = Factory.New<JobDeclaration>();
					var log2 = declaration2.Logs.AddNew(Events.CustomsEntryStatus, Events.CustomsCleared.Code, testDate.ToOffset());
					Factory.Save();
					CheckDateFilter("Clearance Date", testDate, declaration2);

					var declaration3 = Factory.New<JobDeclaration>();
					Factory.Save();
					CheckDateFilterForHasNoDateEntered("Clearance Date", declaration3);
				});
			}
		}

		public void TestInlandTransportDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var cei = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var line1 = invoice.InvoiceLines.AddNew();
			line1.JI_Procedure = "4000000";
			line1.JI_BrandName = "BRAND";
			line1.JI_Model = "MODEL";

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_Box18TransportID = "XXXX";
			declaration.JE_TransportIDInland = "NNNN";
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
			declaration.JE_RN_NKTransportNationalityInland = "ZA";
			declaration.ZG_Box18TransportNationality = GlbCompany.CurrentCompany.Country.Code;
			declaration.JE_RN_NKTransportNationality = "FR";
			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				LoadDeclarationCollectionTextFilter("Transport Mode (inland)", TransportTypeList.Codes.Road, declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Transport ID (inland)", "XXXX", declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Transport ID (inland)", "NNNN", declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				LoadDeclarationCollectionNkFilter("Transport Nationality (inland)", GlbCompany.CurrentCompany.Country.Code, declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				LoadDeclarationCollectionNkFilter("Transport Nationality (inland)", "ZA", declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				LoadDeclarationCollectionNkFilter("Transport Nationality", "FR", declarationCollection, stripBO);
				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);

				AssertEquals("Total Declarations", 1, declarationCollection.Count);
			});
			declaration.Delete();
		}

		public void TestJobDeclarationFilterWithIndirectExports()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "GB";
			company.GC_Code = "DJC";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "GBRIX";
			branch.GB_Code = "DJC";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branch.GB_OH_OrgProxy = orgProxy.PK;
			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var declaration2 = Factory.New<JobDeclaration>();
				declaration2.JE_MessageType = MessageTypeList.Codes.Export;
				declaration2.JE_CustomsOffice = "GB000001";
				var declaration3 = Factory.New<JobDeclaration>();
				declaration3.JE_MessageType = MessageTypeList.Codes.Export;
				declaration3.JE_CustomsOffice = "NL000001";
				Factory.Save();

				var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
				declarationCollection.Load(stripBO.Filter);
				AssertEquals(3, declarationCollection.Count);

				var filter = (ModuleFlagsFilter)stripBO["Indirect Export"];
				filter.IsActive = true;
				filter.Property0 = true;
				declarationCollection.Load(stripBO.Filter);
				AssertEquals(1, declarationCollection.Count);
				Assert(declarationCollection.Contains(declaration3));
			}
		}

		public void TestSupportingDocumentsFilterType()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "ES";
			company.GC_Code = "DJC";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "ESBCN";
			branch.GB_Code = "DJC";
			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();
			orgProxy.CustomsCodes.AddNew("EOR", "123456789000");
			branch.GB_OH_OrgProxy = orgProxy.PK;

			Factory.Save();

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				var declaration1 = Factory.New<JobDeclaration>();
				var invoice1 = declaration1.Invoices.AddNew();
				var line1 = invoice1.InvoiceLines.AddNew();
				declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
				var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				var invLineEntry = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
				invLineEntry.JI_JZ = invoice1.PK;

				var declarationSupDoc1 = declaration1.SupportingDocuments.AddNew();
				declarationSupDoc1.CSI_Code = "N740";
				var instructionSupDoc1 = entryInstruction1.SupportingDocuments.AddNew();
				instructionSupDoc1.CSI_Code = "N720";
				var invoiceSupDoc1 = invoice1.SupportingDocuments.AddNew();
				invoiceSupDoc1.CSI_Code = "N705";
				var lineSupDoc1 = line1.SupportingDocuments.AddNew();
				lineSupDoc1.CSI_Code = "N380";

				var document = entryLine.Factory.New<SupportingDocument>();
				document.CSI_Code = "7002";
				document.CSI_ParentID = entryLine.PK;
				document.CSI_ParentTableCode = entryLine.TablePrefix;

				var document2 = entryHeader.Factory.New<SupportingDocument>();
				document2.CSI_Code = "7003";
				document2.CSI_ParentID = entryHeader.PK;
				document2.CSI_ParentTableCode = entryHeader.TablePrefix;

				var declaration2 = Factory.New<JobDeclaration>();

				var declarationSupDoc2 = declaration2.SupportingDocuments.AddNew();
				declarationSupDoc2.CSI_Code = "N740";

				Factory.Save();

				var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
				declarationCollection.Load(stripBO.Filter);

				CombineAssertions(() =>
				{
					AssertEquals("Total Declarations", 2, declarationCollection.Count);

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "N740", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as Declaration document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is in the filter cause Type is the same as Declaration document type", true, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "N720", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as Entry Instruction document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration document type", false, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "N705", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as Invoice document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration document type", false, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "N380", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as Invoice document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration document type", false, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "7002", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as EntryLine document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration document type", false, declarationCollection.Contains(declaration2));

					LoadDeclarationCollectionNkFilter("Supporting Document Type", "7003", declarationCollection, stripBO);

					AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
					AssertEquals("Declaration 1 is in the filter cause Type is the same as EntryHeader document type", true, declarationCollection.Contains(declaration1));
					AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration document type", false, declarationCollection.Contains(declaration2));
				});

				declaration1.Delete();
				declaration2.Delete();
			}
		}

		public void TestSupportingDocumentsFilterReference()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invLineEntry = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLineEntry.JI_JZ = invoice1.PK;

			var declarationSupDoc1 = declaration1.SupportingDocuments.AddNew();
			declarationSupDoc1.CSI_ReferenceNumber = "Reference1";
			var instructionSupDoc1 = entryInstruction1.SupportingDocuments.AddNew();
			instructionSupDoc1.CSI_ReferenceNumber = "Reference2";
			var invoiceSupDoc1 = invoice1.SupportingDocuments.AddNew();
			invoiceSupDoc1.CSI_ReferenceNumber = "Reference3";
			var lineSupDoc1 = line1.SupportingDocuments.AddNew();
			lineSupDoc1.CSI_ReferenceNumber = "Reference4";

			var document = entryLine.Factory.New<SupportingDocument>();
			document.CSI_ReferenceNumber = "ReferenceCL";
			document.CSI_ParentID = entryLine.PK;
			document.CSI_ParentTableCode = entryLine.TablePrefix;

			var document2 = entryHeader.Factory.New<SupportingDocument>();
			document2.CSI_ReferenceNumber = "ReferenceCH";
			document2.CSI_ParentID = entryHeader.PK;
			document2.CSI_ParentTableCode = entryHeader.TablePrefix;

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationSupDoc2 = declaration2.SupportingDocuments.AddNew();
			declarationSupDoc2.CSI_ReferenceNumber = "Reference1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "Reference1", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Declaration document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Reference is the same as Declaration document reference", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "Reference2", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Entry Instruction document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration document reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "Reference3", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Invoice document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration document reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "Reference4", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Invoice document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration document reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "ReferenceCL", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as EntryLine document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration document reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Supporting Document Reference", "ReferenceCH", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as EntryHeader document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration document reference", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestSupportingDocumentsFilterIssueDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invLineEntry = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLineEntry.JI_JZ = invoice1.PK;

			var declarationSupDoc1 = declaration1.SupportingDocuments.AddNew();
			declarationSupDoc1.CSI_DateOfIssue = new ZDate(2021, 1, 16);
			var instructionSupDoc1 = entryInstruction1.SupportingDocuments.AddNew();
			instructionSupDoc1.CSI_DateOfIssue = new ZDate(2021, 6, 16);
			var invoiceSupDoc1 = invoice1.SupportingDocuments.AddNew();
			invoiceSupDoc1.CSI_DateOfIssue = new ZDate(2021, 9, 16);
			var lineSupDoc1 = line1.SupportingDocuments.AddNew();
			lineSupDoc1.CSI_DateOfIssue = new ZDate(2021, 7, 16);

			var document = entryLine.Factory.New<SupportingDocument>();
			document.CSI_DateOfIssue = new ZDate(2021, 2, 26);
			document.CSI_ParentID = entryLine.PK;
			document.CSI_ParentTableCode = entryLine.TablePrefix;

			var document2 = entryHeader.Factory.New<SupportingDocument>();
			document2.CSI_DateOfIssue = new ZDate(2021, 3, 26);
			document2.CSI_ParentID = entryHeader.PK;
			document2.CSI_ParentTableCode = entryHeader.TablePrefix;

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationSupDoc2 = declaration2.SupportingDocuments.AddNew();
			declarationSupDoc2.CSI_DateOfIssue = new ZDate(2021, 1, 16);

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 1, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Declaration document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Date is the same as Declaration document date of issue", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 6, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Entry Instruction document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 9, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Invoice document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 7, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Invoice Line document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 2, 26), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as EntryLine document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 2, 26), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as EntryLine document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Issue Date", new ZDate(2021, 3, 26), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as EntryHeader document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestSupportingDocumentsFilterExpiryDate()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();
			declaration1.CustomsEntryInstructions.RemoveAndDeleteAll();
			var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
			var entryHeader = declaration1.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction1.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			var invLineEntry = (JobComInvoiceLine)entryLine.InvoiceLines.AddNew();
			invLineEntry.JI_JZ = invoice1.PK;

			var declarationSupDoc1 = declaration1.SupportingDocuments.AddNew();
			declarationSupDoc1.CSI_DateOfExpiry = new ZDate(2021, 2, 16);
			var instructionSupDoc1 = entryInstruction1.SupportingDocuments.AddNew();
			instructionSupDoc1.CSI_DateOfExpiry = new ZDate(2021, 6, 16);
			var invoiceSupDoc1 = invoice1.SupportingDocuments.AddNew();
			invoiceSupDoc1.CSI_DateOfExpiry = new ZDate(2021, 10, 16);
			var lineSupDoc1 = line1.SupportingDocuments.AddNew();
			lineSupDoc1.CSI_DateOfExpiry = new ZDate(2021, 8, 16);

			var document = entryLine.Factory.New<SupportingDocument>();
			document.CSI_DateOfExpiry = new ZDate(2021, 2, 26);
			document.CSI_ParentID = entryLine.PK;
			document.CSI_ParentTableCode = entryLine.TablePrefix;

			var document2 = entryHeader.Factory.New<SupportingDocument>();
			document2.CSI_DateOfExpiry = new ZDate(2021, 3, 26);
			document2.CSI_ParentID = entryHeader.PK;
			document2.CSI_ParentTableCode = entryHeader.TablePrefix;

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationSupDoc2 = declaration2.SupportingDocuments.AddNew();
			declarationSupDoc2.CSI_DateOfExpiry = new ZDate(2021, 2, 16);

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 2, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Declaration document date of expiry", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Date is the same as Declaration document date of expiry", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 6, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Entry Instruction document date of expiry", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of expiry", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 10, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Invoice document date of expiry", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of expiry", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 8, 16), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as Invoice Line document date of expiry", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of expiry", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 2, 26), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as EntryLine document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionDateFilter("Supporting Document Expiry Date", new ZDate(2021, 3, 26), declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Date is the same as EntryHeader document date of issue", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Date is not the same as Declaration document date of issue", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestPreviousDocumentsFilterClass()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();

			var declarationPreDoc1 = declaration1.PreviousDocuments.AddNew();
			declarationPreDoc1.CSI_SubType = "A";
			var invoicePreDoc1 = invoice1.PreviousDocuments.AddNew();
			invoicePreDoc1.CSI_SubType = "B";
			var linePreDoc1 = line1.PreviousDocuments.AddNew();
			linePreDoc1.CSI_SubType = "C";

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationPreDoc2 = declaration2.PreviousDocuments.AddNew();
			declarationPreDoc2.CSI_SubType = "A";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Previous Document Class", "A", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Class is the same as Declaration previous document class", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Class is the same as Declaration previous document class", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Class", "B", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Class is the same as Invoice previous document class", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Class is not the same as Declaration previous document class", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Class", "C", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Class is the same as Invoice previous document class", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Class is not the same as Declaration previous document class", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestPreviousDocumentsFilterType()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();

			var declarationPreDoc1 = declaration1.PreviousDocuments.AddNew();
			declarationPreDoc1.CSI_Code = "DUA";
			var invoicePreDoc1 = invoice1.PreviousDocuments.AddNew();
			invoicePreDoc1.CSI_Code = "SUM";
			var linePreDoc1 = line1.PreviousDocuments.AddNew();
			linePreDoc1.CSI_Code = "ADD";

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationPreDoc2 = declaration2.PreviousDocuments.AddNew();
			declarationPreDoc2.CSI_Code = "DUA";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Previous Document Type", "DUA", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Type is the same as Declaration previous document type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Type is the same as Declaration previous document type", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Type", "SUM", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Type is the same as Invoice previous document type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration previous document type", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Type", "ADD", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Type is the same as Invoice previous document type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Type is not the same as Declaration previous document type", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestPreviousDocumentsFilterReference()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();

			var declarationPreDoc1 = declaration1.PreviousDocuments.AddNew();
			declarationPreDoc1.CSI_ReferenceNumber = "Reference1";
			var invoicePreDoc1 = invoice1.PreviousDocuments.AddNew();
			invoicePreDoc1.CSI_ReferenceNumber = "Reference2";
			var linePreDoc1 = line1.PreviousDocuments.AddNew();
			linePreDoc1.CSI_ReferenceNumber = "Reference3";

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationPreDoc2 = declaration2.PreviousDocuments.AddNew();
			declarationPreDoc2.CSI_ReferenceNumber = "Reference1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Previous Document Reference", "Reference1", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Declaration previous document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Reference is the same as Declaration previous document reference", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Reference", "Reference2", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Invoice previous document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration previous document reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Reference", "Reference3", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Reference is the same as Invoice previous document reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Reference is not the same as Declaration previous document reference", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestPreviousDocumentsFilterLineNumber()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoice1 = declaration1.Invoices.AddNew();
			var line1 = invoice1.InvoiceLines.AddNew();

			var declarationPreDoc1 = declaration1.PreviousDocuments.AddNew();
			declarationPreDoc1.CSI_LineNo = 1;
			var invoicePreDoc1 = invoice1.PreviousDocuments.AddNew();
			invoicePreDoc1.CSI_LineNo = 0;
			var linePreDoc1 = line1.PreviousDocuments.AddNew();
			linePreDoc1.CSI_LineNo = 2;

			var declaration2 = Factory.New<JobDeclaration>();

			var declarationPreDoc2 = declaration2.PreviousDocuments.AddNew();
			declarationPreDoc2.CSI_LineNo = 1;

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Previous Document Line No", "1", declarationCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Line No is the same as Declaration previous document line No", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Line No is the same as Declaration previous document line No", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Line No", "0", declarationCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Line No is the same as Invoice previous document line No", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Line No is not the same as Declaration previous document line No", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Previous Document Line No", "2", declarationCollection, stripBO, ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Line No is the same as Invoice previous document line No", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Line No is not the same as Declaration previous document line No", false, declarationCollection.Contains(declaration2));
			});

			declaration1.Delete();
			declaration2.Delete();
		}

		public void TestGuaranteeReferenceFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();

			var guarantee1 = declaration1.Guarantees.AddNew();
			guarantee1.PW_BondNumber = "GUA1";
			var guarantee2 = declaration1.Guarantees.AddNew();
			guarantee2.PW_BondNumber = "GUA2";
			var guarantee3 = declaration1.Guarantees.AddNew();
			guarantee3.PW_BondNumber = "GUA3";

			var declaration2 = Factory.New<JobDeclaration>();

			var guarantee4 = declaration2.Guarantees.AddNew();
			guarantee4.PW_BondNumber = "GUA1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Guarantee Reference", "GUA1", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Guarantee Reference is the same as declaration guarantee reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Guarantee Reference is the same as declaration guarantee reference", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Guarantee Reference", "GUA2", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Guarantee is the same as declaration guarantee reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Guarantee is not the same as declaration guarantee reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Guarantee Reference", "GUA3", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Guarantee is the same as declaration guarantee reference", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Guarantee is not the same as declaration guarantee reference", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Guarantee Reference", "GUA4", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause Guarantee is not the same as declaration guarantee reference", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Guarantee is not the same as declaration guarantee reference", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestPackingTypeFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType("UNPKG", "Packagings", "UNE");
			helper.CreateCusCodeList("UNE", "UNPKG", "ABC", "abc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList("UNE", "UNPKG", "DEF", "def", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var declaration1 = Factory.New<JobDeclaration>();

			var packingGroups1 = declaration1.Bills.AddNew().PackingGroups.AddNew();
			var pack1 = packingGroups1.Packages.AddNew();
			pack1.CW_PackType = "AA";
			pack1.CW_PackQty = 1;
			var pack2 = packingGroups1.Packages.AddNew();
			pack2.CW_PackType = "BB";
			pack2.CW_PackQty = 1;
			var pack3 = packingGroups1.Packages.AddNew();
			pack3.CW_PackType = "CC";
			pack3.CW_PackQty = 1;

			var declaration2 = Factory.New<JobDeclaration>();

			var packingGroups2 = declaration2.Bills.AddNew().PackingGroups.AddNew();
			var pack4 = packingGroups2.Packages.AddNew();
			pack4.CW_PackType = "AA";
			pack4.CW_PackQty = 1;

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			AssertEquals("ABC, DEF", stripBO.Lookups.PackTypeList.CodesAsString);
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Packing Type", "AA", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Type is the same as declaration packing type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Packing Type is the same as declaration packing type", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Type", "BB", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Type is the same as declaration packing type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Type is not the same as declaration packing type", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Type", "CC", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Type is the same as declaration packing type", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Type is not the same as declaration packing type", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Type", "DD", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause Packing Type is not the same as declaration packing type", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Type is not the same as declaration packing type", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestPackingMarksFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();

			var packingGroups1 = declaration1.Bills.AddNew().PackingGroups.AddNew();
			var pack1 = packingGroups1.Packages.AddNew();
			pack1.CW_MarksAndNos = "MARK1";
			pack1.CW_PackQty = 1;
			var pack2 = packingGroups1.Packages.AddNew();
			pack2.CW_MarksAndNos = "MARK2";
			pack2.CW_PackQty = 1;
			var pack3 = packingGroups1.Packages.AddNew();
			pack3.CW_MarksAndNos = "MARK3";
			pack3.CW_PackQty = 1;

			var declaration2 = Factory.New<JobDeclaration>();

			var packingGroups2 = declaration2.Bills.AddNew().PackingGroups.AddNew();
			var pack4 = packingGroups2.Packages.AddNew();
			pack4.CW_MarksAndNos = "MARK1";
			pack4.CW_PackQty = 1;

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Packing Marks", "MARK1", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Marks is the same as declaration packing marks", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Packing Marks is the same as declaration packing marks", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Marks", "MARK2", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Marks is the same as declaration packing marks", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Marks is not the same as declaration packing marks", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Marks", "MARK3", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Packing Marks is the same as declaration packing marks", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Marks is not the same as declaration packing marks", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Packing Marks", "MARK4", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause Packing Marks is not the same as declaration packing marks", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Packing Marks is not the same as declaration packing marks", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestCustomsOfficeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();

			var customsOffice1 = declaration1.CustomsOffices.AddNew();
			customsOffice1.CY_Data = "ES009999";
			customsOffice1.CY_Code = "ENT";
			var customsOffice2 = declaration1.CustomsOffices.AddNew();
			customsOffice2.CY_Data = "ES009998";
			customsOffice2.CY_Code = "EXP";
			var customsOffice3 = declaration1.CustomsOffices.AddNew();
			customsOffice3.CY_Data = "ES002800";
			customsOffice3.CY_Code = "";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_CustomsOffice = "ES009998";

			var customsOffice4 = declaration2.CustomsOffices.AddNew();
			customsOffice4.CY_Data = "ES009999";
			customsOffice4.CY_Code = "ENT";
			var customsOffice5 = declaration2.CustomsOffices.AddNew();
			customsOffice5.CY_Data = "ES009999";
			customsOffice5.CY_Code = "EXP";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES009999", "ENT", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES009999, ENT)", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES009998", "EXP", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES009998, ENT)", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Customs Office is not the same as declaration customs office", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES002800", "", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES002800, )", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Customs Office is not the same as declaration customs office", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES009999", "EXP", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES009999, EXP)", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause Customs Office is not the same as declaration customs office", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES009998", "", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES009998, )", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause Customs Office is the same as declaration customs office", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionCustomsOfficeFilter("Customs Office", "ES08140", "ENT", declarationCollection, stripBO);

				AssertEquals("Total declarations match the filter (ES08140, ENT)", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause Customs Office is not the same as declaration customs office", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause Customs Office is not the same as declaration customs office", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestSealsNumberFilterContainers_FirstSeal()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerMode = "CNT";

			var container1 = declaration1.CusContainers.AddNew();
			container1.CO_Seal = "SEAL1";
			var container2 = declaration1.CusContainers.AddNew();
			container2.CO_Seal = "SEAL2";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ContainerMode = "CNT";

			var container3 = declaration2.CusContainers.AddNew();
			container3.CO_Seal = "SEAL1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with Seal Number with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has a container with Seal Number with value SEAL1", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with Seal Number with value SEAL2", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with Seal Number with value SEAL2", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL3", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no container with Seal Number with value SEAL3", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with Seal Number with value SEAL3", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestSealsNumberFilterContainers_SecondSeal()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerMode = "CNT";
			var container1 = declaration1.CusContainers.AddNew();
			container1.CO_SecondSeal = "SEAL1";
			var container2 = declaration1.CusContainers.AddNew();
			container2.CO_SecondSeal = "SEAL2";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ContainerMode = "CNT";
			var container3 = declaration2.CusContainers.AddNew();
			container3.CO_SecondSeal = "SEAL1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with Second Seal Number with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has a container with Second Seal Number with value SEAL1", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with Second Seal Number with value SEAL2", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with Second Seal Number with value SEAL2", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL3", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no container with Second Seal Number with value SEAL3", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with Second Seal Number with value SEAL3", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestExportExitedStatusFilter()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var entry1 = dec1.CustomsEntryHeaders.AddNew();
			entry1.CH_ExitedStatus = ZString.Empty;

			var dec2 = Factory.New<JobDeclaration>();
			var entry3 = dec2.CustomsEntryHeaders.AddNew();
			entry3.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;
			var entry4 = dec2.CustomsEntryHeaders.AddNew();
			entry4.CH_ExitedStatus = ExportExitStatus.Codes.ReminderForNonExitedGoodsReceived;

			var dec3 = Factory.New<JobDeclaration>();
			var entry5 = dec3.CustomsEntryHeaders.AddNew();
			entry5.CH_ExitedStatus = ExportExitStatus.Codes.ExitedSatisfactorily;

			var dec4 = Factory.New<JobDeclaration>();
			Factory.Save();

			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			CombineAssertions(() =>
			{
				var filter = (ModuleTextFilter)stripBO[EntryHeaderFilterBusinessObject.EUFilterConstants.ExitedStatus];
				AssertContainsExactElementsInAnyOrder("filter list", stripBO.Lookups.ExportExitStatusList, filter.List);

				filter.IsActive = true;
				filter.Property = ExportExitStatus.Codes.ExitedSatisfactorily;
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				Assert("Equal EXT not match empty exited status", !dec1.MatchesFilter(stripBO.Filter));
				Assert("Equal EXT match multiple headers", dec2.MatchesFilter(stripBO.Filter));
				Assert("Equal EXT match single header", dec3.MatchesFilter(stripBO.Filter));
				Assert("Equal EXT not match empty header", !dec4.MatchesFilter(stripBO.Filter));

				var operatorsToTest = new[] { SQLComparisonOperator.NotEqual, SQLComparisonOperator.NotContains, SQLComparisonOperator.DoesNotStartWith };
				foreach (var comparisonOperator in operatorsToTest)
				{
					filter.SqlComparisonOperator = comparisonOperator;
					Assert($"{comparisonOperator} EXT match empty", dec1.MatchesFilter(stripBO.Filter));
					Assert($"{comparisonOperator} EXT not match multiple headers", !dec2.MatchesFilter(stripBO.Filter));
					Assert($"{comparisonOperator} EXT not match single header", !dec3.MatchesFilter(stripBO.Filter));
					Assert($"{comparisonOperator} EXT match empty header", dec4.MatchesFilter(stripBO.Filter));
				}

				filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
				Assert("IsBlank match empty exited status", dec1.MatchesFilter(stripBO.Filter));
				Assert("IsBlank not match multiple headers", !dec2.MatchesFilter(stripBO.Filter));
				Assert("IsBlank not match single header", !dec3.MatchesFilter(stripBO.Filter));
				Assert("IsBlank match empty header", dec4.MatchesFilter(stripBO.Filter));

				filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
				Assert("IsNotBlank not match empty exited status", !dec1.MatchesFilter(stripBO.Filter));
				Assert("IsNotBlank match multiple headers", dec2.MatchesFilter(stripBO.Filter));
				Assert("IsNotBlank match single header", dec3.MatchesFilter(stripBO.Filter));
				Assert("IsNotBlank not match empty header", !dec4.MatchesFilter(stripBO.Filter));
			});
		}

		public void TestSealsNumberFilterEntryInstruction()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration1.CustomsEntryInstructions.AddNew();
			entryInstruction1.Seals.AddNew().CY_Data = "SEAL1";
			var entryInstruction2 = declaration1.CustomsEntryInstructions.AddNew();
			entryInstruction2.Seals.AddNew().CY_Data = "SEAL3";

			var declaration2 = Factory.New<JobDeclaration>();
			var entryInstruction3 = declaration2.CustomsEntryInstructions.AddNew();
			entryInstruction3.Seals.AddNew().CY_Data = "SEAL1";
			entryInstruction3.Seals.AddNew().CY_Data = "SEAL2";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an entryInstruction with Seals with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an entryInstruction with Seals with value SEAL1", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no entryInstruction with Seals with value SEAL2", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an entryInstruction with Seals with value SEAL2", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL3", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an entryInstruction with Seals with value SEAL3", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no entryInstruction with Seals with value SEAL3", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL4", declarationCollection, stripBO);
				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no entryInstruction with Seals with value SEAL4", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no entryInstruction with Seals with value SEAL4", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestSealsNumberFilterContainersAdditionalSeals()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerMode = "CNT";
			var container1 = declaration1.CusContainers.AddNew();
			container1.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";
			var container2 = declaration1.CusContainers.AddNew();
			container2.AdditionalSeals.AddNew().BK_SealNumber = "SEAL3";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ContainerMode = "CNT";
			var container3 = declaration2.CusContainers.AddNew();
			container3.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";
			container3.AdditionalSeals.AddNew().BK_SealNumber = "SEAL2";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with AdditionalSeals with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has a container with AdditionalSeals with value SEAL1", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no container with AdditionalSeals with value SEAL2", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has a container with AdditionalSeals with value SEAL2", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL3", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with AdditionalSeals with value SEAL3", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with AdditionalSeals with value SEAL3", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL4", declarationCollection, stripBO);
				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no container with AdditionalSeals with value SEAL4", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no container with AdditionalSeals with value SEAL4", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestSealsNumberFilterEquipments()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var equipment1 = declaration1.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL1";
			var equipment2 = declaration1.Equipments.AddNew();
			equipment2.CEQ_IdentificationNumber = "EQUIP2";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL3";

			var declaration2 = Factory.New<JobDeclaration>();

			var equipment3 = declaration2.Equipments.AddNew();
			equipment3.CEQ_IdentificationNumber = "EQUIP3";
			equipment3.Seals.AddNew().BK_SealNumber = "SEAL1";
			equipment3.Seals.AddNew().BK_SealNumber = "SEAL2";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an equipment with Seals with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an equipment with Seals with value SEAL1", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no equipment with Seals with value SEAL2", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an equipment with Seals with value SEAL2", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL3", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an equipment with Seals with value SEAL3", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no equipment with Seals with value SEAL3", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL4", declarationCollection, stripBO);
				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no equipment with Seals with value SEAL4", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no equipment with Seals with value SEAL4", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestSealsNumberFilterContainersAndEntryInstruction()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_ContainerMode = "CNT";
			var container1 = declaration1.CusContainers.AddNew();
			container1.CO_Seal = "SEAL1";

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_ContainerMode = "CNT";
			var container2 = declaration2.CusContainers.AddNew();
			container2.CO_SecondSeal = "SEAL1";

			var declaration3 = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration3.CustomsEntryInstructions.AddNew();
			entryInstruction1.Seals.AddNew().CY_Data = "SEAL1";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_ContainerMode = "CNT";
			var container3 = declaration4.CusContainers.AddNew();
			container3.AdditionalSeals.AddNew().BK_SealNumber = "SEAL1";

			var declaration5 = Factory.New<JobDeclaration>();
			var equipment1 = declaration5.Equipments.AddNew();
			equipment1.CEQ_IdentificationNumber = "EQUIP1";
			equipment1.Seals.AddNew().BK_SealNumber = "SEAL1";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 5, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL1", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 5, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has a container with Seal Number with value SEAL1", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has a container with Second Seal Number with value SEAL1", true, declarationCollection.Contains(declaration2));
				AssertEquals("Declaration 3 is in the filter cause it has an entryInstruction with Seals with value SEAL1", true, declarationCollection.Contains(declaration3));
				AssertEquals("Declaration 4 is in the filter cause it has a container with AdditionalSeals with value SEAL1", true, declarationCollection.Contains(declaration4));
				AssertEquals("Declaration 5 is in the filter cause it has an equipment with Seals with value SEAL1", true, declarationCollection.Contains(declaration5));

				LoadDeclarationCollectionTextFilter("Seal #", "SEAL2", declarationCollection, stripBO);
				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has no container with Seal Number with value SEAL2", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has no container with Second Seal Number with value SEAL2", false, declarationCollection.Contains(declaration2));
				AssertEquals("Declaration 3 is in the filter cause it has no entryInstruction with Seals with value SEAL2", false, declarationCollection.Contains(declaration3));
				AssertEquals("Declaration 4 is in the filter cause it has no container with AdditionalSeals with value SEAL2", false, declarationCollection.Contains(declaration4));
				AssertEquals("Declaration 5 is in the filter cause it has no equipment with Seals with value SEAL2", false, declarationCollection.Contains(declaration5));
			});
		}

		public void TestOriginInvoiceLineFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration1.Invoices.AddNew();
			var line1 = invoiceHeader1.InvoiceLines.AddNew();
			line1.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;
			var line2 = invoiceHeader1.InvoiceLines.AddNew();
			line2.JI_CountryOfOrigin = Core.Constants.CountryCodes.Italy;

			var declaration2 = Factory.New<JobDeclaration>();
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			var line3 = invoiceHeader2.InvoiceLines.AddNew();
			line3.JI_CountryOfOrigin = Core.Constants.CountryCodes.Spain;

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Origin - Invoice Line", "ES", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an Invoice Line with its Country of Origin is ES", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an Invoice Line with its Country of Origin is ES", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Origin - Invoice Line", "IT", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an Invoice Line with its Country of Origin is IT", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no Invoice Line with its Country of Origin is IT", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Origin - Invoice Line", "FR", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no Invoice Line with its Country of Origin is FR", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no Invoice Line with its Country of Origin is FR", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestPreferenceCodeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration1.Invoices.AddNew();
			var line1 = invoiceHeader1.InvoiceLines.AddNew();
			line1.JI_PrimaryPreference = "100";
			var line2 = invoiceHeader1.InvoiceLines.AddNew();
			line2.JI_PrimaryPreference = "300";

			var declaration2 = Factory.New<JobDeclaration>();
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			var line3 = invoiceHeader2.InvoiceLines.AddNew();
			line3.JI_PrimaryPreference = "100";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Preference", "100", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an Invoice Line with its Preference is 100", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause it has an Invoice Line with its Preference is 100", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Preference", "300", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause it has an Invoice Line with its Preference is 300", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no Invoice Line with its Preference is 300", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Preference", "200", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause it has no Invoice Line with its Preference is 200", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause it has no Invoice Line with its Preference is 200", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestDeclarantFilter_Enabled()
		{
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();

			AssertNotNull(stripBO[Customs.Module.DeclarationFilterConstants.OrgFilterTypes.Declarant]);
		}

		void CheckTextFilter(ZString filterField, ZString filterValue, JobDeclaration declaration, bool expectAHit = true, bool isExpensive = true)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleTextFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			if (expectAHit)
			{
				AssertEquals(filterField + " Expected count 1", 1, declarationCollection.Count);
				AssertEquals(filterField + " Expected declaration", true, declarationCollection.Contains(declaration));
			}
			else
			{
				AssertEquals(filterField + " Expected count 0", 0, declarationCollection.Count);
			}
			AssertEquals(filterField + " IsExpensiveQuery", isExpensive, filter.IsExpensiveQuery);
		}

		void LoadDeclarationCollectionNkFilter(ZString filterField, ZString filterValue, JobDeclarationCollection declarationCollection, JobDeclarationFilterBusinessObject stripBO)
		{
			var filter = (ModuleNkFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
		}

		void LoadDeclarationCollectionTextFilter(ZString filterField, ZString filterValue, JobDeclarationCollection declarationCollection, JobDeclarationFilterBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
		{
			var filter = (ModuleTextFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
		}

		void LoadDeclarationCollectionCustomsOfficeFilter(ZString filterField, ZString filterOfficeValue, ZString filterPurposeValue, JobDeclarationCollection declarationCollection, JobDeclarationFilterBusinessObject stripBO)
		{
			var filter = (CustomsOfficeFilter)stripBO[filterField];
			filter.Property = filterOfficeValue;
			filter.Purpose = filterPurposeValue;
			filter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
		}

		void CheckDateFilter(ZString filterField, ZDateTime filterValue, JobDeclaration declaration)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)stripBO[filterField];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = filterValue;
			filter.Property2 = ZDate.Empty;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			AssertEquals(1, declarationCollection.Count);
			Assert(declarationCollection.Contains(declaration));
		}

		void CheckDateFilterForHasNoDateEntered(ZString filterField, JobDeclaration declaration)
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			var filter = (ModuleDateFilter)stripBO[filterField];
			filter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
			AssertEquals(2, declarationCollection.Count);
			Assert(declarationCollection.Contains(declaration));
		}

		void LoadDeclarationCollectionDateFilter(ZString filterField, ZDateTime filterValue, JobDeclarationCollection declarationCollection, JobDeclarationFilterBusinessObject stripBO)
		{
			var filter = (ModuleDateFilter)stripBO[filterField];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.Property1 = filterValue;
			filter.Property2 = ZDate.Empty;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
		}

		public void TestSupportsMultipleVehicles()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(!filterBizObj.SupportsMultipleVehicles);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();
		FilterStripBusinessObject GetNewFilterStripBusinessObjectForSupportingTest() => new JobDeclarationFilterBusinessObjectForSupportingTest();
	}
	sealed class JobDeclarationFilterBusinessObjectForSupportingTest : JobDeclarationFilterBusinessObject
	{
		protected override bool SupportsExitControlCore => true;

		protected override bool SupportRequestedProcedureCore => true;

		protected override bool SupportsMultipleVehiclesCore => true;
	}
}
