using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.BISI.Testing
{
	public class PWSRecordBaseTest : TestCase
	{
		public void TestConstructor()
		{
			AssertEquals("RecordData should be set", "RecordData", Record.RecordData);
			AssertEquals("NotificationSubscriber should be set", Notifications, Record.Notifications);
			Assert("ValidateRecordData should be called in the constructor", Record.ValidateRecordDataCalled);
		}

		TestPWSRecordBase Record
		{
			get
			{
				if (fRecord == null)
				{
					fRecord = new TestPWSRecordBase("RecordData", Notifications);
				}

				return fRecord;
			}
		}

		NotificationBuffer Notifications
		{
			get
			{
				if (fNotifications == null)
				{
					fNotifications = new NotificationBuffer();
				}

				return fNotifications;
			}
		}

		TestPWSRecordBase fRecord;
		NotificationBuffer fNotifications;
		#region TestPWSRecordBase
		class TestPWSRecordBase : PWSRecordBase
		{
			public TestPWSRecordBase(ZString recordData, INotifications notifications) : base(recordData, notifications)
			{
			}

			public new ZString RecordData
			{
				get
				{
					return base.RecordData;
				}
			}

			public new INotifications Notifications
			{
				get
				{
					return base.Notifications;
				}
			}

			public new ZInt ToZInt(string value)
			{
				return base.ToZInt(value);
			}

			protected override bool ValidateRecordData()
			{
				ValidateRecordDataCalled = true;
				return true;
			}

			public bool ValidateRecordDataCalled;
		}
		#endregion
	}
}
