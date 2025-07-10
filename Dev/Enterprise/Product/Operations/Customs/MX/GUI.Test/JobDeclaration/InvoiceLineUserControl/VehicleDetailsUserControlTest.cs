using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.MX.GUI.Testing
{
	[TestedType(typeof(VehicleDetailsUserControl))]
	internal class VehicleDetailsUserControlTest : TestCase
	{
		public void TestProperties()
		{
			using (var control = new VehicleDetailsUserControl())
			{
				AssertType<ZArchitecture.ZTextBox>("VehicleVINTextBox type should be", control.VehicleVINTextBox);
				AssertType<ZCalcDropEdit>("VehicleMileageCalcDropEdit type should be", control.VehicleMileageCalcDropEdit);
			}
		}
	}
}
