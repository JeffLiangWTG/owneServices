using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5SGFilterStripBusinessObject))]
	sealed class EntryDetailsFor5SGFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryDetailsFor5SGFilterStripBusinessObject();

		public void TestFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.Payer]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.PayerCompanyName]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.Importer]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.ImporterCompanyName]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.EstimatedDateOfFinalPrice]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.ClearedDate]);
			AssertNotNull(filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.AcceptedDate]);
		}

		public void TestPayer()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var payerFilter = (ModuleGuidFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.Payer];
			payerFilter.Property = payer;
			payerFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123452X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestImporter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var importerFilter = (ModuleGuidFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.Importer];
			importerFilter.Property = importer;
			importerFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123451X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestPayerCompanyNameFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var payerCompanyNameFilter = (ModuleTextFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.PayerCompanyName];
			payerCompanyNameFilter.Property = "RK TestData3";
			payerCompanyNameFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123452X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestImporterCompanyNameFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var payerCompanyNameFilter = (ModuleTextFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.ImporterCompanyName];
			payerCompanyNameFilter.Property = "RK TestData2";
			payerCompanyNameFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123451X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestAcceptedDateFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var timeFilter = (ModuleDateFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.AcceptedDate];
			timeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			timeFilter.Property2 = new ZDateTime(2022, 01, 01);
			timeFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123450X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestEstimatedDateFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var timeFilter = (ModuleDateFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.EstimatedDateOfFinalPrice];
			timeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			timeFilter.Property2 = new ZDateTime(2022, 02, 01);
			timeFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123451X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestClearedDateTimeFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var timeFilter = (ModuleDateFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.ClearedDate];
			timeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			timeFilter.Property2 = new ZDateTime(2022, 03, 01);
			timeFilter.IsActive = true;

			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5SG();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals("1234522123452X", coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		public void TestDefaultFilter()
		{
			var filter = new EntryDetailsFor5SGFilterStripBusinessObject();
			var estimatedDateFilter = (ModuleDateFilter)filter[EntryDetailsFor5SGFilterStripBusinessObject.Schema.EstimatedDateOfFinalPrice];
			AssertEquals(ModuleDateFilter.DateRangeSearchTexts.Next14Days, estimatedDateFilter.PropertySearch);
		}

		protected override void SetUp()
		{
			base.SetUp();
			#region TestData1
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Category = "BUS";
			orgHeader1.OH_Code = "RK1";
			orgHeader1.OH_FullName = "RK TestData1";

			var declaration1 = Factory.New<JobDeclaration>();
			declaration1.JE_MessageType = "IMP";
			declaration1.JE_ApplicationCode = "BLT";
			declaration1.JE_OH_Supplier = orgHeader1.PK;
			declaration1.JE_OA_SupplierAddress = orgHeader1.MainAddress.PK;

			var invoice1 = declaration1.Invoices.AddNew();
			invoice1.JZ_NoOfPacks = 100;
			invoice1.JZ_Weight = 100;
			invoice1.JZ_WeightUQ = "KG";

			var entry1 = declaration1.CustomsEntryHeaders.AddNew();
			entry1.CH_EntryReleaseDate = new ZDateTime(2022, 03, 02);
			entry1.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNum1 = entry1.EntryNumbers.AddNew();
			entryNum1.CE_EntryType = "IMP";
			entryNum1.CE_EntryNum = "1234522123450X";
			entryNum1.CE_IssueDate = new ZDateTime(2022, 01, 01);

			var entryNum934 = entry1.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 02, 03);

			var entryLine1 = entry1.MergedLines.AddNew();
			entryLine1.CL_CustomsValue = 100;

			var invoiceLine1 = invoice1.InvoiceLines.AddNew();
			invoiceLine1.JI_CL = entryLine1.PK;
			invoiceLine1.JI_Weight = 100;
			#endregion

			#region TestData2
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Category = "BUS";
			orgHeader2.OH_Code = "RK2";
			orgHeader2.OH_FullName = "RK TestData2";
			importer = orgHeader2.PK;

			var declaration2 = Factory.New<JobDeclaration>();
			declaration2.JE_MessageType = "IMP";
			declaration2.JE_ApplicationCode = "BLT";
			declaration2.JE_OH_Importer = orgHeader2.PK;

			var invoice2 = declaration2.Invoices.AddNew();
			invoice2.JZ_NoOfPacks = 200;
			invoice2.JZ_Weight = 200;
			invoice2.JZ_WeightUQ = "KG";

			var entry2 = declaration2.CustomsEntryHeaders.AddNew();
			entry2.CH_EntryReleaseDate = new ZDateTime(2022, 03, 02);
			entry2.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNum2 = entry2.EntryNumbers.AddNew();
			entryNum2.CE_EntryType = "IMP";
			entryNum2.CE_EntryNum = "1234522123451X";
			entryNum2.CE_IssueDate = new ZDateTime(2022, 01, 02);

			entryNum934 = entry2.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 02, 01);

			var entryLine2 = entry2.MergedLines.AddNew();
			entryLine2.CL_CustomsValue = 200;

			var invoiceLine2 = invoice2.InvoiceLines.AddNew();
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Weight = 200;
			#endregion

			#region TestData3
			var orgHeader3 = Factory.New<OrgHeader>();
			orgHeader3.OH_Category = "BUS";
			orgHeader3.OH_Code = "RK3";
			orgHeader3.OH_FullName = "RK TestData3";
			payer = orgHeader3.PK;

			var declaration3 = Factory.New<JobDeclaration>();
			declaration3.JE_MessageType = "IMP";
			declaration3.JE_ApplicationCode = "BLT";
			declaration3.JE_OH_DutyPayer = orgHeader3.PK;

			var invoice3 = declaration3.Invoices.AddNew();
			invoice3.JZ_NoOfPacks = 300;
			invoice3.JZ_Weight = 300;
			invoice3.JZ_WeightUQ = "KG";

			var entry3 = declaration3.CustomsEntryHeaders.AddNew();
			entry3.CH_EntryReleaseDate = new ZDateTime(2022, 03, 01);
			entry3.CH_Status = CustomsMessageStatusTypeList.Codes.OriginalAccepted;
			var entryNum3 = entry3.EntryNumbers.AddNew();
			entryNum3.CE_EntryType = "IMP";
			entryNum3.CE_EntryNum = "1234522123452X";
			entryNum3.CE_IssueDate = new ZDateTime(2022, 01, 03);

			entryNum934 = entry3.EntryNumbers.AddNew();
			entryNum934.CE_EntryType = "934";
			entryNum934.CE_ExpiryDate = new ZDateTime(2022, 02, 02);

			var entryLine3 = entry3.MergedLines.AddNew();
			entryLine3.CL_CustomsValue = 300;

			var invoiceLine3 = invoice3.InvoiceLines.AddNew();
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Weight = 300;
			#endregion

			Factory.Save();
		}

		ZGuid payer;
		ZGuid importer;
	}
}
