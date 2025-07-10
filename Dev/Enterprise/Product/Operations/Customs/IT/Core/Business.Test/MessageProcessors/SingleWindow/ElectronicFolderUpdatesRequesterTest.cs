using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ElectronicFolderUpdatesRequesterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("EntryHeader cannot be null", () => new ElectronicFolderUpdatesRequester(null, new CargoWise.EntityFramework.BusinessObjectFactory()));
		AssertExceptionThrown<ArgumentNullException>("Factory cannot be null", () => new ElectronicFolderUpdatesRequester(entryHeader, null));
	}

	public void TestRequestUpdates()
	{
		Factory.NewCusEntryNumber(entryHeader, entryType: "REG", entryNum: "4 -95751G", issueDate: new ZDateTime(2021, 01, 28));
		var newEdiMessage = new ElectronicFolderUpdatesRequester(entryHeader, entryHeader.Factory).RequestUpdates();
		var expectedContent =
			"<richiesta_esito>" +
				"<dichiarazione>" +
					"<num_reg>95751</num_reg>" +
					"<cod_uff_dog>279100</cod_uff_dog>" +
					"<cod_reg>4</cod_reg>" +
					"<anno_reg>28012021</anno_reg>" +
				"</dichiarazione>" +
			"</richiesta_esito>";

		CombineAssertions(() => AssertExpectedEDIMessageValues(newEdiMessage, entryHeader, expectedContent));
	}

	public void TestRequestUpdatesWithoutRegistrationInfo()
	{
		var newEdiMessage = new ElectronicFolderUpdatesRequester(entryHeader, entryHeader.Factory).RequestUpdates();

		var expectedContent =
			"<richiesta_esito>" +
				"<dichiarazione>" +
					"<num_reg />" +
					"<cod_uff_dog>279100</cod_uff_dog>" +
					"<cod_reg />" +
					"<anno_reg />" +
				"</dichiarazione>" +
			"</richiesta_esito>";

		CombineAssertions("Edge Case", () => AssertExpectedEDIMessageValues(newEdiMessage, entryHeader, expectedContent));
	}

	void AssertExpectedEDIMessageValues(ITEDIMessage message, CusEntryHeader entryHeader, ZString expectedContent)
	{
		AssertEquals("EM_ApplicationCode", Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.ITCustoms, message.EM_ApplicationCode);
		AssertEquals("EM_MessageType", MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageType);
		AssertEquals("EM_MessageSubType", MessageProcessorConstants.InterchangeTypes.SingleWindowRequest, message.EM_MessageSubType);
		Assert("IsTransmitMessage", message.IsTransmitMessage);
		AssertEquals("EM_ApplicationReference", entryHeader.Declaration.GetApplicationReference(), message.EM_ApplicationReference);
		AssertEquals("EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		AssertEquals("EM_LinkTable", CusEntryHeader.Schema.TableName, message.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", entryHeader.PK, message.EM_LinkUniqueID);
		AssertMultilineASCIIEquals("EM_MessageText", expectedContent, message.EM_MessageText);
		AssertEquals("EM_MessageNum", ZString.Empty, message.EM_MessageNum);
	}

	protected override void SetUp()
	{
		base.SetUp();

		var jobDeclaration = Factory.New<JobDeclaration>();
		jobDeclaration.JE_CustomsProfile = "845A";
		jobDeclaration.JE_GS_NKCusAgent = "CRR";
		jobDeclaration.JE_CustomsOffice = "IT279100";
		jobDeclaration.JE_MessageType = "IMP";
		entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
	}

	CusEntryHeader entryHeader;
}
