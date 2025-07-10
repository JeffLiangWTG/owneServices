using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing;

public class PreviousDocumentLookupsTest : TestCaseWithFactory
{
	public void TestPreviousDocumentCodeList()
	{
		var previousDocument = Factory.New<PreviousDocument>();
		(string code, string description)[] expectedValues =
			{
				("337", "Declaración de depósito temporal."),
				("355", "Declaración sumaria de entrada."),
		};

		var codeList = (CodeDescriptionPairList)previousDocument.Lookups.CodeList;
		AssertCodeDescriptionPairList(codeList, expectedValues);
	}
}
