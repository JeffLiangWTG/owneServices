using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.AirCargo.GUI.Testing
{
	[TestedType(typeof(AirCargoHouseForm))]
	sealed class CMRAirCargoFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var cusMAWB = Factory.New<CusMAWB>();
			var cusHAWB = cusMAWB.ChildBills.AddNew();
			var result = new AirCargoHouseForm(cusHAWB);
			result.ControllerID = ControllerIDs.Customs.AU.HouseAirCargo;
			return result;
		}
	}
}
