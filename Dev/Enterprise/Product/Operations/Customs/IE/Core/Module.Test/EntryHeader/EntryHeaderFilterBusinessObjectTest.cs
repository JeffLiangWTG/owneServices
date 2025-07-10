using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing
{
	[TestedType(typeof(EntryHeaderFilterBusinessObject))]
	public class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestSupportRequestedProcedure()
		{
			Assert(new EntryHeaderFilterBusinessObject().SupportRequestedProcedure);
		}

		[TestDate(2021, 6, 15)]
		public void TestCustomsDocsReqdByDateFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration1.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;
			var requestedDocument2 = instruction1.RequestedDocuments.AddNew();

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			entryHeader3.CH_CEI_Instruction = instruction3.PK;
			var requestedDocument3 = instruction3.RequestedDocuments.AddNew();
			var entryHeader4 = declaration3.CustomsEntryHeaders.AddNew();
			var instruction4 = declaration3.CustomsEntryInstructions.AddNew();
			var requestedDocument4 = instruction4.RequestedDocuments.AddNew();
			entryHeader4.CH_CEI_Instruction = instruction4.PK;

			requestedDocument2.CSI_DateOfExpiry = new ZDateTime(2021, 6, 12);
			requestedDocument3.CSI_DateOfExpiry = new ZDateTime(2021, 6, 16);
			requestedDocument4.CSI_DateOfExpiry = new ZDateTime(2021, 6, 12);

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleDateFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.CustomsDocsReqdByDate];
			filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			filter.IsActive = true;
			filter.Property1 = ZDateTime.Today.AddDays(-2);
			filter.Property2 = ZDateTime.Today.AddDays(2);
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.Dates, filter.Category);
				AssertEquals("RequestedDocuments empty should not match", false, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("RequestedDocuments OUT of range should not match", false, entryHeader2.MatchesFilter(filterQuery));
				AssertEquals("RequestedDocuments IN range should match", true, entryHeader3.MatchesFilter(filterQuery));
				AssertEquals("RequestedDocuments OUT of range should not match", false, entryHeader4.MatchesFilter(filterQuery));
			});
		}

		public void TestCustomsDocsReqdFlagFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration1.CustomsEntryInstructions.AddNew();
			var requestedDocument2 = instruction2.RequestedDocuments.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			var requestedDocument3 = instruction3.RequestedDocuments.AddNew();
			entryHeader3.CH_CEI_Instruction = instruction3.PK;

			var declaration4 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader4 = declaration4.CustomsEntryHeaders.AddNew();
			var instruction4 = declaration4.CustomsEntryInstructions.AddNew();
			var requestedDocument4 = instruction4.RequestedDocuments.AddNew();
			entryHeader4.CH_CEI_Instruction = instruction4.PK;

			var entryHeader5 = declaration4.CustomsEntryHeaders.AddNew();
			var instruction5 = declaration4.CustomsEntryInstructions.AddNew();
			var requestedDocument5 = instruction5.RequestedDocuments.AddNew();
			entryHeader5.CH_CEI_Instruction = instruction5.PK;

			requestedDocument2.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestCancelled;
			requestedDocument3.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;
			requestedDocument4.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.RequestOpened;
			requestedDocument5.CSI_Status = EU.Business.CodeDescriptionPairLists.RequestedDocumentStatusList.Codes.DocumentsConfirmedReceived;

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.CustomsDocsReqd];
			filter.IsActive = true;
			filter.Property0 = false;
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.StatusAndFlags, filter.Category);
				AssertEquals("When unflagged RequestedDocuments empty should match", true, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'CAN' should match", true, entryHeader2.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'RCV' should match", true, entryHeader3.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'OPE' should match", true, entryHeader4.MatchesFilter(filterQuery));
				AssertEquals("When unflagged RequestedDocuments with CSI_Status = 'RCV' should match", true, entryHeader5.MatchesFilter(filterQuery));
			});

			filter.Property0 = true;
			filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.StatusAndFlags, filter.Category);
				AssertEquals("When flagged RequestedDocuments empty should not match", false, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'CAN' should not should match", false, entryHeader2.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'RCV' should not should match", false, entryHeader3.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'OPE' should match", true, entryHeader4.MatchesFilter(filterQuery));
				AssertEquals("When flagged RequestedDocuments with CSI_Status = 'RCV' should not should match - do not match to other CusEntryInstruction on same JobDeclaration", false, entryHeader5.MatchesFilter(filterQuery));
			});
		}

		public void TestCustomsRegistrationNumberFilter()
		{
			var declaration1 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader1 = declaration1.CustomsEntryHeaders.AddNew();
			var instruction1 = declaration1.CustomsEntryInstructions.AddNew();
			entryHeader1.CH_CEI_Instruction = instruction1.PK;
			entryHeader1.CRN = "CRN-00001";

			var declaration2 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;
			entryHeader2.CRN = "CRN-00002";

			var declaration3 = Factory.NewWithValidTestData<JobDeclaration>();
			var entryHeader3 = declaration3.CustomsEntryHeaders.AddNew();
			var instruction3 = declaration3.CustomsEntryInstructions.AddNew();
			entryHeader2.CH_CEI_Instruction = instruction2.PK;

			Factory.Save();

			var filterObj = new EntryHeaderFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[EntryHeaderFilterBusinessObject.FilterConstants.CustomsRegistrationNumber];
			filter.IsActive = true;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.Property = "CRN-00001";
			var filterQuery = filterObj.Filter;

			CombineAssertions(() =>
			{
				AssertEquals("FilterCategory", FilterCategories.NumbersAndReferences, filter.Category);
				AssertEquals("CRN in entry is CRN-00001, should match the filter", true, entryHeader1.MatchesFilter(filterQuery));
				AssertEquals("CRN in entry is not CRN-00001, should not match the filter", false, entryHeader2.MatchesFilter(filterQuery));
				AssertEquals("CRN is empty, should not match the filter", false, entryHeader3.MatchesFilter(filterQuery));
			});
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();
	}
}
