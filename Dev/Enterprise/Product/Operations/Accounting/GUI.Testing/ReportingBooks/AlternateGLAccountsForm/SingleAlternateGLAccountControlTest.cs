using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.GUI.ARAP.Invoicing;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Testing
{
	public class SingleAlternateGLAccountControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new SingleAlternateGLAccountControl())
			{
				AssertNotNull(control.GetControl<ZTextBox>("accountNumberWithSeparatorTextBox", true));
				AssertNotNull(control.GetControl<ZDropEdit>("reportSectionDropEdit", true));
				AssertNotNull(control.GetControl<AlternateGLAccountZGuidFindBox>("totalReferenceGuidFindBox", true));
				AssertNotNull(control.GetControl<AlternateGLAccountZGuidFindBox>("alternateNumGuidFindBox", true));
				AssertNotNull(control.GetControl<ZDropEdit>("debitCreditDropEdit", true));
				AssertNotNull(control.GetControl<AlternateGLAccountZGuidFindBox>("consolidateGuidFindBox", true));
				AssertNotNull(control.GetControl<AlternateGLAccountZGuidFindBox>("percentNumberGuidFindBox", true));
				AssertNotNull(control.GetControl<ZCalcEdit>("printSequenceCalcEdit", true));
				AssertNotNull(control.GetControl<ZCalcEdit>("totalLevelCalcEdit", true));
				AssertNotNull(control.GetControl<ZTextBox>("prefixedACNumTextBox", true));
				AssertNotNull(control.GetControl<ZTextBox>("accountNameTextBox", true));
				AssertNotNull(control.GetControl<ZTextBox>("accountNumberTextBox", true));
				AssertNotNull(control.GetControl<ZLabel>("existAlternateAccountLabel", true));
			}
		}
	}
}
