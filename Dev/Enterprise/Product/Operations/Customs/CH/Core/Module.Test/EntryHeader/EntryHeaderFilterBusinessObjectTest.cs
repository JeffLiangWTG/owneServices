using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.SwissCustomsConstants;

namespace Enterprise.Customs.CH.Module.Testing;

[TestedType(typeof(EntryHeaderFilterBusinessObject))]
sealed class EntryHeaderFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestLookups()
	{
		var filterBizObj = new EntryHeaderFilterBusinessObject();
		AssertType<EntryHeaderFilterLookups>("Lookups of correct type", filterBizObj.Lookups);
	}

	public void TestPhaseStatusFilter() => CombineAssertions(() =>
	{
		var differentBranch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();

		var entryHeader1 = AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment);
		var entryHeader2 = AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.RequestDataJourney);
		var entryHeader3 = AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, entryHeader2.Declaration);

		AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.EDecToPassarDataTransfer);
		Factory.Save();

		var filterStrip = GetNewFilterStripBusinessObject();
		var filter = (ModuleTextFilter)filterStrip[EntryHeaderFilterBusinessObject.FilterConstants.PhaseStatus];
		AssertNotNull($"Filter {EntryHeaderFilterBusinessObject.FilterConstants.PhaseStatus}", filter);
		AssertEquals("PhaseStatus MultilingualDescription", EntryHeaderFilterBusinessObject.FilterConstants.PhaseStatus, filter.MultilingualDescription);

		filter.IsActive = true;
		filter.Property = PassarDeclarationPhaseList.Codes.Amendment;

		var filteredDecs = Factory.Load<CusEntryHeader>(filterStrip.Filter);
		AssertEquals($"{filter.Property} Total Count", 2, filteredDecs.Length);
		AssertEquals($"{filter.Property} entryHeader1 found.", true, filteredDecs.Any(e => e.PK == entryHeader1.PK));
		AssertEquals($"{filter.Property} entryHeader2 found.", true, filteredDecs.Any(e => e.PK == entryHeader3.PK));

		filter.Property = PassarDeclarationPhaseList.Codes.RequestDataJourney;

		filteredDecs = Factory.Load<CusEntryHeader>(filterStrip.Filter);
		AssertEquals($"{filter.Property} Total Count", 1, filteredDecs.Length);
		AssertEquals($"{filter.Property} entryHeader2 found.", true, filteredDecs.Any(e => e.PK == entryHeader2.PK));
	});

	public void TestAcceptanceDateFilter() => CombineAssertions(() =>
	{
		CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "111", ZDateTime.BrettsBirthday.AddDays(-3), ZDateTime.BrettsBirthday.AddDays(-2));
		var entryHeader2 = CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "222", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(1));
		var entryHeader3 = CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "333", ZDateTime.BrettsBirthday.AddDays(3), ZDateTime.BrettsBirthday.AddDays(4));
		CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "444", ZDateTime.BrettsBirthday.AddDays(10), ZDateTime.BrettsBirthday.AddDays(11));
		Factory.Save();

		var filterStrip = GetNewFilterStripBusinessObject();
		var filter = (ModuleDateFilter)filterStrip[EntryHeaderFilterBusinessObject.FilterConstants.AcceptanceDate];
		AssertNotNull($"Filter {EntryHeaderFilterBusinessObject.FilterConstants.AcceptanceDate}", filter);
		AssertEquals("AcceptanceDate MultilingualDescription", EntryHeaderFilterBusinessObject.FilterConstants.AcceptanceDate, filter.MultilingualDescription);
		filter.IsActive = true;
		filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

		filter.Property1 = ZDateTime.BrettsBirthday.AddDays(-2);
		filter.Property2 = ZDateTime.BrettsBirthday.AddDays(9);
		var filteredDecs = Factory.Load<CusEntryHeader>(filterStrip.Filter);
		AssertEquals("Total Count", 2, filteredDecs.Length);
		AssertEquals("entryHeader2 found.", true, filteredDecs.Any(d => d.PK == entryHeader2.PK));
		AssertEquals("entryHeader3 found.", true, filteredDecs.Any(d => d.PK == entryHeader3.PK));
	});

	public void TestActivationDeadlineFilter() => CombineAssertions(() =>
	{
		var differentBranch = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();

		CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "111", ZDateTime.BrettsBirthday.AddDays(-3), ZDateTime.BrettsBirthday.AddDays(-2));
		var entryHeader2 = CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "222", ZDateTime.BrettsBirthday, ZDateTime.BrettsBirthday.AddDays(1));
		var entryHeader3 = CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "333", ZDateTime.BrettsBirthday.AddDays(3), ZDateTime.BrettsBirthday.AddDays(4));
		CreateEntryHeaderWithMRN(GlbBranch.CurrentBranch.PK, "444", ZDateTime.BrettsBirthday.AddDays(10), ZDateTime.BrettsBirthday.AddDays(11));
		Factory.Save();

		var filterStrip = GetNewFilterStripBusinessObject();
		var filter = (ModuleDateFilter)filterStrip[EntryHeaderFilterBusinessObject.FilterConstants.ActivationDeadline];
		AssertNotNull($"Filter {EntryHeaderFilterBusinessObject.FilterConstants.ActivationDeadline}", filter);
		AssertEquals("ActivationDeadline MultilingualDescription", EntryHeaderFilterBusinessObject.FilterConstants.ActivationDeadline, filter.MultilingualDescription);
		filter.IsActive = true;
		filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;

		filter.Property1 = ZDateTime.BrettsBirthday.AddDays(-1);
		filter.Property2 = ZDateTime.BrettsBirthday.AddDays(10);
		var filteredDecs = Factory.Load<CusEntryHeader>(filterStrip.Filter);
		AssertEquals("Total Count", 2, filteredDecs.Length);
		AssertEquals($"entryHeader2 found.", true, filteredDecs.Any(d => d.PK == entryHeader2.PK));
		AssertEquals($"entryHeader3 found. ", true, filteredDecs.Any(d => d.PK == entryHeader3.PK));
	});

	public void TestLastEComStatusFilter()
	{
		var eComplaintStatusCodes = new string[] { EComplaintStatusList.Codes.Sent, EComplaintStatusList.Codes.Accepted,
			EComplaintStatusList.Codes.Rejected, EComplaintStatusList.Codes.Received, EComplaintStatusList.Codes.Closed };

		var entryHeaders = new CusEntryHeader[] {
		AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, null, EComplaintStatusList.Codes.Sent),
			AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, null, EComplaintStatusList.Codes.Accepted),
			AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, null, EComplaintStatusList.Codes.Rejected),
			AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, null, EComplaintStatusList.Codes.Received),
			AddOrCreateEntryHeader(GlbBranch.CurrentBranch.PK, PassarDeclarationPhaseList.Codes.Amendment, null, EComplaintStatusList.Codes.Closed) };
		Factory.Save();

		var filterStrip = GetNewFilterStripBusinessObject();
		var filter = (ModuleTextFilter)filterStrip[EntryHeaderFilterBusinessObject.FilterConstants.LastEComStatus];
		AssertNotNull($"Filter {EntryHeaderFilterBusinessObject.FilterConstants.LastEComStatus}", filter);
		AssertEquals("LastEComStatus MultilingualDescription", EntryHeaderFilterBusinessObject.FilterConstants.LastEComStatus, filter.MultilingualDescription);
		AssertEquals("LastEComStatus MaxLength", CusEntryHeader.Schema.CH_LastEComplaintStatusMaxLength, filter.MaxLength);
		filter.IsActive = true;

		for (var i = 0; i < entryHeaders.Length; i++)
		{
			filter.Property = eComplaintStatusCodes[i];
			var filteredDecs = Factory.Load<CusEntryHeader>(filterStrip.Filter);
			AssertEquals($"{filter.Property} Item found", 1, filteredDecs.Length);
			AssertEquals($"{filter.Property} Item matches", true, filteredDecs.Any(e => e.PK == entryHeaders[i].PK));
		}
	}

	public void TestSelectionResultFilter()
	{
		var entryHeader1 = AddOrCreateEntryHeaderEntryStatus(SelectionResultCodes.FreeWith);
		var entryHeader2 = AddOrCreateEntryHeaderEntryStatus(SelectionResultCodes.FreeWithout);
		var entryHeader3 = AddOrCreateEntryHeaderEntryStatus(SelectionResultCodes.FreeWithout);
		var entryHeader4 = AddOrCreateEntryHeaderEntryStatus(SelectionResultCodes.Blocked);
		Factory.Save();

		var filterBizObj = new EntryHeaderFilterBusinessObject();
		var selectionResultFilter = (ModuleTextFilter)filterBizObj[EntryHeaderFilterBusinessObject.FilterConstants.SelectionResult];
		selectionResultFilter.IsActive = true;
		selectionResultFilter.Property = SelectionResultCodes.FreeWithout;

		var filteredHeaders = Factory.Load<CusEntryHeader>(filterBizObj.Filter);

		AssertEquals("Total Count", 2, filteredHeaders.Length);
		AssertEquals("EntryHeader 2 FreeWithout", true, filteredHeaders.Any(d => d.PK == entryHeader2.PK));
		AssertEquals("EntryHeader 3 FreeWithout", true, filteredHeaders.Any(d => d.PK == entryHeader3.PK));
	}

	CusEntryHeader AddOrCreateEntryHeaderEntryStatus(string selectionResult)
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		cusEntryNumber.CE_EntryNum = "123456";
		cusEntryNumber.CE_IssueDate = ZDateTime.Today;
		cusEntryNumber.CE_EntryStatus = selectionResult;

		return entryHeader;
	}

	CusEntryHeader AddOrCreateEntryHeader(ZGuid branchPK, string phaseStatus = "", JobDeclaration declaration = null, string lastEComStatus = "")
	{
		declaration ??= Factory.New<JobDeclaration>();
		declaration.JE_GB = branchPK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_PhaseStatus = phaseStatus;
		entryHeader.CH_LastEComplaintStatus = lastEComStatus;
		return entryHeader;
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new EntryHeaderFilterBusinessObject();

	CusEntryHeader CreateEntryHeaderWithMRN(ZGuid branchPK, ZString mrn, ZDateTime issueDate, ZDateTime expireDate)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_GB = branchPK;
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter(mrn, issueDate: issueDate, expiryDate: expireDate);
		return entryHeader;
	}
}
