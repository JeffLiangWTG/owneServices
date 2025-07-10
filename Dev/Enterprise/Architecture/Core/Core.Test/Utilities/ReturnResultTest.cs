using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class ReturnResultTest : TestCase
	{
		public void TestReturnResultProperties()
		{
			ReturnResult result = new ReturnResult();

			AssertNull("Message", result.Message);
			AssertEquals("Success", false, result.Success);

			result.Message = "Aye.";
			result.Success = true;

			AssertEquals("Message", "Aye.", result.Message);
			AssertEquals("Success", true, result.Success);
		}

		public void TestReturnResultWithValueProperties()
		{
			ReturnResult<Guid> result = new ReturnResult<Guid>();

			AssertNull("Message", result.Message);
			AssertEquals("Success", false, result.Success);
			AssertEquals("Value", Guid.Empty, result.Value);

			result.Message = "Nay.";
			result.Success = true;
			Guid guid = Guid.NewGuid();
			result.Value = guid;

			AssertEquals("Message", "Nay.", result.Message);
			AssertEquals("Success", true, result.Success);
			AssertEquals("Value", guid, result.Value);
		}
	}
}
