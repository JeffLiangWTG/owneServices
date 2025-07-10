using System;
using System.Data;
using BorderWise.Sync;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.BorderWise.Test
{
	class ChangeTypeTest : TestCase
	{
		public void TestToChangeType()
		{
			AssertEquals(ChangeType.None, DataRowState.Unchanged.ToChangeType());
			AssertEquals(ChangeType.Add, DataRowState.Added.ToChangeType());
			AssertEquals(ChangeType.Delete, DataRowState.Deleted.ToChangeType());
			AssertEquals(ChangeType.Update, DataRowState.Modified.ToChangeType());
			AssertExceptionThrown("Should throw exception for unsupported row state", typeof(ArgumentException), "Unsupported DataRowState value Detached", () => DataRowState.Detached.ToChangeType(), true);
		}
	}
}
