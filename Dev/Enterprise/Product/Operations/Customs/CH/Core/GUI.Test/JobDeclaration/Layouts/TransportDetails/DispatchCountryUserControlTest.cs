using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI.Testing;

public class DispatchCountryUserControlTest : TestCaseWithFactory
{
	public void TestDispatchUserControlFields()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var parent = new TransportDetailsUserControl())
		using (var control = new DispatchCountryUserControl())
		{
			parent.Controls.Add(control);
			form.Controls.Add(parent);
			form.Show();

			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(control.DispatchCountryTextBox);
				AssertType<ZCheckBox>(control.DispatchCountryConfirmationCheckBox);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		declaration.Transports.AddNew();
	}
	JobDeclaration declaration;
}

