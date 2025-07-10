using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCargoOutturnBillsForm))]
	sealed class AirCargoOutturnBillsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var underbond = Factory.New<Declaration.Business.CusUnderbond>();
			var result = new AirCargoOutturnBillsForm(underbond);
			result.ControllerID = ControllerIDs.Customs.AU.AirCargoOutturnBillsController;
			return result;
		}
	}
}
