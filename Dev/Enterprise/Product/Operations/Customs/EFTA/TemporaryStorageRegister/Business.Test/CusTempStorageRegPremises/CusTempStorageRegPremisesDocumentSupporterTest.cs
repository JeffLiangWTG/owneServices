using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(CusTempStorageRegPremisesDocumentSupporter))]
sealed class CusTempStorageRegPremisesDocumentSupporterTest : DocumentSupporterTest
{
	public void TestBusinessContext()
	{
		AssertEquals(BusinessContext.TempStorageRegPremis, Premises.DocumentSupporter.BusinessContext);
	}

	public void TestSupportedDataContexts()
	{
		AssertEquals(true, Premises.DocumentSupporter.ListOfSupportedDataContexts.ContainsCode(DataContext.TempStorageRegPremises));
	}

	public void TestGetDocumentWrappersInternal()
	{
		AssertEquals("Wrapper created for CusTempStorageRegPremises", 0, Premises.DocumentSupporter.GetDocumentWrappers(DataContext.TempStorageRegPremises, null).Length);
	}

	protected override IDocumentSupportable GetDocumentSupportableBusinessObject() => Premises;

	CusTempStorageRegPremises Premises => premises ??= Factory.New<CusTempStorageRegPremises>();
	CusTempStorageRegPremises premises;
}
