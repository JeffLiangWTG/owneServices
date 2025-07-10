using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class Ucc6ExportPreviousDocumentFieldsInfoTest : TestCase
{
	public void TestReferenceNumberEditable()
	{
		var fieldsInfo = new Ucc6ExportPreviousDocumentFieldsInfo();
		AssertEquals("IsReferenceNumberEditable", true, fieldsInfo.IsReferenceNumberEditable);
	}
}
