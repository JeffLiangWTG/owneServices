using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(AVSQueryRetryInfoBO))]
	sealed class AVSQueryRetryInfoBOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSerialize()
		{
			var retryInfo = new AVSQueryRetryInfoBO();
			retryInfo.RetryTimes = 1;
			retryInfo.LastFailure = "Failed";

			AssertEquals("<AVSQueryRetryInfo><RetryTimes>1</RetryTimes><LastFailure>Failed</LastFailure></AVSQueryRetryInfo>", retryInfo.Serialize());
		}

		public void TestDeserialize()
		{
			var retryInfo = new AVSQueryRetryInfoBO();
			AssertEquals(0, retryInfo.RetryTimes);
			AssertEquals("", retryInfo.LastFailure);

			retryInfo.Deserialize("<AVSQueryRetryInfo><RetryTimes>1</RetryTimes><LastFailure>Failed</LastFailure></AVSQueryRetryInfo>");

			AssertEquals(1, retryInfo.RetryTimes);
			AssertEquals("Failed", retryInfo.LastFailure);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AVSQueryRetryInfoBO();
		}

		#endregion
	}
}
