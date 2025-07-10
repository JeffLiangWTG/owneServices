using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class StatusLogManagerTest : TestCaseWithFactory
	{
		public void TestAddALogIfNecessary()
		{
			AssertNull(StatusLogManager.AddALogIfNecessary(entry.Logs, MessageStatusList.Codes.NotSent));

			var log = StatusLogManager.AddALogIfNecessary(entry.Logs, MessageStatusList.Codes.Sent);
			AssertEquals(MessageStatusList.Codes.Sent, log.SL_Reference);
		}

		public void TestExcludeEstimated()
		{
			var list = new MessageStatusList();
			entry.Logs.AddNew(Events.CustomsEntryStatus, MessageStatusList.Codes.ClearOriginal, new ZDateTimeOffset(2013, 6, 6), true);
			AssertEquals(false, StatusLogManager.HasAClearLog(entry.Logs, MessageTypeList.Codes.B3CUSDEC, list));
			entry.Logs.AddNew(Events.CustomsEntryStatus, MessageStatusList.Codes.ClearOriginal, new ZDateTimeOffset(2013, 6, 6), false);
			AssertEquals(true, StatusLogManager.HasAClearLog(entry.Logs, MessageTypeList.Codes.B3CUSDEC, list));
		}

		#region Implementation
		CusEntryHeader entry;

		protected override void SetUp()
		{
			base.SetUp();
			entry = Factory.New<JobDeclaration>().CustomsEntryHeaders.AddNew();
		}
		#endregion Implementation
	}
}
