using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IT.Business.Declaration;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class LocationOfGoodsUserControlTest : TestCaseWithFactory
{
	public void TestGetCusGoodsLocationForm()
	{
		using (var control = new LocationOfGoodsUserControlForTest())
		using (var cusGoodsLocationForm = control.GetCusGoodsLocationFormExposed(Factory.New<JobDeclaration>()))
		{
			AssertType<CusGoodsLocationForm>("Type", cusGoodsLocationForm);
		}
	}

	public void TestMoreButton()
	{
		using (var control = new LocationOfGoodsUserControlForTest())
		{
			var moreButton = control.MoreButtonExposed;
			AssertEquals("Location - X", 252, moreButton.Location.X);
			AssertEquals("Size - Width", 40, moreButton.Width);
		}
	}

	public void TestLocationOfGoodsDescription()
	{
		using (var control = new LocationOfGoodsUserControlForTest())
		{
			var locationOfGoodsDescription = control.LocationOfGoodsDescriptionExposed;
			AssertEquals("Size - Width", 250, locationOfGoodsDescription.Width);
		}
	}

	class LocationOfGoodsUserControlForTest : LocationOfGoodsUserControl
	{
		public EU.GUI.CusGoodsLocationForm GetCusGoodsLocationFormExposed(ICusGoodsLocationProvider provider) => base.GetCusGoodsLocationForm(provider);

		public ZArchitecture.GUI.ZButton MoreButtonExposed => MoreButton;
		public ZArchitecture.ZTextBox LocationOfGoodsDescriptionExposed => LocationOfGoodsDescription;
	}
}
