using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Messaging.SAD;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

public abstract class SADMessageSendingObjectTest<T> : JobDeclarationMessageSendingObjectTest
	where T : SADMessageSendingObject
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentOutOfRangeException>(() =>
		{
			var entryHeaderNoLines = Factory.New<CusEntryHeader>();
			GetNewSADMessageSendingObject(entryHeaderNoLines, JobDeclarationMessageSendingObjectParent);
		});
		AssertNoExceptionThrown(() => GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent));
	}

	public void TestNBMessages()
	{
		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("No NB messages (because there are no previous documents)", 0, messageSendingObject.NBMessages.Count());

		var paDocument1 = Declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = Declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = Declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";

		Declaration.ResetApportionedPreviousDocuments();
		var nbMessages = messageSendingObject.NBMessages;
		AssertEquals("One NB message", 1, nbMessages.Count());
		AssertType<NBMessageWrapperWithinOriginalDeclaration>(nbMessages.SingleOrDefault());
	}

	public void TestNBMessagesForDeterminingMessageChangedStatus()
	{
		var messageSendingObject = GetNewSADMessageSendingObjectForDeterminingMessageChangedStatus(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("No NB messages (because there are no previous documents)", 0, messageSendingObject.NBMessages.Count());

		var paDocument1 = Declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = Declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = Declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";

		var entryLine = EntryHeader.MergedLines.SingleOrDefault();
		Declaration.ResetApportionedPreviousDocuments();

		entryLine.ZG_NBStatus = "";
		AssertEquals("NBMessages [ZG_NBStatus is Empty]", 0, messageSendingObject.NBMessages.Count());

		entryLine.ZG_NBStatus = "NBR";
		AssertEquals("NBMessages [ZG_NBStatus = NBR]", 0, messageSendingObject.NBMessages.Count());

		entryLine.ZG_NBStatus = "NBA";
		AssertEquals("NBMessages [ZG_NBStatus = NBA]", 1, messageSendingObject.NBMessages.Count());
		AssertType<NBMessageWrapperWithinOriginalDeclaration>(messageSendingObject.NBMessages.SingleOrDefault());

		entryLine.ZG_NBStatus = "NBS";
		AssertEquals("NBMessages [ZG_NBStatus = NBS]", 1, messageSendingObject.NBMessages.Count());
		AssertType<NBMessageWrapperWithinOriginalDeclaration>(messageSendingObject.NBMessages.SingleOrDefault());
	}

	public override void TestCombinedCustomsMessageSubType()
	{
		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		var expectedMessageSubType = GetMessageSubType();
		AssertEquals("CombinedCustomsMessageSubType", expectedMessageSubType, messageSendingObject.CombinedCustomsMessageSubType);

		var paDocument1 = Declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = Declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = Declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";

		Declaration.ResetApportionedPreviousDocuments();
		AssertEquals("CombinedCustomsMessageSubType", $"{expectedMessageSubType} + NB", messageSendingObject.CombinedCustomsMessageSubType);
	}

	public override void TestStatusAllowSending()
	{
		base.TestStatusAllowSending();

		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		EntryHeader.CH_Status = "ACO";
		EntryHeader.CH_EntryStatus = "REG";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: REG] StatusAllowSending", false, messageSendingObject.StatusAllowsSending);

		EntryHeader.CH_EntryStatus = "NBR";
		AssertEquals("[CHStatus: ACO, CH_EntryStatus: NBR] StatusAllowSending", true, messageSendingObject.StatusAllowsSending);

		EntryHeader.CH_Status = "AWO";
		EntryHeader.CH_EntryStatus = "NBR";
		AssertEquals("[CHStatus: AWO, CH_EntryStatus: NBR] StatusAllowSending", false, messageSendingObject.StatusAllowsSending);
	}

	public void TestFallbackProcedure()
	{
		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		var sadMessageSendingObject = (ISadMessageSendingObject)messageSendingObject;
		messageSendingObject.CustomsMessageSendingMode = ZString.Empty;
		Assert("Fallback procedure is not expected as default behaviour", !sadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.FallbackProcedure;
		Assert("Expected fallback procedure", sadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
		Assert("AutomaticProcedure is not FallbackProcedure", !sadMessageSendingObject.FallbackProcedure);

		messageSendingObject.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.ManualProcedure;
		Assert("ManualProcedure is not FallbackProcedure", !sadMessageSendingObject.FallbackProcedure);
	}

	public void TestDeclarantTaxNumber()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		new AccountCollectionTestBuilder(company.PK)
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();

		var messageSendingObject = GetNewSADMessageSendingObject(EntryHeader, JobDeclarationMessageSendingObjectParent);
		var sadMessageSendingObject = (ISadMessageSendingObject)messageSendingObject;
		Declaration.JE_CustomsProfile = "";
		AssertEquals(ZString.Empty, sadMessageSendingObject.DeclarantTaxNumber);

		Declaration.JE_CustomsProfile = "9999";
		AssertEquals(ZString.Empty, sadMessageSendingObject.DeclarantTaxNumber);

		Declaration.JE_CustomsProfile = "1234-DEC1";
		AssertEquals("11111111111", sadMessageSendingObject.DeclarantTaxNumber);
	}

	protected abstract T GetNewSADMessageSendingObject(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent);
	protected abstract T GetNewSADMessageSendingObjectForDeterminingMessageChangedStatus(CusEntryHeader entryHeader, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent);

	protected abstract ZString GetMessageSubType();
}
