using System;
using CargoWise.Data;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace Enterprise.Accounting.GUI.Testing.ARAP.PendingAllocation.Approval
{
	[TestedType(typeof(TransactionPendingAllocationApprovalRequestFactory))]
	class TransactionPendingAllocationApprovalRequestFactoryTest : TransactionedTestCase
	{
		public void TestHasConstructorWithDBConectionAndNoParameter()
		{
			AssertNotNull(TestType.GetConstructor(Type.EmptyTypes));
			AssertNotNull(TestType.GetConstructor(new Type[] { typeof(DbConnection) }));
		}

		Type TestType => TestedTypeHelper.GetTestedType(GetType());
	}
}
