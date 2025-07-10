using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITDocSADHBoxCOfficeOfDepartureAndRegistrationuilderTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when entryHeader parameter is null", () => new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(null));
	}

	public void TestGetOfficeOfDepartureRow()
	{
		SetupOffice();

		CombineAssertions("Assert Office of Departure Row (first row)", () =>
		{
			var boxCOfficeOfDepartureAndRegistrationBuilder = new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(entryHeader);
			AssertEquals(nameof(ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder.GetOfficeOfDepartureAndRegistrationData), "", GetRowOfAccountingDetails(boxCOfficeOfDepartureAndRegistrationBuilder, 0));

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01), "304100");
			AssertEquals("Office of departure Row", "304100 - PESCARA", GetRowOfAccountingDetails(boxCOfficeOfDepartureAndRegistrationBuilder, 0));
		});
	}

	public void TestGetRegistrationRow()
	{
		CombineAssertions("Assert Registration number row", () =>
		{
			var boxCOfficeOfDepartureAndRegistrationBuilder = new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(entryHeader);

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			AssertEquals("Registration number row", "REG: 4 T-95870G", GetRowOfAccountingDetails(boxCOfficeOfDepartureAndRegistrationBuilder, 2));
		});
	}

	public void TestGetExpirationRow()
	{
		CombineAssertions("Assert Expiration row", () =>
		{
			var boxCOfficeOfDepartureAndRegistrationBuilder = new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(entryHeader);

			Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01));
			AssertEquals("Expiration row", "DEL 01/06/2021", GetRowOfAccountingDetails(boxCOfficeOfDepartureAndRegistrationBuilder, 3));
		});
	}

	public void TestGetOfficeOfDepartureAndRegistrationData()
	{
		SetupOffice();

		var boxCOfficeOfDepartureAndRegistrationBuilder = new ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder(entryHeader);
		AssertEquals(nameof(ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder.GetOfficeOfDepartureAndRegistrationData), "", boxCOfficeOfDepartureAndRegistrationBuilder.GetOfficeOfDepartureAndRegistrationData());

		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 T-95870G", issueDate: new ZDateTime(2021, 06, 01), "304100");
		var expectedOfficeOfDepartureAndRegistrationData = @"304100 - PESCARA

REG: 4 T-95870G
DEL 01/06/2021";
		AssertEquals(nameof(ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder.GetOfficeOfDepartureAndRegistrationData), expectedOfficeOfDepartureAndRegistrationData, boxCOfficeOfDepartureAndRegistrationBuilder.GetOfficeOfDepartureAndRegistrationData());
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "EXP";
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}
	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	ZString GetRowOfAccountingDetails(ITDocSADHBoxCOfficeOfDepartureAndRegistrationBuilder builder, int rowIndex)
	{
		return builder.GetOfficeOfDepartureAndRegistrationData().Split("\r\n").ElementAtOrDefault(rowIndex);
	}

	void SetupOffice()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT304100", "PESCARA", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		Factory.Save();
	}
}
