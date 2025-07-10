using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	public class AsycudaPackedItemDetailsControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			using (var control = new AsycudaPackedItemDetailsControl())
			{
				AssertNotNull(control.FindSingle<ZCalcEdit>("SeqCalcEdit"));
				AssertNotNull(control.FindSingle<Universal.GUI.TariffFindBox>("TariffCodeFindBox"));
				AssertNotNull(control.FindSingle<ZTextBox>("GoodsDescriptionTextBox"));
				AssertNotNull(control.FindSingle<ZCalcEdit>("GrossWeightCalcEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("UQDropEdit"));
				AssertNotNull(control.FindSingle<ZDropEdit>("PackStatusDropEdit"));
			}
		}
	}
}
