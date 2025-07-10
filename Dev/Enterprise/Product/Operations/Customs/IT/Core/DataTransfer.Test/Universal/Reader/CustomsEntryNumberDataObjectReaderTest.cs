using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryNumberDataObjectReaderTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestCusEntryNumber_EntryLineReferenceFieldMappings()
	{
		var logger = new TestErrorLogger();
		var currentCompanyHelper = new Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper(Factory, "IT", "IT");
		var entryHeader = Factory.New<CusEntryHeader>();
		var entryNumberDataObject = SetupEntryNumber(new EntryType() { Code = "EXP" }, "B32432", ZBool.False, entryLineReference: "REF");
		var entryNumberBO = new CustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, currentCompanyHelper, entryHeader).ReadIntoBusinessObject();
		AssertCusEntryNumberContents(entryNumberBO, entryHeader.TableName, entryHeader.PK, "EXP", "B32432", ZBool.False, currentCompanyHelper.TargetCountryCode, entryLineReference: "REF");

		entryNumberDataObject = SetupEntryNumber(new EntryType() { Code = "IMP" }, "B32432", ZBool.False, entryLineReference: "REF");
		entryNumberBO = new CustomsEntryNumberDataObjectReader(entryNumberDataObject, logger, currentCompanyHelper, entryHeader).ReadIntoBusinessObject();
		AssertCusEntryNumberContents(entryNumberBO, entryHeader.TableName, entryHeader.PK, "IMP", "B32432", ZBool.False, currentCompanyHelper.TargetCountryCode, entryLineReference: "REF");
	}

	UniversalCustoms.EntryNumber SetupEntryNumber(EntryType type, ZString? number, ZBool? entryIsSystemGenerated, EntryStatus entryStatus = null, ZDateTime? issueDate = null, ZDateTime? expiryDate = null, ZString? entryLineReference = null)
	{
		return new UniversalCustoms.EntryNumber()
		{
			Type = type,
			Number = number,
			EntryIsSystemGenerated = entryIsSystemGenerated,
			EntryLineReference = entryLineReference,
			EntryStatus = entryStatus,
			IssueDate = issueDate,
			ExpiryDate = expiryDate
		};
	}

	void AssertCusEntryNumberContents(CusEntryNumber entryNumberBO, ZString parentTable, ZGuid parentID, ZString type, ZString number, ZBool isSystemGenerated, ZString countryCode, ZString? entryStatusCode = null, ZDateTime? issueDate = null, ZDateTime? expiryDate = null, ZString? entryLineReference = null)
	{
		AssertEquals("entryNumberBO.CE_ParentTable", parentTable, entryNumberBO.CE_ParentTable);
		AssertEquals("entryNumberBO.CE_ParentID", parentID, entryNumberBO.CE_ParentID);
		AssertEquals("entryNumberBO.CE_Type", type, entryNumberBO.CE_EntryType);
		AssertEquals("entryNumberBO.CE_EntryNum", number, entryNumberBO.CE_EntryNum);
		AssertEquals("entryNumberBO.CE_EntryIsSystemGenerated", isSystemGenerated, entryNumberBO.CE_EntryIsSystemGenerated);
		AssertEquals("entryNumberBO.CE_RN_NKCountryCode", countryCode, entryNumberBO.CE_RN_NKCountryCode);
		if (entryStatusCode.HasValue)
		{
			AssertEquals("entryNumberBO.CE_EntryStatus", entryStatusCode, entryNumberBO.CE_EntryStatus);
		}
		if (issueDate.HasValue)
		{
			AssertEquals("entryNumberBO.CE_IssueDate", issueDate, entryNumberBO.CE_IssueDate);
		}
		if (expiryDate.HasValue)
		{
			AssertEquals("entryNumberBO.CE_ExpiryDate", expiryDate, entryNumberBO.CE_ExpiryDate);
		}
		if (entryLineReference.HasValue)
		{
			AssertEquals("entryNumberBO.CE_EntryLineReference", entryLineReference, entryNumberBO.CE_EntryLineReference);
		}
	}
}
