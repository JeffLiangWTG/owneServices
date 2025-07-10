using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1198
	{
		public void BadCode()
		{
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();

			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			_ = dummyBizo.GetLogs().AddNew(AutoEvents.AddedARecordToTheSystem);
			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			_ = dummyBizo.GetLogs().AddNew(AutoEvents.EditedARecord);
			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			_ = dummyBizo.GetLogs().AddNew(AutoEvents.DeletedARecordInTheSystem);

			var log = factory.New<StmALog>();

			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			log.SL_SE_NKEvent = AutoEvents.AddedARecordToTheSystem.Code;
			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			log.SL_SE_NKEvent = AutoEvents.EditedARecordCode;
			// CW1198 - DoNotAddAuditEventsToStmALogAnalyzer
			log.SL_SE_NKEvent = AutoEvents.DeletedARecordInTheSystemCode;
		}
	}
}
