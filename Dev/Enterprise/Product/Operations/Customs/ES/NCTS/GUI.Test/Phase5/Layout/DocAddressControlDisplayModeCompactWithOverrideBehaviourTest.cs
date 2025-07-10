using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class DocAddressControlDisplayModeCompactWithOverrideBehaviourTest : TestCaseWithFactory
	{
		public void TestUpdateBehaviour()
		{
			var nctsHeader = Factory.New<Business.NctsHeader>();
			using (var zDocAddressControl = new ZDocAddressControl())
			{
				CombineAssertions(() =>
				{
					AssertEquals("base zDocAddressControl DisplayMode", ZDocAddressControlDisplayMode.ShowOverrideAndTabs, zDocAddressControl.DisplayMode);

					var behaviour = new DocAddressControlDisplayModeCompactWithOverrideBehaviour();
					behaviour.UpdateBehaviour(zDocAddressControl, nctsHeader);
					AssertEquals("after zDocAddressControl DisplayMode", ZDocAddressControlDisplayMode.CompactWithOverride, zDocAddressControl.DisplayMode);
				});
			}
		}
	}
}
