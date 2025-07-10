using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(UpdateImportEntryStatusObject))]
	class UpdateImportEntryStatusObjectTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2023, 1, 1)]
		public void TestProperties()
		{
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);
			var date = ZDateTime.Now.AddDays(1);

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Red;
			entryHeader.CH_EntryStatus = "S01";

			var updateImportEntryStatus = new UpdateImportEntryStatusObject(declaration);
			AssertEquals("EventDate should be equal", ZDateTime.Now.ToShortDateString(), updateImportEntryStatus.EventDate.ToShortDateString());
			AssertEquals("EntryStatus.MaxLength", 3, updateImportEntryStatus.EntryStatusInfo.MaxLength);
			AssertEquals("RiskChannel.MaxLength", 1, updateImportEntryStatus.RiskChannelInfo.MaxLength);
			AssertEquals("EntryStatus should be equal", "S01", updateImportEntryStatus.EntryStatus);
			AssertEquals("RiskChannel should be equal", RiskChannelList.Codes.Red, updateImportEntryStatus.RiskChannel);

			updateImportEntryStatus.EntryStatus = "S02";
			updateImportEntryStatus.EventDate = date;
			updateImportEntryStatus.RiskChannel = RiskChannelList.Codes.Green;
			AssertEquals("EntryStatus should be equal", "S02", updateImportEntryStatus.EntryStatus);
			AssertEquals("EventDate should be equal", date, updateImportEntryStatus.EventDate);
			AssertEquals("RiskChannel should be equal", RiskChannelList.Codes.Green, updateImportEntryStatus.RiskChannel);
		}

		[TestDate(2023, 1, 1)]
		public void TestUpdateEntryNumber()
		{
			ReferenceTestDataHelper.CreateEntryStatusForISWList(Factory);
			var date = ZDateTime.Now;

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;

			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			entryHeader.CH_EntryStatus = "S02";
			entryHeader.CH_EntryReleaseDate = date.AddDays(-1);
			entryHeader.CH_RiskChannel = RiskChannelList.Codes.Red;
			AssertEquals("CH_EntryStatus should be equal", "S02", entryHeader.CH_EntryStatus);
			AssertEquals("CH_EntryReleaseDate should be equal", date.AddDays(-1), entryHeader.CH_EntryReleaseDate);
			AssertEquals("CH_RiskChannel should be equal", RiskChannelList.Codes.Red, entryHeader.CH_RiskChannel);

			var updateImportEntryNumber = new UpdateImportEntryStatusObject(declaration);
			updateImportEntryNumber.EntryStatus = "S05";
			updateImportEntryNumber.EventDate = date;
			updateImportEntryNumber.RiskChannel = RiskChannelList.Codes.Green;
			updateImportEntryNumber.UpdateEntryStatus();
			AssertEntryHeader("S05", date.AddDays(-1), RiskChannelList.Codes.Green);
			var log = entryHeader.Logs.MostRecentLog;
			AssertLogEvent(log, "S05", date);

			updateImportEntryNumber.EntryStatus = ZString.Empty;
			updateImportEntryNumber.EventDate = date.AddDays(2);
			updateImportEntryNumber.RiskChannel = ZString.Empty;
			updateImportEntryNumber.UpdateEntryStatus();
			AssertEntryHeader(ZString.Empty, date.AddDays(-1), ZString.Empty);
			AssertLogEvent(log, "S05", date);

			updateImportEntryNumber.EntryStatus = "S02";
			updateImportEntryNumber.EventDate = date.AddDays(1);
			updateImportEntryNumber.RiskChannel = ZString.Empty;
			updateImportEntryNumber.UpdateEntryStatus();
			log = entryHeader.Logs.MostRecentLog;
			AssertEntryHeader("S02", date.AddDays(1), ZString.Empty);
			AssertLogEvent(log, ZString.Empty, date.AddDays(2));

			updateImportEntryNumber.EntryStatus = ZString.Empty;
			updateImportEntryNumber.EventDate = date.AddDays(2);
			updateImportEntryNumber.RiskChannel = RiskChannelList.Codes.Gray;
			updateImportEntryNumber.UpdateEntryStatus();
			AssertEntryHeader(ZString.Empty, ZDateTime.Empty, RiskChannelList.Codes.Gray);
			AssertLogEvent(log, ZString.Empty, date.AddDays(2));

			void AssertEntryHeader(ZString expectedEntryStatus, ZDateTime expectedEntryReleaseStatus, ZString expectedRiskChannel)
			{
				CombineAssertions(() =>
				{
					AssertEquals("CH_EntryStatus should be equal", expectedEntryStatus, entryHeader.CH_EntryStatus);
					AssertEquals("CH_EntryReleaseDate should be equal", expectedEntryReleaseStatus, entryHeader.CH_EntryReleaseDate);
					AssertEquals("CH_RiskChannel should be equal", expectedRiskChannel, entryHeader.CH_RiskChannel);
				});
			}

			void AssertLogEvent(StmALog aLog, ZString expectedEntryStatus, ZDateTime expectedEventDate)
			{
				CombineAssertions(() =>
				{
					AssertEquals("A CES Log should be added", Events.CustomsEntryStatus.Code, aLog.SL_SE_NKEvent);
					AssertEquals("CES Log Reference should be ", expectedEntryStatus, aLog.SL_Reference);
					AssertEquals("CES Log Event Time should be ", expectedEventDate.ToShortDateString(), aLog.SL_EventTime.ToShortDateString());
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new UpdateImportEntryStatusObject(declaration);
		}
	}
}
