using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(CusEntryNumbersProvider))]
public abstract class CusEntryNumbersProviderTest<TParentBizObj, TCusEntryNumbersProvider> : NonPersistentBusinessObjectTestCase
	where TParentBizObj : BusinessObject
	where TCusEntryNumbersProvider : CusEntryNumbersProvider
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("When parentBizObj is null", () => GetCusEntryNumbersProvider(parentBizObj: null));
		AssertNoExceptionThrown("When parentBizObj is not null", () => GetCusEntryNumbersProvider(ParentBizObj));
	}

	public void TestRegistrationInfo()
	{
		AssertNull("PRE-CONDITION", entryNumbersProvider.RegistrationInfo);

		Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.RegistrationNumber, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", entryNumbersProvider.RegistrationInfo);
	}

	public void TestRegistrationInfoWrapper()
	{
		CombineAssertions("When REG CusEntryNum is not present", () =>
		{
			var registrationInfoWrapper = entryNumbersProvider.RegistrationInfoWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), ZString.Empty, registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), ZString.Empty, registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), ZDate.Empty, registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), ZString.Empty, registrationInfoWrapper.Series);
		});

		Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.RegistrationNumber, "4 T-2343G", issueDate: new ZDateTime(2020, 01, 01));
		CombineAssertions("When REG CusEntryNum is present", () =>
		{
			var registrationInfoWrapper = entryNumbersProvider.RegistrationInfoWrapper;
			AssertEquals(nameof(registrationInfoWrapper.Register), "4", registrationInfoWrapper.Register);
			AssertEquals(nameof(registrationInfoWrapper.RegistrationNumber), "2343G", registrationInfoWrapper.RegistrationNumber);
			AssertEquals(nameof(registrationInfoWrapper.IssueDate), new ZDateTime(2020, 01, 01), registrationInfoWrapper.IssueDate);
			AssertEquals(nameof(registrationInfoWrapper.Series), "T", registrationInfoWrapper.Series);
		});
	}

	public void TestIrildes()
	{
		AssertNull("PRE-CONDITION", entryNumbersProvider.Irildes);

		Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.Irildes, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", entryNumbersProvider.Irildes);
	}

	public void TestIrildesWrapper()
	{
		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "TR341200", "ERENKÖY GÜMRÜK MÜDÜRLÜĞÜ", startDate, endDate);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "GRL", "Garanzia svincolata", startDate, endDate);
		Factory.Save();

		CombineAssertions("When IRI CusEntryNum is not present", () =>
		{
			var irildesWrapper = entryNumbersProvider.IrildesWrapper;
			AssertEquals(nameof(irildesWrapper.Date), ZDateTime.Empty, irildesWrapper.Date);
			AssertEquals(nameof(irildesWrapper.Office), ZString.Empty, irildesWrapper.Office);
			AssertEquals(nameof(irildesWrapper.OfficeDescription), ZString.Empty, irildesWrapper.OfficeDescription);
			AssertEquals(nameof(irildesWrapper.Status), ZString.Empty, irildesWrapper.Status);
			AssertEquals(nameof(irildesWrapper.StatusDescription), ZString.Empty, irildesWrapper.StatusDescription);
		});

		var entryNumber = Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.Irildes, ZString.Empty, ZDate.Today);
		entryNumber.CE_EntryLineReference = "TR341200";
		entryNumber.CE_EntryStatus = "GRL";

		CombineAssertions("When IRI CusEntryNum is present", () =>
		{
			var irildesWrapper = entryNumbersProvider.IrildesWrapper;
			AssertEquals(nameof(irildesWrapper.Date), ZDateTime.Today, irildesWrapper.Date);
			AssertEquals(nameof(irildesWrapper.Office), "TR341200", irildesWrapper.Office);
			AssertEquals(nameof(irildesWrapper.OfficeDescription), "ERENKÖY GÜMRÜK MÜDÜRLÜĞÜ", irildesWrapper.OfficeDescription);
			AssertEquals(nameof(irildesWrapper.Status), "GRL", irildesWrapper.Status);
			AssertEquals(nameof(irildesWrapper.StatusDescription), "Garanzia svincolata", irildesWrapper.StatusDescription);
		});
	}

	public void TestIvisto()
	{
		AssertNull("PRE-CONDITION", entryNumbersProvider.Ivisto);

		Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.Ivisto, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", entryNumbersProvider.Ivisto);
	}

	public void TestIvistoWrapper()
	{
		var startDate = ZDateTime.Today.AddDays(-2);
		var endDate = ZDateTime.Today.AddDays(2);
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "IT275100", "PONTE CHIASSO", startDate, endDate);

		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "Test");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Italy, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "EXC", "Uscita conclusa", startDate, endDate);
		Factory.Save();

		CombineAssertions("When IVI CusEntryNum is not present", () =>
		{
			var ivistoWrapper = entryNumbersProvider.IvistoWrapper;
			AssertEquals(nameof(ivistoWrapper.Date), ZDateTime.Empty, ivistoWrapper.Date);
			AssertEquals(nameof(ivistoWrapper.Office), ZString.Empty, ivistoWrapper.Office);
			AssertEquals(nameof(ivistoWrapper.OfficeDescription), ZString.Empty, ivistoWrapper.OfficeDescription);
			AssertEquals(nameof(ivistoWrapper.Status), ZString.Empty, ivistoWrapper.Status);
			AssertEquals(nameof(ivistoWrapper.StatusDescription), ZString.Empty, ivistoWrapper.StatusDescription);
		});

		var entryNumber = Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.Ivisto, ZString.Empty, ZDate.Today);
		entryNumber.CE_EntryLineReference = "IT275100";
		entryNumber.CE_EntryStatus = "EXC";

		CombineAssertions("When IVI CusEntryNum is present", () =>
		{
			var ivistoWrapper = entryNumbersProvider.IvistoWrapper;
			AssertEquals(nameof(ivistoWrapper.Date), ZDateTime.Today, ivistoWrapper.Date);
			AssertEquals(nameof(ivistoWrapper.Office), "IT275100", ivistoWrapper.Office);
			AssertEquals(nameof(ivistoWrapper.OfficeDescription), "PONTE CHIASSO", ivistoWrapper.OfficeDescription);
			AssertEquals(nameof(ivistoWrapper.Status), "EXC", ivistoWrapper.Status);
			AssertEquals(nameof(ivistoWrapper.StatusDescription), "Uscita conclusa", ivistoWrapper.StatusDescription);
		});
	}

	public void TestInsertOrUpdateReleaseCode()
	{
		AssertEquals("PRE-CONDITION", 0, GetEntryNumbers(CusEntryNumberConstants.EntryTypes.ClereanceCode).Length);

		var entryNumber = entryNumbersProvider.InsertOrUpdateReleaseCode("XXX", new ZDateTime(2021, 01, 01));
		CombineAssertions("ReleaseCodeEntryNumber Insert", () => AssertEntryNumber(entryNumber, CusEntryNumberConstants.EntryTypes.ClereanceCode, "XXX", new ZDateTime(2021, 01, 01)));

		entryNumber = entryNumbersProvider.InsertOrUpdateReleaseCode("ZZZ", new ZDateTime(2021, 12, 31));
		CombineAssertions("ReleaseCodeEntryNumber Update", () => AssertEntryNumber(entryNumber, CusEntryNumberConstants.EntryTypes.ClereanceCode, "ZZZ", new ZDateTime(2021, 12, 31)));

		entryNumber = entryNumbersProvider.InsertOrUpdateReleaseCode(new ZString('A', 40), new ZDateTime(2020, 12, 31));
		CombineAssertions("CE_EntryNum is truncated to CE_EntryNum.MaxLength", () =>
		{
			AssertEquals("Count of CLR entry numbers", 1, GetEntryNumbers(CusEntryNumberConstants.EntryTypes.ClereanceCode).Length);
			AssertEquals(new ZString('A', CusEntryNumber.Schema.CE_EntryNumMaxLength), entryNumber.CE_EntryNum);
		});
	}

	public void TestInsertOrUpdateManualReleaseCode()
	{
		AssertEquals("PRE-CONDITION", 0, GetEntryNumbers(CusEntryNumberConstants.EntryTypes.ClereanceCode).Length);

		var entryNumber = entryNumbersProvider.InsertOrUpdateManualReleaseCode("XXX", new ZDateTime(2021, 01, 01));
		CombineAssertions("ReleaseCodeEntryNumber Manual Insert", () =>
		{
			AssertEntryNumber(entryNumber, CusEntryNumberConstants.EntryTypes.ClereanceCode, "XXX", new ZDateTime(2021, 01, 01));
			AssertEquals("Manually inserted release codes must be not System generated", false, entryNumber.CE_EntryIsSystemGenerated);
		});
	}

	public void TestInsertOrUpdateEntryNum()
	{
		AssertEquals("PRE-CONDITION", 0, GetEntryNumbers(CusEntryNumberConstants.EntryTypes.ClereanceCode).Length);

		var entryNumber = entryNumbersProvider.InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.ClereanceCode, "XXX", new ZDateTime(2021, 01, 01));
		CombineAssertions("CusEntryNumber Insert", () => AssertEntryNumber(entryNumber, CusEntryNumberConstants.EntryTypes.ClereanceCode, "XXX", new ZDateTime(2021, 01, 01)));

		entryNumber = entryNumbersProvider.InsertOrUpdateEntryNum(CusEntryNumberConstants.EntryTypes.ClereanceCode, "ZZZ", new ZDateTime(2021, 12, 31));
		CombineAssertions("CusEntryNumber Update", () => AssertEntryNumber(entryNumber, CusEntryNumberConstants.EntryTypes.ClereanceCode, "ZZZ", new ZDateTime(2021, 12, 31)));
	}

	public void TestReleaseInfo()
	{
		AssertNull("PRE-CONDITION", entryNumbersProvider.ReleaseInfo);

		Factory.NewCusEntryNumber(ParentBizObj, CusEntryNumberConstants.EntryTypes.ClereanceCode, ZString.Empty, ZDate.Today);
		AssertNotNull("POST-CONDITION", entryNumbersProvider.ReleaseInfo);
	}

	CusEntryNumber[] GetEntryNumbers(ZString entryType)
	{
		var query = new ZQuery(CusEntryNumSchema.CE_ParentID, ParentBizObj.PK);
		query.AddToFilter(CusEntryNumSchema.CE_EntryType, entryType);
		return Factory.Load<CusEntryNumber>(query);
	}

	void AssertEntryNumber(CusEntryNumber cusEntryNum, ZString entryType, ZString entryNum, ZDateTime issueDate)
	{
		AssertEquals("Entry numbers count of specified type", 1, GetEntryNumbers(entryType).Length);
		AssertEquals("CE_EntryNum", entryNum, cusEntryNum.CE_EntryNum);
		AssertEquals("CE_IssueDate", issueDate, cusEntryNum.CE_IssueDate);
		AssertEquals("CE_ParentID", ParentBizObj.PK, cusEntryNum.CE_ParentID);
		AssertEquals("CE_ParentTable", ParentBizObj.TableName, cusEntryNum.CE_ParentTable);
		AssertEquals("CE_EntryType", entryType, cusEntryNum.CE_EntryType);
		AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Italy, cusEntryNum.CE_RN_NKCountryCode);
	}

	protected override BusinessObject GetNewBusinessObject() => GetCusEntryNumbersProvider(ParentBizObj);

	protected abstract TCusEntryNumbersProvider GetCusEntryNumbersProvider(TParentBizObj parentBizObj);

	protected TParentBizObj ParentBizObj => parentBizObj ?? (parentBizObj = GetParentBizObj());
	TParentBizObj parentBizObj;

	protected abstract TParentBizObj GetParentBizObj();

	protected override void SetUp()
	{
		base.SetUp();
		entryNumbersProvider = GetCusEntryNumbersProvider(ParentBizObj);
	}

	TCusEntryNumbersProvider entryNumbersProvider;
}
