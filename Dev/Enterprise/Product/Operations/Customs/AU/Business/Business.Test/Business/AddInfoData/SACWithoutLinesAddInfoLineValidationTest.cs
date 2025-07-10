namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SACWithoutLinesAddInfoLineValidationTest : AUAddInfoValidationTest
	{
		public void TestZA_ORGValidationIsByPassed()
		{
			JobDeclaration declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			JobComInvoiceLine line = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			line.AddInfo.ZA_ORG = "43G4"; //Wrong Origin
			AssertNoNotifications(addInfo.ZA_ORGInfo);
			line.AddInfo.ZA_ORG = "USA"; //Correct Origin
			AssertNoNotifications(addInfo.ZA_ORGInfo);
		}
	}
}
