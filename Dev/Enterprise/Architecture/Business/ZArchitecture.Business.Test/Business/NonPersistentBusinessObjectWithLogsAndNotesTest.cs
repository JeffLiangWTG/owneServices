using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NonPersistentBusinessObjectWithLogsAndNotesTest : TestCaseWithFactory
	{
		public void TestLogsAndNotes()
		{
			AssertEquals(BusinessObject.Dummy.Notes, BusinessObject.Notes);
			AssertEquals(BusinessObject.Dummy.Logs, BusinessObject.Logs);
		}

		#region Test Classes

		class TestNonPersistentBusinessObjectWithLogsAndNotes : NonPersistentBusinessObjectWithLogsAndNotes
		{
			public TestNonPersistentBusinessObjectWithLogsAndNotes(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DummyEnterpriseBusinessObject Dummy
			{
				get { return dummy ?? (dummy = Factory.New<DummyEnterpriseBusinessObject>()); }
			}
			DummyEnterpriseBusinessObject dummy;

			protected override BusinessObject LogsAndNotesTarget
			{
				get { return Dummy; }
			}
		}

		#endregion

		#region Implementation

		TestNonPersistentBusinessObjectWithLogsAndNotes BusinessObject
		{
			get { return businessObject ?? (businessObject = new TestNonPersistentBusinessObjectWithLogsAndNotes(Factory)); }
		}
		TestNonPersistentBusinessObjectWithLogsAndNotes businessObject;

		#endregion
	}
}
