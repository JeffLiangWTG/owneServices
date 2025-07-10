using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentScanning.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.DocumentScanning.DataTransfer.Test.Universal.DocumentRequest.DocumentFilters
{
	public class DocumentFilterBoolTest : TestCaseWithFactory
	{
		public void TestAddFilterValue()
		{
			var filter = new DocumentFilterBool("TestFilter", eDoc => eDoc.IsPublished);
			Assert(!filter.Value.HasValue);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Cannot parse filter TestFilter value from text 'abc'. Supported values: True, False.", () => filter.AddFilterValue("abc"));
			Assert(!filter.Value.HasValue);

			filter.AddFilterValue("True");
			Assert(filter.Value.HasValue);
			Assert(filter.Value.Value);

			AssertNoExceptionThrown(() => filter.AddFilterValue("True"));
			AssertNoExceptionThrown(() => filter.AddFilterValue("true"));
			Assert(filter.Value.Value);

			AssertExceptionThrown(typeof(DataObjectReadFailureException), "Both True and False are specified for filter TestFilter.", () => filter.AddFilterValue("False"));
			Assert(filter.Value.Value);

			filter = new DocumentFilterBool("TestFilter", eDoc => eDoc.IsPublished);
			Assert(!filter.Value.HasValue);

			filter.AddFilterValue("False");
			Assert(filter.Value.HasValue);
			Assert(!filter.Value.Value);
		}

		public void TestIsMatch()
		{
			var eDoc1 = new eDocForTest { IsPublished = true };
			var eDoc2 = new eDocForTest { IsPublished = false };

			var filter = new DocumentFilterBool("TestFilter", eDoc => eDoc.IsPublished);
			Assert(filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));

			filter.AddFilterValue("True");
			Assert(filter.IsMatch(eDoc1));
			Assert(!filter.IsMatch(eDoc2));

			filter = new DocumentFilterBool("TestFilter", eDoc => eDoc.IsPublished);
			filter.AddFilterValue("False");
			Assert(!filter.IsMatch(eDoc1));
			Assert(filter.IsMatch(eDoc2));
		}
	}
}
