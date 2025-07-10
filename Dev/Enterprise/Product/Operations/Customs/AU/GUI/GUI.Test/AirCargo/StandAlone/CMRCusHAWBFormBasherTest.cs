using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(CusHAWBForm))]
	sealed class CMRCusHAWBFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			return new CusHAWBForm(cusHAWB) { ControllerID = ControllerIDs.Customs.AU.HouseAirCargo };
		}
	}
}
