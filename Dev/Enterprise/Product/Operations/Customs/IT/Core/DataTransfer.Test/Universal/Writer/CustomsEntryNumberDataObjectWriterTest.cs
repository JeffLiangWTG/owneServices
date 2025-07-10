using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.DataTransfer.Universal;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.IT.DataTransfer.Universal.Testing;

sealed class CustomsEntryNumberDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
{
	public void TestCustomsEntryNumberMappings()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.CustomsEntryHeaders.AddNew();
		var entryNumberBO = SetupCusEntryNumber(Factory.BOFactory);
		entryNumberBO.Parent = header;

		var writer = new CustomsEntryNumberDataObjectWriter(
			new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, entryNumberBO)),
			new UniversalDataObjectWriterHelper(Factory.BOFactory, "IT"));
		var entryNumberDataObject = writer.GetDataObject(entryNumberBO);

		AssertContents(entryNumberDataObject);
	}

	CusEntryNumber SetupCusEntryNumber(CusEntryNumber cusEntryNumber, ZString entryNum, ZString entryType, ZString entryLineReference)
	{
		cusEntryNumber.CE_EntryNum = entryNum;
		cusEntryNumber.CE_EntryType = entryType;
		cusEntryNumber.CE_EntryLineReference = entryLineReference;
		cusEntryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
		cusEntryNumber.CE_IssueDate = new ZDateTime(2011, 6, 1);

		return cusEntryNumber;
	}

	CusEntryNumber SetupCusEntryNumber(BusinessObjectFactory factory)
	{
		return SetupCusEntryNumber(factory.New<CusEntryNumber>(), "CE00001", "IMP", "REFERENCE");
	}

	void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject, ZString entryLineReference, ZString number, ICodeDescription type)
	{
		AssertNotNull("Precondition: entryNumberDataObject", entryNumberDataObject);
		CombineAssertions(delegate
		{
			AssertEquals("entryNumberDataObject.EntryLineReference", entryLineReference, entryNumberDataObject.EntryLineReference);
			AssertEquals("entryNumberDataObject.Number", number, entryNumberDataObject.Number);
			AssertNotNull("entryNumberDataObject.Type", entryNumberDataObject.Type);
			AssertEquals("entryNumberDataObject.Type.Code", type.Code, entryNumberDataObject.Type.Code);
			AssertEquals("entryNumberDataObject.Type.Description", type.Description, entryNumberDataObject.Type.Description);
		});
	}

	void AssertContents(UniversalCustoms.EntryNumber entryNumberDataObject)
	{
		AssertContents(entryNumberDataObject, "REFERENCE", "CE00001", CodeDescriptionPairForTesting.New("IMP", "Import", 0));
	}
}
