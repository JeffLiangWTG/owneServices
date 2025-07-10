using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class CusEntryHeaderDocumentSupporterConfiguratorTest : TestCaseWithFactory
{
	public void TestConfigure()
	{
		var declaration = Factory.New<JobDeclaration>();
		var supporter = declaration.DocumentSupporter.SadDocumentSupporter;

		var configurator = new CusEntryHeaderDocumentSupporterConfigurator();

		ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.OK;
		var result = configurator.Configure(supporter);
		AssertEquals("Result Cancel", false, result.Cancel);

		ZFormModaliser.ResultToReturnFromShowDialog = System.Windows.Forms.DialogResult.Cancel;
		result = configurator.Configure(supporter);
		AssertEquals("Result Cancel", true, result.Cancel);
	}
}
