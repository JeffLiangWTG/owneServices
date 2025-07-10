using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class ImportLicenseMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldSendForImportLicense()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var messageSending = new ImportLicenseMessageSendingObject(entryHeader);
			messageSending.Validation.ValidateShouldSend();
			AssertHasErrorContaining(messageSending.ShouldSendInfo, "Message should only be sent if the Entry Header contains at least one Entry Line");
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "Original Message has already been sent. License already contains a License number");

			entryHeader.MergedLines.AddNew();
			entryHeader.CH_Status = BRMessageStatusList.Codes.Accepted;
			messageSending.Validation.ValidateShouldSend();
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "Message should only be sent if the Entry Header contains at least one Entry Line");
			AssertHasErrorContaining(messageSending.ShouldSendInfo, "Original Message has already been sent. License already contains a License number");

			entryHeader.CH_Status = BRMessageStatusList.Codes.AwaitingResponse;
			messageSending.Validation.ValidateShouldSend();
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "Message should only be sent if the Entry Header contains at least one Entry Line");
			AssertNoErrorContaining(messageSending.ShouldSendInfo, "Original Message has already been sent. License already contains a License number");
		}
	}
}
