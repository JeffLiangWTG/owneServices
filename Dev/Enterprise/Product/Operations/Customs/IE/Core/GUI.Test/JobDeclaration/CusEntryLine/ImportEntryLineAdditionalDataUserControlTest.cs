using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.GUI.Testing;

public class ImportEntryLineAdditionalDataUserControlTest : TestCaseWithFactory
{
	public void TestRefundsGrid()
	{
		using var control = new ImportEntryLineAdditionalDataUserControl();
		CombineAssertions("Existence of RefundsTabPage and RefundsGrid; grid invisible by default in Winzor.", () =>
		{
			control.ExtendInfoTabControl.Visible = true;
			control.ExtendInfoTabControl.SelectedTab = control.RefundsTabPage;

			control.AssertContainsControl<ZTabPage>("RefundsTabPage", x => x
				.WithCaption("Refunds").WithoutStrategy("IsVisible")
			);
			control.AssertContainsControl<ZGrid>("RefundsGrid", x => x
				.WithBindTo("CustomsEntryHeaders.AllEntryLines.RefundDuties").WithoutStrategy("IsVisible")
			);
		});
	}

	public void TestGetDutyAndTaxDetailsUserControlType()
	{
		using var form = new ZForm();
		using var control = new ImportEntryLineAdditionalDataUserControl();

		form.Controls.Add(control);
		form.Show();
		var dutyAndTaxDetailsControl = control.FindSingle<ZDynamicControlCreationUserControl>("DutyAndTaxDetails");
		AssertEquals("Using Calculated and Confirmed Fees", typeof(EntryLineTaxAndConfirmedFeeUserControl), dutyAndTaxDetailsControl.UserControlType);
	}
}
