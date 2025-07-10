using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportsExitControl()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(filterBizObj.SupportsExitControl);
		}

		public void TestJobDeclarationFilterLookups()
		{
			AssertType<JobDeclarationFilterLookups>(filterBusinessObject.Lookups);
		}

		public void TestJobDeclarationFilterWithIndirectExportsES()
		{
			var declaration1 = SetDeclarationWithEAD(EADPrintProcedureCodeList.Codes._0NoEADPrint);
			var declaration2 = SetDeclarationWithEAD(EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities);
			var declaration3 = SetDeclarationWithCustomData("ES009999", "ES009999000002");
			var declaration4 = SetDeclarationWithCustomData("ES009998", "ES009999000002");
			var declaration5 = SetDeclarationWithCustomData("ES009998", "9999000002", EuOfficeCodesTypes.Codes.OfficeOfExit, "ES009999");
			var declaration6 = SetDeclarationWithCustomData("ES009999", "9999000002", EuOfficeCodesTypes.Codes.OfficeOfExit, "ES009998");
			var declaration7 = SetDeclarationWithCustomData("ES009999", "ES009999000002", EuOfficeCodesTypes.Codes.OfficeOfExport, "ES009998");
			var declaration8 = SetDeclarationWithCustomData("ES009999", "9999000002", EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "ES009998");
			var declaration9 = SetDeclarationWithCustomData("ES009999", "9999000002", EuOfficeCodesTypes.Codes.OfficeOfEntryFirstOrSubsequent, "ES009998", MessageTypeList.Codes.Import);
			var declaration10 = SetDeclarationWithCheckIndirectExport(true);
			var declaration11 = SetDeclarationWithCheckIndirectExport(false);

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);
			CombineAssertions(() =>
			{
				AssertEquals("Total Declarations", 11, declarationCollection.Count);

				var filter = (ModuleFlagsFilter)stripBO["Indirect Export"];
				filter.IsActive = true;
				filter.Property0 = true;
				declarationCollection.Load(stripBO.Filter);
				AssertEquals("Total declarations match the filter", 5, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause EAD Procedure is 0", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause EAD Procedure is more than 0", true, declarationCollection.Contains(declaration2));
				AssertEquals("Declaration 3 is not in the filter cause CustomsOffice is the same as Location Office", false, declarationCollection.Contains(declaration3));
				AssertEquals("Declaration 4 is in the filter cause JE_CustomsOffice is not the same as Location Office", true, declarationCollection.Contains(declaration4));
				AssertEquals("Declaration 5 is not in the filter cause CustomsOffice is EXT and is the same as Location Office, no matters if JE_CustomsOffice is different", false, declarationCollection.Contains(declaration5));
				AssertEquals("Declaration 6 is in the filter cause CustomsOffice is EXT and is not the same as Location Office", true, declarationCollection.Contains(declaration6));
				AssertEquals("Declaration 7 is in the filter cause CustomsOffice is EXP and is not the same as Location Office", true, declarationCollection.Contains(declaration7));
				AssertEquals("Declaration 8 is not in the filter cause CustomsOffices is not EXT or EXP and CustomOffice is the same as Location Office", false, declarationCollection.Contains(declaration8));
				AssertEquals("Declaration 9 is not in the filter cause is an Import Declaration", false, declarationCollection.Contains(declaration9));
				AssertEquals("Declaration 10 is in the filter cause have an Import Declaration check true", true, declarationCollection.Contains(declaration10));
				AssertEquals("Declaration 11 is not in the filter cause have an Import Declaration check false", false, declarationCollection.Contains(declaration11));
			});
		}

		public void TestOriginStateIsland()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var invoiceHeader1 = declaration1.Invoices.AddNew();
			var line1 = invoiceHeader1.InvoiceLines.AddNew();
			line1.JI_StateOrRegionOfOrigin = "13";
			var line2 = invoiceHeader1.InvoiceLines.AddNew();
			line2.JI_StateOrRegionOfOrigin = "28";

			var declaration2 = Factory.New<JobDeclaration>();
			var invoiceHeader2 = declaration2.Invoices.AddNew();
			var line3 = invoiceHeader2.InvoiceLines.AddNew();
			line3.JI_StateOrRegionOfOrigin = "13";

			Factory.Save();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var stripBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			declarationCollection.Load(stripBO.Filter);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total Declarations", 2, declarationCollection.Count);

				LoadDeclarationCollectionTextFilter("Origin – State/Island", "13", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 2, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause has a Invoice Line with its State/Island of Origin is 13", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is in the filter cause has a Invoice Line with its State/Island of Origin is 13", true, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Origin – State/Island", "28", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 1, declarationCollection.Count);
				AssertEquals("Declaration 1 is in the filter cause has a Invoice Line with its State/Island of Origin is 28", true, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause has not a Invoice Line with its State/Island of Origin is 28", false, declarationCollection.Contains(declaration2));

				LoadDeclarationCollectionTextFilter("Origin – State/Island", "08", declarationCollection, stripBO);

				AssertEquals("Total Declarations match the filter", 0, declarationCollection.Count);
				AssertEquals("Declaration 1 is not in the filter cause has not a Invoice Line with its State/Island of Origin is 08", false, declarationCollection.Contains(declaration1));
				AssertEquals("Declaration 2 is not in the filter cause has not a Invoice Line with its State/Island of Origin is 08", false, declarationCollection.Contains(declaration2));
			});
		}

		public void TestParallel()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var declaration2 = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			entryHeader1.ZG_Parallel = true;
			entryHeader2.ZG_Parallel = false;
			Factory.Save();

			declarationCollection.Load(filterBusinessObject.Filter);
			AssertEquals("[PRE-CONDITION] When filter is not applied, EntryHeaderCollection Count", 2, declarationCollection.Count);

			var entryParallelFilter = (AddInfoModuleBooleanFilter)filterBusinessObject["Parallel"];
			entryParallelFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact;
			entryParallelFilter.Property = YesNoList.Codes.Yes;
			entryParallelFilter.IsActive = true;
			declarationCollection.Load(filterBusinessObject.Filter);

			CombineAssertions("[POST-CONDITION] When Exact 'Y' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 1, declarationCollection.Count);
				AssertEquals("Single EntryHeader found PK", declaration1.PK, declarationCollection[0].PK);
			});

			entryParallelFilter.Property = YesNoList.Codes.No;
			declarationCollection.Load(filterBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Exact 'N' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 1, declarationCollection.Count);
				AssertEquals("Single EntryHeader found PK", declaration2.PK, declarationCollection[0].PK);
			});

			entryParallelFilter.Property = "Z";
			declarationCollection.Load(filterBusinessObject.Filter);
			CombineAssertions("[POST-CONDITION] When Exact 'Z' filter is applied", () =>
			{
				AssertEquals("EntryHeaderCollection Count", 0, declarationCollection.Count);
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBusinessObject;

		JobDeclaration SetDeclarationWithEAD(string eadProcedure)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.EUH_EADPrintProcedure = eadProcedure;
			return declaration;
		}

		JobDeclaration SetDeclarationWithCheckIndirectExport(bool checkIndirectExport)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.IndirectExport = checkIndirectExport;
			return declaration;
		}

		JobDeclaration SetDeclarationWithCustomData(ZString customsOffice, ZString localtionOfGoods, string customOfficeType = "", string customOfficeData = "", string messageType = MessageTypeList.Codes.Export)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_CustomsOffice = customsOffice;
			declaration.JE_LocationOfGoods = localtionOfGoods;
			if (!string.IsNullOrEmpty(customOfficeType))
			{
				var customOffice = declaration.CustomsOffices.AddNew();
				customOffice.CY_Code = customOfficeType;
				customOffice.CY_Data = customOfficeData;
			}
			return declaration;
		}

		void LoadDeclarationCollectionTextFilter(ZString filterField, ZString filterValue, JobDeclarationCollection declarationCollection, JobDeclarationFilterBusinessObject stripBO, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.StartsWith)
		{
			var filter = (ModuleTextFilter)stripBO[filterField];
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			filter.IsActive = true;
			declarationCollection.Load(stripBO.Filter);
		}

		public void TestSupportsMultipleVehicles()
		{
			var filterBizObj = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();
			Assert(filterBizObj.SupportsMultipleVehicles);
		}

		public void TestVehicleBrandAndModelFilters() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();

			line.JI_BrandName = "BRAND";
			line.JI_Model = "MODEL";
			Factory.Save();

			CheckTextFilter("Vehicle Brand", "BRAND", declaration, false, isExpensive: false);
			CheckTextFilter("Vehicle Model", "MODEL", declaration, false, isExpensive: false);

			var vehicle = new CusVehicleCollection<Business.CusVehicle, BaseJobComInvoiceLine>(line).AddNew();
			vehicle.CVH_BrandName = "BRAND";
			vehicle.CVH_ModelName = "MODEL";
			Factory.Save();

			CheckTextFilter("Vehicle Brand", "BRAND", declaration, true, isExpensive: false);
			CheckTextFilter("Vehicle Model", "MODEL", declaration, true, isExpensive: false);

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
		});

		protected override void SetUp()
		{
			base.SetUp();
			filterBusinessObject = new JobDeclarationFilterBusinessObject();
		}
		JobDeclarationFilterBusinessObject filterBusinessObject;
	}
}
