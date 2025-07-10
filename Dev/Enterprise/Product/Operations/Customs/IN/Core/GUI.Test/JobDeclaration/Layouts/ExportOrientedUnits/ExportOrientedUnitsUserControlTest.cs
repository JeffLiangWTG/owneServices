using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IN.GUI.Testing;

sealed class ExportOrientedUnitsUserControlTest : TestCase
{
	public void TestControlTypes()
	{
		CombineAssertions(() =>
		{
			AssertType<ZDocAddressControl>(control.ExportOrientedUnitsDocAddressControl);
			AssertType<ZDateEdit>(control.ExaminationDateEdit);
			AssertType<ZTextBox>(control.ExaminingOfficerNameTextBox);
			AssertType<ZTextBox>(control.ExaminingOfficerDesignationTextBox);
			AssertType<ZTextBox>(control.SupervisingOfficerNameTextBox);
			AssertType<ZTextBox>(control.SupervisingOfficerDesignationTextBox);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();
		control = new ExportOrientedUnitsUserControl();
	}

	protected override void TearDown()
	{
		base.TearDown();
		control.Dispose();
	}
	ExportOrientedUnitsUserControl control;
}
