using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Documents.CertificateOfOrigin.Testing;

class CustomsEndorsementWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader parameter is null", () => new CustomsEndorsementWrapper(entryHeader: null));
		AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader.Declaration parameter is null", () => new CustomsEndorsementWrapper(entryHeader: Factory.New<CusEntryHeader>()));
	}

	public void TestForm()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("Form", "D.U.A", wrapper.Form);
	}

	public void TestFormNo()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("FormNo", "", wrapper.FormNo);

		entryHeader.MovementReferenceNumberSetter("20ES00999923239");
		wrapper = GetNewWrapper();
		AssertEquals("FormNo", "20ES00999923239", wrapper.FormNo);
	}

	public void TestDate()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("Date", ZDate.Empty, wrapper.Date);

		entryHeader.MovementReferenceNumberIssueDate = ZDateTime.BrettsBirthday;
		wrapper = GetNewWrapper();
		AssertEquals("Date", ZDateTime.BrettsBirthday.Date, wrapper.Date);
	}

	public void TestCustomsOffice()
	{
		SetUpCustomsOffices();

		declaration.JE_CustomsOffice = "IT000001";
		var wrapper = GetNewWrapper();
		AssertEquals("CustomsOffice", "IT000001 Rome", wrapper.CustomsOffice);

		declaration.JE_CustomsOffice = "";
		wrapper = GetNewWrapper();
		AssertEquals("When JE_CustomsOffice is empty, CustomsOffice", ZString.Empty, wrapper.CustomsOffice);

		declaration.JE_CustomsOffice = "XXXXX";
		wrapper = GetNewWrapper();
		AssertEquals("When JE_CustomsOffice is invalid, CustomsOffice", ZString.Empty, wrapper.CustomsOffice);
	}

	public void TestIssuingCountry()
	{
		var spanish = Res.GetLanguageInstance(Enterprise.Core.SharedConstants.Languages.Spanish);
		var spanishMock = spanish.UseMockData();
		spanishMock.Put("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", new ResourceStringData("D239BB93-9FAF-4F4F-B9B3-0B76A6BB77BF", "I am Spanish translation: SPAIN"));

		var wrapper = GetNewWrapper();
		AssertEquals("IssuingCountry is translated to ES", "I am Spanish translation: SPAIN", wrapper.IssuingCountry);
	}

	public void TestPlace()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("Place", "", wrapper.Place);

		var supplier = Factory.NewWithValidTestData<OrgHeader>();
		var supplierAddress = supplier.MainAddress;
		supplierAddress.OA_City = "BARCELONA";
		declaration.JE_OH_Supplier = supplier.PK;

		var declarant = Factory.NewWithValidTestData<OrgHeader>();
		var declarantMainAddress = declarant.MainAddress;
		declarantMainAddress.OA_City = "VALENCIA";

		var declarantOrgAddress = Factory.New<OrgAddress>();
		declarantOrgAddress.OA_City = "MADRID";

		declaration.JE_OA_DeclarantAddress = declarantOrgAddress.PK;

		wrapper = GetNewWrapper();
		AssertEquals("Place", "MADRID", wrapper.Place);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	EU.Business.Documents.CertificateOfOrigin.ICustomsEndorsement GetNewWrapper() => new CustomsEndorsementWrapper(entryHeader);

	public void TestEntryNumber()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("no entry number ", "", wrapper.EntryNumber);

		entryHeader.EntryNumber = "entrynum";

		wrapper = GetNewWrapper();
		AssertEquals("entry with an entrynumber => EntryNumber ok", "entrynum", wrapper.EntryNumber);
	}

	void SetUpCustomsOffices()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var eunZZZPK = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Italy, parent: eunZZZPK);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT000001", "Rome", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		Factory.Save();
	}
}
