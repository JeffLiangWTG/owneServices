#if DEBUG
using System;
using CargoWise.Data;

namespace NUnit.Framework
{
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
	public sealed class RunInExtraTransactionAttribute : TestSetupAttribute
	{
		public override void SetUp(TestCase testCase)
		{
			Db.Connection.BeginTransaction();
		}

		public override void TearDown(TestCase testCase)
		{
			Db.Connection.RollbackTransaction();
		}
	}
}

#endif
