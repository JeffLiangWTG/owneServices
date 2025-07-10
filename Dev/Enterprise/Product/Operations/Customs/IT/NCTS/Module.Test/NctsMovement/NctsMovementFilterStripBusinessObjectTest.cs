using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Module;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
sealed class NctsMovementFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestRepresentationTypeFilter()
	{
		var departureNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader2 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var arrivalNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Arrival);

		departureNctsHeader1.RepresentationType = "SEL";
		departureNctsHeader2.RepresentationType = "";
		arrivalNctsHeader1.RepresentationType = "SEL";

		Factory.Save();

		var filterStripBO = GetNewFilterStripBusinessObject();
		var representationTypeFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.RepresentationType];

		AssertNotNull(representationTypeFilter);
		representationTypeFilter.IsActive = true;
		representationTypeFilter.Property = "SEL";

		CombineAssertions("Assert filter results", () =>
		{
			AssertEquals("departureNctsHeader1 matches filter", true, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", false, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNctsHeader1 matches the filter", true, arrivalNctsHeader1.MatchesFilter(filterStripBO.Filter));
		});
	}

	public void TestDelcarantFilter()
	{
		var declarant = Factory.NewWithValidTestData<OrgHeader>();

		var departureNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader2 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var arrivalNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		var departureNctsHeader3 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		departureNctsHeader1.DeclarantAddressPK = declarant.MainAddress.PK;
		departureNctsHeader2.DeclarantAddressPK = ZGuid.Empty;
		arrivalNctsHeader1.DeclarantAddressPK = declarant.MainAddress.PK;
		departureNctsHeader3.DeclarantAddressPK = Factory.NewWithValidTestData<OrgHeader>().MainAddress.PK;

		Factory.Save();

		var filterStripBO = GetNewFilterStripBusinessObject();
		var declarantFilter = (ModuleGuidFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.Declarant];
		AssertNotNull(declarantFilter);
		declarantFilter.IsActive = true;
		declarantFilter.Property = declarant.PK;

		CombineAssertions("Assert filter results", () =>
		{
			AssertEquals("departureNctsHeader1 matches filter", true, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", false, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNctsHeader1 matches the filter", true, arrivalNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader3 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});
	}

	public void TestDefermentAccountNumberFilter()
	{
		var departureNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader2 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader3 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);

		departureNctsHeader1.MovementHeader.DefermentAccountNumber = "IT1234";
		departureNctsHeader2.MovementHeader.DefermentAccountNumber = "";
		departureNctsHeader3.MovementHeader.DefermentAccountNumber = "IT4321";

		Factory.Save();

		var filterStripBO = GetNewFilterStripBusinessObject();
		var defermentAccountNumberFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.DefermentAccountNumber];

		AssertNotNull(defermentAccountNumberFilter);
		defermentAccountNumberFilter.IsActive = true;
		defermentAccountNumberFilter.Property = "IT1234";

		CombineAssertions("Assert filter results", () =>
		{
			AssertEquals("departureNctsHeader1 matches filter", true, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", false, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNctsHeader1 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});
	}

	public void TestRegistrationDateFilter()
	{
		var departureNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		CreateNewEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber, departureNctsHeader1, "4 T-123456G", "", ZDate.Today, "");
		var departureNctsHeader2 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader3 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		CreateNewEntryNumber(CusEntryNumberConstants.EntryTypes.RegistrationNumber, departureNctsHeader3, "4 T-123456G", "", ZDate.Today.AddDays(-5), "");

		Factory.Save();

		var filterStripBO = GetNewFilterStripBusinessObject();
		var registrationDateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.RegistrationDate];

		AssertNotNull(registrationDateFilter);
		registrationDateFilter.IsActive = true;
		registrationDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		registrationDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
		registrationDateFilter.Property2 = ZDateTime.Today.AddDays(+1);

		CombineAssertions("Assert filter results", () =>
		{
			AssertEquals("departureNctsHeader1 matches filter", true, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", false, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader3 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});

		registrationDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		CombineAssertions("Assert filter results for HasNoDateEntered", () =>
		{
			AssertEquals("departureNctsHeader1 does not match the filter", false, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", true, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader3 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});
	}

	public void TestReleaseDateFilter()
	{
		var departureNctsHeader1 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		CreateNewEntryNumber(CusEntryNumberConstants.EntryTypes.ClereanceCode, departureNctsHeader1, "4 T-123456G", "", ZDate.Today, "");
		var departureNctsHeader2 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		var departureNctsHeader3 = GetNewNctsHeader(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		CreateNewEntryNumber(CusEntryNumberConstants.EntryTypes.ClereanceCode, departureNctsHeader3, "4 T-123456G", "", ZDate.Today.AddDays(-5), "");

		Factory.Save();

		var filterStripBO = GetNewFilterStripBusinessObject();
		var releaseDateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ReleaseDate];

		AssertNotNull(releaseDateFilter);
		releaseDateFilter.IsActive = true;
		releaseDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
		releaseDateFilter.Property1 = ZDateTime.Today.AddDays(-1);
		releaseDateFilter.Property2 = ZDateTime.Today.AddDays(+1);

		CombineAssertions("Assert filter results", () =>
		{
			AssertEquals("departureNctsHeader1 matches filter", true, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", false, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalNctsHeader1 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});

		releaseDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		CombineAssertions("Assert filter results for HasNoDateEntered", () =>
		{
			AssertEquals("departureNctsHeader1 does not match the filter", false, departureNctsHeader1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader2 does not match the filter", true, departureNctsHeader2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("departureNctsHeader3 does not match the filter", false, departureNctsHeader3.MatchesFilter(filterStripBO.Filter));
		});
	}

	public void TestMRNReleaseDateFilterDescription()
	{
		var filterStripBO = GetNewFilterStripBusinessObject();
		var euReleaseDateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.FilterConstants.MRNReleaseDate];
		var itReleaseDateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ReleaseDate];
		AssertEquals("MRN Release Date", euReleaseDateFilter.Description);
		AssertNotEquals(euReleaseDateFilter.Description, itReleaseDateFilter.Description);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new NctsMovementFilterStripBusinessObject();
	}

	NctsHeader GetNewNctsHeader(string movementType)
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(movementType);

		return nctsHeader;
	}

	CusEntryNumber CreateNewEntryNumber(ZString type, NctsHeader parent, ZString entryNum, ZString entryLineReference, ZDate issueDate, ZString entryStatus)
	{
		var cusEntryNum = Factory.New<CusEntryNumber>();
		cusEntryNum.CE_EntryType = type;
		cusEntryNum.CE_ParentID = parent.PK;
		cusEntryNum.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNum.CE_Category = "CUS";
		cusEntryNum.CE_EntryNum = entryNum;
		cusEntryNum.CE_EntryLineReference = entryLineReference;
		cusEntryNum.CE_IssueDate = issueDate;
		cusEntryNum.CE_EntryStatus = entryStatus;
		return cusEntryNum;
	}
}
