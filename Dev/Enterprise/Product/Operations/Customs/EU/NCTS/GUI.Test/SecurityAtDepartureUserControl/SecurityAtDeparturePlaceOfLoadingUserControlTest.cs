using System.Windows.Forms;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class SecurityAtDeparturePlaceOfLoadingUserControlTest : TestCase
	{
		public void TestBindingSourceDataType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestPlaceOfLoadingCodeFindBox()
		{
			var placeOfLoadingCodeFindBox = control.PlaceOfLoadingCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", placeOfLoadingCodeFindBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_PortOfPresentationCode), placeOfLoadingCodeFindBox.BindTo);
				AssertEquals(false, placeOfLoadingCodeFindBox.ShowDescriptionBox);
			});
		}

		public void TestPlaceOfLoadingTextBox()
		{
			var placeOfLoadingTextBox = control.PlaceOfLoadingTextBox;

			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", placeOfLoadingTextBox);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_PlaceOfLoading), placeOfLoadingTextBox.BindTo);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, placeOfLoadingTextBox.CharacterCasing);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new SecurityAtDeparturePlaceOfLoadingUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		SecurityAtDeparturePlaceOfLoadingUserControl control;
	}
}
