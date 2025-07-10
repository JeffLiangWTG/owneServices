using System.IO;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Web.Testing
{
	sealed class WebDeliveryMethodTest : TestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMergeFilesIntoSingleExcelFile()
		{
			using (WebDeliveryMethod method = new WebDeliveryMethod())
			{
				DeliveryInfo info1 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);
				DeliveryInfo info2 = new DeliveryInfo(DeliveryInfo.DeliveryFormats.Document);

				method.AddFile(info1);
				method.AddFile(info2);

				info1.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge1.xls", FileMode.Open, FileAccess.Read), "xls");
				info2.SetFileContents(new FileStream(UnitTestingConstants.TestFilesDir + "Merge2.xls", FileMode.Open, FileAccess.Read), "xls");

				using (Stream mergedStream = method.MergeFilesIntoSingleExcelFile())
				{
					AssertNotNull("Stream should be instance of MemoryStream type", mergedStream);
					Assert("Stream should not be empty", ((MemoryStream)mergedStream).ToArray().Length > 0);
				}
			}
		}
	}
}
