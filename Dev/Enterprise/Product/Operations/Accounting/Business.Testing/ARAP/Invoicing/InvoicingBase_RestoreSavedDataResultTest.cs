using System;
using CargoWise.EntityFramework.Testing;
using static Enterprise.Accounting.Business.ARAP.Invoicing.InvoicingBase;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class InvoicingBase_RestoreSavedDataResultTest : TestCaseWithFactory
	{
		public void TestRestoreSavedData_ThrowsArgumentException_WhenErrorNotEmptyOnSuccess()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Success, "TestError"));
			AssertNoExceptionThrown(() => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Success, ""));
		}

		public void TestRestoreSavedData_ThrowsArgumentException_WhenErrorEmptyOnSuccessWithErrors()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.SuccessWithErrors, ""));
			AssertNoExceptionThrown(() => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.SuccessWithErrors, "TestError"));
		}

		public void TestRestoreSavedData_ThrowsArgumentException_WhenErrorEmptyOnFailed()
		{
			AssertExceptionThrown(typeof(ArgumentException), () => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Failed, ""));
			AssertNoExceptionThrown(() => new RestoreSavedDataResult(RestoreSavedDataResult.ResultType.Failed, "TestError"));
		}
	}
}
