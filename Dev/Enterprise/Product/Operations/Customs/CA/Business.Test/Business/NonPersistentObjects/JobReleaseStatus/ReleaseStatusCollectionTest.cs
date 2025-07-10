using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ReleaseStatusCollection))]
	sealed class ReleaseStatusCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReleaseStatusCollection>
	{
		public void TestSuspendCountChangeInConstructure()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn1 = declaration.AdditionalReferenceNumbers.AddNew();
			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn1.CE_EntryNum = "CCN1";
			var ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN2";

			var refreshBindingCalled = 0;
			declaration.EffectiveCCNInfo.ValueChanged += (s, e) => refreshBindingCalled++;
			var releaseStatus = declaration.ReleaseStatuses;
			AssertEquals("RefreshBinding Not called", 0, refreshBindingCalled);
			declaration.ReleaseStatuses.AddNew();
			AssertEquals("RefreshBinding called", 1, refreshBindingCalled);

			declaration.ReleaseStatuses.RemoveAll();
			AssertEquals("RefreshBinding called", 5, refreshBindingCalled);
		}

		public void TestLoadAndReleaseCCNsIfWTA()
		{
			var ccn3 = declaration.CargoControlNumbers.AddNew();
			ccn3.CY_CargoControlNumber = "CCN3";

			var releaseColl = new ReleaseStatusCollection(declaration);
			var rs1 = releaseColl.Where(x => x.RL_CargoControlNumber == "CCN3").FirstOrDefault();
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased, rs1.RL_ReleaseStatus);
		}

		public void TestLoadAndReloadOnSave()
		{
			AssertEquals("ReleaseStatuses.Count", 2, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN1", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 1, PersistentCCN", ccn1, declaration.ReleaseStatuses[0].PersistentCCN);
			AssertEquals("ReleaseStatuses 1, RL_ProcessingDate", day3, declaration.ReleaseStatuses[0].RL_ReleaseDate);

			AssertEquals("ReleaseStatuses 2", "CCN2", declaration.ReleaseStatuses[1].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 2, PersistentCCN", ccn2, declaration.ReleaseStatuses[1].PersistentCCN);
			AssertEquals("ReleaseStatuses 2, RL_ProcessingDate", day1, declaration.ReleaseStatuses[1].RL_ReleaseDate);

			declaration.ReleaseStatuses.RemoveAndDelete(declaration.ReleaseStatuses[1]);
			var day4 = new ZDateTime(2011, 07, 02, 01, 02, 00);
			var day5 = new ZDateTime(2011, 07, 03, 01, 02, 00);
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage(string.Empty, day4, "7", day4));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", day5, "8", day5));
			AssertEquals("ReleaseStatuses.Count", 1, declaration.ReleaseStatuses.Count);

			Factory.Save();

			AssertEquals("ReleaseStatuses.Count", 1, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN1", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 1, PersistentCCN", ccn1, declaration.ReleaseStatuses[0].PersistentCCN);
			AssertEquals("ReleaseStatuses 1, RL_ProcessingDate", day4, declaration.ReleaseStatuses[0].RL_ReleaseDate);

			var releaseStatus = declaration.ReleaseStatuses[0];
			declaration.ReleaseStatuses.RemoveAndDelete(declaration.ReleaseStatuses[0]);
			AssertEquals("ReleaseStatuses.Count", 0, declaration.ReleaseStatuses.Count);
			AssertEquals("Should not be reloaded if persistent collections are not modified", true, releaseStatus.IsDeleted);
		}

		public void TestReleaseStatusAndCCNCusEntryNumberSynchronization()
		{
			declaration.ReleaseStatuses[0].RL_CargoControlNumber = "CCN4";
			AssertEquals("If CCN of release status changed then CusEntryNumber should be synchronised", "CCN4", ccn1.CE_EntryNum);
			ccn1.CE_EntryNum = "CCN1";
			AssertEquals("If CCN CusEntryNumber changed then release status should be synchronised", "CCN1", declaration.ReleaseStatuses[0].RL_CargoControlNumber);

			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.PCN;

			AssertEquals("If CusEntryNumber type changed from CCN to something else then release status should be removed", 1, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN2", declaration.ReleaseStatuses[0].RL_CargoControlNumber);

			var ccn3 = Factory.New<CusEntryNumber>();
			ccn3.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn3.CE_EntryNum = "CCN3";
			declaration.AdditionalReferenceNumbers.Add(ccn3);

			AssertEquals("If CCN CusEntryNumber added then release status should be added", 2, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN2", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 3", "CCN3", declaration.ReleaseStatuses[1].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 2: Matched message has ben found", day3, declaration.ReleaseStatuses[1].RL_ReleaseDate);

			declaration.ReleaseStatuses.RemoveAndDelete(declaration.ReleaseStatuses[1]);
			AssertEquals("If release status removed then CCN CusEntryNumber should be removed too", true, ccn3.IsDeleted);
			AssertEquals("ReleaseStatuses.Count", 1, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN2", declaration.ReleaseStatuses[0].RL_CargoControlNumber);

			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			AssertEquals("If CusEntryNumber type changed to CCN then release status should be added", 2, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN2", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 2", "CCN1", declaration.ReleaseStatuses[1].RL_CargoControlNumber);
			AssertEquals("ReleaseStatuses 2: Matched message has ben found", day3, declaration.ReleaseStatuses[1].RL_ReleaseDate);

			ccn1.Delete();
			AssertEquals("If CCN CusEntryNumber removed then release status should be removed too, the CCN1 message should be added", 1, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1", "CCN2", declaration.ReleaseStatuses[0].RL_CargoControlNumber);
		}

		public void TestReleaseStatusAsCargoControlNumberWrapper()
		{
			AssertEquals("CargoControlNumbers.Count", 2, declaration.CargoControlNumbers.Count);
			AssertEquals("CY_CargoControlNumber 1", "CCN1", declaration.CargoControlNumbers[0].CY_CargoControlNumber);
			AssertEquals("CY_CargoControlNumber 2", "CCN2", declaration.CargoControlNumbers[1].CY_CargoControlNumber);

			declaration.ReleaseStatuses[1].RL_CargoControlNumber = "CCN4";
			declaration.ReleaseStatuses.AddNew().RL_CargoControlNumber = "CCN5";
			AssertEquals("CargoControlNumbers.Count", 3, declaration.CargoControlNumbers.Count);
			AssertEquals("CY_CargoControlNumber 1", "CCN1", declaration.CargoControlNumbers[0].CY_CargoControlNumber);
			AssertEquals("CY_CargoControlNumber 2", "CCN4", declaration.CargoControlNumbers[1].CY_CargoControlNumber);
			AssertEquals("CY_CargoControlNumber 3", "CCN5", declaration.CargoControlNumbers[2].CY_CargoControlNumber);

			declaration.ReleaseStatuses.RemoveAndDelete(declaration.ReleaseStatuses[1]);
			AssertEquals("CargoControlNumbers.Count", 2, declaration.CargoControlNumbers.Count);
			AssertEquals("CY_CargoControlNumber 1", "CCN1", declaration.CargoControlNumbers[0].CY_CargoControlNumber);
			AssertEquals("CY_CargoControlNumber 2", "CCN5", declaration.CargoControlNumbers[1].CY_CargoControlNumber);
		}

		public void TestReleaseMessageWithoutCCNShouldNotBecomeReleaseStatusIfNoCCNsOnDeclaration()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage(string.Empty, day1, "1", day1));
			AssertEquals("Common release response (no CCN) should not be added to CCNs grid if no CCNs on declaration", 0, declaration.ReleaseStatuses.Count);

			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN1";
			declaration.ReleaseStatuses.Load();
			AssertEquals("Release Status should be added when CCN entered", 1, declaration.ReleaseStatuses.Count);
			AssertEquals("ReleaseStatuses 1, RL_ProcessingDate", day1, declaration.ReleaseStatuses[0].RL_ReleaseDate);
		}

		public void TestReleaseDateWhenTwoOrMoreMessageCreateTimeVeryClose()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CargoControlNumbers.AddNew().CY_CargoControlNumber = "CCN1";
			var processingDate = new ZDateTime(2024, 11, 11, 01, 11, 00);
			var createTime1 = new ZDateTime(2024, 11, 22, 02, 11, 13);
			var createTime2 = new ZDateTime(2024, 11, 22, 02, 11, 24);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", processingDate, "1", createTime1));
			entry.Messages.Add(helper.GetIIDResponseMessage(processingDate, "1", createTime2));

			var releaseCollection = new ReleaseStatusCollection(declaration);
			var release = releaseCollection.Where(x => x.RL_CargoControlNumber == "CCN1").FirstOrDefault();
			AssertEquals(EDIReleaseImportEntryStatusList.Codes.GoodsReleased, release.RL_ReleaseStatus);
			AssertEquals(processingDate, release.RL_ReleaseDate);
		}

		public void TestHasChangesNotSetAfterFactoryTransactionCommitted()
		{
			AssertEquals("ReleaseStatuses.Count", 2, declaration.ReleaseStatuses.Count);
			declaration.ReleaseStatuses.RemoveAndDelete(declaration.ReleaseStatuses[1]);
			var day4 = new ZDateTime(2011, 07, 02, 01, 02, 00);
			var day5 = new ZDateTime(2011, 07, 03, 01, 02, 00);
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage(string.Empty, day4, "7", day4));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", day5, "8", day5));
			AssertEquals("ReleaseStatuses.Count", 1, declaration.ReleaseStatuses.Count);

			declaration.HasChangesChanged += declaration_HasChangesChanged;
			Factory.Save();

			AssertEquals("ReleaseStatuses.Count", 1, declaration.ReleaseStatuses.Count);
			Assert("HasChanges should not be set after factory transaction is committed", !declaration.HasChanges);
		}

		public void TestCE_EntryTypeInfo_ValueChanged()
		{
			declaration.ReleaseStatuses.Load();
			var ccn1 = declaration.AdditionalReferenceNumbers[0];
			AssertNoExceptionThrown(() =>
			{
				ccn1.CE_EntryTypeInfo.RefreshBinding((ZString)CanadaAdditionalReferenceNumberTypes.Codes.CTN);
			});
			AssertNoExceptionThrown(() =>
			{
				ccn1.CE_EntryTypeInfo.RefreshBinding();
			});
		}

		void declaration_HasChangesChanged(object sender, HasChangesChangedEventArgs e)
		{
			if (((IBusinessObjectFactoryInternals)((BusinessObject)sender).Factory).IsProcessingOnAllTransactionsCommitted && e.ObjectJustWasChanged)
			{
				throw new System.Exception("HasChanges should not be set after factory transaction is committed");
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			helper = new DeclarationTestHelper(Factory, true);
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ccn1 = declaration.AdditionalReferenceNumbers.AddNew();
			ccn1.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			ccn1.CE_EntryNum = "CCN1";
			ccn2 = declaration.CargoControlNumbers.AddNew();
			ccn2.CY_CargoControlNumber = "CCN2";
			var day0 = new ZDateTime(2011, 06, 28, 01, 02, 00);
			day1 = new ZDateTime(2011, 06, 29, 01, 02, 00);
			var day2 = new ZDateTime(2011, 06, 30, 01, 02, 00);
			day3 = new ZDateTime(2011, 07, 01, 01, 02, 00);

			entry = declaration.CustomsEntryHeaders.AddNew();
			entry.CH_MessageType = MessageTypeList.Codes.EDIRelease;
			entry.Messages.Add(helper.GetStatusQueryMessage("CCN1"));
			entry.Messages.Add(helper.GetStatusQueryMessage("CCN2"));
			entry.Messages.Add(helper.GetStatusQueryMessage("CCN3"));

			entry.Messages.Add(helper.GetEDIReleaseResponseMessage(string.Empty, day0, "1", day0));

			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", day1, "2", day1));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN2", day1, "3", day1));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", day1, "4", day1));

			entry.Messages.Add(helper.GetStatusQueryMessage("CCN1", day2));
			entry.Messages.Add(helper.GetStatusQueryMessage("CCN3", day2));

			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN1", day3, "5", day3));
			entry.Messages.Add(helper.GetEDIReleaseResponseMessage("CCN3", day3, "6", day3));
		}

		protected override ReleaseStatusCollection GetCollectionToTest()
		{
			return new ReleaseStatusCollection(Factory.New<JobDeclaration>());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReleaseStatus(Factory.New<CargoControlNumber>());
		}

		JobDeclaration declaration;
		CusEntryNumber ccn1;
		CargoControlNumber ccn2;
		ZDateTime day1;
		ZDateTime day3;
		CusEntryHeader entry;
		DeclarationTestHelper helper;

		#endregion
	}
}
