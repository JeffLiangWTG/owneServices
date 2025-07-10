using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Testing;
sealed class UCC6EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestDutyAndTaxDetailsIncludesConfirmedFees()
	{
		using (var form = new ZForm())
		using (var importEntryLineAdditionalDataUserControl = new UCC6EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(importEntryLineAdditionalDataUserControl);
			form.Show();
			var control = importEntryLineAdditionalDataUserControl.FindSingle<ZDynamicControlCreationUserControl>("DutyAndTaxDetails");
			AssertEquals("Using Calculated and Confirmed Fees", typeof(EntryLineTaxAndConfirmedFeeUserControl), control.UserControlType);
		}
	}
}
