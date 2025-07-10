using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class CustomsEndorsementWrapperTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Should be exception when entryHeader parameter is null", () => new CustomsEndorsementWrapper(entryHeader: null));
	}

	public void TestForm()
	{
		declaration.JE_EntryStyle = "";
		var customsEndorsement = GetNewWrapper();
		AssertEquals("Form", "", customsEndorsement.Form);

		declaration.JE_MessageType = "EXP";
		declaration.JE_EntryStyle = "EX";
		customsEndorsement = GetNewWrapper();
		AssertEquals("Form", "EX", customsEndorsement.Form);

		AddRegEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("Form", "EX 4 T", customsEndorsement.Form);

		entryHeader.ZG_AmendmentStatus = AmendmentStatusList.Codes.Amendment;
		AddRetEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("When ZG_AmendmentStatus is Amendment, Form", "EX 2", customsEndorsement.Form);
	}

	public void TestFormNo()
	{
		var customsEndorsement = GetNewWrapper();
		AssertEquals("FormNo", "", customsEndorsement.FormNo);

		AddRegEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("FormNo", "123456G", customsEndorsement.FormNo);

		entryHeader.ZG_AmendmentStatus = AmendmentStatusList.Codes.Amendment;
		AddRetEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("When ZG_AmendmentStatus is Amendment, Form", "654321R", customsEndorsement.FormNo);
	}

	public void TestDate()
	{
		var customsEndorsement = GetNewWrapper();
		AssertEquals("Date", ZDate.Empty, customsEndorsement.Date);

		AddRegEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("Date", new ZDate(2021, 04, 28), customsEndorsement.Date);

		entryHeader.ZG_AmendmentStatus = AmendmentStatusList.Codes.Amendment;
		AddRetEntryNumber();
		customsEndorsement = GetNewWrapper();
		AssertEquals("When ZG_AmendmentStatus is Amendment, Date", new ZDate(2021, 05, 28), customsEndorsement.Date);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		entryHeader = declaration.CustomsEntryHeaders.AddNew();
	}

	void AddRegEntryNumber()
	{
		var cusEntryNumber = Factory.New<CusEntryNumber>();
		cusEntryNumber.CE_EntryType = "REG";
		cusEntryNumber.CE_ParentID = entryHeader.PK;
		cusEntryNumber.CE_ParentTable = entryHeader.TableName;
		cusEntryNumber.CE_Category = "CUS";
		cusEntryNumber.CE_EntryNum = "4 T-123456G";
		cusEntryNumber.CE_IssueDate = new ZDateTime(2021, 04, 28);
	}

	void AddRetEntryNumber()
	{
		var retEntryNumber = Factory.New<CusEntryNumber>();
		retEntryNumber.CE_EntryType = "RET";
		retEntryNumber.CE_ParentID = entryHeader.PK;
		retEntryNumber.CE_ParentTable = entryHeader.TableName;
		retEntryNumber.CE_Category = "CUS";
		retEntryNumber.CE_EntryNum = "2 -654321R";
		retEntryNumber.CE_IssueDate = new ZDateTime(2021, 05, 28);
	}

	public void TestEntryNumber()
	{
		var wrapper = GetNewWrapper();
		AssertEquals("no entry number ", "", wrapper.EntryNumber);

		entryHeader.EntryNumber = "entrynum";

		wrapper = GetNewWrapper();
		AssertEquals("entry with an entrynumber => EntryNumber ok", "entrynum", wrapper.EntryNumber);
	}

	JobDeclaration declaration;
	CusEntryHeader entryHeader;

	EU.Business.Documents.CertificateOfOrigin.ICustomsEndorsement GetNewWrapper() => new CustomsEndorsementWrapper(entryHeader);
}
