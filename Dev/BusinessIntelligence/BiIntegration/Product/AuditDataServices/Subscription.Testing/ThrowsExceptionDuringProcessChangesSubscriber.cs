namespace Enterprise.AuditDataServices.Subscription.Testing
{
	using System;
	using System.Data;
	using CargoWise.Schema;
	using Enterprise.Integration;
	using NUnit.Framework;

	public class ThrowsExceptionDuringProcessChangesSubscriber : GenericTestDataChangeSubscriber
	{
		public ThrowsExceptionDuringProcessChangesSubscriber(string code, string description, ITableSchema sourceTable, Exception exceptionToThrow = null) : base(code, description, sourceTable)
		{
			this.ExceptionToThrow = exceptionToThrow ?? new Exception();
		}

		public Exception ExceptionToThrow { get; }

		public override void ProcessChanges(ILogger logger, DataTable changeTable)
		{
			throw ExceptionToThrow;
		}
	}

	[TestFixture]
	public class DummyNunitTest
	{
		[Test]
		public void AllowNUnitModernAssertionsInNUnitCore()
		{
			// This is a dummy test method that will always pass.
			Assert.Pass("This test is intentionally placed for NUnit3TestAdapter to find a Nunit test and DAT not throw an error.");
		}
	}
}
