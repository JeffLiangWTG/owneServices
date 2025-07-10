using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.EventManagement.Testing
{
	[TestedType(typeof(LogCopy))]
	sealed class LogCopyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new LogCopy(Factory.NewWithValidTestData<StmALog>());
		}

		public void TestLogCopy_PK()
		{
			var log = Factory.NewWithValidTestData<StmALog>();
			var logCopy = ((IStmALog)log).WeakCopy();
			AssertEquals(log.PK, ((LogCopy)logCopy).SL_PK);
			AssertEquals(log.PK, logCopy.Identifier);
		}
	}
}
