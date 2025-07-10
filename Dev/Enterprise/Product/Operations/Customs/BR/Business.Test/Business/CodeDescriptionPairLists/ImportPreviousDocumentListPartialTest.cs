using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportPreviousDocumentListPartialTest : TestCase
	{
		public void TestMapToIdentifier()
		{
			AssertEquals("Result empty", string.Empty, ImportPreviousDocumentList.MapToCustomsCode(null));
			AssertEquals("Result empty", string.Empty, ImportPreviousDocumentList.MapToCustomsCode(""));
			AssertEquals("Result empty", string.Empty, ImportPreviousDocumentList.MapToCustomsCode("XX"));
			AssertEquals("1 -> DE", "DE", ImportPreviousDocumentList.MapToCustomsCode(ImportPreviousDocumentList.Codes.DE));
			AssertEquals("2 -> DI", "DI", ImportPreviousDocumentList.MapToCustomsCode(ImportPreviousDocumentList.Codes.DI));
			AssertEquals("3 -> DUE", "DUE", ImportPreviousDocumentList.MapToCustomsCode(ImportPreviousDocumentList.Codes.DEU));
			AssertEquals("4 -> DUIMP", "DUIMP", ImportPreviousDocumentList.MapToCustomsCode(ImportPreviousDocumentList.Codes.DIU));
		}
	}
}
