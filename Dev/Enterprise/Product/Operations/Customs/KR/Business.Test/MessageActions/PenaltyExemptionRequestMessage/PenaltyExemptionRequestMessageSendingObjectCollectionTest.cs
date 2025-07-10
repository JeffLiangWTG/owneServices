using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyExemptionRequestMessageSendingObjectCollection))]
	sealed class PenaltyExemptionRequestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PenaltyExemptionRequestMessageSendingObjectCollection>
	{
		protected override PenaltyExemptionRequestMessageSendingObjectCollection GetCollectionToTest()
		{
			return new PenaltyExemptionRequestMessageSendingObjectCollection(Declaration);
		}

		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var entry = Declaration.CustomsEntryHeaders.AddNew();

			var message = entry.Messages.AddNew();
			message.EM_MessageType = ElectronicDocumentTypeList.Codes._5FE;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			message.EM_LinkUniqueID = entry.PK;
			message.EM_LinkedObject = entry;
			message.EM_MessageSubType = "AX";
			message.EM_ApplicationReference = "1";

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var sessionalData = instruction.AmendmentSessionalDataCollection.AddNew();
			sessionalData.CSI_Code = DutyTaxCorrectionCodeList.Codes.A;
			sessionalData.CSI_LineNo = ZInt.ParseEmptyAsZero(message.EM_ApplicationReference);

			return new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
		}

		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		public void TestFilter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var testDataSetupHelper = new TestDataSetupHelper(Factory);

			testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.O);

			var sessionalDataNotIncluded5FE = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1002", "2", DutyTaxCorrectionCodeList.Codes.A);
			sessionalDataNotIncluded5FE.PenaltyExemptionSessionalData.PenaltyExemptionCode = YesNoList.Codes.No;

			var sessionalDataIncluded5FE = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1003", "3", DutyTaxCorrectionCodeList.Codes.B);
			sessionalDataIncluded5FE.PenaltyExemptionSessionalData.PenaltyExemptionCode = YesNoList.Codes.Yes;

			var collection = new PenaltyExemptionRequestMessageSendingObjectCollection(declaration);
			AssertEquals(1, collection.Count);
			AssertEquals(sessionalDataNotIncluded5FE, collection[0].SessionalData5FE);
			collection[0].ShouldSend = true;

			new GOVCBR5UASender(collection.Cast<PenaltyExemptionRequestMessageSendingObject>(), Factory).Send();

			var message5UA = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5UA);
			message5UA.EM_MessageNum = "1004";

			var messageR20 = entry.Messages.AddNew();
			messageR20.EM_MessageType = ElectronicDocumentTypeList.Codes._R20;
			messageR20.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageR20.EM_ApplicationReference = message5UA.EM_MessageNum;

			collection = new PenaltyExemptionRequestMessageSendingObjectCollection(declaration);
			AssertEquals(1, collection.Count);

			new GOVCBR5UASender(collection.Cast<PenaltyExemptionRequestMessageSendingObject>(), Factory).Send();

			message5UA = entry.Messages.GetLastMessage(EDIMessage.ApplicationCodes.KRCustoms, ElectronicDocumentTypeList.Codes._5UA);
			message5UA.EM_MessageNum = "1005";

			var messageR99 = entry.Messages.AddNew();
			messageR99.EM_MessageType = ElectronicDocumentTypeList.Codes._R99;
			messageR99.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			messageR99.EM_ApplicationReference = message5UA.EM_MessageNum;

			collection = new PenaltyExemptionRequestMessageSendingObjectCollection(declaration);
			AssertEquals(0, collection.Count);
		}
	}
}

