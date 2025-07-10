using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class SecurityAtDeparturePlaceOfUnloadingUserControlTest : TestCase
	{
		public void TestBindingSourceDataType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestPlaceOfUnloadingCodeFindBox()
		{
			var placeOfUnloadingCodeFindBox = control.PlaceOfUnloadingCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", placeOfUnloadingCodeFindBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_ForeignDestPortKCode), placeOfUnloadingCodeFindBox.BindTo);
				AssertEquals("ShowDescriptionBox", false, placeOfUnloadingCodeFindBox.ShowDescriptionBox);
			});
		}

		public void TestPlaceOfUnloadingTextBox()
		{
			var placeOfUnloadingTextBox = control.PlaceOfUnloadingTextBox;

			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", placeOfUnloadingTextBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_PlaceOfUnloading), placeOfUnloadingTextBox.BindTo);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, placeOfUnloadingTextBox.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SecurityAtDeparturePlaceOfUnloadingUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		SecurityAtDeparturePlaceOfUnloadingUserControl control;
	}
}
