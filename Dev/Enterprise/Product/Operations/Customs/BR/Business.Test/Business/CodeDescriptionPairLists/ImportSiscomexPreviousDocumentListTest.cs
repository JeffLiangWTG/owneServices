using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ImportSiscomexPreviousDocumentListTest : TestCase
	{
		public void TestMapToCustomsCode()
		{
			AssertEquals("Result empty", ZString.Empty, ImportSiscomexPreviousDocumentList.MapToCustomsCode(""));
			AssertEquals("Result empty", ZString.Empty, ImportSiscomexPreviousDocumentList.MapToCustomsCode("XX"));
			AssertEquals("DI -> 2", "2", ImportSiscomexPreviousDocumentList.MapToCustomsCode(ImportSiscomexPreviousDocumentList.Codes.DI));
			AssertEquals("RE -> 3", "3", ImportSiscomexPreviousDocumentList.MapToCustomsCode(ImportSiscomexPreviousDocumentList.Codes.RE));

			foreach (var code in new ImportSiscomexPreviousDocumentList().GetAllCodes())
			{
				AssertNotEquals(ZString.Empty, ImportSiscomexPreviousDocumentList.MapToCustomsCode(code));
			}
		}
	}
}
