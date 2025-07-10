using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class NBMessageSerializationTest : TestCaseWithFactory
{
	public void TestSerializeMessageAnEntryWithMultipleM2Lines()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var jobDeclarationMessageSendingObjectParent = new JobDeclarationMessageSendingObjectParent(declaration);

		var paDocument1 = declaration.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1A";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Status = "X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_LineNo = 1;

		var paDocument2 = declaration.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2";

		var rpDocument1 = declaration.PreviousDocuments.AddNew();
		rpDocument1.CSI_Procedure = "2";
		rpDocument1.CSI_ReferenceNumber = "1";

		declaration.ResetApportionedPreviousDocuments();

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		entryHeader.CH_EntryStatus = "NBR";
		var entryNumber = entryHeader.Factory.New<CusEntryNumber>();
		entryNumber.CE_ParentID = entryHeader.PK;
		entryNumber.CE_ParentTable = entryHeader.TableName;
		entryNumber.CE_EntryType = "REG";
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_RN_NKCountryCode = entryHeader.CountryCode;
		entryNumber.CE_EntryNum = "4 T-61689G";
		entryNumber.CE_IssueDate = new ZDateTime(2020, 04, 23);

		var entryLine1 = entryHeader.MergedLines.AddNew();
		entryLine1.CL_LineNumber = 1;
		entryLine1.ZG_NBStatus = "NBR";
		var entryLine2 = entryHeader.MergedLines.AddNew();
		entryLine2.CL_LineNumber = 2;
		entryLine2.ZG_NBStatus = "NBA";

		var serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent))).GenerateMessage().EM_MessageText;
		AssertMultilineASCIIEquals("Seriliaze NB Messages"
			, "TNB           <<MSGNO PLACEHOLDER NB 1>>004 T	61689	G	230420	1	A3	1	A	010120	X	1	137100		2	1						0	0				A3	2							2	1						0	0																																												0	"
			, serializedMessage);

		entryLine2.ZG_NBStatus = "NBR";

		serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, new NBStandaloneMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent))).GenerateMessage().EM_MessageText;
		AssertMultilineASCIIEquals("Seriliaze NB Messages"
			, "TNB           <<MSGNO PLACEHOLDER NB 1>>004 T	61689	G	230420	1	A3	1	A	010120	X	1	137100		2	1						0	0				A3	2							2	1						0	0																																												0	" +
			  "\r\nTNB           <<MSGNO PLACEHOLDER NB 2>>004 T	61689	G	230420	2	A3	1	A	010120	X	1	137100		2	1						0	0				A3	2							2	1						0	0																																												0	"
			, serializedMessage);
	}
}
