using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	class SecurityTabUserControlTest : TestCaseWithFactory
	{
		public void TestPortOfUnloading_Departure()
		{
			var header = GetNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			using (var control = new SecurityTabUserControl())
			{
				control.SetDataBinding(header, "");
				var placeOfUnloadingTextBox = control.FindSingle<ZTextBox>("PlaceOfUnloadingTextBox");
				var placeOfUnloadingFindBox = control.FindSingle<ZCodeFindBox>("PlaceOfUnloadingFindBox");
				CombineAssertions(() =>
				{
					AssertEquals("Text", true, placeOfUnloadingTextBox.Visible);
					AssertEquals("FindBox", false, placeOfUnloadingFindBox.Visible);
				});
			}
		}

		public void TestPortOfUnloading_Arrival()
		{
			var header = GetNctsHeader();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var control = new SecurityTabUserControl())
			{
				control.SetDataBinding(header, "");
				var placeOfUnloadingTextBox = control.FindSingle<ZTextBox>("PlaceOfUnloadingTextBox");
				var placeOfUnloadingFindBox = control.FindSingle<ZCodeFindBox>("PlaceOfUnloadingFindBox");
				CombineAssertions(() =>
				{
					AssertEquals("Text", false, placeOfUnloadingTextBox.Visible);
					AssertEquals("Find Box", true, placeOfUnloadingFindBox.Visible);
				});
			}
		}

		public void TestConveyanceReferenceNumberTextBoxAllowsLowerCase()
		{
			using (var control = new SecurityTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("ConveyanceReferenceNumberTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestPlaceOfUnloadingTextBoxAllowsLowerCase()
		{
			using (var control = new SecurityTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("PlaceOfUnloadingTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		public void TestCommercialReferenceNumberTextBoxAllowsLowerCase()
		{
			using (var control = new SecurityTabUserControl())
			{
				var editControl = control.FindSingle<ZTextBox>("CommercialReferenceNumberTextBox");
				AssertEquals("normal casing", CharacterCasing.Normal, editControl.CharacterCasing);
			}
		}

		protected virtual NctsHeader GetNctsHeader() => Factory.New<NctsHeader>();
	}
}
