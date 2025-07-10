using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;

sealed class EntryInstructionDetailBasicUserControlTest : TestCaseWithFactory
{
	public void TestActivateByOperatorCheckBox()
	{
		AssertType<ZCheckBox>(control.ActivateByOperatorCheckBox);
	}

	public void TestIncludeRoutingSecurityDataCheckBox()
	{
		AssertType<ZCheckBox>(control.IncludeRoutingSecurityDataCheckBox);
	}

	public void TestRequestLabel()
	{
		AssertType<ZLabel>(control.RequestLabel);
	}

	public void TestRequestTypeDropEdit()
	{
		AssertType<ZDropEdit>(control.RequestTypeDropEdit);
	}

	public void TestNationalCheckBox()
	{
		AssertType<ZCheckBox>(control.NationalCheckBox);
	}

	public void TestNumberOfDaysCalcEdit()
	{
		AssertType<ZCalcEdit>(control.NumberOfDaysCalcEdit);
	}

	public void TestJustificationTextBox()
	{
		AssertType<ZTextBox>(control.JustificationTextBox);
	}

	public void TestIndirectTypeDropEdit()
	{
		AssertType<ZDropEdit>(control.IndirectTypeDropEdit);
	}

	EntryInstructionDetailBasicUserControl control;
	protected override void SetUp()
	{
		base.SetUp();
		control = new EntryInstructionDetailBasicUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
}
