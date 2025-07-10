using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	public class DocumentFilterGuidTest : TestCaseWithFactory
	{
		public void TestIsMatch()
		{
			var eDoc1 = new eDocForTest { UniqueKey = ZGuid.NewZGuid() };
			var eDoc2 = new eDocForTest { UniqueKey = ZGuid.NewZGuid() };

			var filter = new DocumentFilterGuid("TestFilter", eDoc => eDoc.UniqueKey);
			Assert(!filter.IsMatch(eDoc1));

			filter.AddFilterValue(eDoc1.UniqueKey.ToString().ToUpper());
			filter.AddFilterValue(eDoc1.UniqueKey.ToString().ToUpper());
			Assert(filter.IsMatch(eDoc1));
			Assert(!filter.IsMatch(eDoc2));

			filter.AddFilterValue(eDoc2.UniqueKey.ToString().ToLower());
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
		}

		public void TestInvalidGuid()
		{
			var filter = new DocumentFilterGuid("TestFilter", eDoc => eDoc.UniqueKey);
			AssertExceptionThrown<DataObjectReadFailureException>(() => filter.AddFilterValue("abc"));
		}
	}
}
