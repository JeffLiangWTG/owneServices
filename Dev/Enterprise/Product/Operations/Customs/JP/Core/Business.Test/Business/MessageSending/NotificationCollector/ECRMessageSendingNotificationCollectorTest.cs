using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.JP.Common;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing;

[TestedType(typeof(ECRMessageSendingNotificationCollector))]
sealed class ECRMessageSendingNotificationCollectorTest : TestCaseWithFactory
{
	public void TestShouldIncludeNotificationsFromObject()
	{
		CombineAssertions(() =>
		{
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(parent));
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(parent.SendingObjectsCollection[0]));
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(entryInstruction));
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(declaration));
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(moveInDestination));
			Assert(collector.ShouldIncludeNotificationsFromObjectForTest(inventoryInfo));
		});
	}

	public void TestShouldIncludeNotificationsFromInfo()
	{
		CombineAssertions(() =>
		{
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(declaration.JE_ReceiptModeInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(declaration.JE_RL_NKFinalDestinationInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(entryInstruction.CEI_GoodsDescriptionInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(entryInstruction.CEI_GrossWeightInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(entryInstruction.CEI_CargoQuantityInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(entryInstruction.CEI_VolumeInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_CodeInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_DateOfIssueInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_ReferenceNumberInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_QuantityInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_Quantity2Info));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_Quantity3Info));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(moveInDestination.CSI_DescriptionInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(inventoryInfo.CSI_CodeInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(inventoryInfo.CSI_QuantityInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(parent.ExportPathInfo));
			Assert(collector.ShouldIncludeNotificationsFromInfoForTest(parent.SendingObjectsCollection[0].DeclarationCorrectionCopyRequestInfo));
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var messageSendingContext = new MessageSendingContext() { ProcedureCode = JPProcedureCodeList.Codes.ECR };
		declaration.SetCurrentMessageSendingContext(messageSendingContext);
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		moveInDestination = entryInstruction.MoveInDestinationInfos.AddNew();
		inventoryInfo = moveInDestination.InventoryNumbers.AddNew();
		var entryHeader = declaration.ActiveEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;

		parent = new DeclarationMessageSendingObjectParent(declaration);
		collector = new ECRMessageSendingNotificationCollectorForTest(parent, parent.SendingObjectsCollection.Select(x => x.Header));
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	MoveInDestination moveInDestination;
	InventoryInfo inventoryInfo;
	DeclarationMessageSendingObjectParent parent;
	ECRMessageSendingNotificationCollectorForTest collector;
}

class ECRMessageSendingNotificationCollectorForTest(DeclarationMessageSendingObjectParent parent, IEnumerable<CusEntryHeader> selectedEntries) : ECRMessageSendingNotificationCollector(parent, selectedEntries, false)
{
	public bool ShouldIncludeNotificationsFromObjectForTest(BusinessObject businessObject) => ShouldIncludeNotificationsFromObject(businessObject);

	public bool ShouldIncludeNotificationsFromInfoForTest(ZPropertyInfo info) => ShouldIncludeNotificationsFromInfo(info);
}
