using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	class TemporaryStoragePreviousDocumentComparerTest : TestCaseWithFactory
	{
		public void TestComparer()
		{
			var comparer = new TemporaryStoragePreviousDocumentComparer();

			var first = Factory.New<TemporaryStoragePreviousDocument>();
			first.CSI_Code = "N355";
			first.CSI_ReferenceNumber = "1234";

			var second = Factory.New<TemporaryStoragePreviousDocument>();
			second.CSI_Code = "N355";
			second.CSI_ReferenceNumber = "1234";
			Assert("If CSI_Code and CSI_ReferenceNumber are equal, then two previous document should be considered equal.", comparer.Equals(first, second));

			second.CSI_Code = "CL907";
			Assert("If CSI_Code are not equal, then two previous documents should not be considered equal.", !comparer.Equals(first, second));

			first.CSI_Code = "CL907";
			Assert("If CSI_Code and CSI_ReferenceNumber are equal, then two previous document should be considered equal.", comparer.Equals(first, second));

			second.CSI_ReferenceNumber = "890876";
			Assert("If CSI_ReferenceNumber are not equal, then two previous documents should not be considered equal.", !comparer.Equals(first, second));
		}
	}
}
