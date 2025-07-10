using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using EDIReleaseImportEntryStatusList = Enterprise.Customs.CA.Business.EDIReleaseImportEntryStatusList;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class CSAManualReleaseStrategyTest : TestCaseWithFactory
	{
		[TestDate(2022, 02, 11, 08, 09, 01)]
		public void TestRunPreSaveAction()
		{
			CombineAssertions("Null Source", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var tester = new CSAManualReleaseStrategy(null);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));
				AssertEquals(ContinueWithSave.No, tester.ShowPreSaveDialogs(ContinueWithSave.No));
			});

			CombineAssertions("Test CSA Manual Release", () =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var dec = Factory.New<JobDeclaration>();
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.CA_ServiceOption = ACROSSServiceOptions.Codes.CSA;
				dec.JE_EntryAuthorisationDate = new ZDateTime(2022, 02, 11, 08, 08, 08);

				var header = dec.CustomsEntryHeaders.AddNew();
				header.CH_MessageType = MessageTypeList.Codes.EDIRelease;

				var tester = new CSAManualReleaseStrategy(dec);
				AssertEquals(ContinueWithSave.Yes, tester.ShowPreSaveDialogs(ContinueWithSave.Yes));

				AssertNotNull(dec.ManualReleaseNote);
				AssertEquals(@"2022-02-11 08:08
Authorized to deliver - CSA Shipment
CWSupport
2022-02-11 08:09", dec.ManualReleaseText);
				AssertEquals(EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver, dec.JE_EntryStatus);

				AssertEquals(EDIReleaseImportEntryStatusList.Codes.AuthorisedToDeliver, dec.ReleaseEntryHeader.CH_EntryStatus);
				AssertEquals(new ZDateTime(2022, 02, 11, 08, 08, 08), dec.ReleaseEntryHeader.CH_EntryReleaseDate);
			});
		}
	}
}
