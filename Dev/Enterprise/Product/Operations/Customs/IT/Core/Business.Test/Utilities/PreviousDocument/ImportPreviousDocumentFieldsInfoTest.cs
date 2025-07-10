using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ImportPreviousDocumentFieldsInfoTest : TestCase
{
	public void TestLineNoEditable()
	{
		var fieldsInfo = new ImportPreviousDocumentFieldsInfo();
		AssertEquals("IsLineNoEditable", true, fieldsInfo.IsLineNoEditable);
	}
}
