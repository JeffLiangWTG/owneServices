using System.Windows.Forms;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.TNT.GUI.Testing
{
	[TestedType(typeof(DeclarationFromAirCargoForm))]
	public class DeclarationFromAirCargoFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new DeclarationFromAirCargoForm(creator);
		}

		#region Implementation
		DeclarationsFromCusMAWBCreator creator;
		CusMAWB masterBill;
		ForwardingConsol consol;
		protected override void SetUp()
		{
			base.SetUp();
			consol = Factory.NewWithValidTestData<ForwardingConsol>();
			masterBill = CusMAWB.CreateNew(consol);
			creator = new DeclarationsFromCusMAWBCreator(masterBill);
		}
		#endregion
	}
}
