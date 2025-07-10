using System.Windows.Forms;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.UPE.GUI
{
	[TestedType(typeof(UPEAirCargoHouseForm))]
	internal class BasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var master = Factory.New<CusMAWB>();
			var houseBill = (UPECusHAWB)master.ChildBills.AddNew();
			var result = new UPEAirCargoHouseForm(houseBill);
			result.ControllerID = ControllerIDs.Customs.AU.HouseAirCargo;
			return result;
		}

		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}
	}
}
