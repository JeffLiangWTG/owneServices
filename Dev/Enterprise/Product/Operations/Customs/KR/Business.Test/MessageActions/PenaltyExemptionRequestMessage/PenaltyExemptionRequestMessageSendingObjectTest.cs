using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PenaltyExemptionRequestMessageSendingObject))]
	public class PenaltyExemptionRequestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			var sessionalData = new TestDataSetupHelper(Factory).SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.A);
			return new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData, 1);
		}

		public void TestPenaltyAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = KRJobMessageTypeList.Codes.Import;
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var testDataSetupHelper = new TestDataSetupHelper(Factory);

			var sessionalData1 = testDataSetupHelper.SetUpAmendmentSessionalData(entry, "1001", "1", DutyTaxCorrectionCodeList.Codes.A);

			var sendingObject = new PenaltyExemptionRequestMessageSendingObject(entry, sessionalData1, 1);
			AssertEquals(999m, sendingObject.PenaltyAmountFrom5FK);
		}

		public void TestGetNewValidation()
		{
			var sendingObject = GetNewBusinessObject() as PenaltyExemptionRequestMessageSendingObject;
			AssertType(typeof(PenaltyExemptionRequestMessageSendingObjectValidation), sendingObject.Validation);
		}
		public void TestCaptions()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(PenaltyExemptionRequestMessageSendingObject), nameof(PenaltyExemptionRequestMessageSendingObject.DutyPenaltyExemption5UASequenceNumber), false, attribute => attribute.Caption == "Version No.");
		}
	}
}
