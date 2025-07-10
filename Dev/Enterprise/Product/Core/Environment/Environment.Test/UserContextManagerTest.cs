using CargoWise.Application;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Environment.Testing
{
	[TestsSubclassesOf(typeof(IUserContextManager))]
	public abstract class UserContextManagerTest<T> : TransactionedTestCase
			where T : IUserContextManager
	{
		protected abstract T GetNewUserContextManager();

		public void TestSetUserContext()
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData(ObjectFactory.GetType("IGlbStaff"));
			factory.Save();

			var oldContext = Env.Instance.CurrentUserContext;
			var newContext = new UserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK);

			using (var manager = GetNewUserContextManager())
			{
				manager.SetMasterUserContext(oldContext);
				AssertEquals(oldContext, manager.Context);
				AssertEquals(oldContext, manager.ContextForReadOnly);

				manager.SetCurrentThreadUserContext(newContext, isRevert: false);
				AssertEquals(newContext, manager.Context);
				AssertEquals(newContext, manager.ContextForReadOnly);

				manager.SetCurrentThreadUserContext(oldContext, isRevert: true);
				AssertEquals(oldContext, manager.Context);
				AssertEquals(oldContext, manager.ContextForReadOnly);
			}
		}

		public void TestClearCurrentThread()
		{
			using (var manager = GetNewUserContextManager())
			{
				manager.SetCurrentThreadUserContext(Env.CurrentUserContext, isRevert: false);
				manager.ClearCurrentThread();
				AssertNull(manager.Context.User);
			}
		}
	}
}
