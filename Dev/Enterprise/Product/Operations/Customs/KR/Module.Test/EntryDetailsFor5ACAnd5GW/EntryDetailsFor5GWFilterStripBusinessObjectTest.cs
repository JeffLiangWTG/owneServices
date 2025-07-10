using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(EntryDetailsFor5GWFilterStripBusinessObject))]
	sealed class EntryDetailsFor5GWFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryDetailsFor5GWFilterStripBusinessObject();

		public void TestFilter()
		{
			var filter = new EntryDetailsFor5GWFilterStripBusinessObject();
			AssertNotNull(filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.Payer]);
			AssertNotNull(filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.PayerCompanyName]);
			AssertNotNull(filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.EntryCreatedDate]);
		}

		public void TestCreatedTimeFilter()
		{
			var filter = new EntryDetailsFor5GWFilterStripBusinessObject();
			var createdTimeFilter = (ModuleDateFilter)filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.EntryCreatedDate];
			createdTimeFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
			createdTimeFilter.Property2 = new ZDateTime(2022, 01, 01);
			createdTimeFilter.IsActive = true;
			AssertFilter(filter, "1234522123450X");
		}
		public void TestPayerCompanyNameFilter()
		{
			var filter = new EntryDetailsFor5GWFilterStripBusinessObject();
			var payerCompanyNameFilter = (ModuleTextFilter)filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.PayerCompanyName];
			payerCompanyNameFilter.Property = "RK TestData2";
			payerCompanyNameFilter.IsActive = true;
			AssertFilter(filter, "1234522123451X");
		}

		public void TestPayerFilter()
		{
			var filter = new EntryDetailsFor5GWFilterStripBusinessObject();
			var payerFilter = (ModuleGuidFilter)filter[EntryDetailsFor5GWFilterStripBusinessObject.Schema.Payer];
			payerFilter.Property = payer;
			payerFilter.IsActive = true;
			AssertFilter(filter, "1234522123453X");
		}

		void AssertFilter(EntryDetailsFor5GWFilterStripBusinessObject filter, string result)
		{
			var query = new KREntryHeaderDetailsView.Loader(Factory).GetQueryFor5ACAnd5GW();
			var coll = new KREntryHeaderDetailsViewCollection(Factory, Common.KR.KRJobMessageTypeList.Codes.Import, query, GlbCompany.CurrentCompany.PK);
			coll.AdditionalFilter = filter.Filter;
			AssertEquals(1, coll.Count);
			AssertEquals(result, coll.Cast<KREntryHeaderDetailsView>().First().KEH_EntryNum);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupTestData("RK1", "RK TestData1", "1234522123450X", new ZDateTime(2021, 12, 31), new ZDateTime(2022, 01, 01));
			SetupTestData("RK2", "RK TestData2", "1234522123451X", new ZDateTime(2022, 01, 10), new ZDateTime(2022, 01, 11));
			var declaration = SetupTestData("RK3", "RK TestData3", "1234522123452X", new ZDateTime(2022, 01, 20), new ZDateTime(2022, 01, 21));
			declaration = SetupTestData("RK4", "RK TestData4", "1234522123453X", new ZDateTime(2022, 01, 30), new ZDateTime(2022, 01, 31));
			payer = declaration.DutyPayer.PK;
			Factory.Save();
		}

		JobDeclaration SetupTestData(string code, string fullName, string entryNum, ZDateTime systemCreateTime, ZDateTime issueDate)
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Category = "BUS";
			orgHeader.OH_Code = code;
			orgHeader.OH_FullName = fullName;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "BLT";
			declaration.JE_OH_DutyPayer = orgHeader.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_NoOfPacks = 100;
			invoice.JZ_Weight = 100;
			invoice.JZ_WeightUQ = "KG";

			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_SystemCreateTimeUtc = systemCreateTime;
			var entryNumObj = entry.EntryNumbers.AddNew();
			entryNumObj.CE_EntryType = "IMP";
			entryNumObj.CE_EntryNum = entryNum;
			entryNumObj.CE_IssueDate = issueDate;

			var entryLine = entry.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 100;

			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_Weight = 100;

			return declaration;
		}

		ZGuid payer;
	}
}
