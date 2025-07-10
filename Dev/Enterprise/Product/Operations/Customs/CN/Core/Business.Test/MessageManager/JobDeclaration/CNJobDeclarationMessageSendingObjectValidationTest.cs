using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.CN.Business.Testing
{
	class CNJobDeclarationMessageSendingObjectValidationTest : TestCaseWithFactory
	{
		public void TestCheckShouldSend()
		{
			const string message = "This entry has no lines. Please check if there are any Invoice Lines linked to the corresponding Entry Instruction.";
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry = declaration.ActiveEntryHeaders.AddNew();
				var sendingObject = new CNJobDeclarationMessageSendingObject(entry) { ShouldSend = true };
				AssertHasError("No lines", sendingObject.ShouldSendInfo, message);

				sendingObject.Header.MergedLines.AddNew();
				sendingObject.ValidateShouldSend();
				AssertNoError("Has line", sendingObject.ShouldSendInfo, message);
			});
		}

		public void TestCheckEntryStatus()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CSTA", "Customs Status");
			Factory.Save();

			var code09 = helper.CreateNewOrGetExistingCusCodeList("CN", "CSTA", "09", "09", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			code09.ZZD_Description = "已放行";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_EntryStatus = "09";
			var testItem = new CNJobDeclarationMessageSendingObject(entry);

			CombineAssertions(() =>
			{
				AssertEquals("Prerequisite", "已放行", testItem.EntryStatus);
				AssertEquals("No Western European notification", true, !testItem.EntryStatusInfo.HasNotification(EnglishCharactersValidation.GetNotificationMessage(testItem.EntryStatusInfo)));
			});
		}
	}
}
