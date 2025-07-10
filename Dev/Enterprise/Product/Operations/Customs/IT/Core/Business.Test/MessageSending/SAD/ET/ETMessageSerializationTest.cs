using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ETMessageSerializationTest : SADMessageSerializationTest
{
	protected override ZString MessageType => "EXP";

	public void TestSimpleETDeclarationMessageSerialization()
	{
		declaration.JE_DeclarantType = "DIR";
		invoiceLine.JI_StateOrRegionOfOrigin = "PD";
		declaration.DoMerge();

		AssertEquals("PRE-CONDITION: 1 entry header", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("PRE-CONDITION: 1 Merged Line", 1, entryHeader.MergedLines.Count);
		var sendingObject = new ETMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
		var serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject)).GenerateMessage().EM_MessageText;
		AssertMultilineASCIIEquals(
			"TET           <<MSGNO PLACEHOLDER>>00123456D	EX	A		01012020	0			1																2								  	FI	RX2839A		KR	1	DAF	1	PLACE		4343						EUR	1009.00			4	9												0																																										0	0							" + "\r\n" +
			"?ET1          <<MSGNO PLACEHOLDER>>00																																	0	0						1	9301200000		1	S001	PD	0	4000	1	0	100	Z		2																		0									1009.00		0	0.00		"
			, serializedMessage);
	}

	public void TestETDeclarationWithNBMessageSerialization()
	{
		declaration.JE_DeclarantType = "DIR";
		invoiceLine.JI_StateOrRegionOfOrigin = "PD";
		entryInstruction.CEI_Procedure = "40";
		invoiceLine.JI_Procedure = "4000";
		var paDocument1 = invoiceLine.PreviousDocuments.AddNew();
		paDocument1.CSI_Procedure = "A3";
		paDocument1.CSI_ReferenceNumber = "1X";
		paDocument1.CSI_CustomsOffice = "IT137100";
		paDocument1.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument1.CSI_Quantity3 = 200m;
		var paDocument2 = invoiceLine.PreviousDocuments.AddNew();
		paDocument2.CSI_Procedure = "A3";
		paDocument2.CSI_ReferenceNumber = "2Z";
		paDocument2.CSI_DateOfIssue = new ZDate(2020, 01, 01);
		paDocument2.CSI_Quantity3 = 199m;
		declaration.DoMerge();

		AssertEquals("PRE-CONDITION: 1 entry header", 1, declaration.CustomsEntryHeaders.Count);
		var entryHeader = declaration.CustomsEntryHeaders[0];
		AssertEquals("PRE-CONDITION: 1 Merged Line", 1, entryHeader.MergedLines.Count);

		var sendingObject = new ETMessageSendingObject(entryHeader, jobDeclarationMessageSendingObjectParent);
		var serializedMessage = ((IOutgoingCustomsMessageCreationStrategy)new AutomaticProcedureSadOutgoingCustomsMessageCreationStrategy(Factory, sendingObject)).GenerateMessage().EM_MessageText;
		AssertMultilineASCIIEquals(
			"TET           <<MSGNO PLACEHOLDER>>00123456D	EX	A		01012020	0			1																2								  	FI	RX2839A		KR	1	DAF	1	PLACE		4343						EUR	1009.00			4	9												0																																										0	0							" + "\r\n" +
			"?ET1          <<MSGNO PLACEHOLDER>>00																																	0	0						1	9301200000		1	S001	PD	0	4000	1	0	100	Z	ZZZ	M2																		0									1009.00		0	0.00		" + "\r\n" +
			"TNB           <<MSGNO PLACEHOLDER>>01ET	<<MSGNO PLACEHOLDER>>			1	A3	1	X	010120			137100		2							0	200				A3	2	Z	010120					2							0	199																																												0	"
			, serializedMessage);
	}
}
