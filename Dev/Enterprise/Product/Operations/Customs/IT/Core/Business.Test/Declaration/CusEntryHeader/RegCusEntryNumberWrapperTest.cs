using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class RegCusEntryNumberWrapperTest : TestCaseWithFactory
{
	public void TestEmptyWrapper()
	{
		var wrapper = RegCusEntryNumberWrapper.Load(null);
		CombineAssertions(() =>
		{
			AssertEquals(ZString.Empty, wrapper.Register);
			AssertEquals(ZString.Empty, wrapper.RegistrationNumber);
			AssertEquals(ZDate.Empty, wrapper.IssueDate);
			AssertEquals(ZString.Empty, wrapper.Series);
		});
	}

	public void TestRegister()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Register);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("4", wrapper.Register);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("4", wrapper.Register);
	}

	public void TestRegistrationNumber()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumber);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumber);

		cusEntryNumber.CE_EntryNum = "4 T-";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumber);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("123456G", wrapper.RegistrationNumber);
	}

	public void TestRegistrationNumberWithoutCin()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumberWithoutCin);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("123456", wrapper.RegistrationNumberWithoutCin);

		cusEntryNumber.CE_EntryNum = "4 T-G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumberWithoutCin);
	}

	public void TestRegistrationNumberCin()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.RegistrationNumberCin);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("G", wrapper.RegistrationNumberCin);

		cusEntryNumber.CE_EntryNum = "4 T-G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("G", wrapper.RegistrationNumberCin);

		cusEntryNumber.CE_EntryNum = "4 T-123456";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("6", wrapper.RegistrationNumberCin);
	}

	public void TestIssueDate()
	{
		cusEntryNumber.CE_IssueDate = ZDateTime.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(ZDateTime.Empty, wrapper.IssueDate);

		cusEntryNumber.CE_IssueDate = new ZDateTime(2020, 01, 01);
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(new ZDateTime(2020, 01, 01), wrapper.IssueDate);
	}

	public void TestSeries()
	{
		cusEntryNumber.CE_EntryNum = "";
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Series);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Series);

		cusEntryNumber.CE_EntryNum = "4 -";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("", wrapper.Series);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("T", wrapper.Series);
	}

	public void TestRegisterIncludingSeries()
	{
		cusEntryNumber.CE_EntryNum = ZString.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("Empty CE_EntryNum", ZString.Empty, wrapper.RegisterIncludingSeries);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("CE_EntryNum without series", "4", wrapper.RegisterIncludingSeries);

		cusEntryNumber.CE_EntryNum = "4 -";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("CE_EntryNum with invalid series ", "4", wrapper.RegisterIncludingSeries);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals("CE_EntryNum with valid series", "4 T", wrapper.RegisterIncludingSeries);
	}

	public void TestRegistrationNumberIncludingRegisterAndSeries()
	{
		cusEntryNumber.CE_EntryNum = ZString.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"Empty CE_EntryNum, {nameof(RegCusEntryNumberWrapper.RegistrationNumberIncludingRegisterAndSeries)}", ZString.Empty, wrapper.RegistrationNumberIncludingRegisterAndSeries);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"CE_EntryNum without series, {nameof(RegCusEntryNumberWrapper.RegistrationNumberIncludingRegisterAndSeries)}", "4", wrapper.RegistrationNumberIncludingRegisterAndSeries);

		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"CE_EntryNum with valid series, {nameof(RegCusEntryNumberWrapper.RegistrationNumberIncludingRegisterAndSeries)}", "4 T-123456G", wrapper.RegistrationNumberIncludingRegisterAndSeries);
	}

	public void TestCustomsOfficeCode()
	{
		cusEntryNumber.CE_EntryLineReference = ZString.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"Empty CE_EntryLineReference, {nameof(RegCusEntryNumberWrapper.CustomsOfficeCode)}", ZString.Empty, wrapper.CustomsOfficeCode);

		cusEntryNumber.CE_EntryLineReference = "IT304100";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(nameof(RegCusEntryNumberWrapper.CustomsOfficeCode), "IT304100", wrapper.CustomsOfficeCode);
	}

	public void TestCustomsOfficeDescription()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT304100", "PESCARA", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();

		cusEntryNumber.CE_EntryLineReference = ZString.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"Empty CE_EntryLineReference, {nameof(RegCusEntryNumberWrapper.CustomsOfficeDescription)}", ZString.Empty, wrapper.CustomsOfficeDescription);

		cusEntryNumber.CE_EntryLineReference = "304100";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals(nameof(RegCusEntryNumberWrapper.CustomsOfficeDescription), "PESCARA", wrapper.CustomsOfficeDescription);

		cusEntryNumber.CE_EntryLineReference = "XXXXX";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"When CustomsOffice is invalid, {nameof(RegCusEntryNumberWrapper.CustomsOfficeDescription)}", "", wrapper.CustomsOfficeDescription);
	}

	public void TestIsEmpty()
	{
		cusEntryNumber.CE_EntryNum = ZString.Empty;
		var wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"Empty CE_EntryNum, {nameof(RegCusEntryNumberWrapper.IsEmpty)}", true, wrapper.IsEmpty);

		cusEntryNumber.CE_EntryNum = "4";
		wrapper = RegCusEntryNumberWrapper.Load(cusEntryNumber);
		AssertEquals($"CE_EntryNum without series, {nameof(RegCusEntryNumberWrapper.IsEmpty)}", false, wrapper.IsEmpty);

		cusEntryNumber.CE_EntryNum = ZString.Empty;
		cusEntryNumber.CE_IssueDate = ZDate.Today;
		AssertEquals($"CE_IssueDate is Today, {nameof(RegCusEntryNumberWrapper.IsEmpty)}", false, wrapper.IsEmpty);

		cusEntryNumber.CE_IssueDate = ZDate.Empty;
		cusEntryNumber.CE_EntryLineReference = "IT34000";
		AssertEquals($"CE_EntryLineReference is a CustomsOfficeCode, {nameof(RegCusEntryNumberWrapper.IsEmpty)}", false, wrapper.IsEmpty);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_ParentTable = "JobDeclaration";
	}

	CusEntryNumber cusEntryNumber;
}
