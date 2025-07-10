using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusEntryHeader))]
sealed class CusEntryHeaderTest : Customs.Business.Testing.CusEntryHeaderTest
{
	public void TestCreateTime()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		AssertEquals(ZDateTime.Empty, entryHeader.CreateTime);

		entryHeader.CH_SystemCreateTimeUtc = new ZDateTime(2025, 01, 08);

		AssertEquals(new ZDateTime(2025, 01, 08).ToLocalBranchTime(), entryHeader.CreateTime);
	}

	public void TestIMessageAttachee()
	{
		var entryHeader = Factory.New<CusEntryHeader>();
		IMessageAttachee messageAttachee = entryHeader;
		var declartion = Factory.New<JobDeclaration>();
		declartion.ActiveEntryHeaders.Add(entryHeader);
		declartion.JE_CustomsOffice = "INNSA1";

		CombineAssertions(() =>
		{
			AssertSame(entryHeader.Messages, messageAttachee.Messages);
			AssertEquals("INNSA1", messageAttachee.MessageOwner);
			AssertEquals("BranchPK", declartion.JE_GB, messageAttachee.BranchPK);

			messageAttachee.MessageStatus = "QUE";
			AssertEquals("MessageStatus", "QUE", messageAttachee.MessageStatus);
			AssertEquals("CH_Status", "QUE", entryHeader.CH_Status);

			messageAttachee.CustomsStatus = "ACC";
			AssertEquals("CustomsStatus", "ACC", messageAttachee.CustomsStatus);
			AssertEquals("CH_EntryStatus", "ACC", entryHeader.CH_EntryStatus);

			AssertEquals("CalculateStatusAfterSending", ZString.Empty, messageAttachee.CalculateStatusAfterSending("Test"));
		});
	}

	public void TestRollbackChangesOnStatus()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		IMessageAttachee messageAttachee = entryHeader;

		CombineAssertions(() =>
		{
			messageAttachee.MessageStatus = "QUE";
			messageAttachee.CustomsStatus = "ACC";
			messageAttachee.RollbackChangesOnStatus();
			AssertEquals("MessageStatus reset not in db", ZString.Empty, messageAttachee.MessageStatus);
			AssertEquals("CustomsStatus reset not in db", ZString.Empty, messageAttachee.CustomsStatus);

			messageAttachee.MessageStatus = "QUE";
			messageAttachee.CustomsStatus = "ACC";
			Factory.Save();

			messageAttachee.MessageStatus = "FAL";
			messageAttachee.CustomsStatus = "ERR";
			messageAttachee.RollbackChangesOnStatus();
			AssertEquals("CustomsStatus reset in db", "QUE", messageAttachee.MessageStatus);
			AssertEquals("MessageStatus reset in db", "ACC", messageAttachee.CustomsStatus);
		});
	}

	public void TestOnSaving()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;
		Assert(!entryHeader.IsInDatabase);
		AssertEquals(ZString.Empty, entryHeader.CH_BGMReference);
		AssertEquals(ZDateTime.Empty, entryHeader.CH_SystemCreateTimeUtc);

		Factory.Save();
		Assert(entryHeader.IsInDatabase);
		AssertNotEquals(ZString.Empty, entryHeader.CH_BGMReference);
		AssertNotEquals(ZDateTime.Empty, entryHeader.CH_SystemCreateTimeUtc);
	}

	public void TestShouldResetBGMReferenceOnUnsuccessfulSave()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = instruction.PK;

		entryHeader.CH_BGMReference = "123";
		entryHeader.RecoverFromUnsuccessfulSave();
		AssertEquals(ZString.Empty, entryHeader.CH_BGMReference);

		Factory.Save();
		Assert(entryHeader.IsInDatabase);
		entryHeader.RecoverFromUnsuccessfulSave();
		AssertNotEquals(ZString.Empty, entryHeader.CH_BGMReference);
	}

	public void TestJobWorks()
	{
		var declaration1 = Factory.New<JobDeclaration>();
		declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
		var header1 = declaration1.ActiveEntryHeaders.AddNew();
		var entryLine1 = header1.MergedLines.AddNew();
		var invoiceHeader1 = declaration1.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine1.JI_LineNo = 1;

		var jobWork1 = invoiceLine1.JobWorks.AddNew();
		jobWork1.CSI_LineNo = 1;
		jobWork1.CSI_Quantity = 19.000;

		var jobWork2 = invoiceLine1.JobWorks.AddNew();
		jobWork2.CSI_LineNo = 2;
		jobWork2.CSI_Quantity = 29.000;

		var entryLine2 = header1.MergedLines.AddNew();
		var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.JI_LineNo = 2;

		var jobWork3 = invoiceLine2.JobWorks.AddNew();
		jobWork3.CSI_LineNo = 3;
		jobWork3.CSI_Quantity = 39.000;

		var jobWork4 = invoiceLine2.JobWorks.AddNew();
		jobWork4.CSI_LineNo = 4;
		jobWork4.CSI_Quantity = 49.000;

		AssertContainsExactElementsInAnyOrder([jobWork1, jobWork2, jobWork3, jobWork4], header1.JobWorks);
	}

	[TestDate(2005, 6, 2)]
	public override void TestFOBInLocalCurrency()
	{
		RefDataSetupTestHelper.SetupStandardCurrencyCodes(Factory, "MDD");
		base.TestFOBInLocalCurrency();
	}

	protected override Type ExpectedChargeCollectionType => typeof(CusEntryHeaderChargesCollection<CusEntryHeaderCharges>);

	protected override Type ExpectedChargeType => typeof(CusEntryHeaderCharges);

	public void TestSWConstitute()
	{
		var declaration1 = Factory.New<JobDeclaration>();
		declaration1.JE_MessageType = JobMessageTypeList.Codes.Export;
		var header1 = declaration1.ActiveEntryHeaders.AddNew();
		var entryLine1 = header1.MergedLines.AddNew();
		var invoiceHeader1 = declaration1.Invoices.AddNew();
		var invoiceLine1 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine1.JI_CL = entryLine1.PK;
		invoiceLine1.JI_LineNo = 1;

		var constituent1 = invoiceLine1.SWConstituents.AddNew();
		constituent1.CSI_LineNo = 1;
		constituent1.CSI_Quantity = 19.000;

		var constituent2 = invoiceLine1.SWConstituents.AddNew();
		constituent2.CSI_LineNo = 2;
		constituent2.CSI_Quantity = 29.000;

		var entryLine2 = header1.MergedLines.AddNew();
		var invoiceLine2 = invoiceHeader1.InvoiceLines.AddNew();
		invoiceLine2.JI_CL = entryLine2.PK;
		invoiceLine2.JI_LineNo = 2;

		var constituent3 = invoiceLine2.SWConstituents.AddNew();
		constituent3.CSI_LineNo = 3;
		constituent3.CSI_Quantity = 39.000;

		var constituent4 = invoiceLine2.SWConstituents.AddNew();
		constituent4.CSI_LineNo = 4;
		constituent4.CSI_Quantity = 49.000;
		AssertContainsExactElementsInAnyOrder([constituent1, constituent2, constituent3, constituent4], header1.SWConstituents);
	}
}
