using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	class CusGoodsLocationUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new CusGoodsLocationUserControl())
			{
				AssertEquals(typeof(CusGoodsLocation), control.BindingSource.DataSourceType);
			}
		}

		public void TestControls()
		{
			using (var control = new CusGoodsLocationUserControl())
			{
				CombineAssertions(() =>
				{
					AssertNoExceptionThrown("LoadingPlaceTextBox", () => control.FindSingle<ZTextBox>("LoadingPlaceTextBox"));
					AssertNoExceptionThrown("AdditionalIdentifierDropEdit", () => control.FindSingle<ZDropEdit>("AdditionalIdentifierDropEdit"));
				});
			}
		}
	}
}
