using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(NBNEStandaloneMessageSendingObject))]
sealed class NBNEStandaloneMessageSendingObjectTest : JobDeclarationMessageSendingObjectTest
{
	protected override ZString ExpectedMessageSubType => SADConstants.MessageSubTypes.NBE;

	public override void TestCombinedCustomsMessageSubType()
	{
		Declaration.JE_MessageType = "EXP";

		AddPreviousDocumentsForNb();

		var messageSendingObject = new NBNEStandaloneMessageSendingObjectForTest(EntryHeader, JobDeclarationMessageSendingObjectParent);
		AssertEquals("CombinedCustomsMessageSubType", "NB + NE", messageSendingObject.CombinedCustomsMessageSubType);
	}

	#region Implementation

	void AddPreviousDocumentsForNb()
	{
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
	}

	class NBNEStandaloneMessageSendingObjectForTest : NBNEStandaloneMessageSendingObject
	{
		public NBNEStandaloneMessageSendingObjectForTest(CusEntryHeader header, JobDeclarationMessageSendingObjectParent jobDeclarationMessageSendingObjectParent) : base(header, jobDeclarationMessageSendingObjectParent)
		{
		}
	}

	#endregion
}
