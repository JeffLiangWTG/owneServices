using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.CH.GUI.Testing;

internal class DutyRateUserControlTest : TestCaseWithFactory
{
	public void TestBindingSourceDataSourceType()
	{
		using (var control = new DutyRateUserControl())
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}
	}

	public void TestControls()
	{
		using (var control = new DutyRateUserControl())
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(nameof(control.DutyRateDropEdit), control.DutyRateDropEdit);
				AssertType<ZTextBox>(nameof(control.DutyRateDropEdit), control.DutyRateTextBox);
				AssertType<ZCheckBox>(nameof(control.DutyRateDropEdit), control.DutyRateConfirmationCheckBox);
			});
		}
	}

	public void TestDutyRateDropEdit()
	{
		using (var control = new DutyRateUserControl())
		{
			AssertEquals("BindTo", nameof(JobComInvoiceLine.DutyRateDescription), control.DutyRateDropEdit.BindTo);
			AssertEquals("CharacterCasing", CharacterCasing.Normal, control.DutyRateDropEdit.CharacterCasing);
			AssertEquals("ShowCodeInDropDown", ShowInDropDownList.OnlyShowDescription, control.DutyRateDropEdit.ShowInDropDown);
		}
	}
}
