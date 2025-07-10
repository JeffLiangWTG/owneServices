using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	sealed class CMRAirCargoStandAloneHouseMessageUserControlTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CMRAirCargoStandAloneHouseMessageUserControl testControl = new CMRAirCargoStandAloneHouseMessageUserControl();
			ZForm form = new ZForm { Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 680, true) };
			form.Controls.Add(testControl);
			return form;
		}
	}
}
