using System.Linq;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.CusTempStorage.Testing;

[TestedType(typeof(CusTempStorageJobHeaderDocumentSupporter))]
sealed public class CusTempStorageJobHeaderDocumentSupporterTest : EU.Business.CusTempStorage.Testing.CusTempStorageJobHeaderDocumentSupporterTest
{
	public override void TestGetDocumentWrappersInternal_TempStorageJobHeader()
	{
		var wrappers = Header.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TempStorageHeader, null);
		AssertEquals("Enterprise.Customs.FR.DocumentWrappers.TempStorageHeader.DocTempStorageHeader", wrappers.Single().GetType().FullName);
	}
}
