using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AsycudaCustoms.ServiceTasks.Testing
{
	class AsycudaIncomingDataExceptionTest : TestCaseWithFactory
	{
		public void TestAsycudaIncomingDataExceptionClass()
		{
			const string errorMessage = "Something is wrong.";
			string messageRetrieved;
			Type classTypeRetrieved;

			try
			{
				throw new AsycudaIncomingDataException(errorMessage);
			}
			catch (Exception ex)
			{
				messageRetrieved = ex.Message;
				classTypeRetrieved = ex.GetType();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Check error message part of exception class.", errorMessage, messageRetrieved);
				AssertEquals("Check class type of exception class.", typeof(AsycudaIncomingDataException), classTypeRetrieved);
			});
		}
	}
}
