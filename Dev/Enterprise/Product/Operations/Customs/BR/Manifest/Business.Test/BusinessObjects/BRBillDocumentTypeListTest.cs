using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Manifest.Business.Test.BusinessObjects
{
	public class BRBillDocumentTypeListTest : TestCaseWithFactory
	{
		public void TestBRManifestDocType()
		{
			var list = new BRBillDocumentTypeList();
			CombineAssertions(() =>
			{
				AssertEquals("Number of codes", 2, list.Count);
				Assert("Missing code: DUE", list.ContainsCode("DUE"));
				Assert("Missing code: UCR", list.ContainsCode("UCR"));
			});
		}
	}
}
