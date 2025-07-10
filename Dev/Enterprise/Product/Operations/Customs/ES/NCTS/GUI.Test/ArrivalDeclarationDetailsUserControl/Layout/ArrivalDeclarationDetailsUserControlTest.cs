using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalDeclarationDetailsUserControl))]
sealed class ArrivalDeclarationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
	}

	public void TestCircuitTextBox()
	{
		AssertType<ZTextBox>(control.CircuitTextBox);
	}

	public void TestArrivalSummaryDeclaration()
	{
		AssertType<ArrivalSummaryDeclarationUserControl>(control.ArrivalSummaryDeclarationUserControl);
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ArrivalDeclarationDetailsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ArrivalDeclarationDetailsUserControl control;
}
