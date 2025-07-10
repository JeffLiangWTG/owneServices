using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.GUI.Testing;

[TestedType(typeof(DisposalEntitledTraderEORIBranchUserControl))]
sealed class DisposalEntitledTraderEORIBranchUserControlTest : TestCaseWithFactory
{
	public void TestControls() => CombineAssertions(() =>
	{
		using var control = new DisposalEntitledTraderEORIBranchUserControl();
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.DisposalEntitledTraderEORITextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_GoodsOwnerIdentifier"));
		_ = control.AssertContainsControl<ZTextBox>(nameof(control.DisposalEntitledTraderBranchTextBox), x => x.WithBindTo("CusTempStorageRegLines.SRL_GoodsOwnerIdentifierBranchNo"));
	});
}
