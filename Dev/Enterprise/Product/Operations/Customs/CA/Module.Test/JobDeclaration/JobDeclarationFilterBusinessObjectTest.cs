using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Module.Testing
{
	[TestedType(typeof(JobDeclarationFilterBusinessObject))]
	sealed class JobDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestLegacyMergedB3CNeedsToRemergeToCADFilter()
		{
			using (Universal.ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.CarmR2, "CA", ZDateTime.Now, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = CA.Business.JobMessageTypeList.Codes.Import;
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;

				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "1";

				var line = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				var line2 = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
				var lineMerger = new LineMerger(declaration);
				lineMerger.DoMerge();

				var entryHeader = declaration.B3EntryHeader;
				var entryLine = entryHeader.MergedLines.AddNew();

				var link11 = Factory.New<Customs.Business.AdditionalInvoiceLineEntryLineLink>();
				link11.BU_JI = line.PK;
				link11.BU_CL = entryLine.PK;
				var link12 = Factory.New<Customs.Business.AdditionalInvoiceLineEntryLineLink>();
				link12.BU_JI = line2.PK;
				link12.BU_CL = entryLine.PK;
				declaration.CA_RequiresMerge = false;
				Factory.Save();

				var bizObj = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.B3NeedRemergeToCAD];
				AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
				filter.IsActive = true;
				filter.Property0 = true;
				var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
				declarations.Load(bizObj.Filter);
				AssertEquals("Matches filter", true, declarations.Contains(declaration));
			}
		}

		public void TestNoExceptionInCommonFilter()
		{
			JobDeclaration[] retrievedDeclarations;

			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleNumberFilter)filterBO["Common Numbers and References"];

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "1'23";

			var retrievingFactory = new BusinessObjectFactory();
			AssertNoExceptionThrown("Should have no nasty SQL injection exceptions", delegate
			{
				retrievedDeclarations = (JobDeclaration[])retrievingFactory.Load(typeof(JobDeclaration), filterBO.Filter);
			});

			filter.Property = "123";

			retrievedDeclarations = (JobDeclaration[])retrievingFactory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found NO declarations on exact search", 0, retrievedDeclarations.Length);
		}

		public void TestNotSentMessageStatus()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageStatus = MessageStatusList.Codes.NotSent;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageStatus = MessageStatusList.Codes.ClearDelete;

			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();
			var moduleFilter = filter[DeclarationFilterConstants.MessageStatus] as ModuleTextFilter;
			moduleFilter.Property = DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			moduleFilter.IsActive = true;

			Assert("dec1 matches", declaration1.MatchesFilter(filter.Filter));
			Assert("dec2 does not match", !declaration2.MatchesFilter(filter.Filter));
		}

		public void TestLookups()
		{
			JobDeclarationFilterBusinessObject filterBizObj = new JobDeclarationFilterBusinessObject();
			AssertEquals("Lookups of correct type", typeof(JobDeclarationFilterLookups), filterBizObj.Lookups.GetType());
		}

		public void TestExporterSearch()
		{
			TestOrganisationSearch(JobDeclaration.Schema.JE_OH_Supplier, 1, DeclarationFilterConstants.OrgFilterTypes.ExporterConsignee);
		}

		public void TestConsigneeSearch()
		{
			TestOrganisationSearch(JobDeclaration.Schema.JE_OH_Importer, 2, DeclarationFilterConstants.OrgFilterTypes.ExporterConsignee);
		}

		public void TestCarrierSearch()
		{
			TestOrganisationSearch(JobDeclaration.Schema.JE_OH_ShippingLine, 1, DeclarationFilterConstants.OrgFilterTypes.CarrierServiceProvider);
		}

		public void TestServiceProviderSearch()
		{
			TestOrganisationSearch(JobDeclaration.Schema.JE_OH_Forwarder, 2, DeclarationFilterConstants.OrgFilterTypes.CarrierServiceProvider);
		}

		public void TestEntryNumberFilter()
		{
			BusinessObject[] filteredDecs = null;
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "1";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.TransactionNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "2345000000012";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "2345000000013";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength));
		}

		public void TestFormKeyNumberFilterMaxLength()
		{
			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.FormKeyNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;

			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(CusEntryHeaderSchema.CH_BGMReference.MaxLength));
		}

		public void TestProofOfReportNumberFilter()
		{
			BusinessObject[] filteredDecs = null;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_CERSProofOfReportNumber = "XX12345000001";
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.ProofOfReportNumber];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "12345";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "123456";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			AssertEquals(filter.MaxLength, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength));
		}

		public void TestShipmentSubTyperFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryType];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "AB";
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "10";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			declaration.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;
			Factory.Save();

			filter.Property = CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.Warehouse101);
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
		}

		public void TestShipmentSubTypeFilterWithSQLComparisonOperator()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse102;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_MessageSubType = CADEntryTypeList.Codes.Warehouse101;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_MessageSubType = string.Empty;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryType];
			filter.IsActive = true;

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.Warehouse102);
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = CADEntryTypeList.ConvertToShortCode(CADEntryTypeList.Codes.Warehouse101);
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = "10";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);
		}

		public void TestDIFURNFilterAndDIFMessageStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var document11 = declaration1.DocsAndCartage.RequiredDocuments.AddNew();
			var addinfo111 = document11.AddInfos.AddNew();
			addinfo111.EX_ReferenceNumber = "10000000111";
			addinfo111.EX_Status = "AOC";
			addinfo111.EX_ApplicationCode = "CAD";
			Factory.Save();

			var filterURN = filterBO[DeclarationFilterConstants.NumberFilterTypes.DIFURN] as ModuleTextFilter;
			Assert("Should has Exact", filterURN.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.Exact));
			Assert("Should has StartWith", filterURN.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.StartsWith));
			Assert("Should has Contains", filterURN.ComparisonOperator_List.ContainsCode(ModuleNumberFilter.ComparisonConstants.Contains));

			filterURN.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterURN.IsActive = true;
			filterURN.Property = "10000000111";

			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filterURN.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterURN.Property = "10000000";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filterURN.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterURN.Property = "000001";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filterURN.Property = "10000000110";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			filterURN.IsActive = false;

			var filterStatus = filterBO[DeclarationFilterConstants.DIFMessageStatus] as ModuleTextFilter;
			filterStatus.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterStatus.IsActive = true;
			filterStatus.Property = "AOC";

			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filterStatus.Property = "AOB";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			AssertEquals(JobRequiredDocumentAddInfoSchema.EX_Status.MaxLength, filterStatus.MaxLength);
		}

		public void TestEXPStatusFilter()
		{
			var declaration01 = Factory.New<JobDeclaration>();
			var entryHeader01 = declaration01.CustomsEntryHeaders.AddNew();
			entryHeader01.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader01.CH_Status = MessageStatusList.Codes.ErrorOriginal;

			var declaration02 = Factory.New<JobDeclaration>();
			var entryHeader02 = declaration02.CustomsEntryHeaders.AddNew();
			entryHeader02.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader02.CH_Status = MessageStatusList.Codes.AwaitingOriginal;

			var declaration03 = Factory.New<JobDeclaration>();
			var entryHeader03 = declaration03.CustomsEntryHeaders.AddNew();
			entryHeader03.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader03.CH_Status = MessageStatusList.Codes.ClearReplace;

			var declaration04 = Factory.New<JobDeclaration>();
			var entryHeader04 = declaration04.CustomsEntryHeaders.AddNew();
			entryHeader04.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader04.CH_Status = MessageStatusList.Codes.AwaitingOriginal;

			var declaration05 = Factory.New<JobDeclaration>();
			var entryHeader05 = declaration05.CustomsEntryHeaders.AddNew();
			entryHeader05.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader05.CH_Status = MessageStatusList.Codes.ClearReplace;

			var declaration06 = Factory.New<JobDeclaration>();
			var entryHeader06 = declaration06.CustomsEntryHeaders.AddNew();
			entryHeader06.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader06.CH_Status = MessageStatusList.Codes.NotSent;

			var declaration07 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EXPStatus];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = MessageStatusList.Codes.Sent;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 6, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = MessageStatusList.Codes.ClearReplace;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = MessageStatusList.Codes.ClearReplace;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 6 Records", 6, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);
		}

		public void TestEntryMessageStatusFilter()
		{
			var declaration01 = Factory.New<JobDeclaration>();
			declaration01.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader01 = declaration01.CustomsEntryHeaders.AddNew();
			entryHeader01.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader01.CH_Status = MessageStatusList.Codes.ErrorOriginal;

			var declaration02 = Factory.New<JobDeclaration>();
			declaration02.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entryHeader02 = declaration02.CustomsEntryHeaders.AddNew();
			entryHeader02.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader02.CH_Status = MessageStatusList.Codes.AwaitingOriginal;

			var declaration03 = Factory.New<JobDeclaration>();
			declaration03.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryHeader03 = declaration03.CustomsEntryHeaders.AddNew();
			entryHeader03.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader03.CH_Status = MessageStatusList.Codes.ErrorOriginal;

			var declaration04 = Factory.New<JobDeclaration>();
			declaration04.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entryHeader04 = declaration04.CustomsEntryHeaders.AddNew();
			entryHeader04.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader04.CH_Status = MessageStatusList.Codes.AwaitingOriginal;

			var declaration05 = Factory.New<JobDeclaration>();
			declaration05.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			var entryHeader05 = declaration05.CustomsEntryHeaders.AddNew();
			entryHeader05.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader05.CH_Status = MessageStatusList.Codes.NotSent;

			var declaration06 = Factory.New<JobDeclaration>();
			declaration06.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entryHeader06 = declaration06.CustomsEntryHeaders.AddNew();
			entryHeader06.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader06.CH_Status = MessageStatusList.Codes.AwaitingOriginal;

			var declaration07 = Factory.New<JobDeclaration>();
			declaration07.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			var entryHeader07 = declaration07.CustomsEntryHeaders.AddNew();
			entryHeader07.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader07.CH_Status = MessageStatusList.Codes.NotSent;

			var declaration08 = Factory.New<JobDeclaration>();
			declaration08.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryMessageStatus];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.Property = MessageStatusList.Codes.AwaitingOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 4, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 2, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);
		}

		public void TestMessageStatusFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageStatus = MessageStatusList.Codes.ClearChange;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.MessageStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = "CLC";
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "CLD";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			AssertEquals(JobDeclarationSchema.JE_MessageStatus.MaxLength, filter.MaxLength);
		}

		public void TestReleaseStatusFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_EntryStatus = Common.CA.EDIReleaseImportEntryStatusList.Codes.GoodsReleased;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_EntryStatus = Common.CA.EDIReleaseImportEntryStatusList.Codes.Error;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.JE_EntryStatus = Common.CA.EDIReleaseImportEntryStatusList.Codes.ManualRelease;

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_EntryStatus = ZString.Empty;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[filterBO.EntryStatusText];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = Common.CA.EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.IsActive = true;
			filter.Property = Common.CA.EDIReleaseImportEntryStatusList.Codes.GoodsReleased;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);
		}

		public void TestFiltersForImport()
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			var impDeclaration = Factory.New<JobDeclaration>();
			impDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var expDeclaration = Factory.New<JobDeclaration>();
			expDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();

			var dueDateFilter = filter[DeclarationFilterConstants.DateFilterTypes.EstimatedPaymentDueDate] as ModuleDateFilter;
			dueDateFilter.IsActive = true;

			dueDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include IMP filtering", impDeclaration, declarationCollection);
			AssertCollectionNotContains("Filter should include IMP filtering", expDeclaration, declarationCollection);

			dueDateFilter.IsActive = false;

			var statusFilter = filter[DeclarationFilterConstants.EntryStatusText] as EntryStatusFilter;
			statusFilter.IsActive = true;

			statusFilter.Property = ExtraConstantCodes.Codes.NotEntryAccepted;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include IMP filtering", impDeclaration, declarationCollection);
			AssertCollectionNotContains("Filter should include IMP filtering", expDeclaration, declarationCollection);

			statusFilter.IsActive = false;

			var messageFilter = filter[DeclarationFilterConstants.EntryMessageStatus] as ModuleTextFilter;
			messageFilter.IsActive = true;
			messageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			messageFilter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include IMP filtering", impDeclaration, declarationCollection);
			AssertCollectionNotContains("Filter should include IMP filtering", expDeclaration, declarationCollection);
		}

		public void TestFiltersForLVS()
		{
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);

			var lvsDeclaration = Factory.New<JobDeclaration>();
			lvsDeclaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			lvsDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now;
			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();

			var shipmentFilter = filter[DeclarationFilterConstants.ShipmentType] as ModuleTextFilter;
			shipmentFilter.IsActive = true;
			shipmentFilter.Property = JobMessageTypeList.Codes.LowValueShipments;

			var dueDateFilter = filter[DeclarationFilterConstants.DateFilterTypes.EstimatedPaymentDueDate] as ModuleDateFilter;
			dueDateFilter.IsActive = true;

			dueDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include LVS filtering", lvsDeclaration, declarationCollection);

			dueDateFilter.IsActive = false;

			var submissionDate = filter[DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate] as ModuleDateFilter;
			submissionDate.IsActive = true;

			submissionDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include LVS filtering", lvsDeclaration, declarationCollection);

			submissionDate.IsActive = false;

			var acceptedDate = filter[DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate] as ModuleDateFilter;
			acceptedDate.IsActive = true;

			acceptedDate.PropertySearch = ModuleDateFilter.HasNoDateEntered;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include LVS filtering", lvsDeclaration, declarationCollection);

			acceptedDate.IsActive = false;

			var statusFilter = filter[DeclarationFilterConstants.EntryStatusText] as EntryStatusFilter;
			statusFilter.IsActive = true;

			statusFilter.Property = ExtraConstantCodes.Codes.NotEntryAccepted;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include LVS filtering", lvsDeclaration, declarationCollection);

			statusFilter.IsActive = false;

			var messageFilter = filter[DeclarationFilterConstants.EntryMessageStatus] as ModuleTextFilter;
			messageFilter.IsActive = true;
			messageFilter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			messageFilter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;

			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("Filter should include LVS filtering", lvsDeclaration, declarationCollection);
		}

		public void TestDateFilters()
		{
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseDate, JobDeclaration.Schema.JE_EntryAuthorisationDate, null);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ExportDate, JobDeclaration.Schema.JE_ExportDate, null);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.SubLocationETD, JobDeclaration.Schema.JE_WarehouseReleaseDate, null);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.K84AccountingDate, CAAddInfoSchema.Constants.CA_K84AccountingDate, null);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.EstimatedPaymentDueDate, CAAddInfoSchema.Constants.CA_EstimatedPaymentDueDate, null);

			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate, null, MessageTypeList.Codes.B3CUSDEC);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.ReleaseSubmissionDate, null, MessageTypeList.Codes.EDIRelease);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate, null, MessageTypeList.Codes.B3CUSDEC);

			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate, null, MessageTypeList.Codes.CommercialAccountingDeclaration);
			AssertDateFilter(DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate, null, MessageTypeList.Codes.CommercialAccountingDeclaration);
		}

		public void AssertDateFilter(ZString filterName, ZString datePropertyName, ZString messageType)
		{
			var factory = new BusinessObjectFactory();

			var declaration01 = factory.New<JobDeclaration>();
			declaration01.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration02 = factory.New<JobDeclaration>();
			declaration02.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration03 = factory.New<JobDeclaration>();
			declaration03.JE_MessageType = JobMessageTypeList.Codes.Import;

			if (!datePropertyName.IsEmpty)
			{
				declaration01[datePropertyName] = new ZDateTime(2011, 1, 2);
				declaration02[datePropertyName] = new ZDateTime(2011, 2, 1);
				declaration03[datePropertyName] = null;
			}
			else if (!messageType.IsEmpty)
			{
				var schema = filterName != DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate ? CusEntryHeaderSchema.CH_EntrySubmittedDate : CusEntryHeaderSchema.CH_EntryReleaseDate;

				var entryHeader01 = declaration01.CustomsEntryHeaders.AddNew();
				entryHeader01.CH_MessageType = messageType;
				entryHeader01[schema] = new ZDateTime(2011, 1, 2);

				var entryHeader02 = declaration02.CustomsEntryHeaders.AddNew();
				entryHeader02.CH_MessageType = messageType;
				entryHeader02[schema] = new ZDateTime(2011, 2, 1);

				var entryHeader03 = declaration03.CustomsEntryHeaders.AddNew();
				entryHeader03.CH_MessageType = messageType;
				entryHeader03[schema] = null;
			}
			else
			{
				Assert("Test requires either the datePropertyName or the messageType parameter", false);
			}

			factory.Save();

			var declarationCollection01 = new JobDeclarationCollection(factory, GlbCompany.CurrentCompany.PK);
			var filter = new JobDeclarationFilterBusinessObject();

			var submissionDateFilter = filter[filterName] as ModuleDateFilter;
			submissionDateFilter.IsActive = true;

			submissionDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			declarationCollection01.Load(filter.Filter);
			AssertCollectionNotContains("Filter should exclude dated header entries", declaration01, declarationCollection01);
			AssertCollectionNotContains("Filter should exclude dated header entries", declaration02, declarationCollection01);
			AssertCollectionContains("Filter should include un-dated header entries", declaration03, declarationCollection01);

			submissionDateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			declarationCollection01.Load(filter.Filter);
			AssertCollectionContains("Filter should include dated header entries", declaration01, declarationCollection01);
			AssertCollectionContains("Filter should include dated header entries", declaration02, declarationCollection01);
			AssertCollectionNotContains("Filter should exclude un-dated header entries", declaration03, declarationCollection01);

			submissionDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			submissionDateFilter.Property1 = new ZDateTime(2011, 1, 2);
			submissionDateFilter.Property2 = new ZDateTime(2011, 1, 3);
			declarationCollection01.Load(filter.Filter);
			AssertCollectionContains("Filter should include header entries within the specified date range", declaration01, declarationCollection01);
			AssertCollectionNotContains("Filter should exclude header entries outside the specified date range", declaration02, declarationCollection01);
			AssertCollectionNotContains("Filter should exclude un-dated header entries", declaration03, declarationCollection01);
		}

		public void TestPortOfficeFilters()
		{
			var filterBO = new JobDeclarationFilterBusinessObject();

			var portOfClearanceFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.PortOfClearance];
			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, portOfClearanceFilter.ModuleId);

			var portOfUnladingFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.PortOfUnlading];
			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, portOfUnladingFilter.ModuleId);

			var portOfExitFilter = (ModuleNkFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.PortOfExit];
			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, portOfExitFilter.ModuleId);

			var placeOfReport = (ModuleNkFilter)filterBO[DeclarationFilterConstants.PortFilterTypes.PlaceOfReport];
			AssertEquals(ModuleIDs.Customs.Universal.ZZRefCusCodeList, placeOfReport.ModuleId);
		}

		public void TestTextFilters()
		{
			TestTextFilter(DeclarationFilterConstants.PortFilterTypes.PlaceOfReport, CAAddInfoSchema.Constants.CA_PlaceOfReport);
			TestTextFilter(DeclarationFilterConstants.PortFilterTypes.PortOfExit, CAAddInfoSchema.Constants.CA_PortOfExit);
			TestTextFilter(DeclarationFilterConstants.PortFilterTypes.PortOfClearance, JobDeclarationSchema.Constants.JE_CustomsOffice);
			TestTextFilter(DeclarationFilterConstants.PortFilterTypes.PortOfUnlading, CAAddInfoSchema.Constants.CA_UnladingOffice);
			TestTextFilter(DeclarationFilterConstants.OrgFilterTypes.CarrierCode, JobDeclarationSchema.Constants.JE_CarrierCode);
			TestTextFilter(DeclarationFilterConstants.PortFilterTypes.SubLocation, JobDeclarationSchema.Constants.JE_LocationOfGoods);
			TestTextFilter(DeclarationFilterConstants.ServiceOption, CAAddInfoSchema.Constants.CA_ServiceOption);
		}

		public void TestTextFilter(string filterName, string propertyName)
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1[propertyName] = "111";
			declaration2[propertyName] = "222";

			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();
			((ModuleTextBaseFilter)filter[filterName]).Property = "111";
			((ModuleTextBaseFilter)filter[filterName]).IsActive = true;

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filter.Filter);

			AssertCollectionContains(filterName, declaration1, declarationCollection);
			AssertCollectionNotContains(filterName, declaration2, declarationCollection);
		}

		public void TestServiceOptionFilter()
		{
			var filterBizObj = new JobDeclarationFilterBusinessObject();
			var serviceOptionFilter = (ModuleTextBaseFilter)filterBizObj[DeclarationFilterConstants.ServiceOption];
			AssertEquals(CAAddInfoSchema.CA_ServiceOption.MaxLength, serviceOptionFilter.MaxLength);
		}

		public void TestCCNFilterNoMaxLengthExceedException()
		{
			var filter = new JobDeclarationFilterBusinessObject();
			var ccnFilter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.CCN];

			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filter.Filter);

			ccnFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			ccnFilter.Property = "RCL~EMISA~01735E~INHAZ~HAZCB17001225";
			ccnFilter.IsActive = true;

			AssertNoExceptionThrown("NoMaxLengthExceedException", () =>
			{
				declarationCollection.Load(filterBO.Filter);
			});
		}

		public void TestCCNFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration1.CargoControlNumbers.AddNew().CY_CargoControlNumber = "AAAA";
			var number = declaration1.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "BBBB";
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.CargoControlNumbers.AddNew().CY_CargoControlNumber = "AABB";
			number = declaration2.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "CCCC";
			var declaration3 = Factory.New<JobDeclaration>();
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.CargoControlNumbers.AddNew().CY_CargoControlNumber = "1111";
			var declaration5 = Factory.New<JobDeclaration>();
			declaration5.JE_MessageType = JobMessageTypeList.Codes.Import;
			number = declaration5.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "BBBB";
			var declaration6 = Factory.New<JobDeclaration>();
			declaration6.JE_MessageType = JobMessageTypeList.Codes.Import;
			number = declaration6.AdditionalReferenceNumbers.AddNew();
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "AAAAXXXX";
			Factory.Save();

			var filter = new JobDeclarationFilterBusinessObject();
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).IsActive = true;
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "BB";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			var declarationCollection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.NotContains;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "CC";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Contains;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.NotContains;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "AA";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "BB";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "AAAA";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "CCCC";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SpecialComparisonOperator.IsBlank;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration1, declarationCollection);
			AssertCollectionNotContains("CCN", declaration2, declarationCollection);
			AssertCollectionContains("CCN", declaration3, declarationCollection);
			AssertCollectionNotContains("CCN", declaration4, declarationCollection);
			AssertCollectionNotContains("CCN", declaration5, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SpecialComparisonOperator.IsNotBlank;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration1, declarationCollection);
			AssertCollectionContains("CCN", declaration2, declarationCollection);
			AssertCollectionNotContains("CCN", declaration3, declarationCollection);
			AssertCollectionContains("CCN", declaration4, declarationCollection);
			AssertCollectionContains("CCN", declaration5, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "AAAAXXXX";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			declarationCollection.Load(filter.Filter);
			AssertCollectionContains("CCN", declaration6, declarationCollection);

			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).Property = "AAAA XXXX";
			((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).SqlComparisonOperator = SQLComparisonOperator.Equal;
			declarationCollection.Load(filter.Filter);
			AssertCollectionNotContains("CCN", declaration6, declarationCollection);

			AssertEquals(((ModuleNumberFilter)filter[DeclarationFilterConstants.NumberFilterTypes.CCN]).MaxLength, ModuleNumberFilter.MultiplyMaxLength(CusEntryNumSchema.CE_EntryNum.MaxLength));
		}

		public void TestLVXJobsNotAppear()
		{
			var dec1 = Factory.New<JobDeclaration>();
			dec1.JE_MessageType = JobMessageTypeList.Codes.LVSForConsolidation;
			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			var filterObj = new JobDeclarationFilterBusinessObject();

			Assert(!dec1.MatchesFilter(filterObj.Filter));
			Assert(dec2.MatchesFilter(filterObj.Filter));

			//Test for Web
			var oldValue = Globals.IsWeb;
			try
			{
				Globals.IsWeb = true;
				filterObj = new JobDeclarationFilterBusinessObject();
				Assert(dec1.MatchesFilter(filterObj.Filter));
				Assert(dec2.MatchesFilter(filterObj.Filter));
			}
			finally
			{
				Globals.IsWeb = oldValue;
			}
		}

		public void TestReleaseMessageFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader1.CH_Status = MessageStatusList.Codes.Sent;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader2.CH_Status = MessageStatusList.Codes.AcknowledgedChange;
			var entryHeader3 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader3.CH_Status = MessageStatusList.Codes.AcknowledgedChange;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ReleaseMessage];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.Sent;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = MessageStatusList.Codes.AcknowledgedChange;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			entryHeader1.CH_Status = MessageStatusList.Codes.ClearDelete;
			Factory.Save();
			filter.Property = MessageStatusList.Codes.ClearDelete;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			AssertEquals(CusEntryHeaderSchema.CH_Status.MaxLength, filter.MaxLength);
		}

		public void TestReleaseMessageStatusFilter()
		{
			var declaration01 = Factory.New<JobDeclaration>();
			var entryHeader01 = declaration01.CustomsEntryHeaders.AddNew();
			entryHeader01.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader01.CH_Status = MessageStatusList.Codes.ErrorChange;

			var declaration02 = Factory.New<JobDeclaration>();
			var entryHeader02 = declaration02.CustomsEntryHeaders.AddNew();
			entryHeader02.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader02.CH_Status = MessageStatusList.Codes.ClearOriginal;

			var declaration03 = Factory.New<JobDeclaration>();
			var entryHeader03 = declaration03.CustomsEntryHeaders.AddNew();
			entryHeader03.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader03.CH_Status = MessageStatusList.Codes.ClearReplace;

			var declaration04 = Factory.New<JobDeclaration>();
			var entryHeader04 = declaration04.CustomsEntryHeaders.AddNew();
			entryHeader04.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader04.CH_Status = MessageStatusList.Codes.NotSent;

			var declaration05 = Factory.New<JobDeclaration>();
			var entryHeader05 = declaration05.CustomsEntryHeaders.AddNew();
			entryHeader05.CH_MessageType = MessageTypeList.Codes.G7Export;
			entryHeader05.CH_Status = MessageStatusList.Codes.ErrorChange;

			var declaration06 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ReleaseMessage];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.ClearOriginal;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.ClearOriginal;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 5 Records", 5, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.ClearReplace;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.DoesNotStartWith;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.ClearReplace;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 5, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.IsNotBlank;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Customs.Module.DeclarationFilterConstants.EntryStatus.NotSentForFilter;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.NotEqual;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			filter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filter.Property = Common.Shared.MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);
		}

		public void TestEntryStatusFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_DeclarationReference = "declaration";
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader1.CH_EntryStatus = B3EntryStatusList.Codes.Accepted;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader2.CH_EntryStatus = Common.CA.EDIReleaseImportEntryStatusList.Codes.Error;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.JE_DeclarationReference = "declaration2";
			var entryHeader3 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader3.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			entryHeader3.CH_EntryStatus = B3EntryStatusList.Codes.Error;
			var entryHeader4 = declaration2.CustomsEntryHeaders.AddNew();
			entryHeader4.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader4.CH_EntryStatus = Common.CA.EDIReleaseImportEntryStatusList.Codes.Error;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration3.JE_DeclarationReference = "declaration3";

			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.JE_DeclarationReference = "declaration4";
			var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
			entryHeader5.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader5.CH_EntryStatus = B3EntryStatusList.Codes.Confirmed;
			Factory.Save();

			var filter = (EntryStatusFilter)filterBO[DeclarationFilterConstants.EntryStatusText];

			AssertEquals(false, filter.ShowComparisonOperator);
			AssertEquals(false, filter.ShowFilterType);

			filter.IsActive = true;
			filter.Property = B3EntryStatusList.Codes.Accepted;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
			AssertEquals("Search result", "declaration", ((JobDeclaration)(filteredDecs[0])).JE_DeclarationReference);

			filter.Property = Common.CA.EDIReleaseImportEntryStatusList.Codes.Error;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "NOT";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			AssertEquals("Search result", "declaration2", ((JobDeclaration)(filteredDecs[0])).JE_DeclarationReference);

			AssertEquals(CusEntryHeaderSchema.CH_EntryStatus.MaxLength, filter.MaxLength);
		}

		public void TestEntryMessageFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader1.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			entryHeader1.CH_Status = MessageStatusList.Codes.Sent;
			var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			entryHeader2.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entryHeader2.CH_Status = MessageStatusList.Codes.AcknowledgedChange;
			Factory.Save();

			ModuleTextFilter filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryMessageStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.Sent;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = MessageStatusList.Codes.AcknowledgedChange;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			AssertEquals(CusEntryHeaderSchema.CH_Status.MaxLength, filter.MaxLength);

			entryHeader1.CH_MessageType = MessageTypeList.Codes.CommercialAccountingDeclaration;
			filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryMessageStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = MessageStatusList.Codes.Sent;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
		}

		public void TestEmptyFiltersOnNonMergedDeclarations()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = CAJobMessageTypeList.Codes.Import;
			Factory.Save();

			var filterMessage = (ModuleTextFilter)filterBO[DeclarationFilterConstants.EntryMessageStatus];
			filterMessage.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterMessage.Property = ExtraConstantCodes.Codes.NotEntryAccepted;
			filterMessage.IsActive = true;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			filterMessage.IsActive = false;

			var filterStatus = (EntryStatusFilter)filterBO[DeclarationFilterConstants.EntryStatusText];
			filterStatus.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filterStatus.Property = ExtraConstantCodes.Codes.NotEntryAccepted;
			filterStatus.IsActive = true;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			filterStatus.IsActive = false;

			var subDateFilter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntrySubmissionDate];
			subDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			subDateFilter.IsActive = true;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
			subDateFilter.IsActive = false;

			var accDateFilter = (ModuleDateFilter)filterBO[DeclarationFilterConstants.DateFilterTypes.EntryAcceptedDate];
			accDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			accDateFilter.IsActive = true;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Record", 1, filteredDecs.Length);
		}

		public void TestAVSStatusFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.CA_OGDStatus = AVSStatusList.Codes.Error;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CA_OGDStatus = AVSStatusList.Codes.NotValidated;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.OGDStatus];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = AVSStatusList.Codes.NotValidated;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = AVSStatusList.Codes.Rejected;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);

			AssertEquals(CAAddInfoSchema.CA_OGDStatus.MaxLength, filter.MaxLength);
		}

		public void TestAccountingAgeFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.CA_AccountingAge = 1;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CA_AccountingAge = 10;
			var declaration3 = Factory.New<JobDeclaration>();
			Factory.Save();

			var filter = (ModuleNumberRangeFilter)filterBO[DeclarationFilterConstants.AccountingAge];
			filter.IsActive = true;

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.GreaterThanOrEqualTo;
			filter.Property1 = 3;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 3;
			filter.Property2 = 10;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.LessThanOrEqualTo;
			filter.Property2 = 10;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 1;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 4;
			filter.Property2 = 5;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			AssertContains("JE_ClusterKey IN (SELECT JE_ClusterKey FROM dbo.CAJobDeclaration WHERE JE_AccountingAge >= 4 AND JE_AccountingAge <= 5)", filterBO.Filter.LiteralTextADO);
			AssertNotContains("JE_ClusterKey IN (SELECT JE_ClusterKey FROM dbo.CAJobDeclaration WHERE JE_AccountingAge IS NULL)", filterBO.Filter.LiteralTextADO);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.EqualTo;
			filter.Property1 = 0;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.PropertySearch = ModuleNumberRangeFilter.SearchTexts.Between;
			filter.Property1 = 0;
			filter.Property2 = 1;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);
			AssertContains("JE_ClusterKey IN (SELECT JE_ClusterKey FROM dbo.CAJobDeclaration WHERE JE_AccountingAge >= 0 AND JE_AccountingAge <= 1)", filterBO.Filter.LiteralTextADO);
			AssertContains("JE_ClusterKey IN (SELECT JE_ClusterKey FROM dbo.CAJobDeclaration WHERE JE_AccountingAge IS NULL)", filterBO.Filter.LiteralTextADO);
		}

		public void TestImporterOfRecordFilter()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "OH1";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "OH2";
			var orgAddress = org2.Addresses.AddNew();
			orgAddress.OA_Address1 = "Address1";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration2.ImporterOfRecordAddress.E2_OA_Address = org1.MainAddress.PK;
			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration3.ImporterOfRecordAddress.E2_OA_Address = org2.MainAddress.PK;
			var declaration4 = Factory.New<JobDeclaration>();
			declaration4.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration4.ImporterOfRecordAddress.E2_OA_Address = orgAddress.PK;
			Factory.Save();

			var filter = (ModuleGuidFilter)filterBO[DeclarationFilterConstants.OrgFilterTypes.ImporterOfRecord];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = ZGuid.Empty;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 4 Records", 4, filteredDecs.Length);

			filter.Property = org1.PK;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = org2.PK;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);
		}

		public void TestOrganizationFilters()
		{
			org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "ORG2";
			orgAddress = org2.Addresses.AddNew();
			orgAddress.OA_Address1 = "Address1";

			declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice1 = declaration1.Invoices.AddNew();
			declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice2 = declaration2.Invoices.AddNew();
			declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = JobMessageTypeList.Codes.Import;
			invoice3 = declaration3.Invoices.AddNew();
			Factory.Save();

			TestOrganizationFilterDeclaration(DeclarationFilterConstants.OrgFilterTypes.ImporterOfRecord, declaration => declaration.ImporterOfRecordAddress);
			TestOrganizationFilterDeclaration(DeclarationFilterConstants.OrgFilterTypes.BondedWarehouse, declaration => declaration.WarehouseDocAddress);
			TestOrganizationFilterDeclaration(DeclarationFilterConstants.OrgFilterTypes.Originator, declaration => declaration.CommercialInvoiceOriginator);
			TestOrganizationFilterDeclaration(DeclarationFilterConstants.OrgFilterTypes.ExportSeller, declaration => declaration.VendorDocAddress);
			TestOrganizationFilterInvoice(DeclarationFilterConstants.OrgFilterTypes.InvoiceVendor, invoice => invoice.SupplierDocumentaryAddress);
			TestOrganizationFilterInvoicePurchaser(DeclarationFilterConstants.OrgFilterTypes.InvoicePurchaser);
			TestOrganizationFilterInvoice(DeclarationFilterConstants.OrgFilterTypes.InvoiceConsignee, invoice => invoice.FinalConsigneeAddress);
			TestOrganizationFilterInvoice(DeclarationFilterConstants.OrgFilterTypes.InvoiceShipper, invoice => invoice.SupplierPickupDeliveryAddress);
			TestOrganizationFilterInvoice(DeclarationFilterConstants.OrgFilterTypes.InvoiceOriginator, invoice => invoice.CommercialInvoiceOriginator);
			TestOrganizationFilterInvoiceExporter(DeclarationFilterConstants.OrgFilterTypes.InvoiceExporter);
			TestOrganizationFilterInvoiceManufacturer(DeclarationFilterConstants.OrgFilterTypes.InvoiceManufacturer);
			declaration1.JE_MessageType = declaration2.JE_MessageType = declaration3.JE_MessageType = JobMessageTypeList.Codes.Export;
		}

		public void TestExceptionCodeFilter()
		{
			var declaration01 = Factory.New<JobDeclaration>();
			declaration01.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration01.CA_DeclarationException = "010";
			var declaration02 = Factory.New<JobDeclaration>();
			declaration02.JE_MessageType = JobMessageTypeList.Codes.ImportCopyforB2;
			declaration02.CA_DeclarationException = "020";
			var declaration03 = Factory.New<JobDeclaration>();
			declaration03.JE_MessageType = JobMessageTypeList.Codes.B2Adjustments;
			declaration03.CA_DeclarationException = "030";
			var declaration04 = Factory.New<JobDeclaration>();
			declaration04.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			declaration04.CA_DeclarationException = "040";
			var declaration05 = Factory.New<JobDeclaration>();
			declaration05.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration05.CA_DeclarationException = "050";
			var declaration06 = Factory.New<JobDeclaration>();
			declaration06.JE_MessageType = JobMessageTypeList.Codes.Import;
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.ExceptionCode];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = CAExceptionCodeList.Codes.ReleaseAndIIDSentNoResponse;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = "NOT BLANK";
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 3 Records", 3, filteredDecs.Length);

			AssertEquals(GenAddOnColumnSchema.XA_Data.MaxLength, filter.MaxLength);
		}

		public void TestWHSStatusFilter()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.WHSStatus];
			filter.Property = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated;
			filter.IsActive = true;

			var testDec1 = Factory.New<JobDeclaration>();
			var testDec1Entry = testDec1.CustomsEntryHeaders.AddNew();
			testDec1Entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testDec1.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdated;

			var testDec2 = Factory.New<JobDeclaration>();
			var testDec2Entry = testDec2.CustomsEntryHeaders.AddNew();
			testDec2Entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testDec2.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;

			var testDec3 = Factory.New<JobDeclaration>();
			var testDec3Entry = testDec3.CustomsEntryHeaders.AddNew();
			testDec3Entry.CH_MessageType = MessageTypeList.Codes.B3CUSDEC;
			testDec3.WarehouseTransactionStatus = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;
			Factory.Save();

			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Not decs", 0, collection.Count);

			filter.Property = Customs.Business.WarehouseTransactionStatusList.Codes.InwardUpdated;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 1, collection.Count);
			AssertCollectionContains(testDec1, collection);

			filter.Property = ZString.Empty;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 3, collection.Count);
			AssertCollectionContains(testDec1, collection);
			AssertCollectionContains(testDec2, collection);
			AssertCollectionContains(testDec3, collection);

			filter.Property = Customs.Business.WarehouseTransactionStatusList.Codes.InwardCanceled;
			collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Decs", 2, collection.Count);
			AssertCollectionContains(testDec2, collection);
			AssertCollectionContains(testDec3, collection);
		}

		public void TestCSAReleaseOnlyFilter()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.CA_CSAEntry = true;
			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.CA_CSAEntry = false;
			Factory.Save();

			var bizObj = new JobDeclarationFilterBusinessObject();
			var filter = (ModuleFlagsFilter)bizObj[DeclarationFilterConstants.CSAReleaseOnly];
			AssertEquals(FilterCategories.StatusAndFlags, filter.Category);
			filter.IsActive = true;
			filter.Property0 = true;
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			declarations.Load(bizObj.Filter);
			AssertEquals("declaration1 matches filter", true, declarations.Contains(declaration1));
			AssertEquals("declaration2 does not match filter", false, declarations.Contains(declaration2));
		}

		public void TestLPCODIFURN()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lpco1 = declaration.LPCOViews.AddNew();
			lpco1.CLP_DIFRefNumberOrLocation = "1122";
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var lpco2 = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			lpco2.CLP_DIFRefNumberOrLocation = "5566";
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LPCODIFURN];
			Assert("Should not has NotEqual", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			Assert("Should not has NotContain", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotContain));
			Assert("Should not has NotStartsWith", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			Assert("Should not has IsBlank", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert("Should not has IsNotBlank", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));

			filter.IsActive = true;
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "1122", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "3344", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "1122", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "3344", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "11", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "22", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "12", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "21", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "5566", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "7788", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "5566", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "7788", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "55", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "66", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "56", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "65", 0);
		}

		public void TestLPCORefNo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var lpco1 = declaration.LPCOViews.AddNew();
			lpco1.CLP_RefNo = "3344";
			var invoiceLine = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.CA_GACInd = "Y";
			var lpco2 = invoiceLine.GACPGAHeader.LPCOViews.AddNew();
			lpco2.CLP_RefNo = "7788";
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.LPCORefNo];
			Assert("Should not has NotEqual", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotEqual));
			Assert("Should not has NotContain", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotContain));
			Assert("Should not has NotStartsWith", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.NotStartsWith));
			Assert("Should not has IsBlank", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsBlank));
			Assert("Should not has IsNotBlank", !filter.ComparisonOperator_List.ContainsCode(ModuleTextFilter.ComparisonConstants.IsNotBlank));

			filter.IsActive = true;
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "3344", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "1122", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "3344", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "1122", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "33", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "44", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "34", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "43", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "7788", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Equal, "5566", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "7788", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.NotEqual, "5566", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "77", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.StartsWith, "88", 0);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "78", 1);
			AssertLPCOFilter(filter, SQLComparisonOperator.Contains, "87", 0);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = (JobDeclarationFilterBusinessObject)GetNewFilterStripBusinessObject();

			((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = true;
			try
			{
				//GlbCompany.CurrentCompany.SetCountry changes GC_RN_NKCountryCode, and needs to be saved to db as JobDeclarationFilter(DBOnlyQuery) is performed in FilterObject.
				GlbCompany.CurrentCompany.Factory.Save();
			}
			finally
			{
				((IBusinessObjectFactoryInternals)GlbCompany.CurrentCompany.Factory).CanSave = false;
			}
		}

		JobDeclarationFilterBusinessObject filterBO;
		OrgHeader org1;
		OrgHeader org2;
		OrgAddress orgAddress;
		JobDeclaration declaration1;
		JobDeclaration declaration2;
		JobDeclaration declaration3;
		JobComInvoiceHeader invoice1;
		JobComInvoiceHeader invoice2;
		JobComInvoiceHeader invoice3;
		delegate JobDocAddress JobDocAddressOfDeclaration(JobDeclaration declaration);
		delegate JobDocAddress JobDocAddressOfInvoice(JobComInvoiceHeader invoice);

		void TestOrganisationSearch(string declPropName, int property1Or2, string orgFilterType)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "_!_!_!";
			var declaration = Factory.New<JobDeclaration>();
			declaration[declPropName] = org.PK;
			var filter = (ModuleGuidsFilter)filterBO[orgFilterType];
			if (property1Or2 == 1)
			{
				filter.Property1 = org.PK;
			}
			else
			{
				filter.Property2 = org.PK;
			}
			filter.IsActive = true;

			Factory.Save();
			var collection = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			collection.Load(filterBO.Filter);
			AssertEquals("Matched " + orgFilterType + " on declaration", 1, collection.Count);
			AssertEquals("Matched " + orgFilterType + " on declaration", org.PK, collection[0][declPropName]);
		}

		void TestOrganizationFilterDeclaration(ZString filterType, JobDocAddressOfDeclaration jobDocAddress)
		{
			jobDocAddress(declaration1).E2_OA_Address = org1.MainAddress.PK;
			jobDocAddress(declaration2).E2_OA_Address = org2.MainAddress.PK;
			jobDocAddress(declaration3).E2_OA_Address = orgAddress.PK;
			Factory.Save();
			AssertOrganizationFilter(filterType);
		}

		void TestOrganizationFilterInvoicePurchaser(ZString filterType)
		{
			invoice1.JZ_OH_Buyer = org1.PK;
			invoice2.JZ_OH_Buyer = org2.PK;
			invoice3.JZ_OH_Buyer = org2.PK;
			Factory.Save();
			AssertOrganizationFilter(filterType);
		}

		void TestOrganizationFilterInvoice(ZString filterType, JobDocAddressOfInvoice jobDocAddress)
		{
			jobDocAddress(invoice1).E2_OA_Address = org1.MainAddress.PK;
			jobDocAddress(invoice2).E2_OA_Address = org2.MainAddress.PK;
			jobDocAddress(invoice3).E2_OA_Address = orgAddress.PK;
			Factory.Save();
			AssertOrganizationFilter(filterType);
		}

		void TestOrganizationFilterInvoiceExporter(ZString filterType)
		{
			invoice1.ExporterDocumentaryAddress.OrganisationPK = org1.PK;
			invoice2.ExporterDocumentaryAddress.OrganisationPK = org2.PK;
			invoice3.ExporterDocumentaryAddress.OrganisationPK = org2.PK;
			Factory.Save();
			AssertOrganizationFilter(filterType);
		}

		void TestOrganizationFilterInvoiceManufacturer(ZString filterType)
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "111";
			var manufacturerAddress = manufacturer.Addresses.AddNewMainAddress();
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_OA_ManufacturerAddress = manufacturerAddress.PK;
			line1.JI_Description = "a description";

			Factory.Save();
			var filter = (ModuleGuidFilter)filterBO[filterType];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = manufacturer.PK;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);
		}

		void AssertOrganizationFilter(ZString filterType)
		{
			var filter = (ModuleGuidFilter)filterBO[filterType];
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filter.Property = org1.PK;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

			filter.Property = org2.PK;
			filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals("Should have found 2 Records", 2, filteredDecs.Length);
			filter.IsActive = false;
		}

		void AssertLPCOFilter(ModuleTextFilter filter, SQLComparisonOperator sQLComparisonOperator, ZString property, int count)
		{
			filter.SqlComparisonOperator = sQLComparisonOperator;
			filter.Property = property;
			var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
			AssertEquals(string.Format("Should have found {0} Records", count.ToString()), count, filteredDecs.Length);
		}

		public void TestSuretyCodeFilter()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CA_SuretyCode = "123";
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.BondSurety];
				AssertEquals(null, filter);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.BondSurety];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filter.Property = "123";
				var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

				filter.Property = "3";
				filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			}
		}

		public void TestBondtypeFilter()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CA_BondType = "8";
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.BondType];
				AssertEquals(null,filter);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.BondType];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filter.Property = "8";
				var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

				filter.Property = "9";
				filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			}	
		}

		public void TestBondNumberFilter()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.CA_BondNo = "123";
			Factory.Save();

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, false))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.BondNumber];
				AssertEquals(null, filter);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Today, true))
			{
				var filterBO = new JobDeclarationFilterBusinessObject();
				var filter = (ModuleNumberFilter)filterBO[DeclarationFilterConstants.NumberFilterTypes.BondNumber];
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filter.Property = "123";
				var filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 1 Records", 1, filteredDecs.Length);

				filter.Property = "3";
				filteredDecs = Factory.Load(typeof(JobDeclaration), filterBO.Filter);
				AssertEquals("Should have found 0 Records", 0, filteredDecs.Length);
			}
		}
	}
}
