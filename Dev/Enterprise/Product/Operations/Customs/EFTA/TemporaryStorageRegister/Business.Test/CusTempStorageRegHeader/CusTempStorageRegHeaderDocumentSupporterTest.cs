using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegHeaderDocumentSupporter))]
sealed class CusTempStorageRegHeaderDocumentSupporterTest : DocumentSupporterTest
{
	public void TestBusinessContext()
	{
		AssertEquals(BusinessContext.TempStorageRegHeader, Header.DocumentSupporter.BusinessContext);
	}

	public void TestSupportedDataContexts()
	{
		AssertEquals(expected: true, Header.DocumentSupporter.ListOfSupportedDataContexts.ContainsCode(DataContext.TempStorageRegHeader));
	}

	public void TestGetDocumentWrappersInternal()
	{
		AssertEquals("Wrapper created for CusTempStorageRegHeader", 0, Header.DocumentSupporter.GetDocumentWrappers(DataContext.TempStorageRegHeader, null).Length);
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Header;

	CusTempStorageRegHeader Header => header ??= Factory.New<CusTempStorageRegHeader>();
	CusTempStorageRegHeader header;
}
