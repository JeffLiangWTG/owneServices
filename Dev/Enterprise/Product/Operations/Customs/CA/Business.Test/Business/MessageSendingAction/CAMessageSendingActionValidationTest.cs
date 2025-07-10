using CargoWise.EntityFramework.Testing;
using Moq;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class CAMessageSendingActionValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCA_SaveWithoutSendingReasonText()
		{
			CusEntryHeader entry = Declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
			var mock = new Mock<CAMessageSendingAction>(entry, MessageType.DataLoadingModule, Actions) { CallBase = true };
			CAMessageSendingAction action = mock.Object;
			action.CA_SaveWithoutSendingReasonText = "";
			action.CA_SaveWithoutSending = true;

			AssertEquals("CA_SaveWithoutSendingReasonTextInfo.ReadOnly", false, action.CA_SaveWithoutSendingReasonTextInfo.ReadOnly);
			AssertHasError(action.CA_SaveWithoutSendingReasonTextInfo, CAMessageSendingActionValidation.ReasonForNotSendingRequired);
			action.CA_SaveWithoutSending = false;
			AssertEquals("CA_SaveWithoutSendingReasonTextInfo.ReadOnly", true, action.CA_SaveWithoutSendingReasonTextInfo.ReadOnly);
			AssertNoError(action.CA_SaveWithoutSendingReasonTextInfo, CAMessageSendingActionValidation.ReasonForNotSendingRequired);

			action.CA_SaveWithoutSending = true;
			action.CA_SaveWithoutSendingReasonText = "Blahs";
			AssertNoError(action.CA_SaveWithoutSendingReasonTextInfo, CAMessageSendingActionValidation.ReasonForNotSendingRequired);
		}

		CAMessageSendingActionCollection Actions
		{
			get { return actions ?? (actions = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original)); }
		}
		CAMessageSendingActionCollection actions;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
