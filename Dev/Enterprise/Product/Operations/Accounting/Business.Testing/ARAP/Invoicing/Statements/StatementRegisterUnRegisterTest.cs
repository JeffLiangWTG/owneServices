using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class StatementRegisterUnRegisterTest : TestCase
	{
		#region DummyStatement

		public class DummyStatement : Statement
		{
			protected DummyStatement(GlbBranch branch)
				: base(branch)
			{
			}
			public static new Statement New(GlbBranch branch)
			{
				return new DummyStatement(branch);
			}
			public static void Register()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}
		}

		#endregion

		public void TestNew()
		{
			AssertEquals("The new method should return should be of type", typeof(Statement), Statement.New(GlbBranch.CurrentBranch).GetType());
			DummyStatement.Register();
			AssertEquals("The new method should return should be of type", typeof(DummyStatement), Statement.New(GlbBranch.CurrentBranch).GetType());
		}
	}
}
