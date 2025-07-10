using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.GUI.Testing;

sealed class AdditionalGoodsInformationUserControlTest : TestCase
{
	public void TestTabPages()
	{
		using (var control = new AdditionalGoodsInformationUserControl())
		{
			AssertSame(control.SupernumeraryGoodsTabPage, control.AdditionalGoodsInformationTabControl.TabPages[0]);
			AssertSame(control.AdditionalTransitOperationsTabPage, control.AdditionalGoodsInformationTabControl.TabPages[1]);
		}
	}
}
