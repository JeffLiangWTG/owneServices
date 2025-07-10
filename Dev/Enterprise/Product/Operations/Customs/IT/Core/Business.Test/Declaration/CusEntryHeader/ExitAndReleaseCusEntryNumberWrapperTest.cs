using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class ExitAndReleaseCusEntryNumberWrapperTest : TestCaseWithFactory
{
	public void TestEmptyWrapper()
	{
		var wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(null);
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, wrapper.Office);
			AssertEquals(ZString.Empty, wrapper.OfficeDescription);
			AssertEquals(ZString.Empty, wrapper.Status);
			AssertEquals(ZString.Empty, wrapper.StatusDescription);
			AssertEquals(ZDate.Empty, wrapper.Date);
		});
	}

	public void TestOffice()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Office);
		AssertEquals("", wrapper.OfficeDescription);

		cusEntryNumber.CE_EntryLineReference = "IT275100";

		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT275100", "PONTE CHIASSO", startDate, endDate);
		Factory.Save();

		wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("IT275100", wrapper.Office);
		AssertEquals("PONTE CHIASSO", wrapper.OfficeDescription);
	}

	public void TestStatus()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Status);
		AssertEquals("", wrapper.StatusDescription);

		cusEntryNumber.CE_EntryStatus = "GRL";

		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "GRL", "Garanzia svincolata", startDate, endDate);

		Factory.Save();

		wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("GRL", wrapper.Status);
		AssertEquals("Garanzia svincolata", wrapper.StatusDescription);
	}

	public void TestIssueDate()
	{
		cusEntryNumber.CE_IssueDate = ZDateTime.Empty;
		var wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(ZDateTime.Empty, wrapper.Date);

		cusEntryNumber.CE_IssueDate = new ZDateTime(2020, 01, 01);
		wrapper = ExitAndReleaseCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(new ZDateTime(2020, 01, 01), wrapper.Date);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_ParentTable = "JobDeclaration";
	}

	CusEntryNumber cusEntryNumber;
}
