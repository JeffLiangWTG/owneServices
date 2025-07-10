using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	class UpdateImportEntryStatusObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRiskChannel()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ActiveEntryHeaders.AddNew();

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			ValidationTestHelper.AssertErrorIfInvalidCode(updateImportEntryStatus.RiskChannelInfo, "X", RiskChannelList.Codes.Yellow);
		}

		public void TestCheckEntryStatus()
		{
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ActiveEntryHeaders.AddNew();

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			ValidationTestHelper.AssertErrorIfInvalidCode(updateImportEntryStatus.EntryStatusInfo, "X", "S02");
		}

		public void TestCheckEventDate()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			declaration.ActiveEntryHeaders.AddNew().CH_EntryStatus = "S01";

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			updateImportEntryStatus.EventDate = ZDateTime.Empty;
			updateImportEntryStatus.EntryStatus = "S01";
			AssertNoErrorContaining(updateImportEntryStatus.EventDateInfo, "Please enter");
			updateImportEntryStatus.EntryStatus = "S02";
			AssertHasErrorContaining(updateImportEntryStatus.EventDateInfo, "Please enter");
			updateImportEntryStatus.EventDate = ZDateTime.Now;
			AssertNoErrorContaining(updateImportEntryStatus.EventDateInfo, "Please enter");
		}
	}
}
