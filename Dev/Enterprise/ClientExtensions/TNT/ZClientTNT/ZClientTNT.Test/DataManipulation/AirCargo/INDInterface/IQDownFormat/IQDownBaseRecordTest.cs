using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.TNT.Testing
{
	public abstract class IQDownBaseRecordTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			IQDownBaseRecord record = GetRecord(DataString);
			AssertEquals(ExpectedRecordType, record.GetType());
			AssertEquals(ExpectedFieldCount, record.FieldCount);
			NotificationBuffer buffer = new NotificationBuffer();
			AssertEquals("It should be a valid record - Buffer:" + System.Environment.NewLine + buffer.AsString, true, record.IsValid(buffer));
			AssertEquals("Buffer should have no error - Buffer:" + System.Environment.NewLine + buffer.AsString, false, buffer.HasErrors);
		}

		public abstract void TestFieldProperty();
		public abstract void TestHumanReadable();
		protected abstract IQDownBaseRecord GetRecord(ZString rawData);
		protected abstract ZString DataString { get; }

		protected abstract Type ExpectedRecordType { get; }

		protected abstract int ExpectedFieldCount { get; }
	}
}
