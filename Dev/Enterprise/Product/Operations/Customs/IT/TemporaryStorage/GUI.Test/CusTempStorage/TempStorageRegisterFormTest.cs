using System.Windows.Forms;
using Enterprise.Customs.IT.TemporaryStorage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.IT.TemporaryStorage.GUI.Testing;

[TestedType(typeof(TempStorageRegisterForm))]
sealed class TempStorageRegisterFormTest : ZFormBasherTest
{
	public void TestAllowNew()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		using (var form = new TempStorageRegisterForm(header))
		{
			var provider = form as IPostingButtonsProvider;
			AssertNotNull(provider);
			AssertEquals("User should not be able to create new register, the system does instead", false, provider.AllowNew);
		}
	}

	public void TestFormCaption()
	{
		var header = Factory.New<CusTempStorageRegHeader>();
		using (var form = new TempStorageRegisterForm(header))
		{
			form.Show();
			CombineAssertions(() =>
			{
				AssertEquals("No Reference", "Temp. Storage Register", form.FormCaption);
				header.SRH_Reference = "ATB150002110520195876";
				header.SRH_InternalReference = "FRJ00480021";
				AssertEquals("Reference Entered", "Temp. Storage Register - ATB150002110520195876/FRJ00480021", form.FormCaption);
			});
		}
	}

	protected override Form GetFormToBashCore()
	{
		var header = Factory.NewWithValidTestData<CusTempStorageRegHeader>();
		Factory.Save();
		var result = new TempStorageRegisterForm(header);
		result.ControllerID = ControllerIDs.Customs.EU.TempStorageRegister;
		return result;
	}
}
