using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class EarlyReleaseMiscMessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		EarlyReleaseMiscMessageSendingObject sendingObject;
		protected override void SetUp()
		{
			base.SetUp();
			var entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
			entry.Messages.AddNew().EM_MessageType = ElectronicDocumentTypeList.Codes._5BD;

			sendingObject = new EarlyReleaseMiscMessageSendingObject(entry);
			sendingObject.AmendmentReason = "수리전반출신청";
			sendingObject.OtherSecurityType = "현금";
			sendingObject.SecurityType = SecurityTypeCodeList.Codes._99;
			sendingObject.SecurityStartDate = new ZDateTime("2021-04-06");
			sendingObject.SecurityEndDate = new ZDateTime("2021-05-05");
			sendingObject.SecurityAmount = 100000m;
			sendingObject.ReasonForEarlyRemoval = ReasonForEarlyRemovalCodeList.Codes._01;
			sendingObject.Validation.ValidateAll();
			Assert(!sendingObject.HasMessageErrors);
		}

		public void TestWithCheckRequestReason_Mandatory()
		{
			sendingObject.AmendmentReason = "";
			AssertHasMessageErrorContaining(sendingObject.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.AmendmentReason = "수리전반출신청";
			AssertNoMessageErrorContaining(sendingObject.AmendmentReasonInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithCheckSecurityType_Mandatory()
		{
			sendingObject.SecurityType = "";
			AssertHasMessageErrorContaining(sendingObject.SecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.SecurityType = SecurityTypeCodeList.Codes._99;
			AssertNoMessageErrorContaining(sendingObject.SecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithCheckSecurityStartDate_Mandatory()
		{
			sendingObject.SecurityStartDate = new ZDateTime();
			AssertHasMessageErrorContaining(sendingObject.SecurityStartDateInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.SecurityStartDate = new ZDateTime("2021-04-06");
			AssertNoMessageErrorContaining(sendingObject.SecurityStartDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithCheckSecurityEndDate_Mandatory()
		{
			sendingObject.SecurityEndDate = new ZDateTime();
			AssertHasMessageErrorContaining(sendingObject.SecurityEndDateInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.SecurityEndDate = new ZDateTime("2021-04-06");
			AssertNoMessageErrorContaining(sendingObject.SecurityEndDateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithCheckDateIsNotAfterAnotherDateCase()
		{
			sendingObject.SecurityEndDate = new ZDateTime("2021-05-05");
			sendingObject.SecurityStartDate = new ZDateTime("2021-05-06");
			AssertEquals("Expecting Message Error.", "The 'Security Start Date' must be before or the same as the 'Security End Date'.", sendingObject.SecurityStartDateInfo.GetErrors().GetFirstMessage());
		}

		public void TestWithCheckSecurityAmount_Negative()
		{
			sendingObject.SecurityAmount = 0;
			AssertHasMessageErrorContaining(sendingObject.SecurityAmountInfo, MandatoryValidation.ValueCannotBeZero);

			sendingObject.SecurityAmount = -1;
			AssertHasMessageErrorContaining(sendingObject.SecurityAmountInfo, MandatoryValidation.ValueCannotBeNegative);

			sendingObject.SecurityAmount = 100000m;
			AssertNoMessageErrorContaining(sendingObject.SecurityAmountInfo, MandatoryValidation.ValueCannotBeNegative);
		}

		public void TestWithCheckOtherSecurityType_Mandatory()
		{
			sendingObject.SecurityType = SecurityTypeCodeList.Codes._99;
			sendingObject.OtherSecurityType = "";
			AssertHasMessageErrorContaining(sendingObject.OtherSecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.SecurityType = SecurityTypeCodeList.Codes._99;
			sendingObject.OtherSecurityType = "현금";
			AssertNoMessageErrorContaining(sendingObject.OtherSecurityTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestWithCheckOtherSecurityType_NotMandatory()
		{
			sendingObject.SecurityType = SecurityTypeCodeList.Codes._01;
			sendingObject.OtherSecurityType = "";
			AssertEquals(false, sendingObject.HasMessageErrors);
		}

		public void TestWithCheckReasonForEarlyRemoval_Mandatory()
		{
			sendingObject.ReasonForEarlyRemoval = "";
			AssertHasMessageErrorContaining(sendingObject.ReasonForEarlyRemovalInfo, MandatoryValidation.YouHaveNotEntered);

			sendingObject.ReasonForEarlyRemoval = ReasonForEarlyRemovalCodeList.Codes._01;
			AssertNoMessageErrorContaining(sendingObject.ReasonForEarlyRemovalInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
