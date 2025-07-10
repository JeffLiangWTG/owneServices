using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Accounting.Business.Base.Interfaces.Testing
{
	[TestsSubclassesOf(typeof(ITransaction))]
	public abstract class BaseITransactionTestCase : TestCaseWithFactory
	{
		#region Implementation

		protected Type GetExpectedObjectType() => TestedTypeHelper.GetTestedType(GetType());

		protected virtual ITransaction GetNewObject()
		{
			return (ITransaction)Factory.New(GetExpectedObjectType());
		}

		protected ITransaction TestObject
		{
			get { return testObject ?? (testObject = GetNewObject()); }
		}
		ITransaction testObject;

		#endregion

		public virtual void TestUnmatchDateDefaulting()
		{
			AssertEquals(ZDateTime.Empty, TestObject.UnmatchDate);
		}

		public virtual void TestUnmatchDateReadonly()
		{
			AssertEquals(true, TestObject.UnmatchDateInfo.ReadOnly);
			AssertEquals(true, TestObject.UnmatchDate_ReadOnly);
		}

		public virtual void TestUnmatchDateValidation()
		{
			TestObject.UnmatchDate = ZDateTime.Today;
			TestObject.UnmatchDate = ZDateTime.Empty;
			AssertNoErrors(TestObject.UnmatchDateInfo);
		}
	}
}
