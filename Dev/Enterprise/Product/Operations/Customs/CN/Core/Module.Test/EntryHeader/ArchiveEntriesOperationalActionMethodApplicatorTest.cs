using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Module.Testing
{
	[TestedType(typeof(ArchiveEntriesOperationalActionMethodApplicator))]
	class ArchiveEntriesOperationalActionMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestApply()
		{
			var testDeclaration = Factory.New<JobDeclaration>();
			var testEntry = testDeclaration.CustomsEntryHeaders.AddNew();
			AssertNull(testEntry.Logs.Find(x => x.SL_SE_NKEvent == Events.RecordArchivedCode).FirstOrDefault());
			AssertEquals(ZDateTime.Empty, testEntry.ArchiveDate);
			AssertEquals(ZString.Empty, testEntry.ArchiveUser);
			var targets = new BusinessObject[] { testEntry };
			SimulateRun(targets, false);
			var logACVs = testEntry.Logs.Find(x => x.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(1, logACVs.Count());
			var logACV = logACVs.FirstOrDefault();
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
			AssertEquals(Env.CurrentUser.FullName, testEntry.ArchiveUser);
			SimulateRun(targets, false);
			logACVs = testEntry.Logs.Find(x => x.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(1, logACVs.Count());
			logACV = logACVs.FirstOrDefault();
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
			Factory.Save();
			SimulateRun(targets, false);
			logACVs = testEntry.Logs.Find(x => x.SL_SE_NKEvent == Events.RecordArchivedCode);
			AssertEquals(2, logACVs.Count());
			logACV = logACVs.FirstOrDefault(x => !x.IsCancelled);
			AssertEquals(logACV.SL_EventTime, testEntry.ArchiveDate);
			AssertEquals(logACV.User.FullName, testEntry.ArchiveUser);
		}

		protected override BusinessObject GetNewBusinessObject() => new ArchiveEntriesOperationalActionMethodApplicator(Factory);
	}
}
