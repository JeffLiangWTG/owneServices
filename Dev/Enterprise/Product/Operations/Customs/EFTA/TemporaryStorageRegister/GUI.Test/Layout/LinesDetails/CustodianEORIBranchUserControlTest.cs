using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(CustodianEORIBranchUserControl))]
sealed class CustodianEORIBranchUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new CustodianEORIBranchUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.CustodianEORITextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_CustodianIdentifier"));
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.CustodianBranchTextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_CustodianIdentifierBranchNo"));
	});
}
