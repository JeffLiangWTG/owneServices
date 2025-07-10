using System;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class CopyingOperationTest : TestCaseWithDummy
	{
		[ExpectException(typeof(NotSupportedException))]
		public void TestExceptionWhenIsCopyingIsTrue()
		{
			((IBusinessObjectInternals)Dummy).IsCopying = true;
			using (new CopyingOperation(Dummy))
			{
				Fail("Exception should be thrown");
			}
		}

		public void TestSuspensionsAndListChanged()
		{
			ListChangedCount = 0;
			((IBindingList)Dummy).ListChanged += new ListChangedEventHandler(CopyingOperationTest_ListChanged);
			AssertEquals("IsCopying", false, ((IBusinessObjectInternals)Dummy).IsCopying);
			AssertEquals("IsValidationSuspended", false, Dummy.IsValidationSuspended);
			AssertEquals("IsListChangeSuspended", false, ((ISingleElementListInternal)Dummy).IsListChangeSuspended);
			using (new CopyingOperation(Dummy))
			{
				AssertEquals("IsCopying", true, ((IBusinessObjectInternals)Dummy).IsCopying);
				AssertEquals("IsValidationSuspended", true, Dummy.IsValidationSuspended);
				AssertEquals("IsListChangeSuspended", true, ((ISingleElementListInternal)Dummy).IsListChangeSuspended);
				Dummy.Z0_Description = "blah blah blah";
				AssertEquals("ListChangedCount", 0, ListChangedCount);
			}
			AssertEquals("IsCopying", false, ((IBusinessObjectInternals)Dummy).IsCopying);
			AssertEquals("IsValidationSuspended", false, Dummy.IsValidationSuspended);
			AssertEquals("IsListChangeSuspended", false, ((ISingleElementListInternal)Dummy).IsListChangeSuspended);
			AssertEquals("ListChangedCount", 1, ListChangedCount);
		}

		int ListChangedCount;
		void CopyingOperationTest_ListChanged(object sender, ListChangedEventArgs e)
		{
			ListChangedCount++;
		}
	}
}
