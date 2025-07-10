using System.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.OperationalActions;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk
{
	public class RenominateMethod : GbOperationalActionMethod
	{
		public RenominateMethod()
			: base(new ZGuid("A1769D7C-232D-482C-952E-BFB2AF3E9257")) { }

		public override string Name
		{
			get { return "GB CCSUK function: nominate new agent and send FRC"; }
		}

		public override string Description
		{
			get { return "Change the nominated agent and send the FRC message.  Sheds only."; }
		}

		public override OperationalActionMethodApplicator NewApplicator(CargoWise.EntityFramework.BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		{
			return new RenominateApplicator(factory);
		}

		public override bool HasControl
		{
			get { return true; }
		}

		public override IComponent NewGuiControl()
		{
			return new RenominateUserControl();
		}
	}
}
