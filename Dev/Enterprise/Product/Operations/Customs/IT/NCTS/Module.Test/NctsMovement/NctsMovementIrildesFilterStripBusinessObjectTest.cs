using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.NCTS.Module;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.IT.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.IT.Module.Testing;

[TestedType(typeof(NctsMovementFilterStripBusinessObject))]
sealed class NctsMovementIrildesFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestIrildesArrivalDateFilter()
	{
		Factory.Save();

		var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		var arrivalDateFilter = (ModuleDateFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ArrivalDate];
		AssertNotNull(arrivalDateFilter);
		arrivalDateFilter.IsActive = true;
		arrivalDateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateTimeRange;
		arrivalDateFilter.Property1 = ZDateTime.Today.AddDays(-20);
		arrivalDateFilter.Property2 = ZDateTime.Today.AddDays(+20);

		CombineAssertions("Arrivals", () =>
		{
			AssertEquals("arrival1 matches filter", true, arrivalWithIrildes1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match the filter", false, arrivalWithIrildes2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 matches the filter", true, arrivalWithIrildes3.MatchesFilter(filterStripBO.Filter));
		});

		CombineAssertions("CusEntryNumber with EntryType=MRN should not appear in the list", () =>
		{
			AssertEquals("arrivalWithMrn1 does not match the filter", false, arrivalWithMrn1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalWithMrn2 does not match the filter", false, arrivalWithMrn2.MatchesFilter(filterStripBO.Filter));
		});

		arrivalDateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
		AssertArrivalMatchesFilterWhenFilterIsBlankOrDateNotEntered(filterStripBO);
	}

	public void TestIrildesArrivalOfficeCodeFilter()
	{
		cusEntryNumberIrildes1.CE_EntryLineReference = "TR341111";
		cusEntryNumberIrildes2.CE_EntryLineReference = "TR342222";
		cusEntryNumberIrildes3.CE_EntryLineReference = "TR343333";

		cusEntryNumberMrn1.CE_EntryLineReference = "TR341111";
		cusEntryNumberMrn2.CE_EntryLineReference = "TR342222";

		Factory.Save();

		var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		var arrivalOfficeFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ArrivalOfficeCode];
		AssertNotNull(arrivalOfficeFilter);
		arrivalOfficeFilter.IsActive = true;
		arrivalOfficeFilter.Property = "TR3411";

		CombineAssertions("Arrivals", () =>
		{
			AssertEquals("arrival1 matches filter", true, arrivalWithIrildes1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match the filter", false, arrivalWithIrildes2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match the filter", false, arrivalWithIrildes3.MatchesFilter(filterStripBO.Filter));
		});

		CombineAssertions("CusEntryNumber with EntryType=MRN should not appear in the list", () =>
		{
			AssertEquals("arrivalWithMrn1 does not match the filter", false, arrivalWithMrn1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalWithMrn2 does not match the filter", false, arrivalWithMrn2.MatchesFilter(filterStripBO.Filter));
		});

		arrivalOfficeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		AssertArrivalMatchesFilterWhenFilterIsBlankOrDateNotEntered(filterStripBO);
	}

	public void TestIrildesArrivalStatusFilter()
	{
		cusEntryNumberIrildes1.CE_EntryStatus = "GRL";
		cusEntryNumberIrildes2.CE_EntryStatus = "UDF";
		cusEntryNumberIrildes3.CE_EntryStatus = "EXD";

		cusEntryNumberMrn1.CE_EntryStatus = "GRL";
		cusEntryNumberMrn2.CE_EntryStatus = "UDF";

		Factory.Save();

		var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		var arrivalStatusFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ArrivalStatus];
		AssertNotNull(arrivalStatusFilter);
		arrivalStatusFilter.IsActive = true;
		arrivalStatusFilter.Property = "GRL";

		CombineAssertions("Arrivals", () =>
		{
			AssertEquals("arrival1 matches filter", true, arrivalWithIrildes1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match the filter", false, arrivalWithIrildes2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match the filter", false, arrivalWithIrildes3.MatchesFilter(filterStripBO.Filter));
		});

		CombineAssertions("CusEntryNumber with EntryType=MRN should not appear in the list", () =>
		{
			AssertEquals("arrivalWithMrn1 does not match the filter", false, arrivalWithMrn1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalWithMrn2 does not match the filter", false, arrivalWithMrn2.MatchesFilter(filterStripBO.Filter));
		});

		arrivalStatusFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		AssertArrivalMatchesFilterWhenFilterIsBlankOrDateNotEntered(filterStripBO);
	}

	public void TestIrildesArrivalOfficeDescriptionFilter()
	{
		cusEntryNumberIrildes1.CE_EntryLineReference = "IT017000";
		cusEntryNumberIrildes2.CE_EntryLineReference = "IT018100";
		cusEntryNumberIrildes3.CE_EntryLineReference = "IT016199";
		cusEntryNumberIrildes4.CE_EntryLineReference = "IT016200";

		cusEntryNumberMrn1.CE_EntryLineReference = "IT017000";
		cusEntryNumberMrn2.CE_EntryLineReference = "IT018100";

		Factory.Save();

		var filterStripBO = (NctsMovementFilterStripBusinessObject)GetNewFilterStripBusinessObject();
		var arrivalOfficeFilter = (ModuleTextFilter)filterStripBO[NctsMovementFilterStripBusinessObject.ITFilterConstants.ArrivalOfficeDescription];
		AssertNotNull(arrivalOfficeFilter);
		arrivalOfficeFilter.IsActive = true;
		arrivalOfficeFilter.Property = "TARAN";

		CombineAssertions("Arrivals", () =>
		{
			AssertEquals("arrival1 matches filter", true, arrivalWithIrildes1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival2 does not match the filter", false, arrivalWithIrildes2.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival3 does not match the filter", false, arrivalWithIrildes3.MatchesFilter(filterStripBO.Filter));
		});

		CombineAssertions("CusEntryNumber with EntryType=MRN should not appear in the list", () =>
		{
			AssertEquals("arrivalWithMrn1 does not match the filter", false, arrivalWithMrn1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalWithMrn2 does not match the filter", false, arrivalWithMrn2.MatchesFilter(filterStripBO.Filter));
		});

		arrivalOfficeFilter.ComparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.IsBlank;
		AssertArrivalMatchesFilterWhenFilterIsBlankOrDateNotEntered(filterStripBO);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
	{
		return new NctsMovementFilterStripBusinessObject();
	}

	protected override void SetUp()
	{
		base.SetUp();

		var code1 = "IT017000";
		var code2 = "IT018100";
		var code3 = "IT016199";
		var code4 = "IT016200";

		var cusCodeList1 = Factory.New<ZZRefCusCodeListCombined>();
		cusCodeList1.ZZD_Code = code1;
		cusCodeList1.ZZD_Description = "TARANTO";
		cusCodeList1.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		cusCodeList1.ZZD_CountryOrGrouping = "IT";
		cusCodeList1.ZZD_EndDate = ZDateTime.Today.AddDays(2);
		cusCodeList1.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

		var cusCodeList2 = Factory.New<ZZRefCusCodeListCombined>();
		cusCodeList2.ZZD_Code = code2;
		cusCodeList2.ZZD_Description = "BARI";
		cusCodeList2.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		cusCodeList2.ZZD_CountryOrGrouping = "IT";
		cusCodeList2.ZZD_EndDate = ZDateTime.Today.AddDays(2);
		cusCodeList2.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

		var cusCodeList3 = Factory.New<ZZRefCusCodeListCombined>();
		cusCodeList3.ZZD_Code = code3;
		cusCodeList3.ZZD_Description = "LECCE";
		cusCodeList3.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		cusCodeList3.ZZD_CountryOrGrouping = "IT";
		cusCodeList3.ZZD_EndDate = ZDateTime.Today.AddDays(2);
		cusCodeList3.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

		var cusCodeList4 = Factory.New<ZZRefCusCodeListCombined>();
		cusCodeList4.ZZD_Code = code4;
		cusCodeList4.ZZD_Description = "";
		cusCodeList4.ZZD_CodeType = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
		cusCodeList4.ZZD_CountryOrGrouping = "IT";
		cusCodeList4.ZZD_EndDate = ZDateTime.Today.AddDays(2);
		cusCodeList4.ZZD_StartDate = ZDateTime.Today.AddDays(-2);

		arrivalWithIrildes1 = Factory.New<NctsHeader>();
		arrivalWithIrildes1.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithIrildes2 = Factory.New<NctsHeader>();
		arrivalWithIrildes2.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithIrildes3 = Factory.New<NctsHeader>();
		arrivalWithIrildes3.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithIrildes4 = Factory.New<NctsHeader>();
		arrivalWithIrildes4.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithMrn1 = Factory.New<NctsHeader>();
		arrivalWithMrn1.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithMrn2 = Factory.New<NctsHeader>();
		arrivalWithMrn2.SetMovementType(NctsMovementType.Codes.Arrival);

		arrivalWithIrildes1.BH_JobReference = "NCT0000112";
		cusEntryNumberIrildes1 = Factory.New<CusEntryNumber>();
		cusEntryNumberIrildes1.CE_EntryType = CusEntryNumberConstants.EntryTypes.Irildes;
		cusEntryNumberIrildes1.CE_ParentID = arrivalWithIrildes1.PK;
		cusEntryNumberIrildes1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNumberIrildes1.CE_Category = "CUS";
		cusEntryNumberIrildes1.CE_EntryNum = "4 T-123456G";
		cusEntryNumberIrildes1.CE_EntryLineReference = "TR341200";
		cusEntryNumberIrildes1.CE_IssueDate = ZDate.Today;
		cusEntryNumberIrildes1.CE_EntryStatus = "GRL";

		arrivalWithIrildes2.BH_JobReference = "NCT0000113";
		cusEntryNumberIrildes2 = Factory.New<CusEntryNumber>();
		cusEntryNumberIrildes2.CE_EntryType = CusEntryNumberConstants.EntryTypes.Irildes;
		cusEntryNumberIrildes2.CE_ParentID = arrivalWithIrildes2.PK;
		cusEntryNumberIrildes2.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNumberIrildes2.CE_Category = "CUS";
		cusEntryNumberIrildes2.CE_EntryNum = "5 T-123456G";
		cusEntryNumberIrildes2.CE_EntryLineReference = "TR341201";
		cusEntryNumberIrildes2.CE_IssueDate = ZDate.Today.AddDays(400);
		cusEntryNumberIrildes2.CE_EntryStatus = "GRL";

		arrivalWithIrildes3.BH_JobReference = "NCT0000114";
		cusEntryNumberIrildes3 = Factory.New<CusEntryNumber>();
		cusEntryNumberIrildes3.CE_EntryType = CusEntryNumberConstants.EntryTypes.Irildes;
		cusEntryNumberIrildes3.CE_ParentID = arrivalWithIrildes3.PK;
		cusEntryNumberIrildes3.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNumberIrildes3.CE_Category = "CUS";
		cusEntryNumberIrildes3.CE_EntryNum = "6 T-123456G";
		cusEntryNumberIrildes3.CE_EntryLineReference = "TR341202";
		cusEntryNumberIrildes3.CE_IssueDate = ZDate.Today;
		cusEntryNumberIrildes3.CE_EntryStatus = "UDF";

		cusEntryNumberIrildes4 = Factory.New<CusEntryNumber>();
		cusEntryNumberIrildes4.CE_EntryType = CusEntryNumberConstants.EntryTypes.Irildes;
		cusEntryNumberIrildes4.CE_ParentID = arrivalWithIrildes4.PK;
		cusEntryNumberIrildes4.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;

		arrivalWithMrn1.BH_JobReference = "NCT0000112";
		cusEntryNumberMrn1 = Factory.New<CusEntryNumber>();
		cusEntryNumberMrn1.CE_EntryType = CusEntryNumberConstants.EntryTypes.Mrn;
		cusEntryNumberMrn1.CE_ParentID = arrivalWithMrn1.PK;
		cusEntryNumberMrn1.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNumberMrn1.CE_Category = "CUS";
		cusEntryNumberMrn1.CE_EntryNum = "4 T-123456G";
		cusEntryNumberMrn1.CE_EntryLineReference = "TR341200";
		cusEntryNumberMrn1.CE_IssueDate = ZDate.Today;
		cusEntryNumberMrn1.CE_EntryStatus = "GRL";

		arrivalWithMrn2.BH_JobReference = "NCT0000113";
		cusEntryNumberMrn2 = Factory.New<CusEntryNumber>();
		cusEntryNumberMrn2.CE_EntryType = CusEntryNumberConstants.EntryTypes.Mrn;
		cusEntryNumberMrn2.CE_ParentID = arrivalWithMrn2.PK;
		cusEntryNumberMrn2.CE_ParentTable = CusInBondHeaderSchema.Constants.TableName;
		cusEntryNumberMrn2.CE_Category = "CUS";
		cusEntryNumberMrn2.CE_EntryNum = "5 T-123456G";
		cusEntryNumberMrn2.CE_EntryLineReference = "TR341201";
		cusEntryNumberMrn2.CE_IssueDate = ZDate.Today.AddDays(400);
		cusEntryNumberMrn2.CE_EntryStatus = "GRL";
	}

	NctsHeader arrivalWithIrildes1;
	NctsHeader arrivalWithIrildes2;
	NctsHeader arrivalWithIrildes3;
	NctsHeader arrivalWithIrildes4;
	NctsHeader arrivalWithMrn1;
	NctsHeader arrivalWithMrn2;

	CusEntryNumber cusEntryNumberIrildes1;
	CusEntryNumber cusEntryNumberIrildes2;
	CusEntryNumber cusEntryNumberIrildes3;
	CusEntryNumber cusEntryNumberIrildes4;
	CusEntryNumber cusEntryNumberMrn1;
	CusEntryNumber cusEntryNumberMrn2;

	void AssertArrivalMatchesFilterWhenFilterIsBlankOrDateNotEntered(NctsMovementFilterStripBusinessObject filterStripBO)
	{
		CombineAssertions("Arrivals when filter IsBlank/Date not entered", () =>
		{
			AssertEquals("arrival1 does not match the filter", false, arrivalWithIrildes1.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrival4 matches filter", true, arrivalWithIrildes4.MatchesFilter(filterStripBO.Filter));
			AssertEquals("arrivalWithMrn1 matches filter", true, arrivalWithMrn1.MatchesFilter(filterStripBO.Filter));
		});
	}
}
