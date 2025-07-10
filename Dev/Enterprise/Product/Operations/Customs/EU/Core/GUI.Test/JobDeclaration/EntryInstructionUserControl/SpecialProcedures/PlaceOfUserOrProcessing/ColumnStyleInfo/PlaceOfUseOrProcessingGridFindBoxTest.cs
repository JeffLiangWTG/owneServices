using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.Testing
{
	sealed class PlaceOfUseOrProcessingGridFindBoxTest : TestCaseWithFactory
	{
		public void TestCodeBox_ReadOnly()
		{
			using (var findBox = new PlaceOfUseOrProcessingGridFindBox())
			{
				AssertEquals(true, findBox.CodeBox.ReadOnly);
			}
		}

		public void TestPopupButton_ReadOnly()
		{
			using (var findBox = new PlaceOfUseOrProcessingGridFindBox())
			{
				AssertEquals(false, findBox.PopupButton.ReadOnly);
			}
		}

		public void TestPopupForm()
		{
			var placeOfUseOrProcessing = Factory.New<PlaceOfUseOrProcessing>();
			using (var findBox = new PlaceOfUseOrProcessingGridFindBox())
			{
				findBox.PlaceOfUseOrProcessing = placeOfUseOrProcessing;
				var propertyInfo = typeof(PlaceOfUseOrProcessingGridFindBox).GetProperty("PopupForm", BindingFlags.NonPublic | BindingFlags.Instance);
				var popupForm = propertyInfo.GetValue(findBox);
				using ((ZChildForm)popupForm)
				{
					AssertType<PlaceOfUseOrProcessingForm>(popupForm);
				}
			}
		}
	}
}
