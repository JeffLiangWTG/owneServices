using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyExemptionRequestMessageSendingObjectParent))]
	sealed class PenaltyExemptionRequestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new PenaltyExemptionRequestMessageSendingObjectParent(declaration);
		}

		public void TestSendingObjectsCollectionInRelationTo5FK()
		{
			var declaration1 = Factory.New<JobDeclaration>();
			var objectParent1 = new PenaltyExemptionRequestMessageSendingObjectParent(declaration1);
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			declaration1.CustomsEntryHeaders.AddNew();
			declaration1.CustomsEntryHeaders.AddNew();
			AssertEquals(0, objectParent1.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent1.ObjectsToSend.Count());

			var testDataSetupHelper = new TestDataSetupHelper(Factory);
			var declaration2 = Factory.New<JobDeclaration>();

			var entry = declaration2.ActiveEntryHeaders.AddNew();
			entry.AllEntryLines.AddNew();
			entry.EntryNumber = "1234520000045M";

			var instruction = declaration2.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.O);
			testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1002", "2", DutyTaxCorrectionCodeList.Codes.A);
			testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1003", "3", DutyTaxCorrectionCodeList.Codes.B);

			var objectParent2 = new PenaltyExemptionRequestMessageSendingObjectParent(declaration2);
			AssertEquals(2, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());

			AssertEquals("1002", objectParent2.SendingObjectsCollection[0].Message5FE.EM_MessageNum);
			AssertEquals("1003", objectParent2.SendingObjectsCollection[1].Message5FE.EM_MessageNum);

			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());

			var entry2 = declaration2.ActiveEntryHeaders.AddNew();
			entry2.AllEntryLines.AddNew();
			entry2.EntryNumber = "1234520000046M";
			var instruction2 = declaration2.CustomsEntryInstructions.AddNew();
			entry2.CH_CEI_Instruction = instruction2.PK;

			testDataSetupHelper.SetUpAmendmentSessionalData(entry2, "1004", "4", DutyTaxCorrectionCodeList.Codes.A);

			objectParent2 = new PenaltyExemptionRequestMessageSendingObjectParent(declaration2);
			AssertEquals(3, objectParent2.SendingObjectsCollection.Count);
			AssertEquals(0, objectParent2.ObjectsToSend.Count());

			AssertEquals("1002", objectParent2.SendingObjectsCollection[0].Message5FE.EM_MessageNum);
			AssertEquals("1003", objectParent2.SendingObjectsCollection[1].Message5FE.EM_MessageNum);
			AssertEquals("1004", objectParent2.SendingObjectsCollection[2].Message5FE.EM_MessageNum);

			objectParent2.SendingObjectsCollection[0].ShouldSend = true;
			AssertEquals(1, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[1].ShouldSend = true;
			AssertEquals(2, objectParent2.ObjectsToSend.Count());

			objectParent2.SendingObjectsCollection[1].ShouldSend = false;
			var sentMessage = ((IJobDeclarationMessageSendingObjectParent)objectParent2).GetMessageSender(MessageFunctions.MessageFunctionCode.Amendment).Send();
			AssertEquals(1, sentMessage);
		}

		public void TestGetNewParent()
		{
			var declaration = Factory.New<JobDeclaration>();

			var result = JobDeclarationMessageSendingObjectParent.GetJobDeclarationMessageSendingObjectParent(declaration, ElectronicDocumentTypeList.Codes._5UA, MessageFunctions.MessageFunctionCode.Original);
			AssertEquals(typeof(PenaltyExemptionRequestMessageSendingObjectParent), result.GetType());
		}
	}
}
