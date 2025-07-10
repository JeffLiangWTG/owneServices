using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

sealed class EntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestDutyAndTaxDetailsUserControlType()
	{
		using (var form = new ZForm())
		using (var entryLineAdditionalDataUserControl = new EntryLineAdditionalDataUserControl())
		{
			form.Controls.Add(entryLineAdditionalDataUserControl);
			form.Show();
			AssertEquals("Using Correct EntryLineTaxAndFeeUserControl", typeof(EntryLineTaxAndFeeUserControl), entryLineAdditionalDataUserControl.DutyAndTaxDetails.UserControlType);
		}
	}
}
