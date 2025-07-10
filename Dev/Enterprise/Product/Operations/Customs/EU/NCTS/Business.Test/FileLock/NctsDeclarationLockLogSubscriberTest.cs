using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsDeclarationLockLogSubscriber))]
	sealed class NctsDeclarationLockLogSubscriberTest : LogSubscriberTest<NctsDeclarationLockLogSubscriber>
	{
		public void TestProcess()
		{
			var now = ZDateTime.Now;

			var header1 = Factory.NewWithValidTestData<NctsHeader>();
			header1.BH_GB = GlbBranch.CurrentBranch.PK;
			header1.SetMovementType(NctsMovementType.Codes.Departure);

			var header2 = Factory.NewWithValidTestData<NctsHeader>();
			header2.BH_GB = GlbBranch.CurrentBranch.PK;

			var header3 = Factory.NewWithValidTestData<NctsHeader>();
			header3.BH_GB = GlbBranch.CurrentBranch.PK;
			header3.SetMovementType(NctsMovementType.Codes.Departure);

			var header4 = (CusInBondHeader)Factory.New<Integration.Customs.US.USAMS.ICusInBondHeader>();

			Factory.Save();

			var configs = new DeclarationLockConfigCollection(null, Factory);
			var config = configs.AddNew();
			config.DeclarationType = EUJobMessageTypeList.Codes.NctsDeparture;
			config.LockMode = Core.Constants.Customs.DeclarationLockModes.Codes.All;

			var eventInfo = config.EventInfos.AddNew();
			eventInfo.EventType = AutoEvents.DepartureCode;
			eventInfo.EventSource = Core.Constants.Customs.EventLockSourceTypes.Codes.NctsHeader;
			eventInfo.EventReference = DeclarationEventLockInfo.DefaultMatchCharacter;
			eventInfo.EntryType = Core.Constants.Customs.EntryHeaderTypes.Codes.All;

			var tabInfo = config.TabInfos.AddNew();
			tabInfo.TabPage = Core.Constants.Customs.DeclarationTabPages.Codes.NctsDepartureHeader;

			var newFactory = NewFactory();

			var logs = new IQueuedLog[]
			{
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix, SJ_ParentID = header1.PK, SJ_SE_NKEvent = AutoEvents.DepartureCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix, SJ_ParentID = header2.PK, SJ_SE_NKEvent = AutoEvents.ArrivalCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix, SJ_ParentID = header3.MovementHeader.PK, SJ_SE_NKEvent = AutoEvents.DepartureCode, SJ_EventTime = now },
				new QueuedLogForTesting(newFactory) { SJ_ParentTableCode = CusInBondMoveHeaderSchema.Constants.Prefix, SJ_ParentID = header4.MovementHeader.PK, SJ_SE_NKEvent = AutoEvents.DepartureCode, SJ_EventTime = now },
			};

			header1 = newFactory.Load<NctsHeader>(header1.PK);
			header2 = newFactory.Load<NctsHeader>(header2.PK);
			header3 = newFactory.Load<NctsHeader>(header3.PK);
			header4 = newFactory.Load<CusInBondHeader>(header4.PK);

			using (CustomsDataRegistry.Instance.DeclarationLockForEdit.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configs))
			{
				new NctsDeclarationLockLogSubscriberForTest().ProcessLogs(logs);

				CombineAssertions(() =>
				{
					var lockLog = header1.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the NCTS header 1 is matched to the config.", lockLog);
					AssertEquals("Should add it from NctsDeclarationLockLogSubscriber for NCTS header 1.", "NCTS Declaration Lock Log Subscriber", lockLog.SL_Reference);

					lockLog = header2.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the NCTS header 2 is not matched to the config.", lockLog);

					lockLog = header3.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNotNull("Should not be null as the NCTS header 3 is matched to the config and has queued log for MovementHeader.", lockLog);
					AssertEquals("Should add it from NctsDeclarationLockLogSubscriber for NCTS header 3.", "NCTS Declaration Lock Log Subscriber", lockLog.SL_Reference);

					lockLog = header4.Logs.MostRecentLogByEventTime(AutoEvents.LockForEdit);
					AssertNull("Should be null as the AMS header 4 cannot be loaded as NCTS header.", lockLog);
				});
			}
		}

		public override void TestStmJobQueueIsNotSubscribedForEvents() => Assert(true);

		[Serializable]
		sealed class NctsDeclarationLockLogSubscriberForTest : NctsDeclarationLockLogSubscriber
		{
			public void ProcessLogs(IQueuedLog[] queuedLogs) => ProcessLogQueueItems(queuedLogs);
		}
	}
}
