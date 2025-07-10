using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationRatingAdapterTest : TestCaseWithFactory
	{
		public void TestAutoRatingStatusInformationWaitingForResponse()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.CustomsEntryHeaders.AddNew().CH_Status = CustomsEntryStatus.AwaitingAmendment.Code;
			dec.JE_MessageStatus = CustomsEntryStatus.AwaitingAmendment.Code;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			dec.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("Should say waiting for response", true, dec.RatingAdapter.StatusInformation.Message.StartsWith("The Declaration is waiting for a response"));
		}

		public void TestAutoRatingStatusInformationPreLodgeAndWorkComplete()
		{
			var oneToOne = Factory.New<JobDeclaration>();
			oneToOne.JE_EntryStatus = CustomsEntryStatus.AwaitingCPDec.Code;
			oneToOne.JE_MessageType = JobMessageTypeList.Codes.Import;

			AssertEquals("With an incomplete Declaration", true, oneToOne.RatingAdapter.StatusInformation.CanExecute);
			AssertEquals("With an incomplete Declaration", true, oneToOne.RatingAdapter.StatusInformation.Message.StartsWith("The Declaration has not been lodged or received a lodgement response"));

			oneToOne.JE_EntryStatus = CustomsEntryStatus.DeclarationWorkComplete.Code;
			AssertEquals("Autorating can be executed as declaration complete", true, oneToOne.RatingAdapter.StatusInformation.CanExecute);
			AssertEquals("No error message present", ZString.Empty, oneToOne.RatingAdapter.StatusInformation.Message);
		}

		public void TestAutoRatingStatusWarningNotLodgedForExports()
		{
			var oneToOne = Factory.New<JobDeclaration>();
			oneToOne.JE_EntryStatus = CustomsEntryStatus.AwaitingCPDec.Code;
			oneToOne.JE_MessageType = JobMessageTypeList.Codes.Export;

			AssertEquals("With an incomplete Declaration", true, oneToOne.RatingAdapter.StatusInformation.CanExecute);
			AssertEquals("No warning as this is an export", ZString.Empty, oneToOne.RatingAdapter.StatusInformation.Message);

			oneToOne.JE_EntryStatus = CustomsEntryStatus.DeclarationWorkComplete.Code;
			AssertEquals("Autorating can be executed as declaration complete", true, oneToOne.RatingAdapter.StatusInformation.CanExecute);
			AssertEquals("No error message present", ZString.Empty, oneToOne.RatingAdapter.StatusInformation.Message);
		}
	}
}
