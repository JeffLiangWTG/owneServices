using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class SecurityTabUserControlTest : TestCaseWithFactory
{
	public void TestPlaceOfUnloadingControls()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		using (var control = new SecurityTabUserControl())
		{
			control.SetDataBinding(nctsHeader, "");
			var placeOfUnloadingTextBox = control.FindSingle<ZTextBox>("PlaceOfUnloadingTextBox");
			var placeOfUnloadingFindBox = control.FindSingle<ZCodeFindBox>("PlaceOfUnloadingFindBox");
			CombineAssertions(() =>
			{
				AssertEquals("PlaceOfUnloadingTextBox.Visible", false, placeOfUnloadingTextBox.Visible);

				AssertEquals("PlaceOfUnloadingFindBox.Visible", true, placeOfUnloadingFindBox.Visible);
				AssertEquals("PlaceOfUnloadingFindBox.ShowDescriptionBox", false, placeOfUnloadingFindBox.ShowDescriptionBox);
				AssertEquals("PlaceOfUnloadingFindBox.Left", 185, placeOfUnloadingFindBox.Left);
			});
		}
	}
}
